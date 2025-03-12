#if !DEBUG
#define RELEASE
#endif

using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using System.Threading.Tasks;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using Led = FeatherV7.Domain.Models.Led;
using Meadow.Logging;
using FeatherV7.Domain.Models;
using CommonFeather;
using UnitsNet;
using System;
using AsyncAwaitBestPractices;
using Meadow.Peripherals.Displays;
using System.Diagnostics;

namespace FeatherV7
{
    // Change F7CoreComputeV2 to F7FeatherV2 (or F7FeatherV1) for Feather boards
    public sealed class MeadowApp : App<F7FeatherV2>, IDisposable
    {
        private const int _waitIntervalInSeconds = 10;
        private static readonly TimeSpan WaitInterval = TimeSpan.FromSeconds(_waitIntervalInSeconds);
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
        private bool _disposedValue;
        private DateTimeOffset _lastUpdate = DateTimeOffset.UtcNow;
        private bool _isRunning;
        private bool _isFaulty;

        public override async Task Initialize()
        {
            InitLogging();
            await InitDevices();
            await base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            _ballShooterMachine = new BallShooterMachine(_graphics, _speaker, _relay, _onboardLed, _bluetoothHandler);

            var distanceConsumer = Vl53l0x.CreateObserver(
                handler: result =>
                {
                    _lastUpdate = DateTimeOffset.UtcNow;
                    _ballShooterMachine.UpdateDistanceToObject(Length.FromCentimeters(result.New.Centimeters));
                },
                filter: result =>
                {
                    return result.New != null && result.New > Meadow.Units.Length.Zero;
                }
            );

            if (_rotaryEncoder != null)
            {
                _rotaryEncoder.Rotated += (s, e) =>
                {
                    _ballShooterMachine.HandleRotationDirection(e.New);
                };

                _rotaryEncoder.PressStarted += (s, e) =>
                {
                    _relay.ShootAsync().SafeFireAndForget();
                };

                // Left intentionally blank as no action is specified
                _rotaryEncoder.LongClicked += (s, e) => { };
                _rotaryEncoder.Clicked += (s, e) => { };
                _rotaryEncoder.PressEnded += (s, e) => { };
            }

            if (_distanceSensor is null)
            {
                Resolver.Log.Warn("Distance sensor not initialized");
                _ballShooterMachine.SetState(_ballShooterMachine.ErrorState);
                _isFaulty = true;
            }

            _ballShooterMachine.Start();
            _distanceSensor?.Subscribe(distanceConsumer);
            _isRunning = true;

            _ = Task.Run(async () =>
            {
                while (_isRunning)
                {
                    if (_lastUpdate.Add(WaitInterval) < DateTimeOffset.UtcNow)
                    {
                        Resolver.Log.Warn("Distance sensor not updating. Restarting sensor updates.");
                        _distanceSensor?.StopUpdating();
                        _distanceSensor?.StartUpdating(Constants.Sensors.SENSOR_DISTANCE_READ_FREQUENCY);
                    }
                    await Task.Delay(WaitInterval);
                }
            });

            return base.Run();
        }

        public override Task OnError(Exception e)
        {
            Resolver.Log.Error(e);
            return base.OnError(e);
        }

        private void InitLogging()
        {
            var cloudLogger = new CloudLogger();
            Resolver.Log.AddProvider(cloudLogger);
            Resolver.Services.Add(cloudLogger);
            Resolver.Log.Info("Initialize logging");
        }

        private Task InitDevices()
        {
            try
            {
                Resolver.Log.Info("Initialize devices");

                var config = new SpiClockConfiguration(speed: new Meadow.Units.Frequency(12000), mode: SpiClockConfiguration.Mode.Mode3);
                var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

                _st7789 = new St7789(
                    spiBus: spiBus,
                    chipSelectPin: Device.Pins.D02,
                    dcPin: Device.Pins.D01,
                    resetPin: Device.Pins.D00,
                    width: 240,
                    height: 240);

                _graphics = new Display(this, _st7789, RotationType._180Degrees);

                _onboardLed = new Led(
                    redPwmPin: Device.Pins.OnboardLedRed,
                    greenPwmPin: Device.Pins.OnboardLedGreen,
                    bluePwmPin: Device.Pins.OnboardLedBlue);

                _speaker = new Speaker(Device.Pins.D12);
                _relay = new Trigger(Device.Pins.D06);

                _rotaryEncoder = new RotaryEncoderWithButton(Device.Pins.D11, Device.Pins.D10, Device.Pins.D09);

                _bluetoothHandler = new BluetoothHandler(_onboardLed);
                _bluetoothHandler.Initialize();

                _i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
                _distanceSensor = new Vl53l0x(_i2cBus);

                Resolver.Log.Info("Initialize devices done");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Resolver.Log.Error(e, "Devices init failed.");
                return Task.CompletedTask;
            }
        }

        #region Dispose

        private void Dispose(bool disposing)
        {
            Resolver.Log.Info("Disposing...");
            _isRunning = false;

            if (!_disposedValue)
            {
                if (disposing)
                {
                    _speaker.Dispose();
                    _distanceSensor.Dispose();
                    _rotaryEncoder.Dispose();
                    _i2cBus.Dispose();
                    _st7789.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}