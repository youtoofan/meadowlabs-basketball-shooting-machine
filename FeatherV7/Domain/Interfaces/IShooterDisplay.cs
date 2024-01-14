using System;

namespace FeatherV7.Domain.Interfaces
{
    internal interface IShooterDisplay
    {
        void Init();
        void ShowState(string state);
        void ShowCurrentTimeScreen();
        void ShowBoom();
        void ShowCancel();
        void ShowCountDownSequenceScreen(int second);
        void ShowRotatorScreen(TimeSpan duration);
    }
}
