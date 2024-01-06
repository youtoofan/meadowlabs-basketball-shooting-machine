using CommonFeather;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MobileFeather.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        ICharacteristic CharacteristicSsid;
        ICharacteristic CharacteristicPassword;
        ICharacteristic CharacteristicToggleWifiConnection;

        bool _showPassword;
        public bool ShowPassword
        {
            get
            {
                return _showPassword;
            }

            set
            {
                _showPassword = value;
                OnPropertyChanged(nameof(ShowPassword));
            }
        }

        bool _hasJoinedWifi;
        public bool HasJoinedWifi
        {
            get
            {
                return _hasJoinedWifi;
            }

            set
            {
                _hasJoinedWifi = value;
                OnPropertyChanged(nameof(HasJoinedWifi));
            }
        }

        string _ssid;
        public string Ssid
        {
            get
            {
                return _ssid;
            }

            set
            {
                _ssid = value;
                OnPropertyChanged(nameof(Ssid));
            }
        }

        string _password;
        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private double distance;
        public double Distance
        {
            get
            {
                return distance;
            }

            set
            {
                distance = value;
                OnPropertyChanged(nameof(Distance));
            }
        }

        private int rotation;
        public int Rotation
        {
            get
            {
                return rotation;
            }

            set
            {
                rotation = value;
                OnPropertyChanged(nameof(Rotation));
            }
        }

        private bool buttonClicked;
        public bool ButtonClicked
        {
            get
            {
                return buttonClicked;
            }

            set
            {
                buttonClicked = value;
                OnPropertyChanged(nameof(ButtonClicked));
            }
        }

        public ICommand ToggleWifiConnectionCommand { get; set; }

        public ICommand TogglePasswordVisibility { get; set; }

        public MainViewModel()
        {
            adapter.DeviceConnected += AdapterDeviceConnected;
            adapter.DeviceDisconnected += AdapterDeviceDisconnected;

            ToggleWifiConnectionCommand = new Command(async () =>
            {
                if (!IsBlePaired)
                {
                    return;
                }

                if (!HasJoinedWifi)
                {
                    var ssid = Encoding.ASCII.GetBytes(Ssid);
                    await CharacteristicSsid.WriteAsync(ssid);

                    await Task.Delay(500);

                    var password = Encoding.ASCII.GetBytes(Password);
                    await CharacteristicPassword.WriteAsync(password);

                    await Task.Delay(500);

                    await CharacteristicToggleWifiConnection.WriteAsync(TRUE);

                    HasJoinedWifi = true;
                }
                else
                {
                    Ssid = string.Empty;
                    Password = string.Empty;

                    await CharacteristicToggleWifiConnection.WriteAsync(FALSE);

                    IsBlePaired = false;
                    HasJoinedWifi = false;
                }
            });

            TogglePasswordVisibility = new Command(() => ShowPassword = !ShowPassword);
        }

        void AdapterDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Ssid = string.Empty;
            Password = string.Empty;

            HasJoinedWifi = false;
            IsBlePaired = false;
        }

        async void AdapterDeviceConnected(object sender, DeviceEventArgs e)
        {
            IsBlePaired = true;

            IDevice device = e.Device;

            var services = await device.GetServicesAsync();

            foreach (var serviceItem in services)
            {
                if (UuidToUshort(serviceItem.Id.ToString()) == Constants.Bluetooth.WIFI_SERVICE_UID)
                {
                    service = serviceItem;
                }
            }

            CharacteristicSsid = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.SSID));
            CharacteristicPassword = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.PASSWORD));
            CharacteristicToggleBleConnection = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.TOGGLE_BLE_CONNECTION));
            CharacteristicToggleWifiConnection = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.TOGGLE_WIFI_CONNECTION));

            await Task.Delay(1000);

            await CharacteristicToggleBleConnection.WriteAsync(TRUE);

            var temp = await CharacteristicToggleWifiConnection.ReadAsync();
            HasJoinedWifi = temp.data[0] == 1;
        }
    }
}
