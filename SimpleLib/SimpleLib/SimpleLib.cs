using System;
using System.Collections.Generic;
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
            CommonOrbWalker,
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

        public static LevelUpManager LevelUpManager = new LevelUpManager();
        //public static SkinManager SkinManager = new SkinManager();
        public static Orbwalking.Orbwalker CommonOrbwalker;

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

                    case Orbwalker.CommonOrbWalker:
                        var common = new Menu("Common OrbWalker", "COB");
                        CommonOrbwalker = new Orbwalking.Orbwalker(common);
                        MainMenu.AddSubMenu(common);
                        break;

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
    }
}