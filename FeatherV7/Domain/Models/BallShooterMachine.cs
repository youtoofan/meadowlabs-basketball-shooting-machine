using FeatherV7.Domain.Interfaces;
using FeatherV7.Domain.StateMachine;
using Meadow;
using Meadow.Peripherals;
using System;
using UnitsNet;

namespace FeatherV7.Domain.Models
{
    internal sealed class BallShooterMachine
    {
        public readonly State BootingState;
        public readonly State LaunchingState;
        public readonly State NoBallState;
        public readonly State BallState;
        public readonly State ReadyState;
        public readonly State SoftwareUpdateState;
        public readonly State ErrorState;

        public readonly IShooterDisplay Graphics;
        public readonly IShooterSpeaker Speaker;
        public readonly IShooterTrigger Trigger;
        public readonly IShooterLed Led;
        public readonly IBluetoothHandler BluetoothHandler;

        public int CountDownValueInSeconds = 0;
        private State _currentState;

        public BallShooterMachine(IShooterDisplay graphics, IShooterSpeaker speaker, IShooterTrigger trigger, IShooterLed led, IBluetoothHandler bluetoothHandler)
        {
            Graphics = graphics;
            Speaker = speaker;
            Trigger = trigger;
            Led = led;

            BluetoothHandler = bluetoothHandler;
            BluetoothHandler.ButtonClicked += (s, e) => { Graphics.ShowState($"Button {e}"); };
            BluetoothHandler.RotationUpdated += (s, e) => { _currentState.SetLaunchDelay(TimeSpan.FromSeconds(e)); };
            BluetoothHandler.BlePaired += (s, e) => { Graphics.ShowState($"BLE {e}"); };
            BluetoothHandler.WifiEnabled += (s, e) => { Graphics.ShowState($"WIFI {e}"); };
            BluetoothHandler.Launched += (s, e) => { _currentState.ForceLaunch(); };

            BootingState = new BootingState(this);
            LaunchingState = new LaunchingState(this);
            NoBallState = new NoBallState(this);
            BallState = new BallState(this);
            ReadyState = new ReadyState(this);
            SoftwareUpdateState = new SoftwareUpdateState(this);
            ErrorState = new ErrorState(this);

            _currentState = BootingState;
        }

        internal void Start()
        {
            _currentState.Init();
        }

        internal void SetState(State state)
        {
            if (state.Name == _currentState.Name)
                return;

            var name = state.Name;
            _currentState = state;
            _currentState.Init();

            BluetoothHandler.UpdateStatus(name);
        }

        internal void HandleRotationDirection(RotationDirection rotationDirection)
        {
            switch (rotationDirection)
            {
                case RotationDirection.Clockwise:
                    CountDownValueInSeconds++;
                    break;
                case RotationDirection.CounterClockwise:
                    CountDownValueInSeconds--;
                    break;
            }

            _currentState.SetLaunchDelay(TimeSpan.FromSeconds(CountDownValueInSeconds));
            BluetoothHandler.UpdateRotation(TimeSpan.FromSeconds(CountDownValueInSeconds));
        }

        internal void UpdateDistanceToObject(Length distance)
        {
            if (distance > Length.Zero)
                Resolver.Log.Debug($"{distance.Centimeters} cm");

            _currentState.UpdateDistanceToObject(distance);
            BluetoothHandler.UpdateDistance(distance);
        }

        internal void HandleButtonClicked()
        {
            _currentState.ForceLaunch();
            Resolver.Log.Info("Button clicked");
            BluetoothHandler.RotationClicked(true);
        }

        internal void HandleButtonReleased()
        {
            Resolver.Log.Info("Button released");
            BluetoothHandler.RotationClicked(false);
        }
    }
}
