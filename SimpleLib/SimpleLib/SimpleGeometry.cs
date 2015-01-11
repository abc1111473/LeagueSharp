using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ClipperLib;
using Color = System.Drawing.Color;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;

namespace SimpleLib
{
    public static class SimpleGeometry
    {
        public static Vector3 SwitchYZ(this Vector3 v)
        {
            return new Vector3(v.X, v.Z, v.Y);
        }

        public static Vector2 SwitchXY(this Vector2 v)
        {
            return new Vector2(v.Y, v.X);
        }

        public static double DegreeToRadian(this double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static double RadianToDegree(this double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static float DegreeToRadian(this float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static float RadianToDegree(this float angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        public static Vector2 RotateAroundPoint(this Vector2 rotated, Vector2 around, float angle)
        {
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);
            
            var x = ((rotated.X - around.X) * cos) - ((around.Y - rotated.Y) * sin) + around.X;
            var y = ((around.Y - rotated.Y) * cos) + ((rotated.X - around.X) * sin) + around.Y;

            return new Vector2((float) x, (float) y);
        }

        public static Polygon RotatePolygon(this Polygon polygon,Vector2 around ,float angle)
        {
            if (polygon.Points.Count == 0)
                return polygon;

            Polygon p = new Polygon();

            foreach (var poinit in polygon.Points)
            {
                var polygonePoint = poinit.RotateAroundPoint(around, angle);
                p.Add(polygonePoint);
            }
            return p;
        }

        public static Polygon RotatePolygon(this Polygon polygon, Vector2 around, Vector2 direction)
        {
            var deltaX = around.X - direction.X;
            var deltaY = around.Y - direction.Y;
            var angle = (float) Math.Atan2(deltaY, deltaX);
            return RotatePolygon(polygon, around, angle - DegreeToRadian(90));
        }


        public static Polygon MovePolygone(this Polygon polygon, Vector2 moveTo)
        {
            if (polygon.Points.Count == 0)
                return polygon;

            Polygon p = new Polygon();

            p.Add(moveTo);

            int count = polygon.Points.Count;

            var startPoint = polygon.Points[0];

            for (int i = 1; i < count; i++)
            {
                var polygonePoint = polygon.Points[i];

                p.Add(new Vector2(moveTo.X + (polygonePoint.X - startPoint.X), moveTo.Y + (polygonePoint.Y - startPoint.Y)));
            }
            return p;
        }

        public static List<Polygon> ToPolygons(this Paths v)
        {
            var result = new List<Polygon>();
            foreach (var path in v)
            {
                result.Add(path.ToPolygon());
            }
            return result;
        }

        public static Vector2 PositionAfter(this GamePath self, int t, int speed, int delay = 0)
        {
            var distance = Math.Max(0, t - delay) * speed / 1000;
            for (var i = 0; i <= self.Count - 2; i++)
            {
                var from = self[i];
                var to = self[i + 1];
                var d = (int)to.Distance(from);
                if (d > distance)
                {
                    return from + distance * (to - from).Normalized();
                }
                distance -= d;
            }
            return self[self.Count - 1];
        }

        public static Polygon ToPolygon(this Path v)
        {
            var polygon = new Polygon();
            foreach (var point in v)
            {
                polygon.Add(new Vector2(point.X, point.Y));
            }
            return polygon;
        }

        public static Paths ClipPolygons(List<Polygon> polygons)
        {
            var subj = new Paths(polygons.Count);
            var clip = new Paths(polygons.Count);
            foreach (var polygon in polygons)
            {
                subj.Add(polygon.ToClipperPath());
                clip.Add(polygon.ToClipperPath());
            }
            var solution = new Paths();
            var c = new Clipper();
            c.AddPaths(subj, PolyType.ptSubject, true);
            c.AddPaths(clip, PolyType.ptClip, true);
            c.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive, PolyFillType.pftEvenOdd);
            return solution;
        }

        public static class Draw
        {

            public static void DrawArc(Arc arc, Color color, int width = 1)
            {
                var a = arc.ToPolygon();
                DrawPolygon(a, color, width);
            }

            public static void DrawLine(Line line, Color color, int width = 1)
            {
                var from = Drawing.WorldToScreen(line.LineStart.To3D());
                var to = Drawing.WorldToScreen(line.LineEnd.To3D());
                Drawing.DrawLine(from, to, width, color);
            }

            public static void DrawLine(Vector2 start, Vector2 end, Color color, int width = 1)
            {
                var from = Drawing.WorldToScreen(start.To3D());
                var to = Drawing.WorldToScreen(end.To3D());
                Drawing.DrawLine(from[0], from[1], to[0], to[1], width, color);
            }

            public static void DrawLine(Vector3 start, Vector3 end, Color color, int width = 1)
            {
                var from = Drawing.WorldToScreen(start);
                var to = Drawing.WorldToScreen(end);
                Drawing.DrawLine(from[0], from[1], to[0], to[1], width, color);
            }

            public static void DrawText(Vector2 position, string text, Color color)
            {
                var pos = Drawing.WorldToScreen(position.To3D());
                Drawing.DrawText(pos.X, pos.Y, color, text);
            }

            public static void DrawText(Vector3 position, string text, Color color)
            {
                var pos = Drawing.WorldToScreen(position);
                Drawing.DrawText(pos.X, pos.Y, color, text);
            }

            public static void DrawCircle(Vector3 center, float radius, Color color, int width = 1, int quality = 30, bool onMiniMap = false)
            {
                Utility.DrawCircle(center, radius, color, width, quality, onMiniMap);
            }

            public static void DrawCircle(Circle circle, Color color, int width = 1, int quality = 30, bool onMiniMap = false)
            {
                Utility.DrawCircle(circle.Center.To3D(), circle.Radius, color, width, quality, onMiniMap);
            }

            public static void DrawPolygon(Polygon polygon, Color color, int width = 1)
            {
                for (var i = 0; i <= polygon.Points.Count - 1; i++)
                {
                    var nextIndex = (polygon.Points.Count - 1 == i) ? 0 : (i + 1);
                    DrawLine(polygon.Points[i].To3D(), polygon.Points[nextIndex].To3D(), color, width);
                }
            }

            public static void DrawRectangle(Rectangle rectangle, Color color, int width = 1)
            {
                var polygone = rectangle.ToPolygon();

                DrawPolygon(polygone, color, width);
            }

            public static void DrawRectangle(Vector2 start, Vector2 end, Color color, int width = 1)
            {
                var rect = new Rectangle(start, end, width);

                DrawRectangle(rect, color, width);
            }

            public static void DrawRing(Ring ring, Color color, int width = 1, int quality = 30, bool onMiniMap = false)
            {
                DrawCircle(ring.Center.To3D(), ring.InnerRadius, color ,width, quality, onMiniMap);
                DrawCircle(ring.Center.To3D(), ring.OuterRadius, color, width, quality, onMiniMap);
            }

            public static void DrawRing(Vector3 center,
                float innerRadius,
                float outerRadius,
                Color color,
                int width = 1,
                int quality = 30,
                bool onMiniMap = false)
            {
                DrawCircle(center, innerRadius, color, width, quality, onMiniMap);
                DrawCircle(center, outerRadius, color, width, quality, onMiniMap);
            }

            public static void DrawSector(Sector sector, Color color, int width = 1)
            {
                var poly = sector.ToPolygon();
                DrawPolygon(poly, color, width);
            }
        }

        public class Arc
        {
            public Vector2 StartPos;
            public Vector2 EndPos;
            public float Angle;
            public float Radius;
            private int CircleLineSegmentNumber;

            public Arc(Vector2 start, Vector2 end, float angle, float radius, int quality = 20)
            {
                StartPos = start;
                EndPos = (end - start).Normalized();
                Angle = angle;
                Radius = radius;
                CircleLineSegmentNumber = quality;
            }

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();
                var outRadius = (Radius + offset) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentNumber);
                var Side1 = EndPos.Rotated(-Angle * 0.5f);
                for (var i = 0; i <= CircleLineSegmentNumber; i++)
                {
                    var cDirection = Side1.Rotated(i * Angle / CircleLineSegmentNumber).Normalized();
                    result.Add(new Vector2(StartPos.X + outRadius * cDirection.X, StartPos.Y + outRadius * cDirection.Y));
                }
                return result;
            }

            public Path ToClipperPath()
            {
                var poly = ToPolygon();
                var result = new Path(poly.Points.Count);

                foreach (var point in poly.Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }
                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }
        }

        public class Line
        {
            public Vector2 LineStart;
            public Vector2 LineEnd;
            public float Length;

            public Line(Vector2 start, Vector2 end, float length)
            {
                LineStart = start;
                LineEnd = (end - start).Normalized() * length + start;
                Length = length;
            }

            public void ChangeLength(float newLenght)
            {
                LineEnd = (LineEnd - LineStart).Normalized() * newLenght + LineStart;
                Length = newLenght;
            }

            public Polygon ToPolygon()
            {
                var result = new Polygon();
                result.Add(LineStart);
                result.Add(LineEnd);
                return result;
            }
        }

        public class Polygon
        {
            public List<Vector2> Points = new List<Vector2>();

            public void Add(Vector2 point)
            {
                Points.Add(point);
            }

            public void Add(Polygon polygon)
            {
                foreach (var point in polygon.Points)
                {
                    Points.Add(point);
                }
            }

            public Path ToClipperPath()
            {
                var result = new Path(Points.Count);
                foreach (var point in Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }
                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }
        }

        public class Circle
        {
            public Vector2 Center;
            public float Radius;
            private int CircleLineSegmentNumber;

            public Circle(Vector2 center, float radius, int quality = 20)
            {
                Center = center;
                Radius = radius;
                CircleLineSegmentNumber = quality;
            }

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();
                var outRadius = (overrideWidth > 0
                ? overrideWidth
                : (offset + Radius) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentNumber));
                for (var i = 1; i <= CircleLineSegmentNumber; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentNumber;
                    var point = new Vector2(
                    Center.X + outRadius * (float)Math.Cos(angle), Center.Y + outRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }
                return result;
            }

            public Path ToClipperPath()
            {
                var poly = ToPolygon();
                var result = new Path(poly.Points.Count);

                foreach (var point in poly.Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }
                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }
        }

        public class Rectangle
        {
            public Vector2 Direction;
            public Vector2 Perpendicular;
            public Vector2 REnd;
            public Vector2 RStart;
            public float Width;

            public Rectangle(Vector2 start, Vector2 end, float width)
            {
                RStart = start;
                REnd = end;
                Width = width;
                Direction = (end - start).Normalized();
                Perpendicular = Direction.Perpendicular();
            }

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();
                result.Add(
                RStart + (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular - offset * Direction);
                result.Add(
                RStart - (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular - offset * Direction);
                result.Add(
                REnd - (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular + offset * Direction);
                result.Add(
                REnd + (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular + offset * Direction);
                return result;
            }

            public Path ToClipperPath()
            {
                var poly = ToPolygon();
                var result = new Path(poly.Points.Count);

                foreach (var point in poly.Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }
                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }
        }

        public class Ring
        {
            public Vector2 Center;
            public float InnerRadius;
            public float OuterRadius;
            private int CircleLineSegmentNumber;

            public Ring(Vector2 center, float innerRadius, float outerRadius, int quality = 20)
            {
                Center = center;
                InnerRadius = innerRadius;
                OuterRadius = outerRadius;
                CircleLineSegmentNumber = quality;
            }

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();
                var outRadius = (offset + InnerRadius + OuterRadius) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentNumber);
                var innerRadius = InnerRadius - OuterRadius - offset;
                for (var i = 0; i <= CircleLineSegmentNumber; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentNumber;
                    var point = new Vector2(
                    Center.X - outRadius * (float)Math.Cos(angle), Center.Y - outRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }
                for (var i = 0; i <= CircleLineSegmentNumber; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentNumber;
                    var point = new Vector2(
                    Center.X + innerRadius * (float)Math.Cos(angle),
                    Center.Y - innerRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }
                return result;
            }

            public Path ToClipperPath()
            {
                var poly = ToPolygon();
                var result = new Path(poly.Points.Count);

                foreach (var point in poly.Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }
                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }
        }

        public class Sector
        {
            public float Angle;
            public Vector2 Center;
            public Vector2 Direction;
            public float Radius;
            private int CircleLineSegmentNumber;

            public Sector(Vector2 center, Vector2 direction, float angle, float radius, int quality = 20)
            {
                Center = center;
                Direction = (direction - center).Normalized();
                Angle = angle;
                Radius = radius;
                CircleLineSegmentNumber = quality;
            }

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();
                var outRadius = (Radius + offset) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentNumber);
                result.Add(Center);
                var Side1 = Direction.Rotated(-Angle * 0.5f);
                for (var i = 0; i <= CircleLineSegmentNumber; i++)
                {
                    var cDirection = Side1.Rotated(i * Angle / CircleLineSegmentNumber).Normalized();
                    result.Add(new Vector2(Center.X + outRadius * cDirection.X, Center.Y + outRadius * cDirection.Y));
                }
                return result;
            }

            public Path ToClipperPath()
            {
                var poly = ToPolygon();
                var result = new Path(poly.Points.Count);

                foreach (var point in poly.Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }
                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }
        }
    }
}
