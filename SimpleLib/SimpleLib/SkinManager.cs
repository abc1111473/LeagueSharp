using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;


namespace SimpleLib
{
    public class SkinManager
    {
        private static List<string> _skins = new List<string>();
        private static Menu _sMenu;
        private static int _selectedSkin;
        private static bool _initialize = true;

        public void AddToMenu(Menu menu)
        {
            _sMenu = menu;

            if (_skins.Count <= 0)
            {
                return;
            }

            _sMenu.AddItem(new MenuItem(ObjectManager.Player.ChampionName, "Enable Skin Changer").SetValue(true));

            _sMenu.AddItem(
                new MenuItem(ObjectManager.Player.ChampionName, "Skins").SetValue(
                    new StringList(_skins.ToArray())));
            _selectedSkin = _sMenu.Item(ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
        }

        public void AddSkin(string skin)
        {
            _skins.Add(skin);
        }

        public void Update()
        {
            if (!_sMenu.Item(ObjectManager.Player.ChampionName).GetValue<bool>()) return;

            var skin = _sMenu.Item(ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;

            if (!_initialize && skin == _selectedSkin) return;

            GenerateSkinPacket(skin);
            _selectedSkin = skin;
            _initialize = false;
        }

        private void GenerateSkinPacket(int skinNumber)
        {
            GamePacket model =
                 Packet.S2C.UpdateModel.Encoded(
                    new Packet.S2C.UpdateModel.Struct(
                        ObjectManager.Player.NetworkId, skinNumber, ObjectManager.Player.ChampionName));
            model.Process();
        }
    }
}
