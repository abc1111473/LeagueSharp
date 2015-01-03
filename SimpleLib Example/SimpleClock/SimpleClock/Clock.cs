using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SimpleLib;
using Color = System.Drawing.Color;

namespace SimpleClock
{
    public class Clock
    {
        private static Menu mainMenu;

        private static Obj_AI_Hero Player = ObjectManager.Player;

        private static bool _draw = true;

        private static Vector3 position = Vector3.Zero;

        private static int _clockSize = 500;
        private static int _seconds;
        private static int _minutes;
        private static int _hours;

        private static Color _clockColor = Color.Ivory;
        private static Color _secondHandColor = Color.Red;
        private static Color _minuteHandColor = Color.Green;
        private static Color _hourHandColor = Color.Gold;

        private static bool _clockEnabled;
        private static bool _secondHandEnabled;
        private static bool _minuteHandEnabled;
        private static bool _hourHandEnabled;

        public Clock()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        void Game_OnGameLoad(EventArgs args)
        {
            mainMenu = new Menu("Simple Clock", "SC", true);

            mainMenu.AddItem(new MenuItem("SCE", "Enable").SetValue(_draw));
            mainMenu.AddItem(new MenuItem("SCS", "Clock Size").SetValue(new Slider(_clockSize, 400, 1000)));
            mainMenu.AddItem(new MenuItem("SCC", "Clock Color").SetValue(new Circle(true, _clockColor)));
            mainMenu.AddItem(new MenuItem("SHHC", "Hour Hand Color").SetValue(new Circle(true, _hourHandColor)));
            mainMenu.AddItem(new MenuItem("SMHC", "Minute Hand Color").SetValue(new Circle(true, _minuteHandColor)));
            mainMenu.AddItem(new MenuItem("SSHC", "Second Hand Color").SetValue(new Circle(true, _secondHandColor)));

            mainMenu.AddToMainMenu();

            try
            {
                Drawing.OnDraw += Drawing_OnDraw;
            }
            catch
            {
                // ignored
            }
            Game.PrintChat("Simple Clock loaded...");
        }

        public static float GetAngle(int t)
        {
            float angle = 6 * t;
            return angle.DegreeToRadian();
        }

        public static float GetHourAngle(int t)
        {
            if (t == 0)
                t = 12;
            if (t > 12)
                t -= 12;

            float angle = 30 * t;
            return angle.DegreeToRadian();
        }

        public static void DrawAnalogClock()
        {
            var clockFrame = new SimpleGeometry.Ring(position.To2D(), _clockSize * 0.9f, _clockSize);

            var hourHandTipPosition = new Vector2(position.X, position.Y + _clockSize * 0.5f);

            var hourHandCurrentPosition = hourHandTipPosition.RotateAroundPoint(position.To2D(), -GetHourAngle(_hours));

            var minuteHandTipPosition = new Vector2(position.X, position.Y + _clockSize * 0.8f);

            var minuteHandCurrentPosition = minuteHandTipPosition.RotateAroundPoint(position.To2D(), -GetAngle(_minutes));

            var secondHandTipPosition = new Vector2(position.X, position.Y + _clockSize * 0.9f);

            var secondHandCurrentPosition = secondHandTipPosition.RotateAroundPoint(position.To2D(), -GetAngle(_seconds));


            if (_clockEnabled)
                SimpleGeometry.Draw.DrawRing(clockFrame, _clockColor, 4);

            if (_hourHandEnabled)
                SimpleGeometry.Draw.DrawLine(position.To2D(), hourHandCurrentPosition, _hourHandColor, 5);

            if (_minuteHandEnabled)
                SimpleGeometry.Draw.DrawLine(position.To2D(), minuteHandCurrentPosition, _minuteHandColor, 3);

            if (_secondHandEnabled)
                SimpleGeometry.Draw.DrawLine(position.To2D(), secondHandCurrentPosition, _secondHandColor);

        }

        void Drawing_OnDraw(EventArgs args)
        {
            try
            {
                _draw = mainMenu.Item("SCE").GetValue<bool>();
                _clockSize = mainMenu.Item("SCS").GetValue<Slider>().Value;

                _clockEnabled = mainMenu.Item("SCC").GetValue<Circle>().Active;
                _clockColor = mainMenu.Item("SCC").GetValue<Circle>().Color;

                _hourHandEnabled = mainMenu.Item("SHHC").GetValue<Circle>().Active;
                _hourHandColor = mainMenu.Item("SHHC").GetValue<Circle>().Color;

                _minuteHandEnabled = mainMenu.Item("SMHC").GetValue<Circle>().Active;
                _minuteHandColor = mainMenu.Item("SMHC").GetValue<Circle>().Color;

                _secondHandEnabled = mainMenu.Item("SSHC").GetValue<Circle>().Active;
                _secondHandColor = mainMenu.Item("SSHC").GetValue<Circle>().Color;

            }
            catch
            {
                // ignored
            }

            if (!_draw) return;

            position = Player.Position;

            _hours = DateTime.Now.Hour;
            _minutes = DateTime.Now.Minute;
            _seconds = DateTime.Now.Second;

            DrawAnalogClock();
        }
    }
}
