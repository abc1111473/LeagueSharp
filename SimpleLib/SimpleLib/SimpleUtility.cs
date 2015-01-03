using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SimpleLib
{
    public static class SimpleUtility
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static bool IsInsideEnemyTurrentRange(this Obj_AI_Hero target)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsEnemy && turret.Health > 0 && turret.Distance(target) <= 775);
        }

        public static bool IsInsideAllyTurrentRange(this Obj_AI_Hero target)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsAlly && turret.Health > 0 && turret.Distance(target) <= 775);
        }

        public static float TotalMagicalDamage(this Obj_AI_Hero target)
        {
            return target.BaseAbilityDamage + target.FlatMagicDamageMod;
        }

        public static float TotalAttackDamage(this Obj_AI_Hero target)
        {
            return target.BaseAttackDamage + target.FlatPhysicalDamageMod;
        }

        public static int ImmobileTime(this Obj_AI_Hero unit)
        {
            var result = 0f;

            foreach (var buff in unit.Buffs)
            {
                if (buff.IsActive && Game.Time <= buff.EndTime &&
                    (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                     buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                {
                    result = Math.Max(result, buff.EndTime);
                }
            }

            return (result == 0f) ? -1 : (int)(Environment.TickCount + (result - Game.Time) * 1000);
        }

        public static void AddLabel(this Menu menu, string name, string displayName)
        {
            menu.AddItem(new MenuItem(name, displayName));
        }

        public static void AddList(this Menu menu, string name, string displayName, string[] list)
        {
            menu.AddItem(
            new MenuItem(name, displayName).SetValue(new StringList(list)));
        }
        public static void AddBool(this Menu menu, string name, string displayName, bool value)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(value));
        }
        public static void AddSlider(this Menu menu, string name, string displayName, int value, int min, int max)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(new Slider(value, min, max)));
        }
        public static void AddObject(this Menu menu, string name, string displayName, object value)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(value));
        }

        public static void AddKeyBind(this Menu menu, string name, string displayName, char key, KeyBindType type)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(new KeyBind(key, type)));
        }

        public static void AddCircle(this Menu menu, string name, string displayName, bool enabled, System.Drawing.Color color, float radius = 100f)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(new Circle(enabled, color, radius)));
        }
    }
}
