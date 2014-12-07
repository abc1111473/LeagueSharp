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
    public class SC
    {
        private static int WallCastTime;
        private static Vector2 YasuoWallCastedPosition;

        //public static Vector2 YasuoWallCollisionPosition()
        //{
        //    var from = Math.Max(0, Environment.TickCount - WallCastTime - SpellData);

        //    from = (int)Math.Max(0, Math.Min(End.Distance(Start), from * SpellData.MissileSpeed / 1000));
        //    return Start + Direction * from;

        //    if (!STS.IsThereYasuoOnTheEnemyTeam)
        //    {
        //        GameObject wall = null;
        //        foreach (var gameObject in ObjectManager.Get<GameObject>())
        //        {
        //            if (gameObject.IsValid && System.Text.RegularExpressions.Regex.IsMatch(gameObject.Name, "_w_windwall.\\.troy", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        //            {
        //                wall = gameObject;
        //            }
        //        }
        //        if (wall == null)
        //        {
        //            return new Vector2();
        //        }
        //        var level = wall.Name.Substring(wall.Name.Length - 6, 1);
        //        var wallWidth = (300 + 50 * Convert.ToInt32(level));
        //        var wallDirection = (wall.Position.To2D() - YasuoWallCastedPosition).Normalized().Perpendicular();
        //        var wallStart = wall.Position.To2D() + wallWidth / 2 * wallDirection;
        //        var wallEnd = wallStart - wallWidth * wallDirection;
        //        var wallPolygon = new Geometry.Rectangle(wallStart, wallEnd, 75).ToPolygon();
        //        var intersection = new Vector2();
        //        var intersections = new List<Vector2>();
        //        for (var i = 0; i < wallPolygon.Points.Count; i++)
        //        {
        //            var inter = wallPolygon.Points[i].Intersection( wallPolygon.Points[i != wallPolygon.Points.Count - 1 ? i + 1 : 0], from, skillshot.End);
        //            if (inter.Intersects)
        //            {
        //                intersections.Add(inter.Point);
        //            }
        //        }
        //        if (intersections.Count > 0)
        //        {
        //            intersection = intersections.OrderBy(item => item.Distance(from)).ToList()[0];
        //            var collisionT = Environment.TickCount + Math.Max(0, skillshot.SpellData.Delay - (Environment.TickCount - skillshot.StartTick)) + 100 + (1000 * intersection.Distance(from)) / skillshot.SpellData.MissileSpeed;
        //            if (collisionT - WallCastTime < 4000)
        //            {
        //                if (skillshot.SpellData.Type != SkillShotType.SkillshotMissileLine)
        //                {
        //                    skillshot.ForceDisabled = true;
        //                }
        //                return intersection;
        //            }
        //        }
        //    }
        //}
    }
}
