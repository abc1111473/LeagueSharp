using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SimpleLib
{
    public static class SimplePrediction
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static Vector3 GetPrediction(Spell spell, Obj_AI_Base target)
        {
            switch (spell.Type)
            {
                case SkillshotType.SkillshotLine:
                    return GetSkillShotLinePrediction(spell, target);

                case SkillshotType.SkillshotCircle:
                    return GetSkillShotCirclePrediction(spell, target);

                case SkillshotType.SkillshotCone:
                    return GetSkillShotConePrediction(spell, target);
            }
            return spell.GetPrediction(target).UnitPosition;
        }

        public static Vector3 GetSkillShotLinePrediction(Spell spell, Obj_AI_Base target)
        {
            if (spell.Collision)
            {
                return spell.GetPrediction(target).UnitPosition;
            }

            return new Vector3();
        }

        public static Vector3 GetSkillShotCirclePrediction(Spell spell, Obj_AI_Base target)
        {
            return Vector3.Zero;
        }

        public static Vector3 GetSkillShotConePrediction(Spell spell, Obj_AI_Base target)
        {
            return Vector3.Zero;
        }

        public static Vector3 MeleeMovmentPrediction(Obj_AI_Base target)
        {
            var predictionSpell = new Spell(SpellSlot.Unknown, (ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius));

            predictionSpell.SetTargetted(ObjectManager.Player.BasicAttack.SpellCastTime, ObjectManager.Player.BasicAttack.MissileSpeed);

            return predictionSpell.GetPrediction(target).UnitPosition;
        }

    }
}
