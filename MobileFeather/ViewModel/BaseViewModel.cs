﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.EventArgs;
using CommonFeather;
using Microsoft.Maui.ApplicationModel;

namespace MobileFeather.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected byte[] TRUE = new byte[1] { 1 };
        protected byte[] FALSE = new byte[1] { 0 };

        int listenTimeout = 5000;

        protected ushort DEVICE_ID = 253;

        protected IAdapter adapter;
        protected IService service;

        protected ICharacteristic CharacteristicToggleBleConnection;

        public ObservableCollection<IDevice> DeviceList { get; set; }

        IDevice deviceSelected;
        public IDevice DeviceSelected
        {
            get => deviceSelected;
            set { deviceSelected = value; OnPropertyChanged(nameof(DeviceSelected)); }
        }

        bool isScanning;
        public bool IsScanning
        {
            get => isScanning;
            set { isScanning = value; OnPropertyChanged(nameof(IsScanning)); }
        }

        bool isConnected;
        public bool IsBlePaired
        {
            get => isConnected;
            set { isConnected = value; OnPropertyChanged(nameof(IsBlePaired)); }
        }

        bool isDeviceListEmpty;
        public bool IsDeviceListEmpty
        {
            get => isDeviceListEmpty;
            set { isDeviceListEmpty = value; OnPropertyChanged(nameof(IsDeviceListEmpty)); }
        }

        public ICommand CmdToggleConnection { get; set; }

        public ICommand CmdSearchForDevices { get; set; }

        public BaseViewModel()
        {
            DeviceList = new ObservableCollection<IDevice>();

            adapter = CrossBluetoothLE.Current.Adapter;
            adapter.ScanTimeout = listenTimeout;
            adapter.ScanMode = ScanMode.LowLatency;
            adapter.DeviceDiscovered += AdapterDeviceDiscovered;

            CmdToggleConnection = new Command(async () => await ToggleConnection());
            CmdSearchForDevices = new Command(async () => await DiscoverDevices());
        }

        async Task ScanTimeoutTask()
        {
            await Task.Delay(listenTimeout);
            await adapter.StopScanningForDevicesAsync();
            IsScanning = false;
        }

        void AdapterDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            if(string.IsNullOrEmpty(e.Device.Name))
            {
                return;
            }

            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    DeviceList.Add(e.Device);

                    if (e != null &&
                        e.Device != null &&
                        !string.IsNullOrEmpty(e.Device.Name) &&
                        e.Device.Name.Contains(Constants.Bluetooth.DEFINITION_SERVICE_NAME))
                    {
                        await adapter.StopScanningForDevicesAsync();
                        IsDeviceListEmpty = false;
                        DeviceSelected = e.Device;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        async Task DiscoverDevices()
        {
            try
            {
                IsScanning = true;
                DeviceList.Clear();

                var tasks = new Task[]
                {
                    ScanTimeoutTask(),
                    adapter.StartScanningForDevicesAsync()
                };

                await Task.WhenAny(tasks);
            }
            catch (DeviceConnectionException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        async Task ToggleConnection()
        {
            try
            {
                if (IsBlePaired)
                {
                    IsBlePaired = false;
                    await CharacteristicToggleBleConnection.WriteAsync(FALSE);
                    await adapter.DisconnectDeviceAsync(DeviceSelected);
                }
                else
                {
                    await adapter.ConnectToDeviceAsync(DeviceSelected);
                    IsBlePaired = true;
                }
            }
            catch (DeviceConnectionException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        protected int UuidToUshort(string uuid)
        {
            return int.Parse(uuid.Substring(4, 4), System.Globalization.NumberStyles.HexNumber); ;
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
