using System;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

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

            Game.OnGameSendPacket += OnSendPacket;
            Game.OnGameProcessPacket += OnProcessPacket;

            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
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

        public static void CastBarrier()
        {
            if (Barrier.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Barrier.Slot) != SpellState.Ready) return;

            Barrier.Cast();
        }

        public static void CastBarrier(float healthPrecent)
        {
            if (Barrier.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Barrier.Slot) != SpellState.Ready) return;

            if (Player.HealthPercentage() <= healthPrecent)
            {
                Barrier.Cast();
            }
        }

        public static void CastClairvoyance(Vector3 position)
        {
            if (Clairvoyance.Slot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(Clairvoyance.Slot) == SpellState.Ready)
            {
                Clairvoyance.Cast(position);
            }
        }

        public static void CastClarity()
        {
            if (Clarity.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Clarity.Slot) != SpellState.Ready) return;

            Clarity.Cast();
        }

        public static void CastClarity(float manaProcent)
        {
            if (Clarity.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Clarity.Slot) != SpellState.Ready) return;

            if (Player.ManaPercentage() <= manaProcent)
            {
                Clarity.Cast();
            }
        }

        public static void CastClarity(int numberOfAllys)
        {
            if (Clarity.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Clarity.Slot) != SpellState.Ready) return;

            var temp = ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && ally.IsValidTarget(600));

            if (temp.Count() >= numberOfAllys)
            {
                Clarity.Cast();
            }
        }

        public static void CastCleanse()
        {
            if (Cleanse.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Cleanse.Slot) != SpellState.Ready) return;

            Cleanse.Cast();
        }

        public static void CastExhaust(Obj_AI_Hero target)
        {
            if (Exhaust.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Exhaust.Slot) != SpellState.Ready) return;
            
            if (target.IsValidTarget(650))
            {
                Exhaust.Cast(target);
            }
        }

        public static void CastFlash(Vector3 position)
        {
            if (Flash.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Flash.Slot) != SpellState.Ready) return;

            Flash.Cast(position);
        }

        public static void CastGhost()
        {
            if (Ghost.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ghost.Slot) != SpellState.Ready) return;

            Ghost.Cast();
        }

        public static void CastHeal()
        {
            if (Heal.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Heal.Slot) != SpellState.Ready) return;

            Heal.Cast();
        }

        public static void CastHeal(float healtProcent)
        {
            if (Heal.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Heal.Slot) != SpellState.Ready) return;

            if (Player.HealthPercentage() <= healtProcent)
            {
                Heal.Cast();
            }
        }

        public static void CastHeal(int numberOfAllys)
        {
            if (Heal.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Heal.Slot) != SpellState.Ready) return;

            var temp = ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && ally.IsValidTarget(700));

            if (temp.Count() >= numberOfAllys)
            {
                Heal.Cast();
            }
        }

        public static void CastIgnite(Obj_AI_Hero target)
        {
            if (Ignite.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite.Slot) != SpellState.Ready) return;

            if (target.IsValidTarget(600))
            {
                Ignite.Cast(target);
            }
        }

        public static void CastRevive()
        {
            if (Revive.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Revive.Slot) != SpellState.Ready || !Player.IsDead) return;

            Revive.Cast();
        }

        public static void CastSmite(Obj_AI_Base target)
        {
            if (Smite.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Smite.Slot) != SpellState.Ready) return;

            if (target.IsValidTarget(700))
            {
                Smite.Cast(target);
            }
        }

        public static void CastTeleport(Obj_AI_Base target)
        {
            if (Teleport.Slot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Teleport.Slot) != SpellState.Ready) return;

            Teleport.Cast(target);
        }
    }
}