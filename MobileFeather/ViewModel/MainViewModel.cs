using CommonFeather;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        ICharacteristic CharacteristicDistance;
        ICharacteristic CharacteristicRotation;
        ICharacteristic CharacteristicButton;

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
                if (UuidToUshort(serviceItem.Id.ToString()) == Constants.Bluetooth.MACHINE_SERVICE_UID)
                {
                    service = serviceItem;
                }
            }

            CharacteristicSsid = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.SSID));
            CharacteristicPassword = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.PASSWORD));
            CharacteristicToggleBleConnection = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.TOGGLE_BLE_CONNECTION));
            CharacteristicToggleWifiConnection = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.TOGGLE_WIFI_CONNECTION));
            CharacteristicDistance = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.DISTANCE));
            CharacteristicRotation = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.ROTATION));
            CharacteristicButton = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.BUTTON));

            await Task.Delay(1000);

            await CharacteristicToggleBleConnection.WriteAsync(TRUE);

            await SetupBluetoothDataReceiveHandlersAsync(CancellationToken.None);

            var temp = await CharacteristicToggleWifiConnection.ReadAsync();
            HasJoinedWifi = temp.data[0] == 1;
        }

        private async Task SetupBluetoothDataReceiveHandlersAsync(CancellationToken token)
        {
            CharacteristicButton.ValueUpdated += (s, e) =>
            {
                ButtonClicked = CharacteristicButton.Value[0] == 1;
            };
            CharacteristicRotation.ValueUpdated += (s, e) =>
            {
                Rotation = (int.Parse(CharacteristicRotation.StringValue));
            };
            CharacteristicDistance.ValueUpdated += (s, e) =>
            {
                Distance = (int.Parse(CharacteristicDistance.StringValue));
            };

#if !WINDOWS
            if (CharacteristicDistance.CanUpdate)
                await CharacteristicDistance.StartUpdatesAsync(token);

            if (CharacteristicRotation.CanUpdate)
                await CharacteristicRotation.StartUpdatesAsync(token);

            if (CharacteristicButton.CanUpdate)
                await CharacteristicButton.StartUpdatesAsync(token);
#else
            while (true)
            {
                try 
	            {	        
		            var t1 = await CharacteristicDistance.ReadAsync(token);
                    var t2 = await CharacteristicRotation.ReadAsync(token);
                    var t3 = await CharacteristicButton.ReadAsync(token);

                    if (t1.data.Length > 0)
                        Distance = (double.Parse(Encoding.Default.GetString(t1.data))) / 10;

                    if (t2.data.Length > 0)
                        Rotation = (int.Parse(Encoding.Default.GetString(t2.data)));

                    if (t3.data.Length > 0)
                        ButtonClicked = t3.data[0] == 1;
	            }
	            catch (Exception ex)
	            {
                    Debug.WriteLine(ex.Message);
	            }

                await Task.Delay(500);
            }
#endif
        }
    }
}
