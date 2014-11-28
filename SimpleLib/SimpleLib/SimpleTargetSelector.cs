using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleLib
{
    namespace SimpleTargetSelector
    {
        class STS
        {
            public static Menu Config;
            public static Obj_AI_Hero Self = ObjectManager.Player;
            public static Obj_AI_Base ForcedTarget = null;
            public static IEnumerable<Obj_AI_Hero> AllEnemys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy);

            public enum Mode
            {
                LowHP,
                Priority,
                MostAP,
                MostAD,
                NearMouse,
                None
            }

            public static string[] APCarry = { "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus", "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna", "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz" };
            public static string[] Support = { "Blitzcrank", "Janna", "Karma", "Leona", "Lulu", "Nami", "Sona", "Soraka", "Thresh", "Zilean" };
            public static string[] Tank = { "Amumu", "Chogath", "DrMundo", "Galio", "Hecarim", "Malphite", "Maokai", "Nasus", "Rammus", "Sejuani", "Shen", "Singed", "Skarner", "Volibear", "Warwick", "Yorick", "Zac", "Nunu", "Taric", "Alistar", "Garen", "Nautilus", "Braum" };
            public static string[] ADCarry = { "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "KogMaw", "MissFortune", "Quinn", "Sivir", "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Zed", "Jinx", "Yasuo", "Lucian", "Kalista" };
            public static string[] Bruiser = { "Darius", "Elise", "Evelynn", "Fiora", "Gangplank", "Gnar", "Jayce", "Pantheon", "Irelia", "JarvanIV", "Jax", "Khazix", "LeeSin", "Nocturne", "Olaf", "Poppy", "Renekton", "Rengar", "Riven", "Shyvana", "Trundle", "Tryndamere", "Udyr", "Vi", "MonkeyKing", "XinZhao", "Aatrox", "Rumble", "Shaco", "MasterYi" };

            private static Mode _currentMode;
            private static float _range;
            private static float _extendMonitarRange;
            private static bool _forceMode = false;
            public static Obj_AI_Hero SelectedTarget;
            private static Obj_AI_Hero _focustTarget = null;
            private static int _lastTick;

            public static void InitializeSTS(Mode mode, float range, float extendMonitarRange = 0)
            {
                _currentMode = mode;
                _range = range;
                _extendMonitarRange = extendMonitarRange;

                Game.OnGameUpdate += OnUpdate;
                Drawing.OnDraw += OnGameDraw;
                Game.OnWndProc += OnProc;
            }

            public static void AddToMenu(Menu ConfigMenu)
            {
                Config = ConfigMenu;

                if (!_forceMode)
                {
                    Config.AddItem(new MenuItem("TSMode", "Target Selector Mode:").SetValue(new StringList(new[] { "Low HP", "Priority", "Most AD", "Most AP", "Near Mouse" })));
                }
                Config.AddItem(new MenuItem("FocusMode", "Focus Selected Target")).SetValue<bool>(true);

                int i = 0;
                foreach (var enemy in AllEnemys)
                {
                    Config.AddItem(new MenuItem("Enemy" + i.ToString(), enemy.BaseSkinName)).SetValue(new Slider(GetAutoPriorety(enemy.BaseSkinName), 5, 0));
                    i++;
                }
                InitializeSTS(GetMode(), Self.AttackRange, 200);
            }

            public static void SetMode(Mode setMode)
            {
                _currentMode = setMode;
                _forceMode = true;
            }

            public static void ForceMode(bool force = false)
            {
                _forceMode = force;
            }

            public static bool GetForceMode()
            {
                return _forceMode;
            }

            public static Mode GetMode()
            {
                switch (Config.Item("TSMode").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        return _currentMode = Mode.LowHP;
                    case 1:
                        return _currentMode = Mode.Priority;
                    case 2:
                        return _currentMode = Mode.MostAD;
                    case 3:
                        return _currentMode = Mode.MostAP;
                    case 4:
                        return _currentMode = Mode.NearMouse;
                    default:
                        return _currentMode = Mode.None;
                }
            }

            private static int GetAutoPriorety(string ChampName)
            {
                if (ADCarry.Contains(ChampName)) return 5;
                if (APCarry.Contains(ChampName)) return 4;
                if (Bruiser.Contains(ChampName)) return 3;
                if (Support.Contains(ChampName)) return 2;
                if (Tank.Contains(ChampName)) return 1;
                return 0;
            }

            public static Obj_AI_Hero CompereHealth(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (target_1.Health < target_2.Health) return target_1;

                if (target_1.Health == target_2.Health)
                {
                    return ComperePriorety(target_1, target_2);
                }
                return target_2;
            }

            public static Obj_AI_Hero ComperePriorety(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (GetAutoPriorety(target_1.BaseSkinName) < GetAutoPriorety(target_2.BaseSkinName)) return target_2;

                if (GetAutoPriorety(target_1.BaseSkinName) == GetAutoPriorety(target_2.BaseSkinName))
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            public static Obj_AI_Hero CompereAP(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (target_1.FlatMagicDamageMod + target_1.BaseAbilityDamage < target_2.FlatMagicDamageMod + target_2.BaseAbilityDamage) return target_2;

                if (target_1.FlatMagicDamageMod + target_1.BaseAbilityDamage == target_2.FlatMagicDamageMod + target_2.BaseAbilityDamage)
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            public static Obj_AI_Hero CompereAD(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (target_1.BaseAttackDamage + target_1.FlatPhysicalDamageMod < target_2.BaseAttackDamage + target_2.FlatPhysicalDamageMod) return target_2;

                if (target_1.BaseAttackDamage + target_1.FlatPhysicalDamageMod == target_2.BaseAttackDamage + target_2.FlatPhysicalDamageMod)
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            public static Obj_AI_Hero CompereNearMouse(Obj_AI_Hero target_1, Obj_AI_Hero target_2)
            {
                if (Geometry.Distance(target_1) > Geometry.Distance(target_2)) return target_2;

                if (Geometry.Distance(target_1) == Geometry.Distance(target_2))
                {
                    return CompereHealth(target_1, target_2);
                }
                return target_1;
            }

            public static Obj_AI_Hero GetLowHPEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;
                    else newTarget = CompereHealth(target, newTarget);
                }
                return newTarget;
            }

            public static Obj_AI_Hero GetPrioretyEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;

                    newTarget = ComperePriorety(target, newTarget);
                }
                return newTarget;
            }

            public static Obj_AI_Hero GetMostAPEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;

                    newTarget = CompereAP(target, newTarget);
                }
                return newTarget;
            }

            public static Obj_AI_Hero GetMostADEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;

                    newTarget = CompereAD(target, newTarget);
                }
                return newTarget;
            }

            public static Obj_AI_Hero GetNearMouseEnemy(float aaRange, float extendMonitarRange = 0)
            {
                Obj_AI_Hero newTarget = null;

                foreach (var target in AllEnemys.Where(target => target.IsValidTarget(aaRange + extendMonitarRange)))
                {
                    if (newTarget == null) newTarget = target;

                    newTarget = CompereNearMouse(target, newTarget);
                }
                return newTarget;
            }

            public static void GetTarget()
            {
                if (_focustTarget != null)
                {
                    SelectedTarget = _focustTarget;
                    return;
                }
                switch (GetMode())
                {
                    case Mode.LowHP:
                        SelectedTarget = GetLowHPEnemy(_range, _extendMonitarRange);
                        break;
                    case Mode.Priority:
                        SelectedTarget = GetPrioretyEnemy(_range, _extendMonitarRange);
                        break;
                    case Mode.MostAP:
                        SelectedTarget = GetMostAPEnemy(_range, _extendMonitarRange);
                        break;
                    case Mode.MostAD:
                        SelectedTarget = GetMostADEnemy(_range, _extendMonitarRange);
                        break;
                    case Mode.NearMouse:
                        SelectedTarget = GetNearMouseEnemy(_range, _extendMonitarRange);
                        break;
                    default:
                        break;
                }
            }

            private static void OnProc(WndEventArgs args)
            {
                if (MenuGUI.IsChatOpen || ObjectManager.Player.Spellbook.SelectedSpellSlot != SpellSlot.Unknown)
                {
                    return;
                }
                if (args.WParam == 1)
                {
                    switch (args.Msg)
                    {
                        case 257:
                            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                            {
                                if (hero.IsValidTarget() &&
                                SharpDX.Vector2.Distance(Game.CursorPos.To2D(), hero.ServerPosition.To2D()) < 10)
                                {
                                    SelectedTarget = hero;
                                    _focustTarget = hero;
                                    Game.PrintChat(SelectedTarget.BaseSkinName);
                                }
                            }
                            break;
                    }
                }
            }

            private static void OnGameDraw(EventArgs args)
            {
                if (!ObjectManager.Player.IsDead && SelectedTarget != null && SelectedTarget.IsVisible && !SelectedTarget.IsDead)
                {
                    Drawing.DrawCircle(SelectedTarget.Position, 125, System.Drawing.Color.Red);
                }
            }

            private static void OnUpdate(EventArgs args)
            {
                //if (_focustTarget != null && _focustTarget.IsValidTarget(_range)) return;

                //_lastTick = Environment.TickCount;

                //GetTarget();
            }
        }
    }
}
