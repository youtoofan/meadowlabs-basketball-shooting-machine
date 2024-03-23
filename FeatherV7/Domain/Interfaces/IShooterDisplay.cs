using System;

namespace FeatherV7.Domain.Interfaces
{
    internal interface IShooterDisplay
    {
        void Init();
        void ShowState(string state);
        void ShowCurrentTimeScreen(object state);
        void ShowBoom();
        void ShowCancel();
        void ShowCountDownSequenceScreen(int second);
        void ShowRotatorScreen(TimeSpan duration);
    }
}
