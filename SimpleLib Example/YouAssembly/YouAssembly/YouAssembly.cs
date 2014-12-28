using System;
using LeagueSharp;
using LeagueSharp.Common;
using SimpleLib;

namespace YouAssemblyNameSpace
{
    class YouAssembly : SL
    {
        private static Menu menu;
        public YouAssembly()
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
            InitSimpleLib(1000); //Select range for STS
        }

        public override void Presets()
        {
            // Preset spell info.
            // Basic Spell Q W E R are included in the SimpleLib you just need to set the values
            // For more spells just add more
            //Example Q = new Spell(SpellSlot.Q, 700); 

            // Set you level priorety
            // Here are some exaples
            //var priority1 = new int[] { 1, 2, 1, 3, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
            //levelUpManager.Add("R > Q > W > E ", priority1);            
            //var priority2 = new int[] { 2, 1, 2, 3, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
            //levelUpManager.Add("R > W > Q > E ", priority2);
            //var priority3 = new int[] { 3, 1, 3, 2, 3, 4, 3, 1, 2, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
            //levelUpManager.Add("R > E > Q > W ", priority3);
            //var priority4 = new int[] { 3, 2, 3, 1, 3, 4, 3, 2, 1, 2, 4, 2, 2, 1, 1, 4, 1, 1 };
            //levelUpManager.Add("R > E > W > Q ", priority4);

            // Set you champs Skins
            // Skins need to be set in the right oreder
            // Example for annie
            //skinManager.AddSkin("Classic Annie");
            //skinManager.AddSkin("Goth Annie");
            //skinManager.AddSkin("Red Riding Annie");
            //skinManager.AddSkin("Annie in Wonderland");
            //skinManager.AddSkin("Prom Queen Annie");
            //skinManager.AddSkin("Frostfire Annie");
            //skinManager.AddSkin("Reverse Annie");
            //skinManager.AddSkin("Franken Tibbers Annie");
            //skinManager.AddSkin("Panda Annie");

            //STS.MonitorRange = 1000;
            //STS.EMR = 400;
            //STS.CurrentDamagetType = STS.DamageType.Physical;
        }

        private void OnLoad(EventArgs args)
        {
            Presets();

            menu = new Menu("You Assembly", "YouAssembly", true);
            STS.StsMenu(menu);
            SOW.SowMenu(menu);

            //Your menu comes here

            //Menu for the levelUpManager and skinManager you can add wherever you like
            //Example
            //var ExtrasMenu = new Menu("Extras", "Extras");
            //levelUpManager.AddToMenu(ExtrasMenu);
            //skinManager.AddToMenu(ExtrasMenu);
            //menu.AddSubMenu(ExtrasMenu);
            
            menu.AddToMainMenu();

            //Damage type can be set here STS.CurrentDamagetType = STS.DamageType.Hybrid;
        }

        public override void OnUpdate()
        {
            //In oreder for levelUpManager and SkinManager to work ther update must be set active here
            //levelUpManager.Update(); if you dont have any lvl prioretys set just delete this line
            //skinManager.Update(); if you dont have any skins set just delete this line
        }

        public override void OnDraw()
        {
            
        }

        public override void OnCombo()
        {
            //EnemyTarget is the current enemy target set by the STS Enemy menu
            //AllyTarget is the current ally target set by the STS Ally menu

            //You can manualu search for targets using STS
        }

        public override void OnMixed()
        {
            
        }

        public override void OnLaneClear()
        {
            
        }

        public override void OnLasthit()
        {

        }

        public override void OnLaneFreeze()
        {
           
        }

        public override void OnFlee()
        {
            
        }

        public override void OnPlayerTowerAggro(Obj_AI_Turret turret, Obj_AI_Base target)
        {
            
        }

        public override void OnEnemyTowerAggro(Obj_AI_Turret turret, Obj_AI_Base target)
        {
            
        }

        public override void OnAllyTowerAggro(Obj_AI_Turret turret, Obj_AI_Base target)
        {
            
        }
        
        public override void OnMinionTowerAggro(Obj_AI_Turret turret, Obj_AI_Base target)
        {
            
        }

        public override void OnGapClose(ActiveGapcloser gapcloser)
        {

        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {

        }

        public override void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

        }

        public override void OnProcessPacket(GamePacketEventArgs args)
        {

        }

        public override void OnSendPacket(GamePacketEventArgs args)
        {

        }
    }
}
