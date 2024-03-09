using CommonFeather;
using FeatherV7.Domain.Interfaces;
using FeatherV7.Util;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using UnitsNet;
using System;
using DisplayTest;

namespace FeatherV7.Domain.Models
{
    internal class BluetoothHandler: IBluetoothHandler
    {
        public event EventHandler<bool> WifiEnabled = delegate { };
        public event EventHandler<bool> BleEnabled = delegate { };
        public event EventHandler<bool> BlePaired = delegate { };
        public event EventHandler<int> RotationUpdated = delegate { };
        public event EventHandler<bool> ButtonClicked = delegate { };
        public event EventHandler<bool> Launched = delegate { };

        private readonly IShooterLed led;
        
        private IDefinition bleTreeDefinition;
        
        private ICharacteristic Ssid;
        private ICharacteristic Password;
        private ICharacteristic ToggleBleConnection;
        private ICharacteristic ToggleWifiConnection;
        
        private ICharacteristic Distance;
        private ICharacteristic Rotation;
        private ICharacteristic Button;
        private ICharacteristic Launch;
        private ICharacteristic Status;
        
        private string ssid;
        private string password;
        
        private IWiFiNetworkAdapter wifi;

        public BluetoothHandler(IShooterLed led)
        {
            this.led = led;
        }

        public void Initialize()
        {
            bleTreeDefinition = GetDefinition();
            MeadowApp.Device.BluetoothAdapter.StartBluetoothServer(bleTreeDefinition);

            SetupBluetoothDataReceiveHandlers();

            wifi = MeadowApp.Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            wifi.NetworkConnected += WifiNetworkConnected;
            wifi.NetworkDisconnected += WifiNetworkDisconnected;
        }

        public void UpdateDistance(Length value)
        {
            Distance.SetValue((int)Math.Round(value.Centimeters));
        }

        public void UpdateRotation(TimeSpan value)
        {
            Rotation.SetValue((int)Math.Round(value.TotalSeconds));
        }

        public void RotationClicked(bool value)
        {
            Button.SetValue(value);
        }

        public void LaunchTriggered(bool value)
        {
            Launch.SetValue(value);
        }

        public void UpdateStatus(string status)
        {
            Status.SetValue(status);
        }

        private void SetupBluetoothDataReceiveHandlers()
        {
            Ssid.ValueSet += (s, e) =>
            {
                Resolver.Log.Info("SSID set");
                ssid = (string)e;
            };
            Password.ValueSet += (s, e) =>
            {
                Resolver.Log.Info("PW set");
                password = (string)e;
            };
            ToggleBleConnection.ValueSet += (s, e) =>
            {
                Resolver.Log.Info($"BLE Paired: {e}");
                BlePaired.Invoke(s, (bool)e);
            };
            ToggleWifiConnection.ValueSet += async (s, e) =>
            {
                led.StartPulse(Color.Yellow);

                if ((bool)e)
                {
                    Resolver.Log.Info("WIFI Connecting");
                    await wifi.Connect(ssid, password, TimeSpan.FromSeconds(45));

                    if (wifi.IsConnected)
                    {
                        ConfigFileManager.CreateConfigFiles(ssid, password);
                    }
                }
                else
                {
                    Resolver.Log.Info("WIFI Disconnecting");
                    await wifi.Disconnect(false);
                }
            };
            Button.ValueSet += (s, e) =>
            {
                ButtonClicked.Invoke(s, (bool)e);
            };
            Rotation.ValueSet += (s, e) =>
            {
                RotationUpdated.Invoke(s, (int)e);
            };
            Launch.ValueSet += (s, e) =>
            {
                Launched.Invoke(s, (bool)e);
            };
        }

        private void WifiNetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            ToggleWifiConnection.SetValue(true);

            WifiEnabled.Invoke(sender, true);

            led.StartPulse(Color.Magenta);

            Resolver.Log.Info("WIFI Connected");
        }

        private void WifiNetworkDisconnected(INetworkAdapter sender, NetworkDisconnectionEventArgs args)
        {
            ToggleWifiConnection.SetValue(false);

            ConfigFileManager.DeleteConfigFiles();
            WifiEnabled.Invoke(sender, false);
            BleEnabled.Invoke(sender, false);

            MeadowApp.Device.PlatformOS.Reset();
        }

        private Definition GetDefinition()
        {
            var notificationDescriptor = new Descriptor(Constants.Bluetooth.WRITEABLE, BitConverter.ToInt16(Constants.Bluetooth.ENABLE_NOTIFICATION_VALUE, 0));

            var wifiService = new Service(
                name: Constants.Bluetooth.MACHINE_SERVICE_NAME,
                uuid: Constants.Bluetooth.MACHINE_SERVICE_UID,

                Ssid = new CharacteristicString(
                    name: nameof(Ssid),
                    uuid: Constants.Bluetooth.SSID,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write,
                    maxLength: 256),
                Password = new CharacteristicString(
                    name: nameof(Password),
                    uuid: Constants.Bluetooth.PASSWORD,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write,
                    maxLength: 256),
                ToggleBleConnection = new CharacteristicBool(
                    name: nameof(ToggleBleConnection),
                    uuid: Constants.Bluetooth.TOGGLE_BLE_CONNECTION,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write),
                ToggleWifiConnection = new CharacteristicBool(
                    name: nameof(ToggleWifiConnection),
                    uuid: Constants.Bluetooth.TOGGLE_WIFI_CONNECTION,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write),
                Rotation = new CharacteristicInt32(
                    name: nameof(Rotation),
                    uuid: Constants.Bluetooth.ROTATION,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write | CharacteristicProperty.Broadcast | CharacteristicProperty.Notify,
                    notificationDescriptor),
                Distance = new CharacteristicInt32(
                    name: nameof(Distance),
                    uuid: Constants.Bluetooth.DISTANCE,
                    permissions: CharacteristicPermission.Read,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Broadcast | CharacteristicProperty.Notify,
                    notificationDescriptor),
                Button = new CharacteristicBool(
                    name: nameof(Button),
                    uuid: Constants.Bluetooth.BUTTON,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write | CharacteristicProperty.Broadcast | CharacteristicProperty.Notify,
                    notificationDescriptor),
                Launch = new CharacteristicBool(
                    name: nameof(Launch),
                    uuid: Constants.Bluetooth.LAUNCH,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write | CharacteristicProperty.Broadcast | CharacteristicProperty.Notify,
                    notificationDescriptor),
                Status = new CharacteristicString(
                    name: nameof(Status),
                    uuid: Constants.Bluetooth.STATUS,
                    permissions: CharacteristicPermission.Read,
                    properties: CharacteristicProperty.Read,
                    maxLength: 256)
                );

            return new Definition(Constants.Bluetooth.DEFINITION_SERVICE_NAME, wifiService);
        }

        
    }
}
