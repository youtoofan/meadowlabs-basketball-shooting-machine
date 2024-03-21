using FeatherV7.Domain.Interfaces;
using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using Timer = System.Timers.Timer;

namespace DisplayTest.Domain.Models
{

    internal class Display : DisplayScreen, IShooterDisplay
    {
        private readonly IApp app;
        private readonly PeriodicTimer timer;
        private readonly Color[] colors = new Color[4]
        {
                Color.FromHex("#ffa64d"),
                Color.FromHex("#ff9933"),
                Color.FromHex("#ff8c1a"),
                Color.FromHex("#ff8000")
        };
        private Label clockLabel;
        private Label statusLabel;

        public Display(IApp app, IPixelDisplay physicalDisplay, RotationType rotation = RotationType.Default, ITouchScreen touchScreen = null, DisplayTheme theme = null)
            : base(physicalDisplay, rotation, touchScreen, theme)
        {
            this.app = app;
            this.timer = new Timer(1000);
            this.BackgroundColor = colors[0];
        }

        public void Init()
        {
            this.Controls.Add(new Circle(
                centerX: this.Width / 2,
                centerY: this.Height / 2,
                radius: this.Height / 2)
            {
                ForeColor = colors[3],
                IsFilled = true
            });

            this.Controls.Add(new Label(
                left: 20,
                top: 20,
                width: this.Width - 40,
                height: this.Height - 40)
            {
                Text = "SHOOTING MACHINE V1",
                TextColor = Color.Black,
                BackColor = Color.Transparent,
                Font = new Font12x20(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });

            clockLabel = new Label(
                left: 20,
                top: -20,
                width: this.Width - 40,
                height: this.Height - 40)
            {
                Text = "",
                TextColor = Color.Black,
                BackColor = Color.Transparent,
                Font = new Font8x12(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            statusLabel = new Label(
                left: 20,
                top: 50,
                width: this.Width - 40,
                height: this.Height - 40)
            {
                Text = "",
                TextColor = Color.Black,
                BackColor = Color.Transparent,
                Font = new Font12x16(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            this.Controls.Add(statusLabel);
            this.Controls.Add(clockLabel);

            this.timer.Elapsed += (s, e) => ShowCurrentTimeScreen();
            this.timer.Enabled = true;
        }

        public void ShowBoom()
        {
            if (statusLabel == null)
                return;

            app.InvokeOnMainThread((o) =>
            {
                statusLabel.Text = "BOOOOOM!";
            });
        }

        public void ShowCancel()
        {
            if (statusLabel == null)
                return;

            app.InvokeOnMainThread((o) =>
            {
                statusLabel.Text = $"CANCELLED!";
            });
        }

        public void ShowCountDownSequenceScreen(int second)
        {
            if (statusLabel == null)
                return;

            app.InvokeOnMainThread((o) =>
            {
                statusLabel.Text = second.ToString();
            });
        }

        public void ShowCurrentTimeScreen()
        {
            if (clockLabel == null)
                return;

            app.InvokeOnMainThread((o) =>
            {
                clockLabel.Text = $"{DateTime.Now:T}";
            });
        }

        public void ShowRotatorScreen(TimeSpan duration)
        {
            if (statusLabel == null)
                return;

            app.InvokeOnMainThread((o) =>
            {
                statusLabel.Text = $"{duration.TotalSeconds} seconds";
            });
        }

        public void ShowState(string state)
        {
            if (statusLabel == null)
                return;

            app.InvokeOnMainThread((o) =>
            {
                statusLabel.Text = state;
            });
        }
    }
}
