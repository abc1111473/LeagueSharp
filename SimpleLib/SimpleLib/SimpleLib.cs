using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SimpleLib
{
    public class SL
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

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

        public void InitSimpleLib()
        {
            SOW.OnDraw += SOW_OnDraw;
            SOW.OnUpdate += SOW_OnUpdate;
            SOW.OnAllyTowerAggro += OnAllyTowerAggro;
            SOW.OnEnemyTowerAggro += OnEnemyTowerAggro;
            SOW.OnPlayerTowerAggro += OnPlayerTowerAggro;
            SOW.OnMinionTowerAggro += OnMinionTowerAggro;
            SOW.OnProcessSpellCast += OnProcessSpellCast;

            Game.OnGameSendPacket += OnSendPacket;
            Game.OnGameProcessPacket += OnProcessPacket;

            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += OnGapClose;
        }

        public void InitSimpleLib(float Range)
        {
            STS.MonitorRange = Range;
            InitSimpleLib();
        }
        
        private void SOW_OnUpdate(SOW.Mode currentMode, EventArgs args)
        {
            switch (SOW.CurrentMode)
            {
                case SOW.Mode.Combo:
                    OnCombo();
                    break;
                case SOW.Mode.Mixed:
                    OnMixed();
                    break;
                case SOW.Mode.Lasthit:
                    OnLasthit();
                    break;
                case SOW.Mode.LaneClear:
                    OnLaneClear();
                    break;
                case SOW.Mode.LaneFreeze:
                    OnLaneFreeze();
                    break;
                case SOW.Mode.Flee:
                    OnFlee();
                    break;
                case SOW.Mode.None:
                    break;
            }
            OnUpdate();
        }

        private void SOW_OnDraw(EventArgs args)
        {
            OnDraw();
        }

        public virtual void OnLoad() {}
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
        public virtual void OnDraw() {}
        public virtual void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {}
        public virtual void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell) {}
        public virtual void OnGapClose(ActiveGapcloser gapcloser) {}
        public virtual void OnSendPacket(GamePacketEventArgs args) {}
        public virtual void OnProcessPacket(GamePacketEventArgs args) {}
        public virtual void OnUpdate() {}
    }
}