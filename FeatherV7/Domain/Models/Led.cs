using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;

namespace DisplayTest.Domain.Models
{
    internal interface IShooterLed
    {
        void ShowReady();
        void ShowLaunching();
        void ShowBall();
        void ShowNoBall();
        void ShowError();
        void ShowBooting();
    }

    internal class Led : RgbPwmLed, IShooterLed
    {
        public Led(IPin redPwmPin, IPin greenPwmPin, IPin bluePwmPin, CommonType commonType = CommonType.CommonCathode) 
            : base(redPwmPin, greenPwmPin, bluePwmPin, commonType)
        {
        }

        public void ShowError()
        {
            this.SetColor(Meadow.Foundation.Color.Red);
        }

        public void ShowBall()
        {
            this.SetColor(Meadow.Foundation.Color.Yellow);
        }

        public void ShowLaunching()
        {
            this.SetColor(Meadow.Foundation.Color.Green);
        }

        public void ShowNoBall()
        {
            this.SetColor(Meadow.Foundation.Color.Blue);
        }

        public void ShowReady()
        {
            this.SetColor(Meadow.Foundation.Color.White);
        }

        public void ShowBooting()
        {
            this.SetColor(Meadow.Foundation.Color.LightPink);
        }
    }
}
