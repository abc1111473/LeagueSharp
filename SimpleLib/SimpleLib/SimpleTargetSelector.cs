#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
#endregion

namespace SimpleLib
{
    namespace SimpleTargetSelector
    {
        class STS
        {
            public static Menu Config;
            public static Obj_AI_Hero Self = ObjectManager.Player;
            public static IEnumerable<Obj_AI_Hero> AllEnemys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy);
            public static IEnumerable<Obj_AI_Hero> AllAllys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly);

            public enum Mode
            {
                LowHP,
                Priority,
                MostAP,
                MostAD,
                Closest,
                NearMouse,
                None
            }
            public static string[] APCarry = { "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus", "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna", "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz" };
            public static string[] Support = { "Blitzcrank", "Janna", "Karma", "Leona", "Lulu", "Nami", "Sona", "Soraka", "Thresh", "Zilean" };
            public static string[] Tank = { "Amumu", "Chogath", "DrMundo", "Galio", "Hecarim", "Malphite", "Maokai", "Nasus", "Rammus", "Sejuani", "Shen", "Singed", "Skarner", "Volibear", "Warwick", "Yorick", "Zac", "Nunu", "Taric", "Alistar", "Garen", "Nautilus", "Braum" };
            public static string[] ADCarry = { "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "KogMaw", "MissFortune", "Quinn", "Sivir", "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Zed", "Jinx", "Yasuo", "Lucian", "Kalista" };
            public static string[] Bruiser = { "Darius", "Elise", "Evelynn", "Fiora", "Gangplank", "Gnar", "Jayce", "Pantheon", "Irelia", "JarvanIV", "Jax", "Khazix", "LeeSin", "Nocturne", "Olaf", "Poppy", "Renekton", "Rengar", "Riven", "Shyvana", "Trundle", "Tryndamere", "Udyr", "Vi", "MonkeyKing", "XinZhao", "Aatrox", "Rumble", "Shaco", "MasterYi" };

            public static Obj_AI_Hero SelectedTarget;

            private static float _range = 600f;
            private static bool _enableSTS = true;
            private static bool _updateTarget = true;
            private static Mode _currentEnemyMode;
            private static Mode _currentAllyMode;
            private static bool _enableEMR = true;
            private static int _extendMonitarRange = 400;
            private static bool _smiteTarget = false;
            private static bool _forceMode = false;
            private static bool _focusTarget = true;
            private static bool _enableAllyMenu = true;
            private static Obj_AI_Hero _focustTarget = null;
            private static Obj_AI_Hero _focustEnemySmiteTarget = null;
            
            public static void SetMode(Mode setMode)
            {
                _forceMode = true;
                CurrentEnemyMode = setMode;
            }

            public static void ForceMode(bool force = false)
            {
                _forceMode = force;
            }

            /// <summary>
            ///     Sets if ally menu is included in the STS menu.
            /// </summary>
            public static void EnableAllyMenu(bool setAllyMenu) 
            {
                _enableAllyMenu = setAllyMenu;
            }

            /// <summary>
            ///     Returns the enemy target that has Summener Spell Smite.
            /// </summary>
            public static Obj_AI_Hero EnemyWithSmite()
            {
                return _focustEnemySmiteTarget;
            }

            /// <summary>
            ///     Overrides SelectedTarget with newTarget.
            ///     It disables searching for the new target and it has to be enabled manually with DisableTargetOverride().
            /// </summary>
            public void OverrideTarget(Obj_AI_Hero newTarget)
            {
                SelectedTarget = newTarget;
                _updateTarget = false;
            }
            /// <summary>
            ///     Disables the Target Override.
            /// </summary>
            public void DisableTargetOverride()
            {
                _updateTarget = true;
            }

            /// <summary>
            ///     Enables STS.
            /// </summary>
            public static void EnableSTS()
            {
                _enableSTS = true;
            }

            /// <summary>
            ///     Disables STS.
            /// </summary>
            public static void DisableSTS()
            {
                _enableSTS = false;
            }

            /// <summary>
            ///     Returns or sets current STS monitar range.
            /// </summary>
            public static float Range
            {
                get 
                { 
                    return _range; 
                }
                set 
                { 
                    _range = value; 
                }
            }

            /// <summary>
            ///     Returns or sets current STS enemy mode.
            /// </summary>
            public static Mode CurrentEnemyMode
            {
                get
                {
                    return _currentEnemyMode;
                }
                set
                {
                    _currentEnemyMode = value;
                }
            }

            /// <summary>
            ///     Returns or sets current STS ally mode.
            /// </summary>
            public static Mode CurrentAllyMode
            {
                get
                {
                    return _currentAllyMode;
                }
                set
                {
                    _currentAllyMode = value;
                }
            }

            private static Mode GetCurrentEnemyMode()
            {
                switch (Config.Item("TSMode").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        return Mode.LowHP;
                    case 1:
                        return Mode.Priority;
                    case 2:
                        return Mode.MostAD;
                    case 3:
                        return Mode.MostAP;
                    case 4:
                        return Mode.Closest;
                    case 5:
                        return Mode.NearMouse;
                    default:
                        return Mode.None;
                }
            }

            private static Mode GetCurrentAllyMode()
            {
                switch (Config.Item("AllyTSMode").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        return Mode.LowHP;
                    case 1:
                        return Mode.Priority;
                    case 2:
                        return Mode.MostAD;
                    case 3:
                        return Mode.MostAP;
                    case 4:
                        return Mode.Closest;
                    case 5:
                        return Mode.NearMouse;
                    default:
                        return Mode.None;
                }
            }

            /// <summary>
            ///     Initializes the STS with default values. 
            ///     If u want to set custom values for force mode, EMR and AllyMenu they should be set before this.
            /// </summary>
            public static void InitializeSTS(Menu ConfigMenu)
            {   
                Game.OnGameUpdate += Game_OnGameUpdate;
                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnWndProc += Game_OnWndProc;              

                AddToMenu(ConfigMenu);
                Config.Item("TSMode").ValueChanged += TSMode_ValueChanged;
                Config.Item("AllyTSMode").ValueChanged += AllyTSMode_ValueChanged;
                Config.Item("FocusMode").ValueChanged += FocusMode_ValueChanged;
                Config.Item("SmiteMode").ValueChanged += SmiteMode_ValueChanged;
                Config.Item("EMR").ValueChanged += EMR_ValueChanged;
                GetEnemyWithSmite();
            }            

            private static void AddToMenu(Menu ConfigMenu)
            {
                Config = ConfigMenu;

                var menuEnemyPriorety = new Menu("Enemy Priorety", "Priorety");

                menuEnemyPriorety.AddItem(new MenuItem("TSMode", "Target Selector Mode:").SetValue(new StringList(new[] { "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse" })));

                foreach (var enemy in AllEnemys)
                {
                    menuEnemyPriorety.AddItem(new MenuItem(enemy.BaseSkinName, enemy.BaseSkinName)).SetValue(new Slider(GetAutoPriorety(enemy.BaseSkinName), 5, 0));
                }               

                Config.AddSubMenu(menuEnemyPriorety);

                if (_enableAllyMenu) Config.AddSubMenu(AllyMenu());

                if (_enableEMR)
                {
                    Config.AddItem(new MenuItem("EMR", "Extend Monitar Range").SetValue(new Slider(_extendMonitarRange, 0, 1000)));
                }
                Config.AddItem(new MenuItem("FocusMode", "Focus Selected Target")).SetValue<bool>(true);
                Config.AddItem(new MenuItem("SmiteMode", "Focus Target With Smite")).SetValue<bool>(false);
            }

            /// <summary>
            ///     Returns STS menu for allys. It can be used outside STS but it has to be disabled in the main STS menu EnableAllyMenu(bool)
            /// </summary>
            public static Menu AllyMenu()
            {
                var menuAllyPriorety = new Menu("Ally Priorety", "AllySTS");

                menuAllyPriorety.AddItem(new MenuItem("AllyTSMode", "Target Selector Mode:").SetValue(new StringList(new[] { "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse" })));

                foreach (var ally in AllAllys)
                {
                    menuAllyPriorety.AddItem(new MenuItem(ally.BaseSkinName, ally.BaseSkinName)).SetValue(new Slider(GetAutoPriorety(ally.BaseSkinName), 5, 0));
                }                
                return menuAllyPriorety;
            }

            private static void SmiteMode_ValueChanged(object sender, OnValueChangeEventArgs e)
            {
                _smiteTarget = Config.Item("SmiteMode").GetValue<bool>();
            }

            private static void FocusMode_ValueChanged(object sender, OnValueChangeEventArgs e)
            {
                _focusTarget = Config.Item("FocusMode").GetValue<bool>();
            }

            private static void TSMode_ValueChanged(object sender, OnValueChangeEventArgs e)
            {
                SelectedTarget = null;
                _currentEnemyMode = GetCurrentEnemyMode();
            }

            private static void AllyTSMode_ValueChanged(object sender, OnValueChangeEventArgs e)
            {
                _currentAllyMode = GetCurrentAllyMode();
            }

            private static void EMR_ValueChanged(object sender, OnValueChangeEventArgs e)
            {
                _extendMonitarRange = Config.Item("EMR").GetValue<Slider>().Value;
            }

            /// <summary>
            ///     Returns the auto priorety.
            /// </summary>
            public static int GetAutoPriorety(string ChampName)
            {
                if (ADCarry.Contains(ChampName)) return 5;
                if (APCarry.Contains(ChampName)) return 4;
                if (Bruiser.Contains(ChampName)) return 3;
                if (Support.Contains(ChampName)) return 2;
                if (Tank.Contains(ChampName)) return 1;
                return 0;
            }

            /// <summary>
            ///     Returns the current priorety set by the slider.
            /// </summary>
            public static int GetPriorety(string ChampName)
            {
                return Config.Item(ChampName).GetValue<Slider>().Value;
            }

            /// <summary>
            ///     Comperes T1 and T2 to determin which has lowest current HP and returns it.
            ///     If they have the same then it goes to ComperePriorety();
            /// </summary>
            public static Obj_AI_Hero CompereHealth(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (target_1.Health < target_2.Health) return target_1;

                if (target_1.Health == target_2.Health)
                {
                    return ComperePriorety(target_1, target_2);
                }
                return target_2;
            }

            /// <summary>
            ///     Comperes T1 and T2 to determin which has highest current priorety and returns it.
            ///     If they have the same then it goes to CompereHealth();
            /// </summary>
            public static Obj_AI_Hero ComperePriorety(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (GetPriorety(target_1.BaseSkinName) < GetPriorety(target_2.BaseSkinName)) return target_2;

                if (GetPriorety(target_1.BaseSkinName) == GetPriorety(target_2.BaseSkinName))
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            /// <summary>
            ///     Comperes T1 and T2 to determin which has highest auto priorety and returns it.
            ///     If they have the same then it goes to CompereHealth();
            /// </summary>
            public static Obj_AI_Hero CompereAutoPriorety(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (GetAutoPriorety(target_1.BaseSkinName) < GetAutoPriorety(target_2.BaseSkinName)) return target_2;

                if (GetAutoPriorety(target_1.BaseSkinName) == GetAutoPriorety(target_2.BaseSkinName))
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            /// <summary>
            ///     Comperes T1 and T2 to determin which has more AP and returns it.  
            ///     Compare includes both BaseAbilityDamage and flat FlatMagicDamageMod.
            ///     If they have the same then it goes to CompereHealth();
            /// </summary>
            public static Obj_AI_Hero CompereAP(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (target_1.FlatMagicDamageMod + target_1.BaseAbilityDamage < target_2.FlatMagicDamageMod + target_2.BaseAbilityDamage) return target_2;

                if (target_1.FlatMagicDamageMod + target_1.BaseAbilityDamage == target_2.FlatMagicDamageMod + target_2.BaseAbilityDamage)
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            /// <summary>
            ///     Comperes T1 and T2 to determin which has more AD and returns it.  
            ///     Compare includes both BaseAttackDamage and flat FlatPhysicalDamageMod.
            ///     If they have the same then it goes to CompereHealth();
            /// </summary>
            public static Obj_AI_Hero CompereAD(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (target_1.BaseAttackDamage + target_1.FlatPhysicalDamageMod < target_2.BaseAttackDamage + target_2.FlatPhysicalDamageMod) return target_2;

                if (target_1.BaseAttackDamage + target_1.FlatPhysicalDamageMod == target_2.BaseAttackDamage + target_2.FlatPhysicalDamageMod)
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            /// <summary>
            ///     Comperes T1 and T2 to determin closest to the current players position and returns it.  
            ///     If they have the same then it goes to CompereHealth();
            /// </summary>
            public static Obj_AI_Hero CompereClosest(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (Geometry.Distance(target_1, Self) > Geometry.Distance(target_2, Self)) return target_2;

                if (Geometry.Distance(target_1, Self) == Geometry.Distance(target_2, Self))
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            /// <summary>
            ///     Comperes T1 and T2 to determin nearest to the current currsor position and returns it. 
            ///     If they have the same then it goes to CompereHealth();
            /// </summary>
            public static Obj_AI_Hero CompereNearMouse(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (Geometry.Distance(target_1, Game.CursorPos) > Geometry.Distance(target_2, Game.CursorPos)) return target_2;

                if (Geometry.Distance(target_1, Game.CursorPos) == Geometry.Distance(target_2, Game.CursorPos))
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            /// <summary>
            ///     Return the enemy with lowest current HP in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetLowHPEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereHealth(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the enemy with highest current priorety in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetPrioretyEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = ComperePriorety(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the enemy with most AP in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetMostAPEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereAP(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the enemy with most AD in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetMostADEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereAD(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the closest enemy from current players positon in specified Range and extendMonitarRange. 
            /// </summary>
            public static Obj_AI_Hero GetClosestEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;
                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereClosest(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the enemy nearest to the current cursor position in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetNearMouseEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;
                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereNearMouse(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the ally with lowest current HP in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetLowHPAlly(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllAllys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereHealth(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the ally with highest current priorety in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetPrioretyAlly(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllAllys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = ComperePriorety(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the ally with most AP in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetMostAPAlly(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllAllys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereAP(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the ally with most AD in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetMostADAlly(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllAllys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereAD(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the closest ally from current players positon in specified Range and extendMonitarRange. 
            /// </summary>
            public static Obj_AI_Hero GetClosestAlly(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;
                foreach (var target in AllAllys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereClosest(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Return the ally nearest to the current cursor position in specified Range and extendMonitarRange.
            /// </summary>
            public static Obj_AI_Hero GetNearMouseAlly(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;
                foreach (var target in AllAllys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereNearMouse(target, newTarget);
                }
                return newTarget;
            }

            /// <summary>
            ///     Returns List<Obj_AI_Hero> of allys with smite.
            /// </summary>
            public static List<Obj_AI_Hero> GetAllysWithSmite()
            {
                List<Obj_AI_Hero> SmiteAllys = new List<Obj_AI_Hero>();
                foreach (Obj_AI_Hero ally in AllAllys.Where
                    (ally => ally.SummonerSpellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("smite") ||
                        ally.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("smite")))
                {
                    SmiteAllys.Add(ally);
                }
                return SmiteAllys;
            }

            /// <summary>
            ///     Returns List<Obj_AI_Hero> of enemys with smite.
            /// </summary>
            public static List<Obj_AI_Hero> GetEnemysWithSmite()
            {
                List<Obj_AI_Hero> SmiteEnemys = new List<Obj_AI_Hero>();
                foreach (Obj_AI_Hero enemy in AllEnemys.Where
                    (enemy => enemy.SummonerSpellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("smite") ||
                        enemy.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("smite")))
                {
                    SmiteEnemys.Add(enemy);
                }
                return SmiteEnemys;
            }

            /// <summary>
            ///     Returns List<Obj_AI_Hero> of enemys with specified Summoner Spell.
            ///     For check it uses Contains so you can use "smite" for "SummonerSmite" and it will return correctly.
            /// </summary>
            public static List<Obj_AI_Hero> GetEnemysSummenerSpell(string SummonerSpellName)
            {
                List<Obj_AI_Hero> SmiteEnemys = new List<Obj_AI_Hero>();
                foreach (Obj_AI_Hero enemy in AllEnemys.Where
                    (enemy => enemy.SummonerSpellbook.GetSpell(SpellSlot.Summoner1).Name.Contains(SummonerSpellName) ||
                        enemy.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains(SummonerSpellName)))
                {
                    SmiteEnemys.Add(enemy);
                }
                return SmiteEnemys;
            }

            private static void GetEnemyWithSmite()
            {                
                foreach (var enemy in AllEnemys.Where
                    (enemy => enemy.SummonerSpellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("smite") ||
                        enemy.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("smite")))
                {
                    _focustEnemySmiteTarget = enemy;
                }
            }

            /// <summary>
            ///     Searches for the enemy target with CurrentEnemyMode, range and extend monitar range. 
            ///     Then sets it as SelectedTarget.
            /// </summary>
            public static void SearchForEnemyTarget()
            {
                if (_focustTarget != null && _focustTarget.IsValidTarget(_range + _extendMonitarRange))
                {
                    SelectedTarget = _focustTarget;
                    return;
                }

                if (_smiteTarget == true && _focustEnemySmiteTarget != null && _focustEnemySmiteTarget.IsValidTarget(_range + _extendMonitarRange))
                {
                    SelectedTarget = _focustEnemySmiteTarget;
                    return;
                }
                switch (CurrentEnemyMode)
                {
                    case Mode.LowHP:
                        SelectedTarget = GetLowHPEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.Priority:
                        SelectedTarget = GetPrioretyEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.MostAP:
                        SelectedTarget = GetMostAPEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.MostAD:
                        SelectedTarget = GetMostADEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.Closest:
                        SelectedTarget = GetClosestEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.NearMouse:
                        SelectedTarget = GetNearMouseEnemy(_range, _extendMonitarRange);
                        return;
                    default:
                        return;
                }
            }

            /// <summary>
            ///     Searches for the enemy target with setMode, range and extend monitar range. 
            ///     Then sets it as SelectedTarget.
            /// </summary>
            public static void SearchForEnemyTarget(Mode setMode)
            {
                switch (setMode)
                {
                    case Mode.LowHP:
                        SelectedTarget = GetLowHPEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.Priority:
                        SelectedTarget = GetPrioretyEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.MostAP:
                        SelectedTarget = GetMostAPEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.MostAD:
                        SelectedTarget = GetMostADEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.Closest:
                        SelectedTarget = GetClosestEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.NearMouse:
                        SelectedTarget = GetNearMouseEnemy(_range, _extendMonitarRange);
                        return;
                    default:
                        return;
                }
            }

            /// <summary>
            ///     Searches for the enemy target with setMode, setRange and extend monitar range. 
            ///     Then sets it as SelectedTarget.
            /// </summary>
            public static void SearchForEnemyTarget(Mode setMode, float setRange)
            {
                switch (setMode)
                {
                    case Mode.LowHP:
                        SelectedTarget = GetLowHPEnemy(setRange, _extendMonitarRange);
                        return;
                    case Mode.Priority:
                        SelectedTarget = GetPrioretyEnemy(setRange, _extendMonitarRange);
                        return;
                    case Mode.MostAP:
                        SelectedTarget = GetMostAPEnemy(setRange, _extendMonitarRange);
                        return;
                    case Mode.MostAD:
                        SelectedTarget = GetMostADEnemy(setRange, _extendMonitarRange);
                        return;
                    case Mode.Closest:
                        SelectedTarget = GetClosestEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.NearMouse:
                        SelectedTarget = GetNearMouseEnemy(setRange, _extendMonitarRange);
                        return;
                    default:
                        return;
                }
            }

            /// <summary>
            ///     Searches for the enemy target with setMode, setRange and setExtendMonitarRange. 
            ///     Then sets it as SelectedTarget.
            /// </summary>
            public static void SearchForEnemyTarget(Mode setMode, float setRange, float setExtendMonitarRange)
            {
                switch (setMode)
                {
                    case Mode.LowHP:
                        SelectedTarget = GetLowHPEnemy(setRange, setExtendMonitarRange);
                        return;
                    case Mode.Priority:
                        SelectedTarget = GetPrioretyEnemy(setRange, setExtendMonitarRange);
                        return;
                    case Mode.MostAP:
                        SelectedTarget = GetMostAPEnemy(setRange, setExtendMonitarRange);
                        return;
                    case Mode.MostAD:
                        SelectedTarget = GetMostADEnemy(setRange, setExtendMonitarRange);
                        return;
                    case Mode.Closest:
                        SelectedTarget = GetClosestEnemy(_range, _extendMonitarRange);
                        return;
                    case Mode.NearMouse:
                        SelectedTarget = GetNearMouseEnemy(setRange, setExtendMonitarRange);
                        return;
                    default:
                        return;
                }
            }

            /// <summary>
            ///     Searches for the target with current ally settings and returns it. 
            /// </summary>
            public static Obj_AI_Hero SearchForAllyTarget()
            {
                switch (CurrentAllyMode)
                {
                    case Mode.LowHP:
                        return GetLowHPEnemy(_range, _extendMonitarRange);

                    case Mode.Priority:
                        return GetPrioretyAlly(_range, _extendMonitarRange);

                    case Mode.MostAP:
                        return GetMostAPAlly(_range, _extendMonitarRange);

                    case Mode.MostAD:
                        return GetMostADAlly(_range, _extendMonitarRange);

                    case Mode.Closest:
                        return GetClosestAlly(_range, _extendMonitarRange);

                    case Mode.NearMouse:
                        return GetNearMouseAlly(_range, _extendMonitarRange);

                    default:
                        return null;
                }
            }

            /// <summary>
            ///     Searches for the target with setAllyMode and returns it. 
            /// </summary>
            public static Obj_AI_Hero SearchForAllyTarget(Mode setAllyMode)
            {
                switch (setAllyMode)
                {
                    case Mode.LowHP:
                        return GetLowHPEnemy(_range, _extendMonitarRange);

                    case Mode.Priority:
                        return GetPrioretyAlly(_range, _extendMonitarRange);

                    case Mode.MostAP:
                        return GetMostAPAlly(_range, _extendMonitarRange);

                    case Mode.MostAD:
                        return GetMostADAlly(_range, _extendMonitarRange);

                    case Mode.Closest:
                        return GetClosestAlly(_range, _extendMonitarRange);

                    case Mode.NearMouse:
                        return GetNearMouseAlly(_range, _extendMonitarRange);

                    default:
                        return null;
                }
            }

            /// <summary>
            ///     Searches for the target with setAllyMode, setRange and returns it. 
            /// </summary>
            public static Obj_AI_Hero SearchForAllyTarget(Mode setAllyMode, float setRange)
            {
                switch (setAllyMode)
                {
                    case Mode.LowHP:
                        return GetLowHPEnemy(setRange, _extendMonitarRange);

                    case Mode.Priority:
                        return GetPrioretyAlly(setRange, _extendMonitarRange);

                    case Mode.MostAP:
                        return GetMostAPAlly(setRange, _extendMonitarRange);

                    case Mode.MostAD:
                        return GetMostADAlly(setRange, _extendMonitarRange);

                    case Mode.Closest:
                        return GetClosestAlly(setRange, _extendMonitarRange);

                    case Mode.NearMouse:
                        return GetNearMouseAlly(setRange, _extendMonitarRange);

                    default:
                        return null;
                }
            }

            /// <summary>
            ///     Searches for the target with setAllyMode, setRange, setExtendMonitarRange and returns it. 
            /// </summary>
            public static Obj_AI_Hero SearchForAllyTarget(Mode setAllyMode, float setRange, float setExtendMonitarRange)
            {
                switch (setAllyMode)
                {
                    case Mode.LowHP:
                        return GetLowHPEnemy(setRange, setExtendMonitarRange);

                    case Mode.Priority:
                        return GetPrioretyAlly(setRange, setExtendMonitarRange);

                    case Mode.MostAP:
                        return GetMostAPAlly(setRange, setExtendMonitarRange);

                    case Mode.MostAD:
                        return GetMostADAlly(setRange, setExtendMonitarRange);

                    case Mode.Closest:
                        return GetClosestAlly(setRange, setExtendMonitarRange);

                    case Mode.NearMouse:
                        return GetNearMouseAlly(setRange, setExtendMonitarRange);

                    default:
                        return null;
                }
            }

            static void Game_OnWndProc(WndEventArgs args)
            {
                if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN) return;

                foreach (var enemy in AllEnemys.Where(enemy => enemy.IsValidTarget()))
                {
                    _focustTarget = enemy;
                }
            }

            static void Drawing_OnDraw(EventArgs args)
            {
                if (!Self.IsDead && SelectedTarget != null && SelectedTarget.IsVisible && !SelectedTarget.IsDead)
                {
                    Render.Circle.DrawCircle(SelectedTarget.Position, 150, Color.Red, 5, true);
                }
            }

            static void Game_OnGameUpdate(EventArgs args)
            {
                if (!_enableSTS) return;
                if (!_updateTarget) return;
                SearchForEnemyTarget();
            }  
        }
    }
}
