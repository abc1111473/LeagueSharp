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
        public static Obj_AI_Hero Self = ObjectManager.Player;

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
    }
}
