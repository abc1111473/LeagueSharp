using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleOrbWalker
{
    public class Orbwalker
    {
        public static Menu Config;
        public static Obj_AI_Hero Self = ObjectManager.Player;
        public static Obj_AI_Base ForcedTarget = null;
        public static IEnumerable<Obj_AI_Hero> AllEnemys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy);
        public static IEnumerable<Obj_AI_Hero> AllAllys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly);

        public enum Mode
        {
            Combo,
            Harass,
            LaneClear,
            Lasthit,
            None,
        }

        private static readonly string[] AttackResets = { "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };
        private static readonly string[] NoAttacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
        private static readonly string[] Attacks = { "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "viktorqbuff", "xenzhaothrust2", "xenzhaothrust3" };

        private static bool _drawing = true;
        private static bool _attack = true;
        private static bool _movement = true;
        private static bool _disableNextAttack;
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

        private static void AddToMenu(Menu ConfigMenu) 
        {
            Config = ConfigMenu;

            Config.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));

            var menuDrawing = new Menu("Drawing", "Drawing");
            menuDrawing.AddItem(new MenuItem("DrawAARange", "AA Circle").SetValue(new Circle(true, Color.FloralWhite)));
            menuDrawing.AddItem(new MenuItem("DrawEnemyAARange", "AA Circle Enemy").SetValue(new Circle(true, Color.Pink)));
            menuDrawing.AddItem(new MenuItem("DrawHoldzone", "HoldZone").SetValue(new Circle(true, Color.FloralWhite)));            
            menuDrawing.AddItem(new MenuItem("DrawLasthit", "Minion LastHit").SetValue(new Circle(true, Color.Lime)));
            menuDrawing.AddItem(new MenuItem("DrawnearKill", "Minion NearKill").SetValue(new Circle(true, Color.Gold)));
            Config.AddSubMenu(menuDrawing);

            Config.AddItem(new MenuItem("FarmDelay", "Farm Delay").SetValue(new Slider(0, 0, 200)));
            Config.AddItem(new MenuItem("ExtraWindUpTime", "Extra WindUp Time").SetValue(new Slider(0, 0, 200)));
            Config.AddItem(new MenuItem("Holdzone", "Hold Position").SetValue(new Slider(50, 500, 0)));
            Config.AddItem(new MenuItem("AttackMode", "Attack Mode").SetValue(new StringList(new[] { "Only Farming", "Farming + Carry mode" })));
            Config.AddItem(new MenuItem("Mode", "Orbwalking Mode").SetValue(new StringList(new[] { "To mouse", "To target" })));
            Config.AddItem(new MenuItem("Hotkeys", "Hotkeys"));
            Config.AddItem(new MenuItem("Harass", "Harass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.AddItem(new MenuItem("LaneClear", "LaneClear").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.AddItem(new MenuItem("Lasthit", "Lasthit").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.AddItem(new MenuItem("Combo", "Carry me!").SetValue(new KeyBind(" ".ToCharArray()[0], KeyBindType.Press)));

            Drawing.OnDraw += OnDraw;            
            Game.OnGameUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            GameObject.OnCreate += Obj_SpellMissile_OnCreate;
            Game.OnGameProcessPacket += OnProcessPacket;

            CheckAutoWindUp();            
        }

        public static void InitializeOrbwalker(Menu ConfigMenu, bool Custom = false)
        {
            if (!Custom)
                AddToMenu(ConfigMenu);
            else
                ConfigMenu.AddItem(new MenuItem("Credits", "Powered by SimpleOrbWalker"));
                return;
        }

        public static void DisableAttacks()
        {
            _attack = false;
        }

        public static void EnableAttacks()
        {
            _attack = true;
        }

        public static void ForceTarget(Obj_AI_Base target)
        {
            ForcedTarget = target;
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
            return Self.AttackRange + Self.BoundingRadius;
        }

        public static float AutoAttackRange(Obj_AI_Base target)
        {
            return target.AttackRange + target.BoundingRadius;
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

        public static void Attack(Obj_AI_Base target)
        {
            if (OnAttack != null && target != null) OnAttack(Self, target);
        }

        public static float AutoAttackMissileSpeed()
        {
            return Self.BasicAttack.MissileSpeed;
        }

        public static float AutoAttackCastTime()
        {
            return Self.BasicAttack.SpellCastTime;
        }

        public static double CountKillhits(Obj_AI_Base enemy)
        {
            return enemy.Health / Self.GetAutoAttackDamage(enemy);
        }

        public static float WindUpTime()
        {
            return _windup;
        }

        public static float AnimationTime()
        {
            return (1 / AutoAttackMissileSpeed() * Self.BasicAttack.SpellCastTime);
        }

        public static float Latency()
        {
            return Game.Ping;
        }

        public static bool CanAttack()
        {
            if (_lastAATick <= Environment.TickCount)
                return Environment.TickCount + Game.Ping / 2 + 25 >= _lastAATick + Self.AttackDelay * 1000 && _attack && IsAllowedToAttack();
            return false;
        }

        public static bool CanMove()
        {
            if (_lastAATick <= Environment.TickCount)
                return Environment.TickCount + Game.Ping / 2 >= _lastAATick + Self.AttackCastDelay * 1000 + _windup && _movement && IsAllowedToMove();
            return false;
        }

        public static int FarmDelay(int offset = 0)
        {
            return Config.Item("FarmDelay").GetValue<Slider>().Value + offset;
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
            if (windUp < 40) windUp = 40;

            Config.Item("ExtraWindUpTime").SetValue(windUp < 200 ? new Slider(windUp, 200, 0) : new Slider(200, 200, 0));
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
                BeforeAttack(new BeforeAttackEventArgs{Target = target});
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

        private static bool ShouldWait()
        {
            return ObjectManager.Get<Obj_AI_Minion>().Any(minion => minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral && InRange(minion) &&
            HealthPrediction.LaneClearHealthPrediction(minion, (int)((Self.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay()) <= Self.GetAutoAttackDamage(minion));
        }

        public static bool IsValidBuilding(Obj_Building building)
        {
            if (building.Health > 0f && building.IsEnemy && building.IsTargetable && !building.IsInvulnerable && !building.IsDead && building.IsValid)
                return true;
            return false;
        }

        public static bool InRangeBuildings(Obj_Building target)
        {
            if (target != null)
            {
                var myRange = Self.AttackRange + Self.BoundingRadius + target.BoundingRadius;
                return Vector2.DistanceSquared(target.Position.To2D(), Self.Position.To2D()) <= myRange * myRange;
            }
            return false;
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
            if (MenuGUI.IsChatOpen) return;

            Obj_AI_Base target = GetPossibleTarget();
            Orbwalk(target, Game.CursorPos);
        }

        private static void OnDraw(EventArgs args)
        {
            if (!_drawing) return;

            if (Config.Item("DrawAARange").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(Self.Position, AutoAttackRange(), Config.Item("DrawAARange").GetValue<Circle>().Color);
            }

            if (Config.Item("DrawEnemyAARange").GetValue<Circle>().Active)
            {
                foreach (var enemy in AllEnemys.Where(enemy => enemy.IsValidTarget(1500)))
                    Utility.DrawCircle(enemy.Position, AutoAttackRange(enemy), Config.Item("DrawEnemyAARange").GetValue<Circle>().Color);              
            }

            if (Config.Item("DrawHoldzone").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(Self.Position, Config.Item("Holdzone").GetValue<Slider>().Value, Config.Item("DrawHoldzone").GetValue<Circle>().Color);
            }

            if (Config.Item("DrawLasthit").GetValue<Circle>().Active || Config.Item("DrawnearKill").GetValue<Circle>().Active)
            {
                var minionList = MinionManager.GetMinions(Self.Position, AutoAttackRange() + 500, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                foreach (var minion in minionList.Where(minion => minion.IsValidTarget(AutoAttackRange() + 500)))
                {
                    var attackToKill = Math.Ceiling(minion.MaxHealth / Self.GetAutoAttackDamage(minion, true));
                    var hpBarPosition = minion.HPBarPosition;
                    var barWidth = minion.IsMelee() ? 75 : 80;
                    if (minion.HasBuff("turretshield", true))
                        barWidth = 70;
                    var barDistance = (float)(barWidth / attackToKill);
                   
                    if (Config.Item("DrawLasthit").GetValue<Circle>().Active &&
                        minion.Health <= Self.GetAutoAttackDamage(minion, true))
                        Utility.DrawCircle(minion.Position, minion.BoundingRadius, Config.Item("DrawLasthit").GetValue<Circle>().Color);
                    else if (Config.Item("DrawnearKill").GetValue<Circle>().Active &&
                             minion.Health <= Self.GetAutoAttackDamage(minion, true) * 2)
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
            if (ForcedTarget != null)
            {
                if (InRange(ForcedTarget))
                    return ForcedTarget;
                ForcedTarget = null;
            }

            Obj_AI_Base tempTarget = null;
            Obj_AI_Hero killableEnemy = null;            
            var hitsToKill = double.MaxValue;
            _currentAttackMode = Config.Item("AttackMode").GetValue<StringList>().SelectedIndex;

            switch (CurrentMode)
            {
                case Mode.Combo:
                    if (_currentAttackMode == 1)
                    {
                        foreach (var enemy in AllEnemys.Where(hero => hero.IsValidTarget() && InExtendedRange(hero, 200f)))
                        {
                            var killHits = CountKillhits(enemy);
                            if (killableEnemy != null && (!(killHits < hitsToKill) || enemy.HasBuffOfType(BuffType.Invulnerability))) continue;
                            hitsToKill = killHits;
                            killableEnemy = enemy;
                        }
                        tempTarget = hitsToKill <= 3 ? killableEnemy : SimpleTs.GetTarget(AutoAttackRange(), SimpleTs.DamageType.Physical);
                    }
                    if (tempTarget != null) return tempTarget;    
                    else
                    {
                        foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(AutoAttackRange()))) tempTarget = turret;
                        if (tempTarget != null) return tempTarget;
                        else
                        {
                            foreach (var building in ObjectManager.Get<Obj_Building>().Where(b => IsValidBuilding(b) && InRangeBuildings(b) &&
                                (b.Name.StartsWith("Barracks_") || IsValidBuilding(b) && InRangeBuildings(b) && b.Name.StartsWith("HQ_"))))
                                Self.IssueOrder(GameObjectOrder.AttackUnit, building);
                            return null;
                        }
                    }
                case Mode.Harass:
                    foreach (
                        var minion in from minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.Name != "Beacon" && InExtendedRange(minion, 100f))
                            let t = (int)(Self.AttackCastDelay * 1000) - 100 + Game.Ping / 2 + 1000 * (int)Self.Distance(minion) / (int)AutoAttackMissileSpeed()
                                let predHealth = HealthPrediction.GetHealthPrediction(minion, t, FarmDelay())
                                    where minion.Team != GameObjectTeam.Neutral && predHealth > 0 && predHealth <= Self.GetAutoAttackDamage(minion, true)
                                        select minion) tempTarget = minion;
                    if (tempTarget != null) return tempTarget;
                    else if (ShouldWait()) return null;
                    else
                    {
                        if (_currentAttackMode == 1)
                        {
                            foreach (var enemy in AllEnemys.Where(hero => hero.IsValidTarget() && InExtendedRange(hero, 200f)))
                            {
                                var killHits = CountKillhits(enemy);
                                if (killableEnemy != null && (!(killHits < hitsToKill) || enemy.HasBuffOfType(BuffType.Invulnerability))) continue;
                                hitsToKill = killHits;
                                killableEnemy = enemy;
                            }
                            tempTarget = hitsToKill <= 3 ? killableEnemy : SimpleTs.GetTarget(AutoAttackRange(), SimpleTs.DamageType.Physical);
                        }
                        if (tempTarget != null) return tempTarget;
                        else
                        {
                            foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(AutoAttackRange()))) tempTarget = turret;
                            if (tempTarget != null) return tempTarget;
                            else
                            {
                                foreach (var building in ObjectManager.Get<Obj_Building>().Where(b => IsValidBuilding(b) && InRangeBuildings(b) &&
                                    (b.Name.StartsWith("Barracks_") || IsValidBuilding(b) && InRangeBuildings(b) && b.Name.StartsWith("HQ_"))))
                                        Self.IssueOrder(GameObjectOrder.AttackUnit, building);
                                return null;
                            }
                        }
                    }

                case Mode.LaneClear:
                    foreach (
                        var minion in from minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.Name != "Beacon" && InExtendedRange(minion,100f))
                                      let t = (int)(Self.AttackCastDelay * 1000) - 100 + Game.Ping / 2 + 1000 * (int)Self.Distance(minion) / (int)AutoAttackMissileSpeed()
                                      let predHealth = HealthPrediction.GetHealthPrediction(minion, t, FarmDelay())
                                      where minion.Team != GameObjectTeam.Neutral && predHealth > 0 && predHealth <= Self.GetAutoAttackDamage(minion, true)
                                      select minion) tempTarget = minion;
                    if (tempTarget != null) return tempTarget;
                    else if (ShouldWait()) return null;
                    else
                    {
                        var maxhealth = new float[] { 0 };
                        var maxhealth2 = maxhealth;
                        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(AutoAttackRange())
                            && minion.Name != "Beacon" && minion.Team == GameObjectTeam.Neutral)
                                .Where(minion => minion.MaxHealth >= maxhealth2[0] || Math.Abs(maxhealth2[0] - float.MaxValue) < float.Epsilon))
                        {
                            tempTarget = minion;
                            maxhealth[0] = minion.MaxHealth;
                        }
                        if (tempTarget != null) return tempTarget;
                        else
                        {
                            maxhealth = new float[] { 0 };
                            foreach (var minion in from minion in ObjectManager.Get<Obj_AI_Minion>()
                                .Where(minion => minion.IsValidTarget(AutoAttackRange()) && minion.Name != "Beacon")
                                                   let predHealth = HealthPrediction.LaneClearHealthPrediction(minion, (int)((Self.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay())
                                                   where predHealth >= 2 * Self.GetAutoAttackDamage(minion, true) || Math.Abs(predHealth - minion.Health) < float.Epsilon
                                                   where minion.Health >= maxhealth[0] || Math.Abs(maxhealth[0] - float.MaxValue) < float.Epsilon
                                                   select minion)
                            {
                                tempTarget = minion;
                                maxhealth[0] = minion.MaxHealth;
                            }
                            if (tempTarget != null) return tempTarget;
                            else return null;
                        }
                    }
                case Mode.Lasthit:
                    foreach (
                        var minion in from minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.Name != "Beacon" && InExtendedRange(minion, 200f))
                            let t = (int)(Self.AttackCastDelay * 1000) - 100 + Game.Ping / 2 + 1000 * (int)Self.Distance(minion) / (int)AutoAttackMissileSpeed()
                                let predHealth = HealthPrediction.GetHealthPrediction(minion, t, FarmDelay())
                                    where minion.Team != GameObjectTeam.Neutral && predHealth > 0 && predHealth <= Self.GetAutoAttackDamage(minion, true)
                                        select minion) tempTarget = minion;
                    if (tempTarget != null) return tempTarget;
                    else return null;
                default:
                    return null;                    
            }
        }

        public static void MoveTo(Vector3 position)
        {
            if (Environment.TickCount - _lastMovement < 50)
                return;

            _lastMovement = Environment.TickCount;

            var holdAreaRadius = Config.Item("Holdzone").GetValue<Slider>().Value;
            if (Self.ServerPosition.Distance(position) < holdAreaRadius)
            {
                if (Self.Path.Count() > 1)
                    Self.IssueOrder(GameObjectOrder.HoldPosition, Self.Position);
                return;
            }

            var point = Self.ServerPosition +
            300 * (position.To2D() - Self.ServerPosition.To2D()).Normalized().To3D();
            Self.IssueOrder(GameObjectOrder.MoveTo, point);
        }

        private static void PositonToTarget(Obj_AI_Base target)
        {
            if (!InExtendedRange(target, 200f)) MoveTo(Game.CursorPos);
            
            float point = (AutoAttackRange()/100)*80;

            if (Self.ServerPosition.Distance(target.ServerPosition) < point) MoveTo(Game.CursorPos);
            else Self.IssueOrder(GameObjectOrder.MoveTo, target.ServerPosition);       
        }

        public static void Orbwalk(Obj_AI_Base target, Vector3 positon)
        {
            if (target != null && CanAttack())
            {
                _disableNextAttack = false;
                FireBeforeAttack(target);
                if (!_disableNextAttack)
                {
                    if (CurrentMode != Mode.None)
                    {
                        Self.IssueOrder(GameObjectOrder.AttackUnit, target);
                        _lastAATick = Environment.TickCount + Game.Ping / 2;
                    }
                }
            }
            if (!CanMove()) return;
            if ((_currentMode = Config.Item("Mode").GetValue<StringList>().SelectedIndex) == 1) PositonToTarget(target);
            else MoveTo(positon);
        }  
    }

    interface ICustomOrbwalker
    {
        void CustomOrbwalk(Obj_AI_Base target, Vector3 positon);
        void CustomMenu(Menu CustomM);
        Obj_AI_Base CustomGetPossibleTarget();
        void CustomMoveTo(Vector3 position);
    }
}
