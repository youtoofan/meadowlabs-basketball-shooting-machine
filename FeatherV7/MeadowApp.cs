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

namespace DisplayTest
{
    // Change F7CoreComputeV2 to F7FeatherV2 (or F7FeatherV1) for Feather boards
    public sealed class MeadowApp : App<F7FeatherV2>
    {
        private Display _graphics;
        private BallShooterMachine _ballShooterMachine;
        private Led _onboardLed;
        private Speaker _speaker;
        private Trigger _relay;
        private RotaryEncoderWithButton _rotaryEncoder;
        private BluetoothHandler _bluetoothHandler;
        private Vl53l0x _distanceSensor;

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            _ballShooterMachine = new BallShooterMachine(_graphics, _speaker, _relay, _onboardLed, _bluetoothHandler);

            var distanceConsumer = Vl53l0x.CreateObserver(
                handler: result =>
                {
                    _ballShooterMachine.UpdateDistanceToObject(Length.FromCentimeters(result.New.Centimeters));
                },
                filter: null
            );

            _rotaryEncoder.Rotated += (s, e) =>
            {
                _ballShooterMachine.HandleRotationDirection(e.New);
            };

            _rotaryEncoder.Clicked += (s, e) =>
            {
                _relay.ShootAsync().SafeFireAndForget();
            };

            _distanceSensor.Subscribe(distanceConsumer);
            _distanceSensor.StartUpdating(Constants.Sensors.SENSOR_DISTANCE_READ_FREQUENCY);

            _ballShooterMachine.Start();

            return base.Run();
        }

        public override Task OnError(Exception e)
        {
            Resolver.Log.Error(e);
            return base.OnError(e);
        }

        public override Task Initialize()
        {
#if !DEBUG
            var cloudLogger = new CloudLogger();
            Resolver.Log.AddProvider(cloudLogger);
            Resolver.Services.Add(cloudLogger);
#endif
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);

            var st7789 = new St7789(
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

            _speaker = new Speaker(Device.Pins.D12);
            _relay = new Trigger(Device.CreateDigitalOutputPort(Device.Pins.D05, false, OutputType.PushPull));
            //_relayBackup = new Trigger(Device.CreateDigitalOutputPort(Device.Pins.D06, false, OutputType.PushPull));
            _rotaryEncoder = new RotaryEncoderWithButton(Device.Pins.D11, Device.Pins.D10, Device.Pins.D09);

            _graphics = new Display(this, st7789, RotationType._270Degrees);
            //_graphics.Rotation = RotationType.Default;

            _distanceSensor = new Vl53l0x(i2cBus);

            _bluetoothHandler = new BluetoothHandler(_onboardLed);
            _bluetoothHandler.Initialize();
            
            return base.Initialize();
        }
    }
}