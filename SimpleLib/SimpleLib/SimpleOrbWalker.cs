using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleLib
{
    public class SOW
    {
        public delegate void AfterAttackEvenH(AttackableUnit unit, AttackableUnit target);

        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);

        public delegate void OnAttackEvenH(AttackableUnit unit, AttackableUnit target);

        public delegate void OnDrawH(EventArgs args);

        public delegate void OnProcessSpellCastH(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args);

        public delegate void OnTargetChangeH(AttackableUnit oldTarget, AttackableUnit newTarget);

        public delegate void OnUpdateH(Mode currentMode, EventArgs args);

        public delegate void TurretAttackH(Obj_AI_Turret turret, Obj_AI_Base target);

        public enum Mode
        {
            Combo,
            Mixed,
            Lasthit,
            LaneClear,
            LaneFreeze,
            Flee,
            None
        }

        private static readonly string[] AttackResets =
        {
            "dariusnoxiantacticsonh", "fioraflurry", "garenq",
            "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "luciane", "lucianq",
            "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade",
            "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack",
            "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq",
            "xenzhaocombotarget", "yorickspectral"
        };

        private static readonly string[] NoAttacks =
        {
            "jarvanivcataclysmattack", "monkeykingdoubleattack",
            "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2",
            "zyragraspingplantattackfire", "zyragraspingplantattack2fire"
        };

        private static readonly string[] Attacks =
        {
            "caitlynheadshotmissile", "frostarrow", "garenslash2",
            "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute",
            "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "xenzhaothrust2",
            "xenzhaothrust3"
        };

        private static Menu _config;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        private static bool _orbwalk = true;
        private static bool _drawings = true;
        private static bool _attack = true;
        private static bool _move = true;
        private static bool _nextAutoAttack = true;
        private static bool _playerMelee;
        private static int _lastAutoAttackTime = Environment.TickCount;
        private static int _lastMoveTime = Environment.TickCount;
        private static int _extraWindUp = 70;
        private static int _farmDelay = 70;
        private static int _holdZone = 50;
        private static Vector3 _lastMovePosition = Vector3.Zero;

        public static bool OrbWalk
        {
            get { return _orbwalk; }
            set { _orbwalk = value; }
        }

        public static bool Drawings
        {
            get { return _config.Item("Drawings").GetValue<bool>(); }
            set
            {
                _drawings = value;
                _config.Item("Drawings").SetValue(value);
            }
        }

        public static bool Attack
        {
            get { return _attack; }
            set { _attack = value; }
        }

        public static bool Move
        {
            get { return _move; }
            set { _move = value; }
        }

        public static bool NextAutoAttack
        {
            get { return _nextAutoAttack; }
            set { _nextAutoAttack = value; }
        }

        public static int LastAutoAttackTickCount
        {
            get { return _lastAutoAttackTime; }
        }

        public static int LastMoveTickCount
        {
            get { return _lastMoveTime; }
        }

        public static int ExtraWindUp
        {
            get
            {
                var additional = _config.Item("ExtraWindup").GetValue<Slider>().Value;
                if (Game.Ping >= 100)
                {
                    additional = Game.Ping / 100 * 5;
                }
                else if (Game.Ping > 40 && Game.Ping < 100)
                {
                    additional = Game.Ping / 100 * 10;
                }
                else if (Game.Ping <= 40)
                {
                    additional += 20;
                }
                var windUp = Game.Ping + additional;
                if (windUp < 40)
                {
                    windUp = 40;
                }

                _extraWindUp = windUp;
                return _extraWindUp;
            }
            set
            {
                _config.Item("ExtraWindup").SetValue(new Slider(value, 0, 200));
                _extraWindUp = value;
            }
        }

        public static int FarmDelay
        {
            get
            {
                _farmDelay = _config.Item("FarmDelay").GetValue<Slider>().Value;
                return _farmDelay;
            }
            set
            {
                _config.Item("FarmDelay").SetValue(new Slider(value, 0, 200));
                _farmDelay = value;
            }
        }

        public static int HoldAreaRadius
        {
            get
            {
                _holdZone = _config.Item("Holdzone").GetValue<Slider>().Value;
                return _holdZone;
            }
            set
            {
                _config.Item("Holdzone").SetValue(new Slider(value, 0, 500));
                _holdZone = value;
            }
        }

        public static AttackableUnit LastTarget { get; set; }

        public static Vector3 LastMovePosition
        {
            get { return _lastMovePosition; }
            set { _lastMovePosition = value; }
        }

        public static Mode CurrentMode
        {
            get
            {
                if (_config.Item("Combo").GetValue<KeyBind>().Active)
                {
                    return Mode.Combo;
                }

                if (_config.Item("Mixed").GetValue<KeyBind>().Active)
                {
                    return Mode.Mixed;
                }

                if (_config.Item("LaneClear").GetValue<KeyBind>().Active)
                {
                    return Mode.LaneClear;
                }

                if (_config.Item("Lasthit").GetValue<KeyBind>().Active)
                {
                    return Mode.Lasthit;
                }

                if (_config.Item("LaneFreeze").GetValue<KeyBind>().Active)
                {
                    return Mode.LaneFreeze;
                }

                return _config.Item("Flee").GetValue<KeyBind>().Active ? Mode.Flee : Mode.None;
            }
        }

        public static event OnDrawH OnDraw;
        public static event OnUpdateH OnUpdate;
        public static event OnProcessSpellCastH OnProcessSpellCast;
        public static event BeforeAttackEvenH BeforeAttack;
        public static event OnAttackEvenH OnAttack;
        public static event AfterAttackEvenH AfterAttack;
        public static event OnTargetChangeH TargetChange;
        public static event TurretAttackH OnPlayerTowerAggro;
        public static event TurretAttackH OnAllyTowerAggro;
        public static event TurretAttackH OnEnemyTowerAggro;
        public static event TurretAttackH OnMinionTowerAggro;

        private static void InitSOW()
        {
            _playerMelee = IsMelee(Player);

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate += Obj_SpellMissile_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Turret_OnProcessSpellCast;
        }

        /// <summary>
        ///     Returns the Menu for SOW
        /// </summary>
        public static void SowMenu(Menu menu)
        {
            _config = menu;

            var stsMenu = new Menu("SimpleOrbWalker", "SOW");

            var menuDrawing = new Menu("Drawing", "DrwMenu");
            menuDrawing.AddItem(new MenuItem("Drawings", "Drawings").SetValue(_drawings));
            menuDrawing.AddItem(new MenuItem("DrawAARange", "AA Circle").SetValue(new Circle(true, Color.Aqua)));
            menuDrawing.AddItem(
                new MenuItem("DrawEnemyAARange", "AA Circle Enemy").SetValue(new Circle(true, Color.Chocolate)));
            menuDrawing.AddItem(new MenuItem("DrawHoldzone", "HoldZone").SetValue(new Circle(true, Color.Gray)));
            menuDrawing.AddItem(new MenuItem("DrawLasthit", "Minion LastHit").SetValue(new Circle(true, Color.Lime)));
            menuDrawing.AddItem(new MenuItem("DrawnearKill", "Minion NearKill").SetValue(new Circle(true, Color.Gold)));
            stsMenu.AddSubMenu(menuDrawing);

            var miscMenu = new Menu("Misc", "MiscMenu");
            miscMenu.AddItem(
                new MenuItem("ExtraWindup", "Extra Windup Time").SetValue(new Slider(_extraWindUp, 0, 200)));
            miscMenu.AddItem(new MenuItem("FarmDelay", "Farm Delay").SetValue(new Slider(_farmDelay, 0, 200)));
            miscMenu.AddItem(new MenuItem("Holdzone", "Hold Position").SetValue(new Slider(_holdZone, 500, 0)));
            stsMenu.AddSubMenu(miscMenu);

            stsMenu.AddItem(new MenuItem("Hotkeys", "Hotkeys"));
            stsMenu.AddItem(
                new MenuItem("Mixed", "Mixed").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            stsMenu.AddItem(
                new MenuItem("LaneClear", "LaneClear").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            stsMenu.AddItem(
                new MenuItem("Lasthit", "Lasthit").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            stsMenu.AddItem(
                new MenuItem("LaneFreeze", "Lane Freeze").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            stsMenu.AddItem(new MenuItem("Flee", "Flee").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            stsMenu.AddItem(
                new MenuItem("Combo", "Carry me!").SetValue(new KeyBind(" ".ToCharArray()[0], KeyBindType.Press)));

            _config.AddSubMenu(stsMenu);

            InitSOW();
        }

        public static bool IsAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        public static bool IsAutoAttack(string name)
        {
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) ||
                   Attacks.Contains(name.ToLower());
        }

        private static void ResetAutoAttackTimer()
        {
            _lastAutoAttackTime = 0;
        }

        public static bool IsMelee(Obj_AI_Base unit)
        {
            return unit.CombatType == GameObjectCombatType.Melee;
        }

        public static float AutoAttackRange(AttackableUnit target)
        {
            var result = Player.AttackRange + Player.BoundingRadius;
            if (target.IsValidTarget())
            {
                return result + target.BoundingRadius;
            }
            return result;
        }

        public static float AutoAttackRange()
        {
            return Player.AttackRange + Player.BoundingRadius;
        }

        public static bool InAutoAttackRange(AttackableUnit target)
        {
            if (!target.IsValidTarget())
            {
                return false;
            }
            var myRange = AutoAttackRange(target);

            return Player.Distance((Obj_AI_Base) target) <= myRange;
        }

        public static float BasicAttackMissileSpeed()
        {
            return IsMelee(Player) ? float.MaxValue : Player.BasicAttack.MissileSpeed;
        }

        public static bool CanAttack()
        {
            if (LastAutoAttackTickCount <= Environment.TickCount)
            {
                return Environment.TickCount + Game.Ping / 2 + 25 >=
                       LastAutoAttackTickCount + Player.AttackDelay * 1000 && Attack;
            }
            return false;
        }

        public static bool CanMove(float extraWindup)
        {
            if (LastAutoAttackTickCount <= Environment.TickCount)
            {
                return (Environment.TickCount + Game.Ping / 2 >=
                        LastAutoAttackTickCount + Player.AttackCastDelay * 1000 + extraWindup) && Move;
            }
            return false;
        }

        public static bool IsAllowedToMove()
        {
            if (!Move)
            {
                return false;
            }
            switch (CurrentMode)
            {
                case Mode.Combo:
                    return true;
                case Mode.Mixed:
                    return true;
                case Mode.Lasthit:
                    return true;
                case Mode.LaneClear:
                    return true;
                case Mode.LaneFreeze:
                    return true;
                case Mode.Flee:
                    return true;
            }
            return false;
        }

        public static bool IsAllowedToAttack()
        {
            if (!Attack)
            {
                return false;
            }
            switch (CurrentMode)
            {
                case Mode.Combo:
                    return true;
                case Mode.Mixed:
                    return true;
                case Mode.Lasthit:
                    return true;
                case Mode.LaneClear:
                    return true;
                case Mode.LaneFreeze:
                    return true;
            }

            return false;
        }

        private static void FireBeforeAttack(AttackableUnit target)
        {
            if (BeforeAttack != null)
            {
                BeforeAttack(new BeforeAttackEventArgs { Target = target });
            }
            else
            {
                NextAutoAttack = true;
            }
        }

        private static void FireOnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (OnAttack != null)
            {
                OnAttack(unit, target);
            }
        }

        private static void FireAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (AfterAttack != null)
            {
                AfterAttack(unit, target);
            }
        }

        private static void FireOnTargetSwitch(AttackableUnit newTarget)
        {
            if (TargetChange != null && (!LastTarget.IsValidTarget() || LastTarget != newTarget))
            {
                TargetChange(LastTarget, newTarget);
            }
        }

        private static void MoveTo(Vector3 position, bool overrideTimer = false)
        {
            if (Environment.TickCount - LastMoveTickCount < ExtraWindUp && !overrideTimer)
            {
                return;
            }

            _lastMoveTime = Environment.TickCount;

            if (Player.ServerPosition.Distance(position) < HoldAreaRadius)
            {
                Player.IssueOrder(GameObjectOrder.HoldPosition, Player.ServerPosition);
                LastMovePosition = Player.ServerPosition;
                return;
            }

            Player.IssueOrder(GameObjectOrder.MoveTo, position);
            LastMovePosition = position;
        }

        public static void Orbwalk(AttackableUnit targetUnit, Vector3 moveTo, bool overrideTimer = false)
        {
            if (targetUnit != null && CanAttack() && IsAllowedToAttack())
            {
                NextAutoAttack = true;

                FireBeforeAttack(targetUnit);

                if (NextAutoAttack && InAutoAttackRange(targetUnit))
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, targetUnit);

                    if (LastTarget != null && LastTarget.IsValid && LastTarget != targetUnit)
                    {
                        _lastAutoAttackTime = Environment.TickCount + Game.Ping / 2;
                    }
                }
            }

            if (CanMove(ExtraWindUp) && IsAllowedToMove())
            {
                var target = targetUnit as Obj_AI_Hero;

                if (target != null && _playerMelee && InAutoAttackRange(target) &&
                    Game.CursorPos.Distance(target.Position) < 300)
                {
                    var position = SimplePrediction.MeleeMovmentPrediction(target);
                    MoveTo(position, overrideTimer);
                }
            }
        }

        public static AttackableUnit GetTarget()
        {
            AttackableUnit temp;

            switch (CurrentMode)
            {
                case Mode.Combo:

                    temp = STS.GetTarget(STS.Team.Enemy);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetTurret(AutoAttackRange());

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetInhibitorsNexus(AutoAttackRange());
                    return temp;

                case Mode.Mixed:

                    temp = SMM.GetMinion(Player.AttackRange, SMM.MinionMode.LastHit, true, FarmDelay);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetTarget(STS.Team.Enemy);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetTurret(Player.AttackRange);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetInhibitorsNexus(Player.AttackRange);

                    return temp;

                case Mode.Lasthit:

                    temp = SMM.GetMinion(Player.AttackRange, SMM.MinionMode.LastHit, true, FarmDelay);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetTurret(Player.AttackRange);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetInhibitorsNexus(Player.AttackRange);

                    return temp;

                case Mode.LaneClear:

                    temp = SMM.GetMinion(Player.AttackRange, SMM.MinionMode.LastHit, true, FarmDelay);

                    if (temp != null)
                    {
                        return temp;
                    }

                    if (!SMM.ShouldWait(AutoAttackRange(), FarmDelay))
                    {
                        temp = SMM.GetMinion(Player.AttackRange, SMM.MinionMode.LaneClear, true, FarmDelay);

                        if (temp != null)
                        {
                            return temp;
                        }
                    }

                    temp = STS.GetTurret(Player.AttackRange);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetInhibitorsNexus(Player.AttackRange);

                    return temp;

                case Mode.LaneFreeze:
                    temp = SMM.GetMinion(Player.AttackRange, SMM.MinionMode.LaneFreez, true, FarmDelay);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetTurret(Player.AttackRange);

                    if (temp != null)
                    {
                        return temp;
                    }

                    temp = STS.GetInhibitorsNexus(Player.AttackRange);

                    return temp;
            }
            return null;
        }

        private static void Obj_SpellMissile_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsValid<Obj_SpellMissile>() && sender.IsMe)
            {
                var missile = (Obj_SpellMissile) sender;
                if (missile.SpellCaster.IsValid<Obj_AI_Hero>() && IsAutoAttack(missile.SData.Name))
                {
                    FireAfterAttack(missile.SpellCaster, LastTarget);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            try
            {
                var spellName = spell.SData.Name;
                if (IsAutoAttackReset(spellName) && unit.IsMe)
                {
                    Utility.DelayAction.Add(250, ResetAutoAttackTimer);
                }

                if (unit.IsMe && spell.Target is Obj_AI_Base)
                {
                    _lastAutoAttackTime = Environment.TickCount - Game.Ping / 2;
                    var target = (Obj_AI_Base) spell.Target;
                    if (target.IsValid)
                    {
                        FireOnTargetSwitch(target);
                        LastTarget = target;
                    }
                    if (unit.IsMelee())
                    {
                        Utility.DelayAction.Add(
                            (int) (unit.AttackCastDelay * 1000 + 40), () => FireAfterAttack(unit, LastTarget));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (OnProcessSpellCast != null)
            {
                OnProcessSpellCast(unit, spell);
            }
        }

        private static void Obj_AI_Turret_OnProcessSpellCast(Obj_AI_Base sender,
            GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValidTarget(1000, false, Player.Position))
            {
                return;
            }

            if (sender.IsAlly)
            {
                if (args.Target is Obj_AI_Hero)
                {
                    if (OnEnemyTowerAggro != null)
                    {
                        OnEnemyTowerAggro((Obj_AI_Turret) sender, (Obj_AI_Base) args.Target);
                    }
                }

                if (args.Target is Obj_AI_Minion)
                {
                    if (OnMinionTowerAggro != null)
                    {
                        OnMinionTowerAggro((Obj_AI_Turret) sender, (Obj_AI_Base) args.Target);
                    }
                }
            }

            if (sender.IsEnemy)
            {
                if (args.Target.IsMe)
                {
                    if (OnPlayerTowerAggro != null)
                    {
                        OnPlayerTowerAggro((Obj_AI_Turret) sender, Player);
                    }
                }

                if (args.Target is Obj_AI_Hero)
                {
                    //Da odradim prediction i proveru za aa pa onda da se prosledi signal
                    if (OnAllyTowerAggro != null)
                    {
                        OnAllyTowerAggro((Obj_AI_Turret) sender, (Obj_AI_Base) args.Target);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Drawings)
            {
                return;
            }

            if (_config.Item("DrawAARange").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(
                    Player.Position, AutoAttackRange(), _config.Item("DrawAARange").GetValue<Circle>().Color);
            }
            if (_config.Item("DrawEnemyAARange").GetValue<Circle>().Active)
            {
                foreach (var enemy in STS.AllEnemys.Where(enemy => enemy.IsValidTarget(1500)))
                {
                    Utility.DrawCircle(
                        enemy.Position, AutoAttackRange(enemy),
                        _config.Item("DrawEnemyAARange").GetValue<Circle>().Color);
                }
            }
            if (_config.Item("DrawHoldzone").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(
                    Player.Position, _config.Item("Holdzone").GetValue<Slider>().Value,
                    _config.Item("DrawHoldzone").GetValue<Circle>().Color);
            }
            if (_config.Item("DrawLasthit").GetValue<Circle>().Active ||
                _config.Item("DrawnearKill").GetValue<Circle>().Active)
            {
                var minionList = MinionManager.GetMinions(
                    Player.Position, AutoAttackRange() + 500, MinionTypes.All, MinionTeam.Enemy,
                    MinionOrderTypes.MaxHealth);
                foreach (var minion in minionList.Where(minion => minion.IsValidTarget(AutoAttackRange() + 500)))
                {
                    var attackToKill = Math.Ceiling(minion.MaxHealth / Player.GetAutoAttackDamage(minion, true));
                    //var hpBarPosition = minion.HPBarPosition;
                    var barWidth = minion.IsMelee() ? 75 : 80;
                    if (minion.HasBuff("turretshield", true))
                    {
                        barWidth = 70;
                    }
                    //var barDistance = (float)(barWidth / attackToKill);
                    if (_config.Item("DrawLasthit").GetValue<Circle>().Active &&
                        minion.Health <= Player.GetAutoAttackDamage(minion, true))
                    {
                        Utility.DrawCircle(
                            minion.Position, minion.BoundingRadius, _config.Item("DrawLasthit").GetValue<Circle>().Color);
                    }
                    else if (_config.Item("DrawnearKill").GetValue<Circle>().Active &&
                             minion.Health <= Player.GetAutoAttackDamage(minion, true) * 2)
                    {
                        Utility.DrawCircle(
                            minion.Position, minion.BoundingRadius,
                            _config.Item("DrawnearKill").GetValue<Circle>().Color);
                    }
                }
            }

            if (OnDraw != null)
            {
                OnDraw(args);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (OrbWalk && !MenuGUI.IsChatOpen && !Player.IsChannelingImportantSpell())
            {
                var target = GetTarget();
                Orbwalk(target, Game.CursorPos);
            }

            if (OnUpdate != null)
            {
                OnUpdate(CurrentMode, args);
            }
        }

        public class BeforeAttackEventArgs
        {
            private bool _process = true;
            public AttackableUnit Target;
            public Obj_AI_Base Unit = ObjectManager.Player;

            public bool Process
            {
                get { return _process; }
                set
                {
                    NextAutoAttack = value;
                    _process = value;
                }
            }
        }
    }
}