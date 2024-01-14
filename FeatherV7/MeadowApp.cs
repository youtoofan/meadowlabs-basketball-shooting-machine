﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using System.Threading.Tasks;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using DisplayTest.Domain.Models;
using Led = DisplayTest.Domain.Models.Led;
using Meadow.Peripherals.Sensors.Rotary;
using Meadow.Logging;
using FeatherV7.Domain.Models;
using CommonFeather;
using UnitsNet;

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

            _distanceSensor.Updated += (s, e) =>
            {
                _ballShooterMachine.UpdateDistanceToObject(Length.FromCentimeters(e.New.Centimeters));
            };
            _rotaryEncoder.Rotated += (s, e) =>
            {
                _ballShooterMachine.HandleRotationDirection(e.New);
            };
            _rotaryEncoder.PressStarted += (s, e) =>
            {
                _ballShooterMachine.HandleButtonClicked();
            };
            _rotaryEncoder.PressEnded += (s, e) =>
            {
                _ballShooterMachine.HandleButtonReleased();
            };

            _distanceSensor.StartUpdating(Constants.Sensors.SENSOR_DISTANCE_READ_FREQUENCY);
            _ballShooterMachine.Start();

            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
#if !DEBUG
            var cloudLogger = new CloudLogger();
            Resolver.Log.AddProvider(cloudLogger);
            Resolver.Services.Add(cloudLogger);
#endif
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

            var st7789 = new St7789(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240);

            _onboardLed = new Led(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue);

            _speaker = new Speaker(Device.Pins.D12);
            _relay = new Trigger(Device.CreateDigitalOutputPort(Device.Pins.D05, false, OutputType.PushPull));
            _rotaryEncoder = new RotaryEncoderWithButton(Device.Pins.D11, Device.Pins.D10, Device.Pins.D09);

            _graphics = new Display(st7789);
            _distanceSensor = new Vl53l0x(i2cBus, (byte)Vl53l0x.Addresses.Default);

            _bluetoothHandler = new BluetoothHandler(Device, _onboardLed);
            _bluetoothHandler.Initialize();

            return base.Initialize();
        }
    }
}