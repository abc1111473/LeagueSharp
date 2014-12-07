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
    public class SL
    {
        public Spell Q;
        public Spell W;
        public Spell E;
        public Spell R;

        public static Obj_AI_Hero Self = ObjectManager.Player;

        public Obj_AI_Hero EnemyTarget = STS.SelectedEnemyTarget;
        public Obj_AI_Hero AllyTarget = STS.SelectedAllyTarget;

        public SAM.LevelUpManager levelUpManager = new SAM.LevelUpManager();
        public SAM.SkinManager skinManager = new SAM.SkinManager();

        public SL()
        {
        }

        public void InitSL()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += OnGapClose;
            Game.OnGameSendPacket += OnSendPacket;
            Game.OnGameProcessPacket += OnProcessPacket;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            SOW.AfterAttack += OnAfterAttack;
            SOW.OnAttack += OnAttack;
            SOW.BeforeAttack += OnBeforeAttack;
            SMM.UnderTowerFarm += UnderTowerFarm;
        }

        /// <summary>
        ///     Returns name of the spell in the set SpellSlot and set unit.
        ///     If unit is null then returns the name of the spell in Players SpellSlot.
        /// </summary>
        public string GetSpellName(SpellSlot slot, Obj_AI_Base unit = null)
        {
            return unit != null ? unit.Spellbook.GetSpell(slot).Name : SL.Self.Spellbook.GetSpell(slot).Name;
        }

        /// <summary>
        ///     Returns current health procent for the selected unit.
        ///     If unit is null then returns current health procent for the player.
        /// </summary>
        public float GetHealthPercent(Obj_AI_Base unit = null)
        {
            if (unit == null)
                unit = SL.Self;
            return (unit.Health / unit.MaxHealth) * 100f;
        }

        /// <summary>
        ///     Returns current mana procent for the selected unit.
        ///     If unit is null then returns current mana procent for the player.
        /// </summary>
        public float GetManaPercent(Obj_AI_Hero unit = null)
        {
            if (unit == null)
                unit = SL.Self;
            return (unit.Mana / unit.MaxMana) * 100f;
        }

        public bool EnemysInRange(float range, int min = 1, Obj_AI_Hero unit = null)
        {
            if (unit == null)
                unit = Self;
            return min <= STS.AllEnemys.Count(hero => hero.Distance(unit) < range && hero.IsValidTarget());
        }

        public bool EnemysInRange(float range, int min, Vector3 pos)
        {
            return min <= STS.AllEnemys.Count(hero => hero.Position.Distance(pos) < range && hero.IsValidTarget());
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            switch (SOW.CurrentMode)
            {
                case SOW.Mode.Combo:
                    OnCombo();
                    break;
                case SOW.Mode.Harass:
                    OnHarass();
                    break;
                case SOW.Mode.LaneClear:
                    OnLaneClear();
                    break;
                case SOW.Mode.Lasthit:
                    OnLasthit();
                    break;
                case SOW.Mode.None:
                    OnStandby();
                    break;
            }
            OnPassive();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            OnDraw();
        }
        public virtual void Presets()
        {
        }

        public virtual void OnDraw()
        {
        }

        public virtual void OnBeforeAttack(SOW.BeforeAttackEventArgs args)
        {
        }

        public virtual void OnAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
        }

        public virtual void OnAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
        }

        public virtual void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
        }

        public virtual void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
        }

        public virtual void OnGapClose(ActiveGapcloser gapcloser)
        {
        }

        public virtual void OnSendPacket(GamePacketEventArgs args)
        {
        }

        public virtual void OnProcessPacket(GamePacketEventArgs args)
        {
        }

        public virtual void OnStandby()
        {
        }

        public virtual void OnLasthit()
        {
        }

        public virtual void OnLaneClear()
        {
        }

        public virtual void OnHarass()
        {
        }

        public virtual void OnCombo()
        {
        }

        public virtual void OnPassive()
        {
        }

        public virtual void UnderTowerFarm(Obj_AI_Base minion)
        {
        }
    }
}
