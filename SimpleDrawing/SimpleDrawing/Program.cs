using System;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;


namespace SimpleDrawing
{
    public class Program
    {
        public static Menu MainMenu;

        public static Geometry.Polygon LineOne;
        public static Geometry.Polygon LineTwo;
        public static Geometry.Polygon LineThree;
        public static Geometry.Polygon LineFour;
        public static Geometry.Polygon LineFive;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            LineOne = new Geometry.Polygon();
            LineTwo = new Geometry.Polygon();
            LineThree = new Geometry.Polygon();
            LineFour = new Geometry.Polygon();
            LineFive = new Geometry.Polygon();

            MainMenu = new Menu("Simple Drawing", "SimpleDrawing", true);

            MainMenu.AddItem(new MenuItem("Enable", "Enable Drawing").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

            MainMenu.AddItem(new MenuItem("Line", "Chose Line").SetValue(new StringList(new[] { "Line One", "Line Two", "Line Three", "Line Four", "Line Five" })));

            MainMenu.AddItem(new MenuItem("LineOne", "Line One").SetValue(new Circle(true, Color.Black)));
            MainMenu.AddItem(new MenuItem("LineOneClosed", "Line One Closed").SetValue(true));

            MainMenu.AddItem(new MenuItem("LineTwo", "Line Two").SetValue(new Circle(true, Color.Red)));
            MainMenu.AddItem(new MenuItem("LineTwoClosed", "Line Two Closed").SetValue(true));

            MainMenu.AddItem(new MenuItem("LineThree", "Line Three").SetValue(new Circle(true, Color.Blue)));
            MainMenu.AddItem(new MenuItem("LineThreeClosed", "Line Three Closed").SetValue(true));

            MainMenu.AddItem(new MenuItem("LineFour", "Line Four").SetValue(new Circle(true, Color.Green)));
            MainMenu.AddItem(new MenuItem("LineFourClosed", "Line Four Closed").SetValue(true));

            MainMenu.AddItem(new MenuItem("LineFive", "Line Five").SetValue(new Circle(true, Color.Yellow)));
            MainMenu.AddItem(new MenuItem("LineFiveClosed", "Line Five Closed").SetValue(true));

            MainMenu.AddToMainMenu();
            
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN)
            {
                if(!MainMenu.Item("Enable").GetValue<KeyBind>().Active) return;

                int currentIndex = MainMenu.Item("Line").GetValue<StringList>().SelectedIndex;

                switch (currentIndex)
                {
                    case 0:
                        LineOne.Add(Game.CursorPos.To2D());
                        break;
                    case 1:
                        LineTwo.Add(Game.CursorPos.To2D());
                        break;
                    case 2:
                        LineThree.Add(Game.CursorPos.To2D());
                        break;
                    case 3:
                        LineFour.Add(Game.CursorPos.To2D());
                        break;
                    case 4:
                        LineFive.Add(Game.CursorPos.To2D());
                        break;
                }
            }

            if (args.Msg == (uint) WindowsMessages.WM_RBUTTONDOWN)
            {
                if (!MainMenu.Item("Enable").GetValue<KeyBind>().Active) return;

                int currentIndex = MainMenu.Item("Line").GetValue<StringList>().SelectedIndex;

                switch (currentIndex)
                {
                    case 0:
                        LineOne.Points.RemoveAt(LineOne.Points.Count - 1);
                        break;
                    case 1:
                        LineTwo.Points.RemoveAt(LineOne.Points.Count - 1);
                        break;
                    case 2:
                        LineThree.Points.RemoveAt(LineOne.Points.Count - 1);
                        break;
                    case 3:
                        LineFour.Points.RemoveAt(LineOne.Points.Count - 1);
                        break;
                    case 4:
                        LineFive.Points.RemoveAt(LineOne.Points.Count - 1);
                        break;
                }
            }
        }

        public static void DrawOpenedPolygone(Geometry.Polygon p, Color c, int width = 1)
        {
            for (int i = 0; i < p.Points.Count; i++)
            {
                var f = Drawing.WorldToScreen(p.Points[i].To3D());
                var t = Drawing.WorldToScreen(p.Points[i + 1].To3D());
                Drawing.DrawLine(f, t, width, c);
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {

            var line1 = MainMenu.Item("LineOne").GetValue<Circle>();
            var line2 = MainMenu.Item("LineTwo").GetValue<Circle>();
            var line3 = MainMenu.Item("LineThree").GetValue<Circle>();
            var line4 = MainMenu.Item("LineFour").GetValue<Circle>();
            var line5 = MainMenu.Item("LineFive").GetValue<Circle>();

            try
            {
                if (line1.Active)
                {
                    if (MainMenu.Item("LineOneClosed").GetValue<bool>())
                    {
                        LineOne.Draw(line1.Color);
                    }
                    else
                    {
                        DrawOpenedPolygone(LineOne, line1.Color);
                    }
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                if (line2.Active)
                {
                    if (MainMenu.Item("LineTwoClosed").GetValue<bool>())
                    {
                        LineTwo.Draw(line2.Color);
                    }
                    else
                    {
                        DrawOpenedPolygone(LineTwo, line2.Color);
                    }
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                if (line3.Active)
                {
                    if (MainMenu.Item("LineThreeClosed").GetValue<bool>())
                    {
                        LineThree.Draw(line3.Color);
                    }
                    else
                    {
                        DrawOpenedPolygone(LineThree, line3.Color);
                    }
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                if (line4.Active)
                {
                    if (MainMenu.Item("LineFourClosed").GetValue<bool>())
                    {
                        LineFour.Draw(line4.Color);
                    }
                    else
                    {
                        DrawOpenedPolygone(LineFour, line4.Color);
                    }
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                if (line5.Active)
                {
                    if (MainMenu.Item("LineFiveClosed").GetValue<bool>())
                    {
                        LineFive.Draw(line5.Color);
                    }
                    else
                    {
                        DrawOpenedPolygone(LineFive, line5.Color);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
