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

namespace FeatherV7.Domain.Models
{
    interface IBluetoothHandler
    {
        event EventHandler<bool> WifiEnabled;
        event EventHandler<bool> BleEnabled;
        event EventHandler<bool> BlePaired;
        event EventHandler<int> RotationUpdated;
        event EventHandler<bool> ButtonClicked;

        void Initialize();
        void UpdateDistance(Length value);
        void RotationClicked(bool value);
        void UpdateRotation(TimeSpan value);
    }

    internal class BluetoothHandler: IBluetoothHandler
    {
        public event EventHandler<bool> WifiEnabled = delegate { };
        public event EventHandler<bool> BleEnabled = delegate { };
        public event EventHandler<bool> BlePaired = delegate { };
        public event EventHandler<int> RotationUpdated = delegate { };
        public event EventHandler<bool> ButtonClicked = delegate { };

        
        readonly F7FeatherV2 device;
        readonly IShooterLed led;

        IDefinition bleTreeDefinition;

        ICharacteristic Ssid;
        ICharacteristic Password;
        ICharacteristic ToggleBleConnection;
        ICharacteristic ToggleWifiConnection;

        ICharacteristic Distance;
        ICharacteristic Rotation;
        ICharacteristic Button;
        ICharacteristic WriteAble;

        string ssid;
        string password;

        IWiFiNetworkAdapter wifi;

        public BluetoothHandler(F7FeatherV2 device, IShooterLed led)
        {
            this.device = device;
            this.led = led;
        }

        public void Initialize()
        {
            bleTreeDefinition = GetDefinition();
            device.BluetoothAdapter.StartBluetoothServer(bleTreeDefinition);

            SetupBluetoothDataReceiveHandlers();

            wifi = device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            wifi.NetworkConnected += WifiNetworkConnected;
            wifi.NetworkDisconnected += WifiNetworkDisconnected;
        }

        public void UpdateDistance(Length value)
        {
            Distance.SetValue(value.Centimeters.ToString());
        }

        public void UpdateRotation(TimeSpan value)
        {
            Rotation.SetValue(value.TotalSeconds.ToString());
        }

        public void RotationClicked(bool value)
        {
            Button.SetValue(value ? "true" : "false");
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
        }

        private void WifiNetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            ToggleWifiConnection.SetValue(true);

            WifiEnabled.Invoke(sender, true);

            led.StartPulse(Color.Magenta);

            Resolver.Log.Info("WIFI Connected");
        }

        private void WifiNetworkDisconnected(INetworkAdapter sender)
        {
            ToggleWifiConnection.SetValue(false);

            ConfigFileManager.DeleteConfigFiles();

            WifiEnabled.Invoke(sender, false);
            BleEnabled.Invoke(sender, false);

            device.PlatformOS.Reset();
        }

        private Definition GetDefinition()
        {
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
                Rotation = new CharacteristicString(
                    name: nameof(Rotation),
                    uuid: Constants.Bluetooth.ROTATION,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write | CharacteristicProperty.Broadcast | CharacteristicProperty.Notify,
                    maxLength: 256),
                Distance = new CharacteristicString(
                    name: nameof(Distance),
                    uuid: Constants.Bluetooth.DISTANCE,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write | CharacteristicProperty.Broadcast | CharacteristicProperty.Notify,
                    maxLength: 256),
                Button = new CharacteristicBool(
                    name: nameof(Button),
                    uuid: Constants.Bluetooth.BUTTON,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write | CharacteristicProperty.Broadcast | CharacteristicProperty.Notify
                    ),
                WriteAble = new CharacteristicBool(
                    name: "writable",
                    uuid: Constants.Bluetooth.WRITEABLE,
                    permissions: CharacteristicPermission.Read | CharacteristicPermission.Write,
                    properties: CharacteristicProperty.Read | CharacteristicProperty.Write | CharacteristicProperty.Broadcast | CharacteristicProperty.Notify
                    )
                );

            return new Definition(Constants.Bluetooth.DEFINITION_SERVICE_NAME, wifiService);
        }

        
    }
}
