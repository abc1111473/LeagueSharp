using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace SimpleLib
{
    public static class SimpleCollision
    {
        public static bool YasuoWallCollision(float range, AttackableUnit target)
        {
            if (
                !ObjectManager.Get<Obj_AI_Hero>()
                    .Any(enemy => enemy.ChampionName.ToLower() == "yasuo" && !enemy.IsValidTarget(range)))
            {
                return false;
            }
            
            return Collision.GetCollision(
                new List<Vector3> { ObjectManager.Player.Position, target.Position },
                new PredictionInput
                {
                    CollisionObjects = new[] { CollisionableObjects.YasuoWall },
                    //Type = SkillshotType.SkillshotLine,
                    //Range = range,
                    Radius = ObjectManager.Player.AttackRange,
                    Delay = (int) ObjectManager.Player.AttackDelay * 1000,
                    Speed = (int) ObjectManager.Player.BasicAttackMissileSpeed()
                }).Count == 1;
        }
    }
}