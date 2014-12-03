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
    public class SAM
    {

        //public class SummonerSpellManager
        //{
        //    public static SpellSlot Barrier = ObjectManager.Player.GetSpellSlot("SummonerBarrier");
        //    public static SpellSlot Clairvoyance = ObjectManager.Player.GetSpellSlot("SummonerClairvoyance");
        //    public static SpellSlot Clarity = ObjectManager.Player.GetSpellSlot("SummonerMana");
        //    public static SpellSlot Cleanse = ObjectManager.Player.GetSpellSlot("SummonerBoost");
        //    public static SpellSlot Exhaust = ObjectManager.Player.GetSpellSlot("SummonerExhaust");
        //    public static SpellSlot Flash = ObjectManager.Player.GetSpellSlot("SummonerFlash");
        //    public static SpellSlot Ghost = ObjectManager.Player.GetSpellSlot("SummonerHaste");
        //    public static SpellSlot Heal = ObjectManager.Player.GetSpellSlot("SummonerHeal");
        //    public static SpellSlot Ignite = ObjectManager.Player.GetSpellSlot("SummonerDot");
        //    public static SpellSlot Revive = ObjectManager.Player.GetSpellSlot("SummonerRevive");
        //    public static SpellSlot Smite = ObjectManager.Player.GetSpellSlot("SummonerSmite");
        //    public static SpellSlot Teleport = ObjectManager.Player.GetSpellSlot("SummonerTeleport");


 

        //    public static bool IsSummonerSpellReady(SpellSlot slot)
        //    {
        //        return ObjectManager.Player.SummonerSpellbook.CanUseSpell(slot) == SpellState.Ready;
        //    }

        //    public static bool IsSummonerSpellReady(string name)
        //    {
        //        SpellSlot slot = ObjectManager.Player.GetSpellSlot(name);                
        //        return ObjectManager.Player.SummonerSpellbook.CanUseSpell(slot) == SpellState.Ready;
        //    }
        //}

        //public class ItemManager
        //{
        //}

        public class LevelUpManager
        {
            private static int[] spellPriorityList;
            private static int lastLevel = 0;
            private static Dictionary<string, int[]> SpellPriorityList = new Dictionary<string, int[]>();
            private static Menu LMenu;
            private static int SelectedPriority;

            public static void AddToMenu(Menu menu)
            {
                LMenu = menu;
                if (SpellPriorityList.Count > 0)
                {
                    LMenu.AddItem(new MenuItem(ObjectManager.Player.ChampionName, "Enable").SetValue(true));
                    LMenu.AddItem(new MenuItem(ObjectManager.Player.ChampionName, "").SetValue(new StringList(SpellPriorityList.Keys.ToArray())));
                    SelectedPriority = LMenu.Item(ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
                }
            }
            public static void Add(string spellPriorityDesc, int[] spellPriority)
            {
                SpellPriorityList.Add(spellPriorityDesc, spellPriority);
            }
            public static void Update()
            {
                if (SpellPriorityList.Count == 0 || !LMenu.Item(ObjectManager.Player.ChampionName).GetValue<bool>() ||
                    lastLevel == ObjectManager.Player.Level)
                    return;

                spellPriorityList = SpellPriorityList.Values.ElementAt(SelectedPriority);

                int qL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level;
                int wL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
                int eL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level;
                int rL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level;

                if (qL + wL + eL + rL < ObjectManager.Player.Level)
                {
                    int[] level = new int[] { 0, 0, 0, 0 };

                    for (int i = 0; i < ObjectManager.Player.Level; i++)
                    {
                        level[spellPriorityList[i] - 1] = level[spellPriorityList[i] - 1] + 1;
                    }

                    if (qL < level[0]) ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.Q);
                    if (wL < level[1]) ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.W);
                    if (eL < level[2]) ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.E);
                    if (rL < level[3]) ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.R);
                }
            }
        }

        public class SkinManager
        {
            private static List<string> Skins = new List<string>();
            private static Menu SMenu;
            private static int SelectedSkin;
            private static bool Initialize = true;

            public static void AddToMenu(Menu menu)
            {
                SMenu = menu;
                if (Skins.Count > 0)
                {
                    SMenu.AddItem(new MenuItem(ObjectManager.Player.ChampionName, "Enable Skin Changer").SetValue(true));
                    SMenu.AddItem(new MenuItem(ObjectManager.Player.ChampionName , "Skins").SetValue(new StringList(Skins.ToArray())));
                    SelectedSkin = SMenu.Item(ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
                }
            }

            public static void AddSkin(string skin)
            {
                Skins.Add(skin);
            }

            public static void Update()
            {
                if (SMenu.Item(ObjectManager.Player.ChampionName).GetValue<bool>())
                {
                    int skin = SMenu.Item(ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
                    if (Initialize || skin != SelectedSkin)
                    {
                        GenerateSkinPacket(skin);
                        SelectedSkin = skin;
                        Initialize = false;
                    }
                }
            }

            private static void GenerateSkinPacket(int skinNumber)
            {
                int netID = ObjectManager.Player.NetworkId;
                GamePacket model = Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(ObjectManager.Player.NetworkId, skinNumber, ObjectManager.Player.ChampionName));
                model.Process(PacketChannel.S2C);
            }
        }        
    }
}
