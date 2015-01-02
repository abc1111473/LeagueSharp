using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;


namespace SimpleLib
{
    public class LevelUpManager
    {
        private static int[] _spellPriorityList;
        private static int _lastLevel = 0;
        private static Dictionary<string, int[]> SpellPriorityList = new Dictionary<string, int[]>();
        private static Menu _lMenu;
        private static int _selectedPriority;

        public void AddToMenu(Menu menu)
        {
            _lMenu = menu;
            if (SpellPriorityList.Count > 0)
            {
                _lMenu.AddItem(new MenuItem(ObjectManager.Player.ChampionName, "Enable").SetValue(true));
                _lMenu.AddItem(
                    new MenuItem(ObjectManager.Player.ChampionName, "").SetValue(
                        new StringList(SpellPriorityList.Keys.ToArray())));
                _selectedPriority =
                    _lMenu.Item(ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
            }
        }

        public void Add(string spellPriorityDesc, int[] spellPriority)
        {
            SpellPriorityList.Add(spellPriorityDesc, spellPriority);
        }

        public void Update()
        {
            if (SpellPriorityList.Count == 0 || !_lMenu.Item(ObjectManager.Player.ChampionName).GetValue<bool>() ||
                _lastLevel == ObjectManager.Player.Level)
            {
                return;
            }

            _spellPriorityList = SpellPriorityList.Values.ElementAt(_selectedPriority);

            var qL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level;
            var wL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
            var eL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level;
            var rL = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level;

            if (qL + wL + eL + rL >= ObjectManager.Player.Level)
            {
                return;
            }

            var level = new[] { 0, 0, 0, 0 };

            for (var i = 0; i < ObjectManager.Player.Level; i++)
            {
                level[_spellPriorityList[i] - 1] = level[_spellPriorityList[i] - 1] + 1;
            }

            if (qL < level[0])
            {
                ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.Q);
            }
            if (wL < level[1])
            {
                ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.W);
            }
            if (eL < level[2])
            {
                ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.E);
            }
            if (rL < level[3])
            {
                ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.R);
            }
        }
    }
}
