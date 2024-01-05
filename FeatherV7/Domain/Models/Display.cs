using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using System;
using Timer = System.Timers.Timer;

namespace DisplayTest.Domain.Models
{
    internal interface IShooterDisplay
    {
        void Init();
        void ShowCurrentTimeScreen();
        void ShowBoom();
        void ShowCancel();
        void ShowCountDownSequenceScreen(int second);
        void ShowRotatorScreen(TimeSpan duration);
    }

    internal class Display : MicroGraphics, IShooterDisplay
    {
        private readonly Timer _timer;
        private readonly Color[] colors = new Color[4]
        {
            Color.FromHex("#ffa64d"),
            Color.FromHex("#ff9933"),
            Color.FromHex("#ff8c1a"),
            Color.FromHex("#ff8000")
        };

        public Display(IGraphicsDisplay display) 
            : base(display)
        {
            _timer = new Timer();
            IgnoreOutOfBoundsPixels = true;
            Rotation = RotationType._270Degrees;
        }

        public void Init()
        {
            this.Clear(true);

            int radius = 225;
            int originX = this.Width / 2;
            int originY = this.Height / 2 + 100;

            this.Stroke = 3;
            for (int i = 1; i < colors.Length; i++)
            {
                this.DrawCircle
                (
                    centerX: originX,
                    centerY: originY,
                    radius: radius,
                    color: colors[i - 1],
                    filled: true
                );

                radius -= 20;
            }

            this.DrawLine(0, 220, 239, 220, Color.Black);
            this.DrawLine(0, 230, 239, 230, Color.Black);

            this.CurrentFont = new Font12x20();
            this.DrawText(5, 130, "SHOOTING MACHINE V1", Color.White);

            this.Show();
            
            _timer.Elapsed += (s, e) => ShowCurrentTimeScreen();
            _timer.Interval = 1000;

            _timer.Enabled = true;
        }

        public void ShowCurrentTimeScreen()
        {
            _timer.Enabled = true;

            this.DrawRectangle(
                       x: 0, y: 160,
                       width: 500,
                       height: 50,
                       color: colors[^1],
                       filled: true);

            this.DrawText(
                x: 20, y: 160,
                text: $"{DateTime.Now:T}",
                color: Color.White,
                scaleFactor: ScaleFactor.X2);

            this.Show();

        }

        public void ShowCountDownSequenceScreen(int second)
        {
            _timer.Enabled = false;

            if (second <= 0)
                return;

            this.DrawRectangle(
                x: 0, y: 160,
                width: 500,
                height: 50,
                color: colors[^1],
                filled: true);

            this.DrawText(
                x: 20, y: 160,
                text: $"{second} sec...",
                color: Color.White,
                scaleFactor: ScaleFactor.X2);

            this.Show();

        }

        public void ShowBoom()
        {
            _timer.Enabled = false;

            this.DrawRectangle(
                x: 0, y: 160,
                width: 500,
                height: 50,
                color: colors[^1],
                filled: true);

            this.DrawText(
                x: 20, y: 160,
                text: $"BOOM!",
                color: Color.Red,
                scaleFactor: ScaleFactor.X3);

            this.Show();
        }

        public void ShowCancel()
        {
            _timer.Enabled = false;

            this.DrawRectangle(
                x: 0, y: 160,
                width: 500,
                height: 50,
                color: colors[^1],
                filled: true);

            this.DrawText(
                x: 20, y: 160,
                text: $"CANCELLED!",
                color: Color.LightBlue,
                scaleFactor: ScaleFactor.X3);

            this.Show();
        }

        public void ShowRotatorScreen(TimeSpan duration)
        {
            _timer.Enabled = false;

            this.DrawRectangle(
                       x: 0, y: 160,
                       width: 500,
                       height: 50,
                       color: colors[^1],
                       filled: true);

            this.DrawText(
                x: 20, y: 160,
                text: $"{duration.TotalSeconds} seconds",
                color: Color.White,
                scaleFactor: ScaleFactor.X2);

            this.Show();

            _timer.Enabled = true;
        }

        
    }
}
