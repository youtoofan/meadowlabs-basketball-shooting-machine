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
        private ICharacteristic CharacteristicSsid;
        private ICharacteristic CharacteristicPassword;
        private ICharacteristic CharacteristicToggleWifiConnection;
        private ICharacteristic CharacteristicDistance;
        private ICharacteristic CharacteristicRotation;
        private ICharacteristic CharacteristicButton;
        private ICharacteristic CharacteristicLaunch;
        private ICharacteristic CharacteristicStatus;

        private CancellationTokenSource CancellationTokenSource;

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

        private bool launchClicked;
        public bool LaunchClicked
        {
            get
            {
                return launchClicked;
            }

            set
            {
                if(value && Vibration.Default.IsSupported)
                    Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(1000));

                launchClicked = value;
                OnPropertyChanged(nameof(LaunchClicked));
            }
        }

        string _status;
        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public ICommand ClickButtonCommand { get; set; }
        public ICommand ClickLaunchCommand { get; set; }
        public ICommand ToggleWifiConnectionCommand { get; set; }
        public ICommand TogglePasswordVisibility { get; set; }

        public MainViewModel()
        {
            adapter.DeviceConnected += AdapterDeviceConnected;
            adapter.DeviceDisconnected += AdapterDeviceDisconnected;

            ClickButtonCommand = new Command(async () =>
            {
                if (!IsBlePaired)
                {
                    return;
                }

                await CharacteristicButton.WriteAsync(TRUE);
            });

            ClickLaunchCommand = new Command(async () =>
            {
                if (!IsBlePaired)
                {
                    return;
                }

                await CharacteristicLaunch.WriteAsync(TRUE);
            });

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

        private void AdapterDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Ssid = string.Empty;
            Password = string.Empty;

            HasJoinedWifi = false;
            IsBlePaired = false;

            if (CancellationTokenSource is not null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
            }
        }

        private async void AdapterDeviceConnected(object sender, DeviceEventArgs e)
        {
            IDevice device = e.Device;
            var connected = device.State == Plugin.BLE.Abstractions.DeviceState.Connected;
            CancellationTokenSource = new CancellationTokenSource();
            IsBlePaired = connected;

            if (!connected)
                return;

            await SetupCharacteristics(device);

            await Task.Delay(1000);

            await CharacteristicToggleBleConnection.WriteAsync(TRUE);

            await SetupBluetoothDataReceiveHandlersAsync(CancellationTokenSource.Token);

            if (CharacteristicToggleWifiConnection.CanRead)
            {
                var temp = await CharacteristicToggleWifiConnection.ReadAsync();
                HasJoinedWifi = temp.data[0] == 1;
            }
        }

        private async Task SetupCharacteristics(IDevice device)
        {
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
            CharacteristicLaunch = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.LAUNCH));
            CharacteristicStatus = await service.GetCharacteristicAsync(Guid.Parse(Constants.Bluetooth.STATUS));
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
            CharacteristicLaunch.ValueUpdated += (s, e) =>
            {
                LaunchClicked = CharacteristicLaunch.Value[0] == 1;
            };


            // not supported yet
            //if (CharacteristicDistance.CanUpdate)
            //    await CharacteristicDistance.StartUpdatesAsync(token);

            //if (CharacteristicRotation.CanUpdate)
            //    await CharacteristicRotation.StartUpdatesAsync(token);

            //if (CharacteristicButton.CanUpdate)
            //    await CharacteristicButton.StartUpdatesAsync(token);
            
            while (!token.IsCancellationRequested)
            {
                try 
	            {	        
		            var t1 = await CharacteristicDistance.ReadAsync(token);
                    var t2 = await CharacteristicRotation.ReadAsync(token);
                    var t3 = await CharacteristicButton.ReadAsync(token);
                    var t4 = await CharacteristicLaunch.ReadAsync(token);
                    var t5 = await CharacteristicStatus.ReadAsync(token);

                    if (t1.data.Length > 0)
                    {
                        var temp = (BitConverter.ToInt32(t1.data));
                        Distance = temp > 256 ? 0 : temp;
                    }

                    if (t2.data.Length > 0)
                    {
                        var temp = (BitConverter.ToInt32(t2.data));
                        Rotation = temp > 256 ? 0 : temp;
                    }

                    if (t3.data.Length > 0)
                    {
                        ButtonClicked = t3.data[0] == 1;
                    }

                    if (t4.data.Length > 0)
                    {
                        LaunchClicked = t4.data[0] == 1;
                    }

                    if (t5.data.Length > 0)
                    {
                        var temp = (Encoding.Default.GetString(t5.data));
                        Status = temp;
                    }
                }
	            catch (Exception ex)
	            {
                    Debug.WriteLine(ex.Message);
	            }

                await Task.Delay(500);
            }
        }
    }
}
