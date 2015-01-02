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

            var AllEnemys = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget(spell.Range));

            var playerPosition = Player.Position;
            var targetPosition = target.Position;

            var extend = spell.Range - Player.Distance(target);
            var lastPos = Player.Position.Extend(targetPosition, targetPosition.Distance(playerPosition) + extend);
            var marginWidth = target.BoundingRadius + spell.Width - 5f;

            //Finde left margin
            double x1 = playerPosition.X - targetPosition.X;
            double y1 = playerPosition.Y - targetPosition.Y;
            double dist = Math.Sqrt(x1 * x1 + y1 * y1);
            x1 /= dist;
            y1 /= dist;
            double endPX1 = targetPosition.X + (marginWidth / 2) * y1;
            double endPY1 = targetPosition.Y - (marginWidth / 2) * x1;
            var leftPerpPoint = new Vector3((float)endPX1, (float)endPY1, lastPos.Z);

            var leftMargin = playerPosition.Extend(leftPerpPoint, leftPerpPoint.Distance(playerPosition) + (spell.Range - leftPerpPoint.Distance(playerPosition)));

            //Finde right margin
            double x2 = playerPosition.X - targetPosition.X;
            double y2 = playerPosition.Y - targetPosition.Y;
            double dist2 = Math.Sqrt(x2 * x2 + y2 * y2);
            x2 /= dist2;
            y2 /= dist2;
            double endPX2 = targetPosition.X - (marginWidth / 2) * y2;
            double endPY2 = targetPosition.Y + (marginWidth / 2) * x2;
            var rightPerpPoint = new Vector3((float)endPX2, (float)endPY2, lastPos.Z);

            var rightMargin = playerPosition.Extend(rightPerpPoint, rightPerpPoint.Distance(playerPosition) + (spell.Range - rightPerpPoint.Distance(playerPosition)));
            
            var Points = new List<Vector2>()
            {
                leftMargin.To2D(),
                lastPos.To2D(),
                rightMargin.To2D(),
                playerPosition.To2D()
            };

            foreach (var enemy in AllEnemys)
            {
                
            }


            return Vector3.Zero;
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
