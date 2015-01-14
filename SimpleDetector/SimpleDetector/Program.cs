using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color; 

namespace SimpleDetector
{
    public static class Program
    {
        private static Menu _mainMenu;

        private static bool _scan;
        private static int _size;

        private static List<Geometry.Polygon> _polygoneList; 

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            _scan = false;
            _size = 500;
            _polygoneList = new List<Geometry.Polygon>();

            _mainMenu = new Menu("Simple Detector", "SimpleDetector", true);

            _mainMenu.AddItem(new MenuItem("Enable", "Enable Scan").SetValue(true));
            _mainMenu.AddItem(new MenuItem("draw", "Drawing").SetValue(new Circle(true, Color.Cyan)));

            _mainMenu.AddItem(new MenuItem("Lable1", "Key Binds"));
            _mainMenu.AddItem(new MenuItem("scan", "Scan").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
            _mainMenu.AddItem(new MenuItem("clip", "Merge Polygons").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Press)));
            _mainMenu.AddItem(new MenuItem("zoomIn", "Zoom In").SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Press)));
            _mainMenu.AddItem(new MenuItem("zoomOut", "Zoom Out").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Press)));

            _mainMenu.AddItem(new MenuItem("Lable2", "Scan Objects"));
            _mainMenu.AddItem(new MenuItem("building", "Buildings").SetValue(true));
            _mainMenu.AddItem(new MenuItem("grass", "Bushes").SetValue(true));
            _mainMenu.AddItem(new MenuItem("wall", "Walls").SetValue(true));

            _mainMenu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (_mainMenu.Item("zoomIn").GetValue<KeyBind>().Active)
                _size -= 5;

            if (_mainMenu.Item("zoomOut").GetValue<KeyBind>().Active)
                _size += 5;

            if (!_mainMenu.Item("scan").GetValue<KeyBind>().Active)
            {
                _scan = true;
            }

            if (_mainMenu.Item("Enable").GetValue<bool>() && _mainMenu.Item("scan").GetValue<KeyBind>().Active && _scan)
            {
                var currentPostion = Game.CursorPos.To2D();

                var tempList = new List<Geometry.Polygon>();

                var rect = new Geometry.Polygon.Rectangle(new Vector2(0, 0), new Vector2(0, 5), 5f);

                for (int x = -_size; x < _size; x += 5)
                {
                    for (int y = -_size; y < _size; y += 5)
                    {
                        var test = rect.MovePolygone(new Vector2(currentPostion.X + x, currentPostion.Y + y));

                        if (NavMesh.GetCollisionFlags(test.Points[0].To3D()).HasFlag(CollisionFlags.Building) && _mainMenu.Item("building").GetValue<bool>())
                        {
                            tempList.Add(test);
                        }

                        if (NavMesh.GetCollisionFlags(test.Points[0].To3D()).HasFlag(CollisionFlags.Grass) && _mainMenu.Item("grass").GetValue<bool>())
                        {
                            tempList.Add(test);
                        }

                        if (NavMesh.GetCollisionFlags(test.Points[0].To3D()).HasFlag(CollisionFlags.Wall) && _mainMenu.Item("wall").GetValue<bool>())
                        {
                            tempList.Add(test);
                        }
                    }
                }

                var temp = Geometry.ClipPolygons(tempList);
                temp.TrimExcess();

                foreach (var polygone in temp)
                {
                    _polygoneList.Add(polygone.ToPolygon());
                }
                _scan = false;
            }

            if (_mainMenu.Item("clip").GetValue<KeyBind>().Active)
            {
                _polygoneList = _polygoneList.JoinPolygons();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var draw = _mainMenu.Item("draw").GetValue<Circle>();

            if(!draw.Active) return;

            var detectorRect = new Geometry.Polygon.Rectangle(
                new Vector2(Game.CursorPos.X, Game.CursorPos.Y - _size),
                new Vector2(Game.CursorPos.X, Game.CursorPos.Y + _size), _size);

            if (_mainMenu.Item("Enable").GetValue<bool>())
                detectorRect.Draw(draw.Color);

            foreach (var polygone in _polygoneList)
            {
                polygone.Draw(draw.Color);
            }
        }
    }
}
