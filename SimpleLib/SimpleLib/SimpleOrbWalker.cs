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
    public class SOW
    {
        private static Menu Config;
        public static IEnumerable<Obj_AI_Hero> AllEnemys = STS.AllEnemys;
        public static IEnumerable<Obj_AI_Hero> AllAllys = STS.AllAllys;

        public enum Mode
        {
            Combo,
            Harass,
            LaneClear,
            Lasthit,
            None,
        }

        public static string[] AttackResets = { "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };
        public static string[] NoAttacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
        public static string[] Attacks = { "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "viktorqbuff", "xenzhaothrust2", "xenzhaothrust3" };

        private static bool _drawing = true;
        private static bool _enabled = true;
        private static bool _attack = true;
        private static bool _movement = true;
        private static bool _disableNextAttack = false;
        private const float LaneClearWaitTimeMod = 2f;
        private static int _lastAATick;
        private static Obj_AI_Base _lastTarget;
        private static int _lastMovement;
        private static int _windup;
        private static int _currentMode = 0;
        private static int _currentAttackMode = 1;
        private static int lastRealAttack;

        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);
        public delegate void OnTargetChangeH(Obj_AI_Base oldTarget, Obj_AI_Base newTarget);
        public delegate void AfterAttackEvenH(Obj_AI_Base unit, Obj_AI_Base target);
        public delegate void OnAttackEvenH(Obj_AI_Base unit, Obj_AI_Base target);

        public static event BeforeAttackEvenH BeforeAttack;
        public static event OnTargetChangeH OnTargetChange;
        public static event AfterAttackEvenH AfterAttack;
        public static event OnAttackEvenH OnAttack;

        public static Menu SOWMenu
        {
            get
            {
                var menu = new Menu("Simple OrbWalker", "SOW");

                var menuDrawing = new Menu("Drawing", "Drawing");
                menuDrawing.AddItem(new MenuItem("Drawing", "Drawing").SetValue<bool>(_drawing));
                menuDrawing.AddItem(new MenuItem("DrawAARange", "AA Circle").SetValue(new Circle(true, Color.FloralWhite)));
                menuDrawing.AddItem(new MenuItem("DrawEnemyAARange", "AA Circle Enemy").SetValue(new Circle(true, Color.Pink)));
                menuDrawing.AddItem(new MenuItem("DrawHoldzone", "HoldZone").SetValue(new Circle(true, Color.FloralWhite)));
                menuDrawing.AddItem(new MenuItem("DrawLasthit", "Minion LastHit").SetValue(new Circle(true, Color.Lime)));
                menuDrawing.AddItem(new MenuItem("DrawnearKill", "Minion NearKill").SetValue(new Circle(true, Color.Gold)));
                menu.AddSubMenu(menuDrawing);

                menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue<bool>(_enabled));
                menu.AddItem(new MenuItem("FarmDelay", "Farm Delay").SetValue(new Slider(0, 0, 200)));
                menu.AddItem(new MenuItem("Holdzone", "Hold Position").SetValue(new Slider(50, 500, 0)));
                menu.AddItem(new MenuItem("AttackMode", "Attack Mode").SetValue(new StringList(new[] { "Only Farming", "Farming + Carry mode" })));
                menu.AddItem(new MenuItem("Mode", "Orbwalking Mode").SetValue(new StringList(new[] { "To mouse", "To target" })));
                menu.AddItem(new MenuItem("Hotkeys", "Hotkeys"));
                menu.AddItem(new MenuItem("Harass", "Harass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                menu.AddItem(new MenuItem("LaneClear", "LaneClear").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                menu.AddItem(new MenuItem("Lasthit", "Lasthit").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                menu.AddItem(new MenuItem("Combo", "Carry me!").SetValue(new KeyBind(" ".ToCharArray()[0], KeyBindType.Press)));

                Config = menu;
                return menu;
            }
        }

        public static void InitializeOrbwalker()
        {
            Drawing.OnDraw += OnDraw;
            Game.OnGameUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            GameObject.OnCreate += Obj_SpellMissile_OnCreate;
            Game.OnGameProcessPacket += OnProcessPacket;
            CheckAutoWindUp();
        }

        public static void EnableOrbWalker()
        {
            _enabled = true;
        }

        public static void DisableOrbwalker()
        {
            _enabled = false;
        }

        public static void DisableAttacks()
        {
            _attack = false;
        }

        public static void EnableAttacks()
        {
            _attack = true;
        }

        public static int GetCurrentOrbWalkMode()
        {
            return _currentMode;
        }

        public static int GetCurrentAutoAttackMode()
        {
            return _currentAttackMode;
        }

        public static void SetCurrentOrbWalkMode(int CurrentMode)
        {
            _currentMode = CurrentMode;
        }

        public static void SetCurrentAutoAttackMode(int AAMode)
        {
            _currentAttackMode = AAMode;
        }

        public static bool GetDrawing()
        {
            return _drawing;
        }

        public static void SetDrawing(bool OnOff)
        {
            _drawing = OnOff;
        }

        public static string GetSystemTime()
        {
            return DateTime.Now.ToString();
        }

        public static float GetGameTime()
        {
            return Game.Time;
        }

        public static float AutoAttackRange()
        {
            return SL.Self.AttackRange;
        }

        public static float AutoAttackRange(Obj_AI_Base target)
        {
            return SL.Self.AttackRange + target.BoundingRadius;
        }

        public static bool InRange(Obj_AI_Base target)
        {
            if (target != null) return target.IsValidTarget(AutoAttackRange());
            else return false;
        }

        public static bool InExtendedRange(Obj_AI_Base target, float extendRange = 0)
        {
            if (target != null) return target.IsValidTarget(AutoAttackRange() + extendRange);
            else return false;
        }

        public static bool ValidTarget(Obj_AI_Base target)
        {
            if (target != null && InRange(target)) return true;
            else return false;
        }

        public float GetHealthPercent(Obj_AI_Base unit = null)
        {
            if (unit == null)
                unit = SL.Self;
            return (unit.Health / unit.MaxHealth) * 100f;
        }

        public float GetManaPercent(Obj_AI_Hero unit = null)
        {
            if (unit == null)
                unit = SL.Self;
            return (unit.Mana / unit.MaxMana) * 100f;
        }

        public static void Attack(Obj_AI_Base target)
        {
            if (OnAttack != null && target != null) OnAttack(SL.Self, target);
        }

        public static float AutoAttackMissileSpeed()
        {
            return SL.Self.IsMelee() ? float.MaxValue : SL.Self.BasicAttack.MissileSpeed;
        }

        public static float AutoAttackCastTime()
        {
            return SL.Self.BasicAttack.SpellCastTime;
        }

        public static double CountKillhits(Obj_AI_Base enemy)
        {
            return enemy.Health / SL.Self.GetAutoAttackDamage(enemy);
        }

        public static float WindUpTime()
        {
            return _windup;
        }

        public static float AnimationTime()
        {
            return (1 / AutoAttackMissileSpeed() * SL.Self.BasicAttack.SpellCastTime);
        }

        public static float Latency()
        {
            return Game.Ping;
        }

        public static bool CanAttack()
        {
            if (_lastAATick <= Environment.TickCount)
                return Environment.TickCount + Game.Ping / 2 + 25 >= _lastAATick + SL.Self.AttackDelay * 1000 && _attack && IsAllowedToAttack();
            return false;
        }

        public static bool CanMove()
        {
            if (_lastAATick <= Environment.TickCount)
                return Environment.TickCount + Game.Ping / 2 >= _lastAATick + SL.Self.AttackCastDelay * 1000 + _windup && _movement && IsAllowedToMove();
            return false;
        }

        public static int FarmDelay(int offset = 0)
        {
            return Config.Item("FarmDelay").GetValue<Slider>().Value + offset;
        }

        public static void CheckAutoWindUp()
        {
            var additional = 0;
            if (Game.Ping >= 100)
                additional = Game.Ping / 100 * 5;
            else if (Game.Ping > 40 && Game.Ping < 100)
                additional = Game.Ping / 100 * 10;
            else if (Game.Ping <= 40)
                additional = +20;
            var windUp = Game.Ping + additional;
            if (windUp < 40) windUp = 40;

            _windup = windUp;
        }

        private static void OnProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != 0x34 || new GamePacket(args).ReadInteger(1) != ObjectManager.Player.NetworkId || (args.PacketData[5] != 0x11 && args.PacketData[5] != 0x91))
                return;

            ResetAutoAttackTimer();
        }

        private static void FireBeforeAttack(Obj_AI_Base target)
        {
            if (BeforeAttack != null)
            {
                BeforeAttack(new BeforeAttackEventArgs { Target = target });
            }
            else
            {
                _disableNextAttack = false;
            }
        }

        private static void FireOnTargetSwitch(Obj_AI_Base newTarget)
        {
            if (OnTargetChange != null && (_lastTarget == null || _lastTarget.NetworkId != newTarget.NetworkId))
            {
                OnTargetChange(_lastTarget, newTarget);
            }
        }

        private static void FireAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (AfterAttack != null)
            {
                AfterAttack(unit, target);
            }
        }

        private static void FireOnAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (OnAttack != null)
            {
                OnAttack(unit, target);
            }
        }

        public static Mode CurrentMode
        {
            get
            {
                if (Config.Item("Combo").GetValue<KeyBind>().Active)
                    return Mode.Combo;
                if (Config.Item("Harass").GetValue<KeyBind>().Active)
                    return Mode.Harass;
                if (Config.Item("LaneClear").GetValue<KeyBind>().Active)
                    return Mode.LaneClear;
                if (Config.Item("Lasthit").GetValue<KeyBind>().Active)
                    return Mode.Lasthit;
                return Mode.None;
            }
        }

        public static bool IsAllowedToAttack()
        {
            if (!_attack)
                return false;
            if (CurrentMode == Mode.Combo)
                return true;
            if (CurrentMode == Mode.Harass)
                return true;
            if (CurrentMode == Mode.LaneClear)
                return true;
            if (CurrentMode == Mode.Lasthit)
                return true;
            else return false;
        }

        public static bool IsAllowedToMove()
        {
            if (!_movement)
                return false;
            if (CurrentMode == Mode.Combo)
                return true;
            if (CurrentMode == Mode.Harass)
                return true;
            if (CurrentMode == Mode.LaneClear)
                return true;
            if (CurrentMode == Mode.Lasthit)
                return true;
            else return false;
        }

        private static bool IsAutoAttack(string name)
        {
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) ||
            Attacks.Contains(name.ToLower());
        }

        public static void ResetAutoAttackTimer()
        {
            _lastAATick = 0;
        }

        public static bool IsAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        public class BeforeAttackEventArgs
        {
            public Obj_AI_Base Target;
            public Obj_AI_Base Unit = ObjectManager.Player;
            private bool _process = true;
            public bool Process
            {
                get
                {
                    return _process;
                }
                set
                {
                    _disableNextAttack = !value;
                    _process = value;
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (MenuGUI.IsChatOpen || !Config.Item("Enabled").GetValue<bool>() || SL.Self.IsChannelingImportantSpell()) return;

            CheckAutoWindUp();

            Obj_AI_Base target = GetPossibleTarget();
            Orbwalk(target, Game.CursorPos);
        }

        private static void OnDraw(EventArgs args)
        {
            _drawing = Config.Item("Drawing").GetValue<bool>();
            if (!_drawing) return;

            if (Config.Item("DrawAARange").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(SL.Self.Position, AutoAttackRange(), Config.Item("DrawAARange").GetValue<Circle>().Color);
            }

            if (Config.Item("DrawEnemyAARange").GetValue<Circle>().Active)
            {
                foreach (var enemy in AllEnemys.Where(enemy => enemy.IsValidTarget(1500)))
                    Utility.DrawCircle(enemy.Position, AutoAttackRange(enemy), Config.Item("DrawEnemyAARange").GetValue<Circle>().Color);
            }

            if (Config.Item("DrawHoldzone").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(SL.Self.Position, Config.Item("Holdzone").GetValue<Slider>().Value, Config.Item("DrawHoldzone").GetValue<Circle>().Color);
            }

            if (Config.Item("DrawLasthit").GetValue<Circle>().Active || Config.Item("DrawnearKill").GetValue<Circle>().Active)
            {
                var minionList = MinionManager.GetMinions(SL.Self.Position, AutoAttackRange() + 500, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                foreach (var minion in minionList.Where(minion => minion.IsValidTarget(AutoAttackRange() + 500)))
                {
                    var attackToKill = Math.Ceiling(minion.MaxHealth / SL.Self.GetAutoAttackDamage(minion, true));
                    var hpBarPosition = minion.HPBarPosition;
                    var barWidth = minion.IsMelee() ? 75 : 80;
                    if (minion.HasBuff("turretshield", true))
                        barWidth = 70;
                    var barDistance = (float)(barWidth / attackToKill);

                    if (Config.Item("DrawLasthit").GetValue<Circle>().Active &&
                        minion.Health <= SL.Self.GetAutoAttackDamage(minion, true))
                        Utility.DrawCircle(minion.Position, minion.BoundingRadius, Config.Item("DrawLasthit").GetValue<Circle>().Color);
                    else if (Config.Item("DrawnearKill").GetValue<Circle>().Active &&
                             minion.Health <= SL.Self.GetAutoAttackDamage(minion, true) * 2)
                        Utility.DrawCircle(minion.Position, minion.BoundingRadius, Config.Item("DrawnearKill").GetValue<Circle>().Color);
                }
            }
        }

        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (IsAutoAttackReset(spell.SData.Name) && unit.IsMe)
                Utility.DelayAction.Add(100, ResetAutoAttackTimer);

            if (!IsAutoAttack(spell.SData.Name))
                return;
            if (unit.IsMe)
            {
                _lastAATick = Environment.TickCount - Game.Ping / 2;
                if (spell.Target is Obj_AI_Base)
                {
                    FireOnTargetSwitch((Obj_AI_Base)spell.Target);
                    _lastTarget = (Obj_AI_Base)spell.Target;
                }
                if (unit.IsMelee())
                    Utility.DelayAction.Add(
                        (int)(unit.AttackCastDelay * 1000 + Game.Ping * 0.5) + 50, () => FireAfterAttack(unit, _lastTarget));

                FireOnAttack(unit, _lastTarget);
            }
            else
            {
                FireOnAttack(unit, (Obj_AI_Base)spell.Target);
            }
        }

        private static void Obj_SpellMissile_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsMe)
            {
                var obj = (Obj_AI_Hero)sender;
                if (obj.IsMelee())
                    return;
            }
            if (!(sender is Obj_SpellMissile) || !sender.IsValid)
                return;
            var missile = (Obj_SpellMissile)sender;
            if (missile.SpellCaster is Obj_AI_Hero && missile.SpellCaster.IsValid && IsAutoAttack(missile.SData.Name))
            {
                FireAfterAttack(missile.SpellCaster, _lastTarget);
                if (sender.IsMe)
                    lastRealAttack = Environment.TickCount;
            }
        }

        public static Obj_AI_Base GetPossibleTarget()
        {
            Obj_AI_Base tempTarget = null;
            _currentAttackMode = Config.Item("AttackMode").GetValue<StringList>().SelectedIndex;

            switch (CurrentMode)
            {
                case Mode.Combo:

                    if (_currentAttackMode == 1)
                    {
                        tempTarget = STS.SelectedEnemyTarget;
                    }
                    if (tempTarget != null) return tempTarget;
                    else
                    {
                        tempTarget = STS.GetTurret(AutoAttackRange(), 100f);
                        if (tempTarget != null) return tempTarget;
                        else
                        {
                            if (STS.GetInhibitorsNexus(AutoAttackRange(), 100f) != null)
                            {
                                SL.Self.IssueOrder(GameObjectOrder.AttackUnit, STS.GetInhibitorsNexus(AutoAttackRange(), 100f));
                            }
                            return null;
                        }
                    }

                case Mode.Harass:

                    SMM.Target(AutoAttackRange(), SMM.MinionMode.LaneFreez, 100f);
                    tempTarget = SMM.SelectedMinion;

                    if (tempTarget != null) return tempTarget;
                    else
                    {
                        if (_currentAttackMode == 1)
                        {
                            tempTarget = STS.SelectedEnemyTarget;
                        }
                        if (tempTarget != null) return tempTarget;
                        else
                        {
                            tempTarget = STS.GetTurret(AutoAttackRange(), 100f);
                            if (tempTarget != null) return tempTarget;
                            else
                            {
                                if (STS.GetInhibitorsNexus(AutoAttackRange(), 100f) != null)
                                {
                                    SL.Self.IssueOrder(GameObjectOrder.AttackUnit, STS.GetInhibitorsNexus(AutoAttackRange(), 100f));
                                }
                                return null;
                            }
                        }
                    }

                case Mode.LaneClear:

                    SMM.Target(AutoAttackRange(), SMM.MinionMode.LaneClear, 100f);
                    tempTarget = SMM.SelectedMinion;

                    if (tempTarget != null) return tempTarget;
                    else
                    {
                        SMM.Target(AutoAttackRange(), SMM.MinionMode.LaneClear, 100f, SMM.MinionTeam.Neutral);
                        tempTarget = SMM.SelectedMinion;
                    }

                    if (tempTarget != null) return tempTarget;
                    else
                    {
                        tempTarget = STS.GetTurret(AutoAttackRange(), 100f);
                        if (tempTarget != null) return tempTarget;
                        else
                        {
                            if (STS.GetInhibitorsNexus(AutoAttackRange(), 100f) != null)
                            {
                                SL.Self.IssueOrder(GameObjectOrder.AttackUnit, STS.GetInhibitorsNexus(AutoAttackRange(), 100f));
                            }
                            return null;
                        }
                    }

                case Mode.Lasthit:

                    SMM.Target(AutoAttackRange(), SMM.MinionMode.LastHit, 100f);
                    tempTarget = SMM.SelectedMinion;

                    if (tempTarget != null) return tempTarget;
                    else
                    {
                        tempTarget = STS.GetTurret(AutoAttackRange(), 100f);
                        if (tempTarget != null) return tempTarget;
                        else
                        {
                            if (STS.GetInhibitorsNexus(AutoAttackRange(), 100f) != null)
                            {
                                SL.Self.IssueOrder(GameObjectOrder.AttackUnit, STS.GetInhibitorsNexus(AutoAttackRange(), 100f));
                            }
                            return null;
                        }
                    }

                default:
                    return null;
            }
        }

        public static void MoveTo(Vector3 position)
        {
            if (Environment.TickCount - _lastMovement <= 150)
                return;

            _lastMovement = Environment.TickCount;

            var holdAreaRadius = Config.Item("Holdzone").GetValue<Slider>().Value;

            if (SL.Self.Distance(position) < holdAreaRadius)
            {
                SL.Self.IssueOrder(GameObjectOrder.HoldPosition, SL.Self.Position);
                return;
            }

            SL.Self.IssueOrder(GameObjectOrder.MoveTo, position);
        }

        private static void PositonToTarget(Obj_AI_Base target)
        {
            if (!InExtendedRange(target, 200f)) MoveTo(Game.CursorPos);

            float point = (AutoAttackRange() / 100) * 80;

            if (SL.Self.ServerPosition.Distance(target.ServerPosition) <= point) MoveTo(Game.CursorPos);
            else SL.Self.IssueOrder(GameObjectOrder.MoveTo, target.ServerPosition);
        }

        public static void Orbwalk(Obj_AI_Base target, Vector3 positon)
        {
            if (target != null && CanAttack())
            {
                if (!_disableNextAttack)
                {
                    if (CurrentMode != Mode.None && target.IsValidTarget(AutoAttackRange(target)))
                    {
                        FireBeforeAttack(target);
                        SL.Self.IssueOrder(GameObjectOrder.AttackUnit, target);
                        _lastAATick = Environment.TickCount + Game.Ping / 2;
                        FireAfterAttack(SL.Self, target);
                    }
                }
            }
            if (!CanMove()) return;
            if (_currentMode == 1) PositonToTarget(target);
            else MoveTo(positon);
        }
    }
}