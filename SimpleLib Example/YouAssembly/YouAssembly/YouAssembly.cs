using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SimpleLib;

namespace YouAssemblyNameSpace
{
    class YouAssembly : SL
    {
        public YouAssembly()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            InitSL();
        }

        void Game_OnGameLoad(EventArgs args)
        {
            var menu = new Menu("You Assembly", "YouAssembly", true);

            menu.AddSubMenu(STS.STSMenu);
            menu.AddSubMenu(SOW.SOWMenu);

            //Your menu comes here

            menu.AddToMainMenu();

            SOW.InitializeOrbwalker();
            STS.InitializeSTS();
        }

        public override void OnDraw()
        {

        }

        public override void OnCombo()
        {

        }

        public override void OnHarass()
        {

        }

        public override void OnLaneClear()
        {

        }

        public override void OnLasthit()
        {

        }

        public override void OnStandby()
        {
            //SOW.Mode.None
        }

        public override void OnPassive()
        {
            //It hapens on every update regardless od the current SOW.Mode
        }

        public override void OnGapClose(ActiveGapcloser gapcloser)
        {

        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {

        }

        public override void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

        }

        public override void OnProcessPacket(GamePacketEventArgs args)
        {

        }

        public override void OnSendPacket(GamePacketEventArgs args)
        {

        }

        public override void OnBeforeAttack(SOW.BeforeAttackEventArgs args)
        {

        }

        public override void OnAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {

        }

        public override void OnAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {

        }
    }
}
