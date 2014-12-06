using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SimpleLib;

namespace SimpleOrbWalker_Standalone
{
    class SOW_Standalone
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        public static void Game_OnGameLoad(EventArgs args)
        {
            var menu = new Menu("SimpleLib", "SimpleLib", true);

            var menuSTS = new Menu("Simple Target Selector", "STS");
            STS.InitializeSTS(menuSTS);
            menu.AddSubMenu(menuSTS);

            var orbwalkerMenu = new Menu("SimpleOrbWalker", "SimpleOrbWalker");
            SOW.InitializeOrbwalker(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);

            menu.AddToMainMenu();
        }
    }
}