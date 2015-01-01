using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SimpleLib
{
    public class SL
    {
        public enum Orbwalker
        {
            SimpleOrbWalker,
            LXOrbWalker,
            xSLxOrbWalker,
            //CommonOrbWalker,
            None
        }

        public enum TargetSelector
        {
            SimpleTargetSelector,
            CommonTargetSelector,
            None
        }

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

        public static Orbwalker CurrentOrbWalker
        {
            get { return _orbwalker; }
            set { _orbwalker = value; }
        }

        public static TargetSelector CurrentTargetSelector
        {
            get { return _targetSelector; }
            set { _targetSelector = value; }
        }

        public static Menu MainMenu;

        public static Spell Q = new Spell(SpellSlot.Q);
        public static Spell W = new Spell(SpellSlot.W);
        public static Spell E = new Spell(SpellSlot.E);
        public static Spell R = new Spell(SpellSlot.R);

        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static SAM.LevelUpManager LevelUpManager = new SAM.LevelUpManager();
        public static SAM.SkinManager SkinManager = new SAM.SkinManager();

        public static Spell Barrier = new Spell(Player.GetSpellSlot("SummonerBarrier"));
        public static Spell Clairvoyance = new Spell(Player.GetSpellSlot("SummonerClairvoyance"));
        public static Spell Clarity = new Spell(Player.GetSpellSlot("SummonerMana"), 600);
        public static Spell Cleanse = new Spell(Player.GetSpellSlot("SummonerBoost"));
        public static Spell Exhaust = new Spell(Player.GetSpellSlot("SummonerExhaust"), 650);
        public static Spell Flash = new Spell(Player.GetSpellSlot("SummonerFlash"), 400);
        public static Spell Ghost = new Spell(Player.GetSpellSlot("SummonerHaste"));
        public static Spell Heal = new Spell(Player.GetSpellSlot("SummonerHeal"), 700);
        public static Spell Ignite = new Spell(Player.GetSpellSlot("SummonerDot"), 600);
        public static Spell Revive = new Spell(Player.GetSpellSlot("SummonerRevive"));
        public static Spell Smite = new Spell(Player.GetSpellSlot("SummonerSmite"), 700);
        public static Spell Teleport = new Spell(Player.GetSpellSlot("SummonerTeleport"));

        private static Orbwalker _orbwalker = Orbwalker.SimpleOrbWalker;
        private static TargetSelector _targetSelector = TargetSelector.SimpleTargetSelector;

        public void InitSimpleLib(string assemblyDisplayName, string assemblyName , Orbwalker orbwalker, TargetSelector targetSelector)
        {
            try
            {
                CurrentOrbWalker = orbwalker;
                CurrentTargetSelector = targetSelector;

                MainMenu = new Menu(assemblyDisplayName, assemblyName, true);
                SetTargetSelector(targetSelector);
                SetOrbWalker(orbwalker);
            }
            catch
            {
                Game.PrintChat("Failed to load the menu");
            }

            try
            {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

                Drawing.OnDraw += Drawing_OnDraw;

                Game.OnGameUpdate += Game_OnGameUpdate;
                Game.OnGameSendPacket += OnSendPacket;
                Game.OnGameProcessPacket += OnProcessPacket;

                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
                Obj_AI_Turret.OnProcessSpellCast += Obj_AI_Turret_OnProcessSpellCast;
                Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
                AntiGapcloser.OnEnemyGapcloser += OnGapClose;
            }
            catch
            {
                Game.PrintChat("Failed to initialize SimpleLib");
            }
        }

        private static void SetTargetSelector(TargetSelector targetSelector)
        {
            switch (targetSelector)
            {
                case TargetSelector.SimpleTargetSelector:
                    SimpleTargetSelector.StsMenu(MainMenu);
                    break;

                case TargetSelector.CommonTargetSelector:
                    var common = new Menu("Common Target Selector", "CTS");
                    LeagueSharp.Common.TargetSelector.AddToMenu(common);
                    MainMenu.AddSubMenu(common);
                    break;

                case TargetSelector.None:
                    break;
            }
        }

        private static void SetOrbWalker(Orbwalker orbWalker)
        {
            try
            {
                switch (orbWalker)
                {
                    case Orbwalker.SimpleOrbWalker:
                        MainMenu.AddSubMenu(SimpleOrbWalker.SowMenu);
                        break;

                    case Orbwalker.LXOrbWalker:
                        MainMenu.AddSubMenu(LXOrbwalker.LxMenu);
                        break;

                    case Orbwalker.xSLxOrbWalker:
                        MainMenu.AddSubMenu(xSLxOrbwalker.xSLxMenu);
                        break;

                    //case Orbwalker.CommonOrbWalker:


                    case Orbwalker.None:
                        break;
                }
            }
            catch
            {
                Game.PrintChat("Faild to load orbwalker");
            }
        }

        public static Mode CurrentMode
        {
            get
            {
                switch (CurrentOrbWalker)
                {
                    case Orbwalker.SimpleOrbWalker:
                        return CurrentSowMode;

                    case Orbwalker.LXOrbWalker:
                        return CurrentLxMode;

                    case Orbwalker.xSLxOrbWalker:
                        return CurrentxSLxMode;

                    case Orbwalker.None:
                        return Mode.None;
                }
                return Mode.None;
            }
        }

        private static Mode CurrentSowMode
        {
            get
            {
                switch (SimpleOrbWalker.CurrentMode)
                {
                    case SimpleOrbWalker.Mode.Combo:
                        return Mode.Combo;

                    case SimpleOrbWalker.Mode.Mixed:
                        return Mode.Mixed;

                    case SimpleOrbWalker.Mode.Lasthit:
                        return Mode.Lasthit;

                    case SimpleOrbWalker.Mode.LaneClear:
                        return Mode.LaneClear;

                    case SimpleOrbWalker.Mode.LaneFreeze:
                        return Mode.LaneFreeze;

                    case SimpleOrbWalker.Mode.Flee:
                        return Mode.Flee;

                    case SimpleOrbWalker.Mode.None:
                        return Mode.None;
                }
                return Mode.None;
            }
        }

        private static Mode CurrentLxMode
        {
            get
            {
                switch (LXOrbwalker.CurrentMode)
                {
                    case LXOrbwalker.Mode.Combo:
                        return Mode.Combo;

                    case LXOrbwalker.Mode.Harass:
                        return Mode.Mixed;

                    case LXOrbwalker.Mode.LaneClear:
                        return Mode.LaneClear;

                    case LXOrbwalker.Mode.LaneFreeze:
                        return Mode.LaneFreeze;

                    case LXOrbwalker.Mode.Lasthit:
                        return Mode.Lasthit;

                    case LXOrbwalker.Mode.Flee:
                        return Mode.Flee;

                    case LXOrbwalker.Mode.None:
                        return Mode.None;
                }
                return Mode.None;
            }
        }

        private static Mode CurrentxSLxMode
        {
            get
            {
                switch (xSLxOrbwalker.CurrentMode)
                {
                    case xSLxOrbwalker.Mode.Combo:
                        return Mode.Combo;

                    case xSLxOrbwalker.Mode.Harass:
                        return Mode.Mixed;

                    case xSLxOrbwalker.Mode.LaneClear:
                        return Mode.LaneClear;

                    case xSLxOrbwalker.Mode.LaneFreeze:
                        return Mode.LaneFreeze;

                    case xSLxOrbwalker.Mode.Lasthit:
                        return Mode.Lasthit;

                    case xSLxOrbwalker.Mode.Flee:
                        return Mode.Flee;

                    case xSLxOrbwalker.Mode.None:
                        return Mode.None;
                }
                return Mode.None;
            }
        }

        //public static void SwitchOrbWalker(Orbwalker newOrbWalker)
        //{
        //    string menuName = "";

        //    try
        //    {
        //        switch (CurrentOrbWalker)
        //        {
        //            case Orbwalker.SimpleOrbWalker:
        //                menuName = "SOW";
        //                SimpleOrbWalker.DisableSimpleOrbWalker();
        //                break;
        //            case Orbwalker.LXOrbWalker:
        //                menuName = "LX";
        //                LXOrbwalker.DisableLXOrbWalker();
        //                break;
        //            case Orbwalker.xSLxOrbWalker:
        //                menuName = "xSLx";
        //                xSLxOrbwalker.DisablexSLxOrbWalker();
        //                break;
        //        }
        //    }
        //    catch
        //    {
        //        Game.PrintChat("Failed to disable orbwalker");
        //    }

        //    if (menuName == "")
        //            return;
        //    try { 
        //        switch (newOrbWalker)
        //        {
        //            case Orbwalker.SimpleOrbWalker:
        //                MainMenu.Item(menuName).SetValue(SimpleOrbWalker.SowMenu);
        //                break;
        //            case Orbwalker.LXOrbWalker:
        //                MainMenu.Item(menuName).SetValue(LXOrbwalker.LxMenu);
        //                break;
        //            case Orbwalker.xSLxOrbWalker:
        //                MainMenu.Item(menuName).SetValue(xSLxOrbwalker.xSLxMenu);
        //                break;
        //            case Orbwalker.None:
        //                MainMenu.Item(menuName).SetValue("No OrbWalker Selected");
        //                break;
        //        }
        //        CurrentOrbWalker = newOrbWalker;
        //    }
        //    catch
        //    {
        //        Game.PrintChat("Failed to change orbwalker");
        //    }
        //}

        private void Game_OnGameLoad(EventArgs args)
        {
            Presets();
            OnLoad();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            switch (CurrentMode)
            {
                case Mode.Combo:
                    OnCombo();
                    break;

                case Mode.Mixed:
                    OnMixed();
                    break;

                case Mode.Lasthit:
                    OnLasthit();
                    break;

                case Mode.LaneClear:
                    OnLaneClear();
                    break;

                case Mode.LaneFreeze:
                    OnLaneFreeze();
                    break;

                case Mode.Flee:
                    OnFlee();
                    break;
            }
            OnUpdate();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            OnDraw();
        }

        private void Obj_AI_Turret_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValidTarget(1000, false, Player.Position)) return;

            if (sender.IsAlly)
            {
                if (args.Target is Obj_AI_Hero)
                {
                    OnEnemyTowerAggro((Obj_AI_Turret)sender, (Obj_AI_Base)args.Target);
                }

                if (args.Target is Obj_AI_Minion)
                {
                    OnMinionTowerAggro((Obj_AI_Turret)sender, (Obj_AI_Base)args.Target);
                }
            }

            if (sender.IsEnemy)
            {
                if (args.Target.IsMe)
                {
                    OnPlayerTowerAggro((Obj_AI_Turret)sender, Player);
                }

                if (args.Target is Obj_AI_Hero)
                {
                    OnAllyTowerAggro((Obj_AI_Turret)sender, (Obj_AI_Base)args.Target);
                }
            }
        }

        public virtual void OnCombo() {}
        public virtual void OnMixed() {}
        public virtual void OnLasthit() {}
        public virtual void OnLaneClear() {}
        public virtual void OnLaneFreeze() {}
        public virtual void OnFlee() {}
        public virtual void OnMinionTowerAggro(Obj_AI_Turret sender, Obj_AI_Base target) {}
        public virtual void OnPlayerTowerAggro(Obj_AI_Turret sender, Obj_AI_Base target) {}
        public virtual void OnEnemyTowerAggro(Obj_AI_Turret sender, Obj_AI_Base target) {}
        public virtual void OnAllyTowerAggro(Obj_AI_Turret sender, Obj_AI_Base target) {}
        public virtual void Presets() {}
        public virtual void OnLoad() {}
        public virtual void OnDraw() {}
        public virtual void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {}
        public virtual void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell) {}
        public virtual void OnGapClose(ActiveGapcloser gapcloser) {}
        public virtual void OnSendPacket(GamePacketEventArgs args) {}
        public virtual void OnProcessPacket(GamePacketEventArgs args) {}
        public virtual void OnUpdate() {}

        public static void CastBarrier()
        {
            if (Barrier.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Barrier.Slot) != SpellState.Ready)
            {
                return;
            }

            Barrier.Cast();
        }

        public static void CastBarrier(float healthPrecent)
        {
            if (Barrier.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Barrier.Slot) != SpellState.Ready)
            {
                return;
            }

            if (Player.HealthPercentage() <= healthPrecent)
            {
                Barrier.Cast();
            }
        }

        public static void CastClairvoyance(Vector3 position)
        {
            if (Clairvoyance.Slot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(Clairvoyance.Slot) == SpellState.Ready)
            {
                Clairvoyance.Cast(position);
            }
        }

        public static void CastClarity()
        {
            if (Clarity.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Clarity.Slot) != SpellState.Ready)
            {
                return;
            }

            Clarity.Cast();
        }

        public static void CastClarity(float manaProcent)
        {
            if (Clarity.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Clarity.Slot) != SpellState.Ready)
            {
                return;
            }

            if (Player.ManaPercentage() <= manaProcent)
            {
                Clarity.Cast();
            }
        }

        public static void CastClarity(int numberOfAllys)
        {
            if (Clarity.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Clarity.Slot) != SpellState.Ready)
            {
                return;
            }

            var temp = ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && ally.IsValidTarget(600));

            if (temp.Count() >= numberOfAllys)
            {
                Clarity.Cast();
            }
        }

        public static void CastCleanse()
        {
            if (Cleanse.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Cleanse.Slot) != SpellState.Ready)
            {
                return;
            }

            Cleanse.Cast();
        }

        public static void CastExhaust(Obj_AI_Hero target)
        {
            if (Exhaust.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Exhaust.Slot) != SpellState.Ready)
            {
                return;
            }

            if (target.IsValidTarget(650))
            {
                Exhaust.Cast(target);
            }
        }

        public static void CastFlash(Vector3 position)
        {
            if (Flash.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Flash.Slot) != SpellState.Ready)
            {
                return;
            }

            Flash.Cast(position);
        }

        public static void CastGhost()
        {
            if (Ghost.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ghost.Slot) != SpellState.Ready)
            {
                return;
            }

            Ghost.Cast();
        }

        public static void CastHeal()
        {
            if (Heal.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Heal.Slot) != SpellState.Ready)
            {
                return;
            }

            Heal.Cast();
        }

        public static void CastHeal(float healtProcent)
        {
            if (Heal.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Heal.Slot) != SpellState.Ready)
            {
                return;
            }

            if (Player.HealthPercentage() <= healtProcent)
            {
                Heal.Cast();
            }
        }

        public static void CastHeal(int numberOfAllys)
        {
            if (Heal.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Heal.Slot) != SpellState.Ready)
            {
                return;
            }

            var temp = ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && ally.IsValidTarget(700));

            if (temp.Count() >= numberOfAllys)
            {
                Heal.Cast();
            }
        }

        public static void CastIgnite(Obj_AI_Hero target)
        {
            if (Ignite.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite.Slot) != SpellState.Ready)
            {
                return;
            }

            if (target.IsValidTarget(600))
            {
                Ignite.Cast(target);
            }
        }

        public static void CastRevive()
        {
            if (Revive.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Revive.Slot) != SpellState.Ready ||
                !Player.IsDead)
            {
                return;
            }

            Revive.Cast();
        }

        public static void CastSmite(Obj_AI_Base target)
        {
            if (Smite.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Smite.Slot) != SpellState.Ready)
            {
                return;
            }

            if (target.IsValidTarget(700))
            {
                Smite.Cast(target);
            }
        }

        public static void CastTeleport(Obj_AI_Base target)
        {
            if (Teleport.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Teleport.Slot) != SpellState.Ready)
            {
                return;
            }

            Teleport.Cast(target);
        }

        /// <summary>
        ///     Is Player recalling
        /// </summary>
        public static bool IsRecalling()
        {
            return Player.IsRecalling();
        }

        /// <summary>
        ///     Is target recalling
        /// </summary>
        public static bool IsRecalling(Obj_AI_Hero target)
        {
            return target.IsRecalling();
        }

        /// <summary>
        ///     Returns the number of enemys at position with selected range
        /// </summary>
        public int countEnemiesNearPosition(Vector3 pos, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(hero => hero.IsEnemy && !hero.IsDead && hero.IsValid && hero.Distance(pos) <= range);
        }

        /// <summary>
        ///     Returns the number of allys at position with selected range
        /// </summary>
        public int countAlliesNearPosition(Vector3 pos, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(hero => hero.IsAlly && !hero.IsDead && hero.IsValid && hero.Distance(pos) <= range);
        }

        /// <summary>
        ///     Returns mana procent for the target. If target = null returns mana procent for the Player.
        /// </summary>
        public float GetManaPercent(Obj_AI_Hero unit = null)
        {
            if (unit == null)
            {
                unit = Player;
            }
            return (unit.Mana / unit.MaxMana) * 100f;
        }

        /// <summary>
        ///     Returns health procent for the target. If target = null returns health procent for the Player.
        /// </summary>
        public float GetHealthPercent(Obj_AI_Hero unit = null)
        {
            if (unit == null)
            {
                unit = Player;
            }
            return (unit.Health / unit.MaxHealth) * 100f;
        }

        /// <summary>
        ///     Returns whether the target has the set buff.
        /// </summary>
        public bool HasBuff(Obj_AI_Base target, string buffName)
        {
            return target.Buffs.Any(buff => buff.Name == buffName || buff.Name.Contains(buffName));
        }

        /// <summary>
        ///     Returns weather player is inside player.enemy turret range
        /// </summary>
        public static bool IsInsideEnemyTurrentRange()
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsEnemy && turret.Health > 0 && turret.Distance(Player) <= 775);
        }

        /// <summary>
        ///     Returns weather target is inside player.enemy turret range
        /// </summary>
        public static bool IsInsideEnemyTurrentRange(Obj_AI_Hero target)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsEnemy && turret.Health > 0 && turret.Distance(target) <= 775);
        }

        /// <summary>
        ///     Returns weather player is inside player.ally turret range
        /// </summary>
        public static bool IsInsideAllyTurrentRange()
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsAlly && turret.Health > 0 && turret.Distance(Player) <= 775);
        }

        /// <summary>
        ///     Returns weather target is inside player.ally turret range
        /// </summary>
        public static bool IsInsideAllyTurrentRange(Obj_AI_Hero target)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsAlly && turret.Health > 0 && turret.Distance(target) <= 775);
        }
    }
}