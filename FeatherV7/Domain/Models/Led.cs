using FeatherV7.Domain.Interfaces;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;

namespace DisplayTest.Domain.Models
{

    internal class Led : RgbPwmLed, IShooterLed
    {
        public Led(IPin redPwmPin, IPin greenPwmPin, IPin bluePwmPin, CommonType commonType = CommonType.CommonCathode) 
            : base(redPwmPin, greenPwmPin, bluePwmPin, commonType)
        {
        }

        public void ShowError()
        {
            this.SetColor(Color.Red);
        }

        public void ShowBall()
        {
            this.SetColor(Color.Yellow);
        }

        public void ShowLaunching()
        {
            this.SetColor(Color.Green);
        }

        public void ShowNoBall()
        {
            this.SetColor(Color.Blue);
        }

        public void ShowReady()
        {
            this.SetColor(Color.White);
        }

        public void ShowBooting()
        {
            this.SetColor(Color.LightPink);
        }

        public void StartPulse(Color color)
        {
            this.StartPulse(color);
        }
    }
}
