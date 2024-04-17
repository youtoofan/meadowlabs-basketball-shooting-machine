﻿#if !DEBUG
#define RELEASE
#endif

using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using System.Threading.Tasks;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using DisplayTest.Domain.Models;
using Led = DisplayTest.Domain.Models.Led;
using Meadow.Logging;
using FeatherV7.Domain.Models;
using CommonFeather;
using UnitsNet;
using System;
using AsyncAwaitBestPractices;
using Meadow.Peripherals.Displays;
using System.Diagnostics;

namespace DisplayTest
{
    // Change F7CoreComputeV2 to F7FeatherV2 (or F7FeatherV1) for Feather boards
    public sealed class MeadowApp : App<F7FeatherV2>, IDisposable
    {
        private const int WaitIntervalInSeconds = 10;
        private Display _graphics;
        private BallShooterMachine _ballShooterMachine;
        private II2cBus _i2cBus;
        private St7789 _st7789;
        private Led _onboardLed;
        private Speaker _speaker;
        private Trigger _relay;
        private RotaryEncoderWithButton _rotaryEncoder;
        private BluetoothHandler _bluetoothHandler;
        private Vl53l0x _distanceSensor;
        private bool disposedValue;
        private DateTimeOffset _lastUpdate = DateTime.Now;
        private bool _isRunning;

        public override Task Initialize()
        {
            InitLogging();

            Resolver.Log.Info("Initialize...");

            RegisterUpdateService();

            _speaker = new Speaker(Device.Pins.D12);
            _relay = new Trigger(Device.CreateDigitalOutputPort(Device.Pins.D05, false, OutputType.PushPull));

            _st7789 = new St7789(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240,
                height: 240);

            _onboardLed = new Led(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue);
            
            _rotaryEncoder = new RotaryEncoderWithButton(Device.Pins.D11, Device.Pins.D10, Device.Pins.D09);
            _graphics = new Display(this, _st7789, RotationType._180Degrees);

            _i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            _distanceSensor = new Vl53l0x(_i2cBus);

            _bluetoothHandler = new BluetoothHandler(_onboardLed);
            _bluetoothHandler.Initialize();

            return base.Initialize();
        }

        

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            _ballShooterMachine = new BallShooterMachine(_graphics, _speaker, _relay, _onboardLed, _bluetoothHandler);

            var distanceConsumer = Vl53l0x.CreateObserver(
                handler: result =>
                {
                    _lastUpdate = DateTimeOffset.Now;
                    _ballShooterMachine.UpdateDistanceToObject(Length.FromCentimeters(result.New.Centimeters));
                },
                filter: result => 
                {
                    return result.New != null && result.New > Meadow.Units.Length.Zero;
                }
            );

            _rotaryEncoder.Rotated += (s, e) =>
            {
                _ballShooterMachine.HandleRotationDirection(e.New);
            };

            _rotaryEncoder.PressStarted += (s, e) => { _relay.ShootAsync().SafeFireAndForget(); };
            _rotaryEncoder.LongClicked += (s, e) => {  };
            _rotaryEncoder.Clicked += (s, e) => {  };
            _rotaryEncoder.PressEnded += (s, e) => {  };

            _distanceSensor.Subscribe(distanceConsumer);
            _ballShooterMachine.Start();

            _isRunning = true;

            _ = Task.Run(async () =>
            {
                while (_isRunning)
                {
                    if (_lastUpdate.Add(TimeSpan.FromSeconds(WaitIntervalInSeconds)) < DateTimeOffset.Now)
                    {
                        Resolver.Log.Warn("Distance sensor not updating");

                        _distanceSensor.StopUpdating();
                        _distanceSensor.StartUpdating(Constants.Sensors.SENSOR_DISTANCE_READ_FREQUENCY);

                        Resolver.Log.Warn("Request to start updating sensor");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(WaitIntervalInSeconds));
                }
            });

            return base.Run();
        }

        public override Task OnError(Exception e)
        {
            Resolver.Log.Error(e);
            return base.OnError(e);
        }

        [Conditional("RELEASE")]
        private void InitLogging()
        {
            var cloudLogger = new CloudLogger();
            Resolver.Log.AddProvider(cloudLogger);
            Resolver.Services.Add(cloudLogger);
        }

        [Conditional("RELEASE")]
        private void RegisterUpdateService()
        {
            var svc = Resolver.UpdateService;

            svc.ClearUpdates();

            svc.StateChanged += (sender, updateState) =>
            {
                Resolver.Log.Info($"UpdateState {updateState}");
            };

            svc.RetrieveProgress += (updateService, info) =>
            {
                short percentage = (short)((double)info.DownloadProgress / info.FileSize * 100);

                Resolver.Log.Info($"Downloading... {percentage}%");
            };

            svc.UpdateAvailable += async (updateService, info) =>
            {
                Resolver.Log.Info($"Update available!");

                // Queue update for retrieval "later"
                await Task.Delay(5000);

                updateService.RetrieveUpdate(info);
            };

            svc.UpdateRetrieved += async (updateService, info) =>
            {
                Resolver.Log.Info($"Update retrieved!");

                await Task.Delay(5000);

                updateService.ApplyUpdate(info);
            };
        }

        #region Dispose

        private void Dispose(bool disposing)
        {
            Resolver.Log.Info("Disposing...");

            _isRunning = false;

            if (!disposedValue)
            {
                if (disposing)
                {
                    _speaker.Dispose();
                    _distanceSensor.Dispose();
                    _rotaryEncoder.Dispose();
                    _i2cBus.Dispose();
                    _st7789.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}