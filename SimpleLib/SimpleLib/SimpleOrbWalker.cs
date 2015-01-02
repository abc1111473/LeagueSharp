using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleLib
{
    public static class SimpleOrbWalker
    {
        public delegate void AfterAttackEvenH(AttackableUnit unit, AttackableUnit target);

        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);

        public delegate void OnAttackEvenH(AttackableUnit unit, AttackableUnit target);

        public delegate void OnTargetChangeH(AttackableUnit oldTarget, AttackableUnit newTarget);

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
            "xenzhaocombotarget", "yorickspectral", "reksaiq"
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
        private static AttackableUnit _lastTarget;
        private static Vector3 _lastMovePosition = Vector3.Zero;

        public static bool OrbWalk
        {
            get { return _orbwalk; }
            set { _orbwalk = value; }
        }

        public static bool Drawings
        {
            get
            {
                try
                {
                    return _config.Item("Drawings").GetValue<bool>();
                    
                }
                catch
                {
                    // ignored
                }
                return _drawings;
            }
            set
            {
                _drawings = value;
                try
                {
                    _config.Item("Drawings").SetValue(value);
                }
                catch
                {
                    // ignored
                }
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
            get { return _extraWindUp; }
        }

        public static int FarmDelay
        {
            get
            {
                try
                {
                    _farmDelay = _config.Item("FarmDelay").GetValue<Slider>().Value;
                }
                catch
                {
                    // ignored
                }
                return _farmDelay;
            }
            set
            {
                try
                {
                    _config.Item("FarmDelay").SetValue(new Slider(value, 0, 200));
                }
                catch
                {
                    // ignored
                }
                _farmDelay = value;
            }
        }

        public static int HoldAreaRadius
        {
            get
            {
                try
                {
                    _holdZone = _config.Item("Holdzone").GetValue<Slider>().Value;
                    
                }
                catch
                {
                    // ignored
                }
                return _holdZone;
            }
            set
            {
                try
                {
                    _config.Item("Holdzone").SetValue(new Slider(value, 0, 500));
                    
                }
                catch
                {
                    // ignored
                }
                _holdZone = value;
            }
        }

        public static AttackableUnit LastTarget
        {
            get { return _lastTarget; }
        }

        public static Vector3 LastMovePosition
        {
            get { return _lastMovePosition; }
            set { _lastMovePosition = value; }
        }

        public static Mode CurrentMode
        {
            get
            {
                try
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
                catch
                {
                    return Mode.None;
                }
            }
        }

        public static event BeforeAttackEvenH BeforeAttack;
        public static event OnAttackEvenH OnAttack;
        public static event AfterAttackEvenH AfterAttack;
        public static event OnTargetChangeH TargetChange;

        private static void InitSOW()
        {
            _playerMelee = Player.IsMelee();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate += Obj_SpellMissile_OnCreate;
            Obj_AI_Hero.OnInstantStopAttack += ObjAiHeroOnOnInstantStopAttack;
        }

        public static void DisableSimpleOrbWalker()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Drawing.OnDraw -= Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate -= Obj_SpellMissile_OnCreate;
            Obj_AI_Hero.OnInstantStopAttack -= ObjAiHeroOnOnInstantStopAttack;
        }

        /// <summary>
        ///     Returns the Menu for SOW
        /// </summary>
        public static Menu SowMenu
        {
            get
            {
                var stsMenu = new Menu("SimpleOrbWalker", "SOW");

                var menuDrawing = new Menu("Drawing", "DrwMenu");
                menuDrawing.AddItem(new MenuItem("Drawings", "Drawings").SetValue(_drawings));
                menuDrawing.AddItem(new MenuItem("DrawAARange", "AA Circle").SetValue(new Circle(true, Color.Aqua)));
                menuDrawing.AddItem(
                    new MenuItem("DrawEnemyAARange", "AA Circle Enemy").SetValue(new Circle(true, Color.Chocolate)));
                menuDrawing.AddItem(new MenuItem("DrawHoldzone", "HoldZone").SetValue(new Circle(true, Color.Gray)));
                menuDrawing.AddItem(
                    new MenuItem("DrawLasthit", "Minion LastHit").SetValue(new Circle(true, Color.Lime)));
                menuDrawing.AddItem(
                    new MenuItem("DrawnearKill", "Minion NearKill").SetValue(new Circle(true, Color.Gold)));
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
                    new MenuItem("LaneClear", "LaneClear").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                stsMenu.AddItem(
                    new MenuItem("Lasthit", "Lasthit").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                stsMenu.AddItem(
                    new MenuItem("LaneFreeze", "Lane Freeze").SetValue(
                        new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                stsMenu.AddItem(
                    new MenuItem("Flee", "Flee").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                stsMenu.AddItem(
                    new MenuItem("Combo", "Carry me!").SetValue(new KeyBind(" ".ToCharArray()[0], KeyBindType.Press)));
                
                _config = stsMenu;

                InitSOW();

                return _config;
            }
        }

        private static bool IsAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        private static bool IsAutoAttack(string name)
        {
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) ||
                   Attacks.Contains(name.ToLower());
        }

        private static void ResetAutoAttackTimer()
        {
            _lastAutoAttackTime = 0;
        }

        public static bool IsMelee(this Obj_AI_Hero unit)
        {
            return unit.CombatType == GameObjectCombatType.Melee;
        }


        public static float AutoAttackRange(this Obj_AI_Hero from, AttackableUnit target = null)
        {
            var result = from.AttackRange + from.BoundingRadius;
            if (target != null)
                result += target.BoundingRadius;
            if (target is Obj_AI_Hero)
                result -= 25;
            return result;
        }

        public static bool InAutoAttackRange(AttackableUnit target)
        {
            if (target == null)
                return false;
            var myRange = Player.AutoAttackRange(target);
            return target.IsValidTarget(myRange);
        }

        public static float BasicAttackMissileSpeed(this Obj_AI_Hero target)
        {
            return target.IsMelee() ? float.MaxValue : target.BasicAttack.MissileSpeed;
        }

        private static void CheckAutoWindUp()
        {
            var additional = 0;
            if (Game.Ping >= 100)
                additional = Game.Ping / 100 * 5;
            else if (Game.Ping > 40 && Game.Ping < 100)
                additional = Game.Ping / 100 * 10;
            else if (Game.Ping <= 40)
                additional = +20;
            var windUp = Game.Ping + additional;
            if (windUp < 40)
                windUp = 200;

            _extraWindUp = windUp;
        }

        public static float GetNextAATime()
        {
            return (_lastAutoAttackTime + Player.AttackDelay * 1000) - (Environment.TickCount + Game.Ping / 2 + 25);
        }

        public static float AttackSpeed(this Obj_AI_Base target)
        {
            return 1 / target.AttackDelay;
        }

        private static bool CanAttack()
        {
            if (_lastAutoAttackTime <= Environment.TickCount)
                return Environment.TickCount + Game.Ping / 2 + 25 >= _lastAutoAttackTime + Player.AttackDelay * 1000 && _attack;
            return false;
        }

        private static bool CanMove()
        {
            if (_lastAutoAttackTime <= Environment.TickCount)
                return Environment.TickCount + Game.Ping / 2 >= _lastAutoAttackTime + Player.AttackCastDelay * 1000 + _extraWindUp && _move;
            return false;
        }

        private static bool IsAllowedToMove()
        {
            if (!_move)
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

        private static bool IsAllowedToAttack()
        {
            if (!_attack)
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
            if (targetUnit.IsValidTarget() && CanAttack() && IsAllowedToAttack())
            {
                NextAutoAttack = true;

                FireBeforeAttack(targetUnit);

                if (NextAutoAttack)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, targetUnit);

                    if (LastTarget != null && LastTarget.IsValid && LastTarget != targetUnit)
                    {
                        _lastAutoAttackTime = Environment.TickCount + Game.Ping / 2;
                    }
                }
            }

            if (!CanMove() || !IsAllowedToMove())
            {
                return;
            }

            var target = targetUnit as Obj_AI_Hero;

            if (target != null && _playerMelee && InAutoAttackRange(target) &&
                Game.CursorPos.Distance(target.Position) < 300)
            {
                var position = SimplePrediction.MeleeMovmentPrediction(target);
                MoveTo(position, overrideTimer);
            }
            else
            {
                MoveTo(moveTo, overrideTimer);
            }
        }

        private static AttackableUnit GetTargetSTS()
        {
            AttackableUnit temp;
            if (CurrentMode == Mode.Combo)
            {
                temp = SimpleTargetSelector.GetTarget(SimpleTargetSelector.Team.Enemy);

                if (temp.IsValidTarget(SimpleTargetSelector.MonitorRange))
                {
                    return temp;
                }
            }

            if (CurrentMode == Mode.Mixed)
            {
                temp = SMM.GetMinion(Player.AutoAttackRange(), SMM.MinionMode.LastHit, true, FarmDelay);

                if (temp.IsValidTarget())
                {
                    return temp;
                }

                temp = SimpleTargetSelector.GetTarget(SimpleTargetSelector.Team.Enemy);

                if (temp.IsValidTarget())
                {
                    return temp;
                }
            }

            if (CurrentMode == Mode.Lasthit)
            {
                temp = SMM.GetMinion(Player.AutoAttackRange(), SMM.MinionMode.LastHit, true, FarmDelay);

                if (temp.IsValidTarget())
                {
                    return temp;
                }
            }

            if (CurrentMode == Mode.LaneClear)
            {
                temp = SMM.GetMinion(Player.AutoAttackRange(), SMM.MinionMode.LastHit, true, FarmDelay);

                if (temp.IsValidTarget())
                {
                    return temp;
                }

                if (!SMM.ShouldWait(Player.AutoAttackRange() + 200, FarmDelay))
                {
                    temp = SMM.GetMinion(Player.AutoAttackRange(), SMM.MinionMode.LaneClear, true, FarmDelay);

                    if (temp.IsValidTarget())
                    {
                        return temp;
                    }
                }
            }

            if (CurrentMode == Mode.LaneFreeze)
            {
                temp = SMM.GetMinion(Player.AutoAttackRange() + 200, SMM.MinionMode.LaneFreez, true, FarmDelay);

                if (temp.IsValidTarget())
                {
                    return temp;
                }
            }

            temp = SimpleTargetSelector.GetTurret(Player.AutoAttackRange() + 150);

            if (temp.IsValidTarget())
            {
                return temp;
            }

            temp = SimpleTargetSelector.GetInhibitorsNexus(Player.AutoAttackRange() + 150);

            return temp;
        }

        private static AttackableUnit GetTargetCommonTS()
        {
            AttackableUnit temp;
            if (CurrentMode == Mode.Combo)
            {
                temp = TargetSelector.GetTarget(Player.AutoAttackRange(), TargetSelector.DamageType.Physical);

                if (temp.IsValidTarget())
                {
                    return temp;
                }
            }

            if (CurrentMode == Mode.Mixed)
            {
                temp = SMM.GetMinion(Player.AutoAttackRange(), SMM.MinionMode.LastHit, true, FarmDelay);

                if (temp.IsValidTarget())
                {
                    return temp;
                }

                temp = TargetSelector.GetTarget(Player.AutoAttackRange(), TargetSelector.DamageType.Physical);

                if (temp.IsValidTarget())
                {
                    return temp;
                }
            }

            if (CurrentMode == Mode.Lasthit)
            {
                temp = SMM.GetMinion(Player.AutoAttackRange(), SMM.MinionMode.LastHit, true, FarmDelay);

                if (temp.IsValidTarget())
                {
                    return temp;
                }
            }

            if (CurrentMode == Mode.LaneClear)
            {
                temp = SMM.GetMinion(Player.AutoAttackRange(), SMM.MinionMode.LastHit, true, FarmDelay);

                if (temp.IsValidTarget())
                {
                    return temp;
                }

                if (!SMM.ShouldWait(Player.AutoAttackRange() + 200, FarmDelay))
                {
                    temp = SMM.GetMinion(Player.AutoAttackRange(), SMM.MinionMode.LaneClear, true, FarmDelay);

                    if (temp.IsValidTarget())
                    {
                        return temp;
                    }
                }
            }

            if (CurrentMode == Mode.LaneFreeze)
            {
                temp = SMM.GetMinion(Player.AutoAttackRange() + 200, SMM.MinionMode.LaneFreez, true, FarmDelay);

                if (temp.IsValidTarget())
                {
                    return temp;
                }
            }

            temp = SimpleTargetSelector.GetTurret(Player.AutoAttackRange() + 150);

            if (temp.IsValidTarget())
            {
                return temp;
            }

            temp = SimpleTargetSelector.GetInhibitorsNexus(Player.AutoAttackRange() + 150);

            return temp;
        }

        private static AttackableUnit GetTarget()
        {
            switch (SL.CurrentTargetSelector)
            {
                case SL.TargetSelector.SimpleTargetSelector:
                    return GetTargetSTS();

                case SL.TargetSelector.CommonTargetSelector:
                    break;

                case SL.TargetSelector.None:
                    return null;
            }
            return null;
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
                FireAfterAttack(missile.SpellCaster, _lastTarget);
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (IsAutoAttackReset(spell.SData.Name) && unit.IsMe)
                Utility.DelayAction.Add(100, ResetAutoAttackTimer);

            if (!IsAutoAttack(spell.SData.Name))
                return;
            if (unit.IsMe)
            {
                _lastAutoAttackTime = Environment.TickCount - Game.Ping / 2; 

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

        private static void ObjAiHeroOnOnInstantStopAttack(Obj_AI_Base sender, GameObjectInstantStopAttackEventArgs args)
        {
            if (sender.IsValid && sender.IsMe && (args.BitData & 1) == 0 && ((args.BitData >> 4) & 1) == 1)
            {
                ResetAutoAttackTimer();
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
                    Player.Position, Player.AutoAttackRange(), _config.Item("DrawAARange").GetValue<Circle>().Color);
            }
            if (_config.Item("DrawEnemyAARange").GetValue<Circle>().Active)
            {
                foreach (var enemy in SimpleTargetSelector.AllEnemys.Where(enemy => enemy.IsValidTarget(1500)))
                {
                    Utility.DrawCircle(
                        enemy.Position, Player.AutoAttackRange(enemy),
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
                    Player.Position, Player.AutoAttackRange() + 500, MinionTypes.All, MinionTeam.Enemy,
                    MinionOrderTypes.MaxHealth);
                foreach (var minion in minionList.Where(minion => minion.IsValidTarget(Player.AutoAttackRange() + 500)))
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
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!OrbWalk || MenuGUI.IsChatOpen || Player.IsChannelingImportantSpell())
            {
                return;
            }
            
            CheckAutoWindUp();
            var target = GetTarget();
            Orbwalk(target, Game.CursorPos);
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