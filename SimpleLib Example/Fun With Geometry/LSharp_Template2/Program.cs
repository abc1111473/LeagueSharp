using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using SimpleLib;

namespace ConsoleApplication1
{
    public class Program
    {
        public static Vector3 TestVector3 = Vector3.Zero;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static bool drw;
        public static SimpleGeometry.Polygon Miss;
        public static SimpleGeometry.Rectangle testRect;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

            testRect = new SimpleGeometry.Rectangle(new Vector2(0, 0), new Vector2(500, 500), 100);

        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN) return;
            drw = true;
        }

        public static float GetAngle(int t)
        {
            float angle = 6 * t;
            return angle.DegreeToRadian();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if(!drw) return;

            var c = new SimpleGeometry.Circle(Game.CursorPos.To2D(), 50);
            SimpleGeometry.Draw.DrawCircle(c, Color.Red, 3);

            SimpleGeometry.Draw.DrawLine(Player.Position, Game.CursorPos, Color.Blue);

            var r = new SimpleGeometry.Ring(Player.Position.To2D(), 600, 700);
            SimpleGeometry.Draw.DrawRing(r, Color.Black, 3);

            var rect = new SimpleGeometry.Rectangle(Player.Position.To2D(), Game.CursorPos.To2D(), 100);
            SimpleGeometry.Draw.DrawRectangle(rect, Color.Green, 3);

            var alpha = (25 * 360) / Player.Distance(Game.CursorPos) * Math.PI;

            var s = new SimpleGeometry.Sector(Player.Position.To2D(), Game.CursorPos.To2D(), (float)(alpha.DegreeToRadian()), 1000);
            SimpleGeometry.Draw.DrawSector(s, Color.White, 3);

            var l = new SimpleGeometry.Line(Player.Position.To2D(), Game.CursorPos.To2D(), 700);
            SimpleGeometry.Draw.DrawLine(l, Color.Yellow, 3);

            var playerPosition = Player.Position;
            var targetPosition = Game.CursorPos;

            var extend = 1000 - Player.Distance(Game.CursorPos);
            var lastPos = Player.Position.Extend(targetPosition, targetPosition.Distance(playerPosition) + extend);
            var marginWidth = 50 + 150 - 5f;

            //Finde left margin
            double x1 = playerPosition.X - targetPosition.X;
            double y1 = playerPosition.Y - targetPosition.Y;
            double dist = Math.Sqrt(x1 * x1 + y1 * y1);
            x1 /= dist;
            y1 /= dist;
            double endPX1 = targetPosition.X + (marginWidth / 2) * y1;
            double endPY1 = targetPosition.Y - (marginWidth / 2) * x1;
            var leftPerpPoint = new Vector3((float)endPX1, (float)endPY1, lastPos.Z);

            var leftMargin = playerPosition.Extend(leftPerpPoint, leftPerpPoint.Distance(playerPosition) + (1000 - leftPerpPoint.Distance(playerPosition)));

            //Finde right margin
            double x2 = playerPosition.X - targetPosition.X;
            double y2 = playerPosition.Y - targetPosition.Y;
            double dist2 = Math.Sqrt(x2 * x2 + y2 * y2);
            x2 /= dist2;
            y2 /= dist2;
            double endPX2 = targetPosition.X - (marginWidth / 2) * y2;
            double endPY2 = targetPosition.Y + (marginWidth / 2) * x2;
            var rightPerpPoint = new Vector3((float)endPX2, (float)endPY2, lastPos.Z);

            var rightMargin = playerPosition.Extend(rightPerpPoint, rightPerpPoint.Distance(playerPosition) + (1000 - rightPerpPoint.Distance(playerPosition)));

            var p = new SimpleGeometry.Polygon();
            p.Add(Player.Position.To2D());
            p.Add(leftPerpPoint.To2D());
            p.Add(lastPos.To2D());
            p.Add(rightPerpPoint.To2D());
            SimpleGeometry.Draw.DrawPolygon(p, Color.Yellow);

            var seconds = DateTime.Now.Second;

            var test = new SimpleGeometry.Polygon();
            var start = new Vector2(0,800);
            var testa = new SimpleGeometry.Arc(new Vector2(0, 0), new Vector2(0, 100), 1f, 1000f);
            test.Add(start);
            test.Add(testa.ToPolygon());

            var miss = test.MovePolygone(Game.CursorPos.To2D());
            miss = miss.RotatePolygon(Game.CursorPos.To2D(), Player.Position.To2D());

            SimpleGeometry.Draw.DrawPolygon(miss, Color.Blue, 3);

            var test1 = testRect.ToPolygon().MovePolygone(Game.CursorPos.To2D());
            test1 = test1.RotatePolygon(Game.CursorPos.To2D() ,-GetAngle(seconds));
            SimpleGeometry.Draw.DrawPolygon(test1, Color.Pink, 3);
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            
        }
    }
}
