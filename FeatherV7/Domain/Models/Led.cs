using FeatherV7.Domain.Interfaces;
using Meadow;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;

namespace FeatherV7.Domain.Models
{

    internal class Led : RgbPwmLed, IShooterLed
    {
        public Led(IPin redPwmPin, IPin greenPwmPin, IPin bluePwmPin, CommonType commonType = CommonType.CommonCathode)
            : base(redPwmPin, greenPwmPin, bluePwmPin, commonType)
        {
        }

        public void ShowError()
        {
            SetColor(Color.Red);
        }

        public void ShowBall()
        {
            SetColor(Color.Yellow);
        }

        public void ShowLaunching()
        {
            SetColor(Color.Green);
        }

        public void ShowNoBall()
        {
            SetColor(Color.Blue);
        }

        public void ShowReady()
        {
            SetColor(Color.White);
        }

        public void ShowBooting()
        {
            SetColor(Color.LightPink);
        }

        public void StartPulse(Color color)
        {
            SetColor(color);
            //this.StartPulse(color);
        }
    }
}
