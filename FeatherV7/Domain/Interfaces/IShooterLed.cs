using Meadow;
using Meadow.Foundation;

namespace FeatherV7.Domain.Interfaces
{
    internal interface IShooterLed
    {
        void ShowReady();
        void ShowLaunching();
        void ShowBall();
        void ShowNoBall();
        void ShowError();
        void ShowBooting();
        void StartPulse(Color color);
    }
}
