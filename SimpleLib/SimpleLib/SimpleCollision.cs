using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace SimpleLib
{
    class SimpleCollision
    {

        public static bool YasuoWallCollision(float range, AttackableUnit target)
        {
            return
                Collision.GetCollision(
                    new List<Vector3> { ObjectManager.Player.Position, target.Position },
                    new PredictionInput
                    {
                        Range = range,
                        Radius = ObjectManager.Player.AttackRange,
                        Delay = (int)ObjectManager.Player.AttackDelay * 1000,
                        Speed = (int)ObjectManager.Player.BasicAttack.MissileSpeed
                    }).Count == 0;
        }
    }
}
