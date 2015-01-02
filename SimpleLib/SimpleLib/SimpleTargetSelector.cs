using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SimpleLib
{
    public static class SimpleTargetSelector
    {
        public enum DamageType
        {
            Magical,
            Physical,
            Hybrid,
            True
        }

        public enum Mode
        {
            Auto,
            LowHp,
            Priority,
            MostAp,
            MostAd,
            Closest,
            NearMouse,
            Less,
            None
        }

        public enum Team
        {
            Enemy,
            Ally
        }

        private static readonly string[] ApCarry =
        {
            "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana",
            "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus", "Kassadin", "Katarina", "Kayle", "Kennen",
            "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna", "Ryze", "Sion",
            "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra",
            "Velkoz"
        };

        private static readonly string[] Support =
        {
            "Blitzcrank", "Janna", "Karma", "Leona", "Lulu", "Nami", "Sona",
            "Soraka", "Thresh", "Zilean"
        };

        private static readonly string[] Tank =
        {
            "Amumu", "Chogath", "DrMundo", "Galio", "Hecarim", "Malphite",
            "Maokai", "Nasus", "Rammus", "Sejuani", "Shen", "Singed", "Skarner", "Volibear", "Warwick", "Yorick", "Zac",
            "Nunu", "Taric", "Alistar", "Garen", "Nautilus", "Braum"
        };

        private static readonly string[] AdCarry =
        {
            "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "KogMaw",
            "MissFortune", "Quinn", "Sivir", "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Zed", "Jinx",
            "Yasuo", "Lucian", "Kalista"
        };

        private static readonly string[] Bruiser =
        {
            "Darius", "Elise", "Evelynn", "Fiora", "Gangplank", "Gnar", "Jayce",
            "Pantheon", "Irelia", "JarvanIV", "Jax", "Khazix", "LeeSin", "Nocturne", "Olaf", "Poppy", "Renekton",
            "Rengar", "Riven", "Shyvana", "Trundle", "Tryndamere", "Udyr", "Vi", "MonkeyKing", "XinZhao", "Aatrox",
            "Rumble", "Shaco", "MasterYi", "RekSai"
        };

        private static Menu _stsMenu;
        private static Menu _stsAllyMenu;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static float _monitorRange = Player.AttackRange;
        private static int _extendedMonitorRange = 400;
        private static DamageType _currentDamageType = DamageType.Hybrid;
        private static Obj_AI_Hero _focusedTarget;
        private static Obj_AI_Hero _focusedTargetWithSmite;
        private static bool _enemySearch = true;
        private static bool _enableAllyMenu = true;
        private static bool _enableEmr = true;
        private static bool _focusSelectedTarget = true;
        private static bool _focusTargetWithSmite = true;
        private static bool _enemyModeMenu = true;
        private static bool _damageTyprMenu = true;
        private static bool _allyModeMenu = true;
        private static Mode _enemyMode = Mode.Auto;
        private static Mode _allyMode = Mode.Auto;
        private static Obj_AI_Hero _enemyTarget;
        private static Obj_AI_Hero _allyTarget;
        public static IEnumerable<Obj_AI_Hero> AllEnemys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy);
        public static IEnumerable<Obj_AI_Hero> AllAllys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly);

        /// <summary>
        ///     Enable or disable Ally Menu in the STS Menu
        /// </summary>
        public static bool EnableAllyMenu
        {
            get { return _enableAllyMenu; }
            set { _enableAllyMenu = value; }
        }

        /// <summary>
        ///     Enable or disable Extended Monitor Range in the STS Menu
        /// </summary>
        public static bool EnableEMR
        {
            get { return _enableEmr; }
            set { _enableEmr = value; }
        }

        /// <summary>
        ///     Enable or disable FocusSelectedTarget
        /// </summary>
        public static bool FocusSelectedTarget
        {
            get { return _focusSelectedTarget; }
            set { _focusSelectedTarget = value; }
        }

        /// <summary>
        ///     Enable or disable focusing target with smite
        /// </summary>
        public static bool FocusTargetWithSmite
        {
            get { return _focusTargetWithSmite; }
            set { _focusTargetWithSmite = value; }
        }

        /// <summary>
        ///     Enables or disables enemy mode menu.
        ///     For Menu.
        /// </summary>
        public static bool EnemyModeMenu
        {
            get { return _enemyModeMenu; }
            set { _enemyModeMenu = value; }
        }

        /// <summary>
        ///     Enables or disables damage type menu.
        ///     For Menu.
        /// </summary>
        public static bool DamageTypeMenu
        {
            get { return _damageTyprMenu; }
            set { _damageTyprMenu = value; }
        }

        /// <summary>
        ///     Enables or disables ally mode menu.
        ///     For Menu.
        /// </summary>
        public static bool AllyModeMenu
        {
            get { return _allyModeMenu; }
            set { _allyModeMenu = value; }
        }

        /// <summary>
        ///     Returns or sets monitor range for the targrt selector.
        /// </summary>
        public static float MonitorRange
        {
            get { return _monitorRange; }
            set { _monitorRange = value; }
        }

        /// <summary>
        ///     Returns or sets the value for the extended monitor range for the targrt selector.
        /// </summary>
        public static int EMR
        {
            get
            {
                try
                {
                    _extendedMonitorRange = _stsMenu.Item("EMR").GetValue<Slider>().Value;
                    
                }
                catch
                {
                    // ignored
                }
                return _extendedMonitorRange;
            }
            set
            {
                try
                {
                    _stsMenu.Item("EMR").SetValue(new Slider(value, 0, 500));
                }
                catch
                {
                    // ignored
                }
                _extendedMonitorRange = value;
            }
        }

        public static DamageType CurrentDamagetType
        {
            get
            {
                try
                {
                    switch (_stsMenu.Item("DmgType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            _currentDamageType = DamageType.Magical;
                            break;
                        case 1:
                            _currentDamageType = DamageType.Physical;
                            break;
                        case 2:
                            _currentDamageType = DamageType.Hybrid;
                            break;
                        case 3:
                            _currentDamageType = DamageType.True;
                            break;
                    }
                }
                catch
                {
                    // ignored
                }
                return _currentDamageType;
            }
            set
            {
                _currentDamageType = value;
                try
                {
                    switch (value)
                    {
                        case DamageType.Magical:
                            _stsMenu.Item("DmgType")
                                .SetValue(new StringList(new[] { "Magical", "Physical", "Hybrid", "True" }));
                            break;
                        case DamageType.Physical:
                            _stsMenu.Item("DmgType")
                                .SetValue(new StringList(new[] { "Magical", "Physical", "Hybrid", "True" }, 1));
                            break;
                        case DamageType.Hybrid:
                            _stsMenu.Item("DmgType")
                                .SetValue(new StringList(new[] { "Magical", "Physical", "Hybrid", "True" }, 2));
                            break;
                        case DamageType.True:
                            _stsMenu.Item("DmgType")
                                .SetValue(new StringList(new[] { "Magical", "Physical", "Hybrid", "True" }, 3));
                            break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        public static Mode CurrentEnemyMode
        {
            get
            {
                try
                {
                    switch (_stsMenu.Item("TSMode").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            _enemyMode = Mode.Auto;
                            break;
                        case 1:
                            _enemyMode = Mode.LowHp;
                            break;
                        case 2:
                            _enemyMode = Mode.Priority;
                            break;
                        case 3:
                            _enemyMode = Mode.MostAd;
                            break;
                        case 4:
                            _enemyMode = Mode.MostAp;
                            break;
                        case 5:
                            _enemyMode = Mode.Closest;
                            break;
                        case 6:
                            _enemyMode = Mode.NearMouse;
                            break;
                        case 7:
                            _enemyMode = Mode.Less;
                            break;
                        default:
                            _enemyMode = Mode.None;
                            break;
                    }
                }
                catch
                {
                    // ignored
                }
                return _enemyMode;
            }
            set
            {
                _enemyMode = value;
                try
                {
                    switch (value)
                    {
                        case Mode.Auto:
                            _stsMenu.Item("TSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }));
                            break;
                        case Mode.LowHp:
                            _stsMenu.Item("TSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 1));
                            break;
                        case Mode.Priority:
                            _stsMenu.Item("TSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 2));
                            break;
                        case Mode.MostAp:
                            _stsMenu.Item("TSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 3));
                            break;
                        case Mode.MostAd:
                            _stsMenu.Item("TSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 4));
                            break;
                        case Mode.Closest:
                            _stsMenu.Item("TSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 5));
                            break;
                        case Mode.NearMouse:
                            _stsMenu.Item("TSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 6));
                            break;
                        case Mode.Less:
                            _stsMenu.Item("TSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 7));
                            break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        public static Mode CurrentAllyMode
        {
            get
            {
                try
                {
                    switch (_stsAllyMenu.Item("AllyTSMode").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            _allyMode = Mode.Auto;
                            break;
                        case 1:
                            _allyMode = Mode.LowHp;
                            break;
                        case 2:
                            _allyMode = Mode.Priority;
                            break;
                        case 3:
                            _allyMode = Mode.MostAd;
                            break;
                        case 4:
                            _allyMode = Mode.MostAp;
                            break;
                        case 5:
                            _allyMode = Mode.Closest;
                            break;
                        case 6:
                            _allyMode = Mode.NearMouse;
                            break;
                        case 7:
                            _allyMode = Mode.Less;
                            break;
                        default:
                            _allyMode = Mode.None;
                            break;
                    }
                }
                catch
                {
                    // ignored
                }

                return _allyMode;
            }
            set
            {
                _allyMode = value;
                try
                {
                    switch (value)
                    {
                        case Mode.Auto:
                            _stsAllyMenu.Item("AllyTSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }));
                            break;
                        case Mode.LowHp:
                            _stsAllyMenu.Item("AllyTSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 1));
                            break;
                        case Mode.Priority:
                            _stsAllyMenu.Item("AllyTSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 2));
                            break;
                        case Mode.MostAp:
                            _stsAllyMenu.Item("AllyTSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 3));
                            break;
                        case Mode.MostAd:
                            _stsAllyMenu.Item("AllyTSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 4));
                            break;
                        case Mode.Closest:
                            _stsAllyMenu.Item("AllyTSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 5));
                            break;
                        case Mode.NearMouse:
                            _stsAllyMenu.Item("AllyTSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 6));
                            break;
                        case Mode.Less:
                            _stsAllyMenu.Item("AllyTSMode").SetValue(new StringList(new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" }, 7));
                            break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        /// <summary>
        ///     Returns the current mode for selected team regardles of type of menu (Menu or MenuWrapper) and if mode menus are
        ///     disabled.
        /// </summary>
        public static Mode CurrentMode(Team selectTeam = Team.Enemy)
        {
            switch (selectTeam)
            {
                case Team.Enemy:
                    return CurrentEnemyMode;

                case Team.Ally:
                    return CurrentAllyMode;

                default:
                    return Mode.None;
            }
        }

        private static void InitSTS()
        {
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (_enemyTarget.IsValidTarget() && SimpleOrbWalker.Drawings)
            {
                Render.Circle.DrawCircle(_enemyTarget.Position, 150, Color.Red, 7, true);
            }
        }

        /// <summary>
        ///     Returns Menu for the STS
        /// </summary>
        public static void StsMenu(Menu menu)
        {
            _stsMenu = menu;

            var stsMenu = new Menu("Simple Target Selector", "STS");

            var menuEnemyPriorety = new Menu("Enemy Priorety", "Priorety");

            if (EnemyModeMenu)
            {
                menuEnemyPriorety.AddItem(
                    new MenuItem("TSMode", "Target Selector Mode:").SetValue(
                        new StringList(
                            new[]
                            { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse", "Less" })));
            }

            if (!AllEnemys.Any())
            {
                menuEnemyPriorety.AddItem(new MenuItem("No Enemys", "No Enemys"));
            }
            else
            {
                foreach (var enemy in AllEnemys)
                {
                    menuEnemyPriorety.AddItem(new MenuItem(enemy.ChampionName, enemy.ChampionName))
                        .SetValue(new Slider(GetAutoPriorety(enemy.ChampionName), 1, 5));
                }
            }

            stsMenu.AddSubMenu(menuEnemyPriorety);

            if (EnableAllyMenu)
            {
                StsAllyMenu(stsMenu);
            }

            if (EnableEMR)
            {
                stsMenu.AddItem(
                    new MenuItem("EMR", "Extend Monitar Range").SetValue(new Slider(_extendedMonitorRange, 0, 1000)));
            }
            stsMenu.AddItem(new MenuItem("FocusMode", "Focus Selected Target").SetValue(FocusSelectedTarget));
            stsMenu.AddItem(new MenuItem("SmiteMode", "Focus Target With Smite").SetValue(FocusTargetWithSmite));

            stsMenu.AddItem(
                new MenuItem("DmgType", "Damage Type:").SetValue(
                    new StringList(new[] { "Magical", "Physical", "Hybrid", "True" })));

            _stsMenu.AddSubMenu(stsMenu);

            InitSTS();
        }

        public static void StsAllyMenu(Menu menu)
        {
            var stsmenu = new Menu("Ally Priorety", "AllySTS");

            if (AllyModeMenu)
            {
                stsmenu.AddItem(
                    new MenuItem("AllyTSMode", "Target Selector Mode:").SetValue(
                        new StringList(
                            new[] { "Auto", "Low HP", "Priority", "Most AD", "Most AP", "Closest", "Near Mouse" })));
            }
            if (!AllAllys.Any())
            {
                stsmenu.AddItem(new MenuItem("No Allys", "No Allys"));
            }
            else
            {
                foreach (var ally in AllAllys)
                {
                    stsmenu.AddItem(new MenuItem(ally.ChampionName, ally.ChampionName))
                        .SetValue(new Slider(GetAutoPriorety(ally.ChampionName), 1, 5));
                }
            }

            _stsAllyMenu = menu;
            _stsAllyMenu.AddSubMenu(stsmenu);
        }

        /// <summary>
        ///     Returns the auto priorety.
        /// </summary>
        public static int GetAutoPriorety(string champName)
        {
            if (AdCarry.Contains(champName))
            {
                return 5;
            }
            if (ApCarry.Contains(champName))
            {
                return 4;
            }
            if (Bruiser.Contains(champName))
            {
                return 3;
            }
            if (Support.Contains(champName))
            {
                return 2;
            }
            if (Tank.Contains(champName))
            {
                return 1;
            }
            return 1;
        }

        /// <summary>
        ///     Returns the current priorety for the champ set by the slider regardles of menu type (Menu or MenuWrapper)
        /// </summary>
        public static int GetPriorety(string champName, Team setTeam = Team.Enemy)
        {
            if (champName == "")
            {
                return 1;
            }

            switch (setTeam)
            {
                case Team.Enemy:
                    return _stsMenu.Item(champName).GetValue<Slider>().Value;
                case Team.Ally:
                    return _stsAllyMenu.Item(champName).GetValue<Slider>().Value;
            }
            return 1;
        }

        /// <summary>
        ///     Overrides the current selecterd target with newTarget.
        ///     Override also disables searching for new targets.
        ///     To enable it again use DisableOverrideTarget.
        /// </summary>
        public static void OverrideTarget(Obj_AI_Hero newTarget)
        {
            if (newTarget == null)
            {
                return;
            }

            _focusedTarget = newTarget;
            _enemySearch = false;
        }

        /// <summary>
        ///     Disables override target.
        /// </summary>
        public static void DisableOverrideTarget()
        {
            _enemySearch = true;
        }

        /// <summary>
        ///     Cheacks if the target is invulnerable.
        /// </summary>
        public static bool IsInvulnerable(Obj_AI_Base target)
        {
            if (target.HasBuff("Undying Rage"))
            {
                return true;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return true;
            }

            return false;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint) WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            var temp = Hud.SelectedUnit;

            if (temp != null && temp.Type == GameObjectType.obj_AI_Hero && ((Obj_AI_Hero) temp).IsEnemy &&
                ((Obj_AI_Base) temp).IsValidTarget())
            {
                _focusedTarget = (Obj_AI_Hero) temp;
                return;
            }
            _focusedTarget = null;
        }

        private static void FocusedTargetWithSmite()
        {
            foreach (var enemy in
                AllEnemys.Where(
                    enemy =>
                        enemy.Spellbook.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite") ||
                        enemy.Spellbook.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite")))
            {
                _focusedTargetWithSmite = enemy;
            }
        }

        /// <summary>
        ///     Returns List(Obj_AI_Hero) of enemys with specified Summoner Spell.
        ///     For check it uses Contains so you can use "smite" for "SummonerSmite" and it will return correctly.
        /// </summary>
        public static List<Obj_AI_Hero> GetEnemysWithSummenerSpell(string summonerSpellName)
        {
            return
                AllEnemys.Where(
                    enemy =>
                        enemy.Spellbook.GetSpell(SpellSlot.Summoner1)
                            .Name.ToLower()
                            .Contains(summonerSpellName.ToLower()) ||
                        enemy.Spellbook.GetSpell(SpellSlot.Summoner2)
                            .Name.ToLower()
                            .Contains(summonerSpellName.ToLower())).ToList();
        }

        /// <summary>
        ///     Return enemy turret from current players positon in specified Range and extendMonitarRange.
        /// </summary>
        public static Obj_AI_Turret GetTurret(float range)
        {
            foreach (var turret in
                ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(range) && turret.IsEnemy))
            {
                return turret;
            }
            return null;
        }

        /// <summary>
        ///     Return enemy inhib or nexus from current players positon in specified Range and extendMonitarRange.
        /// </summary>
        public static Obj_Building GetInhibitorsNexus(float range)
        {
            foreach (var inhib in
                ObjectManager.Get<Obj_Barracks>().Where(turret => turret.IsValidTarget(range) && turret.IsEnemy))
            {
                return inhib;
            }

            foreach (
                var nexus in ObjectManager.Get<Obj_HQ>().Where(turret => turret.IsValidTarget(range) && turret.IsEnemy))
            {
                return nexus;
            }

            return null;
        }

        /// <summary>
        ///     Returns target with menu params.
        /// </summary>
        public static Obj_AI_Hero GetTarget(Team selectTeam)
        {
            var mode = CurrentMode(selectTeam);
            return GetTarget(MonitorRange + EMR, mode, CurrentDamagetType, selectTeam);
        }

        /// <summary>
        ///     Returns target for set params and menu params.
        /// </summary>
        public static Obj_AI_Hero GetTarget(float selectRange, Team selectTeam)
        {
            var mode = CurrentMode(selectTeam);
            return GetTarget(selectRange, mode, CurrentDamagetType, selectTeam);
        }

        /// <summary>
        ///     Returns target for set params and menu params.
        /// </summary>
        public static Obj_AI_Hero GetTarget(float selectRange, DamageType selectDamageType, Team selectTeam)
        {
            var mode = CurrentMode(selectTeam);
            return GetTarget(selectRange, mode, selectDamageType, selectTeam);
        }

        /// <summary>
        ///     Returns target for set params.
        /// </summary>
        public static Obj_AI_Hero GetTarget(float selectRange,
            Mode selectMode,
            DamageType selectDamageType,
            Team selectTeam = Team.Enemy)
        {
            if (selectTeam == Team.Enemy && !_enemySearch)
            {
                return _enemyTarget;
            }

            switch (selectTeam)
            {
                case Team.Enemy:
                {
                    if (_focusedTarget != null && _focusedTarget.IsValidTarget(selectRange) &&
                        !SimpleCollision.YasuoWallCollision(selectRange, _focusedTarget))
                    {
                        _enemyTarget = _focusedTarget;
                        return _enemyTarget;
                    }

                    if (FocusTargetWithSmite && _focusedTargetWithSmite != null &&
                        _focusedTargetWithSmite.IsValidTarget(selectRange) &&
                        !SimpleCollision.YasuoWallCollision(selectRange, _focusedTargetWithSmite))
                    {
                        _enemyTarget = _focusedTargetWithSmite;
                        return _enemyTarget;
                    }
                    return GetEnemyTarget(selectRange, selectDamageType, selectMode);
                }
                case Team.Ally:
                {
                    return GetAllyTarget(selectRange, selectDamageType, selectMode);
                }
            }
            return null;
        }

        private static Obj_AI_Hero GetEnemyTarget(float selectRange, DamageType selectDamageType, Mode selectMode)
        {
            Obj_AI_Hero currentTarget = null;
            var currentRatio = 0f;
            var currentPredictedDmg = 0f;
            var tempPredictedDmg = 0f;
            float ratio;

            foreach (var enemy in AllEnemys)
            {
                if (!enemy.IsValidTarget() || IsInvulnerable(enemy) || SimpleCollision.YasuoWallCollision(selectRange, enemy) ||
                    !(Player.Distance(enemy) < selectRange))
                {
                    continue;
                }

                if (currentTarget == null)
                {
                    currentTarget = enemy;

                    switch (selectDamageType)
                    {
                        case DamageType.Magical:
                            currentPredictedDmg =
                                (float) Player.CalcDamage(currentTarget, Damage.DamageType.Magical, 100);
                            break;
                        case DamageType.Physical:
                            currentPredictedDmg =
                                (float) Player.CalcDamage(currentTarget, Damage.DamageType.Physical, 100);
                            break;
                        case DamageType.Hybrid:
                            currentPredictedDmg =
                                (float) Player.CalcDamage(currentTarget, Damage.DamageType.Magical, 50) +
                                (float) Player.CalcDamage(currentTarget, Damage.DamageType.Physical, 50);
                            break;
                        case DamageType.True:
                            currentPredictedDmg = 100;
                            break;
                    }

                    currentRatio = currentPredictedDmg / (1 + enemy.Health) * GetPriorety(enemy.ChampionName);

                    continue;
                }

                switch (selectDamageType)
                {
                    case DamageType.Magical:
                        tempPredictedDmg = (float) Player.CalcDamage(enemy, Damage.DamageType.Magical, 100);
                        break;
                    case DamageType.Physical:
                        tempPredictedDmg = (float) Player.CalcDamage(enemy, Damage.DamageType.Physical, 100);
                        break;
                    case DamageType.Hybrid:
                        tempPredictedDmg = (float) Player.CalcDamage(enemy, Damage.DamageType.Magical, 50) +
                                           (float) Player.CalcDamage(enemy, Damage.DamageType.Physical, 50);
                        break;
                    case DamageType.True:
                        tempPredictedDmg = 100;
                        break;
                }

                switch (selectMode)
                {
                    case Mode.Auto:

                        ratio = tempPredictedDmg / (1 + enemy.Health) * GetPriorety(enemy.ChampionName);

                        if (currentRatio < ratio)
                        {
                            currentRatio = ratio;
                            currentTarget = enemy;
                        }
                        break;

                    case Mode.LowHp:

                        if (enemy.Health < currentTarget.Health)
                        {
                            currentTarget = enemy;
                        }
                        else if (Math.Abs(enemy.Health - currentTarget.Health) < float.Epsilon)
                        {
                            ratio = tempPredictedDmg / (1 + enemy.Health) * GetPriorety(enemy.ChampionName);

                            if (currentRatio < ratio)
                            {
                                currentRatio = ratio;
                                currentTarget = enemy;
                            }
                        }
                        break;

                    case Mode.Priority:

                        if (GetPriorety(currentTarget.ChampionName) < GetPriorety(enemy.ChampionName))
                        {
                            currentTarget = enemy;
                        }
                        else if (GetPriorety(currentTarget.ChampionName) == GetPriorety(enemy.ChampionName))
                        {
                            ratio = tempPredictedDmg / (1 + enemy.Health) * GetPriorety(enemy.ChampionName);

                            if (currentRatio < ratio)
                            {
                                currentRatio = ratio;
                                currentTarget = enemy;
                            }
                        }
                        break;

                    case Mode.MostAp:

                        if (currentTarget.TotalMagicalDamage() < enemy.TotalMagicalDamage())
                        {
                            currentTarget = enemy;
                        }
                        else if (Math.Abs(currentTarget.TotalMagicalDamage() - enemy.TotalMagicalDamage()) < float.Epsilon)
                        {
                            ratio = tempPredictedDmg / (1 + enemy.Health) * GetPriorety(enemy.ChampionName);

                            if (currentRatio < ratio)
                            {
                                currentRatio = ratio;
                                currentTarget = enemy;
                            }
                        }
                        break;

                    case Mode.MostAd:

                        if (currentTarget.TotalAttackDamage() < enemy.TotalAttackDamage())
                        {
                            currentTarget = enemy;
                        }
                        else if (
                            Math.Abs(currentTarget.TotalAttackDamage() - enemy.TotalAttackDamage()) < float.Epsilon)
                        {
                            ratio = tempPredictedDmg / (1 + enemy.Health) * GetPriorety(enemy.ChampionName);

                            if (currentRatio < ratio)
                            {
                                currentRatio = ratio;
                                currentTarget = enemy;
                            }
                        }
                        break;

                    case Mode.Closest:

                        if (Player.Distance(currentTarget) > Player.Distance(enemy))
                        {
                            currentTarget = enemy;
                        }
                        break;

                    case Mode.NearMouse:

                        if (currentTarget.Distance(Game.CursorPos) > enemy.Distance(Game.CursorPos))
                        {
                            currentTarget = enemy;
                        }
                        break;

                    case Mode.Less:

                        if ((enemy.Health - tempPredictedDmg < (currentTarget.Health - currentPredictedDmg)))
                        {
                            currentTarget = enemy;
                            currentPredictedDmg = tempPredictedDmg;
                        }

                        break;
                }
            }
            _enemyTarget = currentTarget;
            return _enemyTarget;
        }

        private static Obj_AI_Hero GetAllyTarget(float selectRange, DamageType selectDamageType, Mode selectMode)
        {
            {
                Obj_AI_Hero currentTarget = null;
                var currentRatio = 0f;
                float ratio;

                foreach (var ally in AllAllys)
                {
                    if (!ally.IsValidTarget() || !(Player.Distance(ally) <= selectRange))
                    {
                        continue;
                    }

                    if (currentTarget == null)
                    {
                        currentTarget = ally;

                        currentRatio = (1 + ally.Health) / GetPriorety(ally.ChampionName);

                        continue;
                    }

                    switch (selectMode)
                    {
                        case Mode.Auto:

                            ratio = (1 + ally.Health) / GetPriorety(ally.ChampionName);

                            if (currentRatio < ratio)
                            {
                                currentRatio = ratio;
                                currentTarget = ally;
                            }
                            break;

                        case Mode.LowHp:

                            if (ally.Health < currentTarget.Health)
                            {
                                currentTarget = ally;
                            }
                            else if (Math.Abs(ally.Health - currentTarget.Health) < float.Epsilon)
                            {
                                ratio = (1 + ally.Health) / GetPriorety(ally.ChampionName);

                                if (currentRatio < ratio)
                                {
                                    currentRatio = ratio;
                                    currentTarget = ally;
                                }
                            }
                            break;

                        case Mode.Priority:

                            if (GetPriorety(currentTarget.ChampionName) < GetPriorety(ally.ChampionName))
                            {
                                currentTarget = ally;
                            }
                            else if (GetPriorety(currentTarget.ChampionName) == GetPriorety(ally.ChampionName))
                            {
                                ratio = (1 + ally.Health) / GetPriorety(ally.ChampionName);

                                if (currentRatio < ratio)
                                {
                                    currentRatio = ratio;
                                    currentTarget = ally;
                                }
                            }
                            break;

                        case Mode.MostAp:

                            if (currentTarget.TotalMagicalDamage() < ally.TotalMagicalDamage())
                            {
                                currentTarget = ally;
                            }
                            else if (Math.Abs(currentTarget.TotalMagicalDamage() - ally.TotalMagicalDamage()) < float.Epsilon)
                            {
                                ratio = (1 + ally.Health) / GetPriorety(ally.ChampionName);

                                if (currentRatio < ratio)
                                {
                                    currentRatio = ratio;
                                    currentTarget = ally;
                                }
                            }
                            break;

                        case Mode.MostAd:

                            if (currentTarget.TotalAttackDamage() < ally.TotalAttackDamage())
                            {
                                currentTarget = ally;
                            }
                            else if (Math.Abs(currentTarget.TotalAttackDamage() - ally.TotalAttackDamage()) < float.Epsilon)
                            {
                                ratio = (1 + ally.Health) / GetPriorety(ally.ChampionName);

                                if (currentRatio < ratio)
                                {
                                    currentRatio = ratio;
                                    currentTarget = ally;
                                }
                            }
                            break;

                        case Mode.Closest:

                            if (Player.Distance(currentTarget) > Player.Distance(ally))
                            {
                                currentTarget = ally;
                            }
                            break;

                        case Mode.NearMouse:

                            if (currentTarget.Distance(Game.CursorPos) > ally.Distance(Game.CursorPos))
                            {
                                currentTarget = ally;
                            }
                            break;
                    }
                }
                _allyTarget = currentTarget;
                return _allyTarget;
            }
        }
    }
}