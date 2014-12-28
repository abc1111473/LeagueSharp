using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SimpleLib
{
    public static class SimplePrediction
    {

        public static Vector3 MeleeMovmentPrediction(Obj_AI_Base target)
        {
            var predictionSpell = new Spell(SpellSlot.Unknown, (ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius));

            predictionSpell.SetTargetted(ObjectManager.Player.BasicAttack.SpellCastTime, ObjectManager.Player.BasicAttack.MissileSpeed);

            return predictionSpell.GetPrediction(target).UnitPosition;
        }
    }
}
