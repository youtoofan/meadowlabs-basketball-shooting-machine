using UnitsNet;
using System;

namespace FeatherV7.Domain.Interfaces
{
    public interface IBluetoothHandler
    {
        event EventHandler<bool> WifiEnabled;
        event EventHandler<bool> BleEnabled;
        event EventHandler<bool> BlePaired;
        event EventHandler<int> RotationUpdated;
        event EventHandler<bool> ButtonClicked;
        event EventHandler<bool> Launched;

        void Initialize();
        void UpdateDistance(Length value);
        void RotationClicked(bool value);
        void LaunchTriggered(bool value);
        void UpdateRotation(TimeSpan value);
        void UpdateStatus(string status);
    }
}
