using DisplayTest.Domain.StateMachine;
using FeatherV7.Domain.Interfaces;
using Meadow;
using Meadow.Peripherals;
using System;
using UnitsNet;

namespace DisplayTest.Domain.Models
{
    internal sealed class BallShooterMachine
    {
        public readonly State BootingState;
        public readonly State LaunchingState;
        public readonly State NoBallState;
        public readonly State BallState;
        public readonly State ReadyState;
        
        public readonly IShooterDisplay Graphics;
        public readonly IShooterSpeaker Speaker;
        public readonly IShooterTrigger Trigger;
        public readonly IShooterLed Led;
        public readonly IBluetoothHandler BluetoothHandler;

        public int CountDownValueInSeconds = 0;
        private State _currentState;

        public BallShooterMachine(IShooterDisplay graphics, IShooterSpeaker speaker, IShooterTrigger trigger, IShooterLed led, IBluetoothHandler bluetoothHandler)
        {
            this.Graphics = graphics;
            this.Speaker = speaker;
            this.Trigger = trigger;
            this.Led = led;

            this.BluetoothHandler = bluetoothHandler;
            this.BluetoothHandler.ButtonClicked += (s, e) => { this.Graphics.ShowState($"Button {e}"); };
            this.BluetoothHandler.RotationUpdated += (s, e) => { _currentState.SetLaunchDelay(TimeSpan.FromSeconds(e)); };
            this.BluetoothHandler.BlePaired += (s, e) => { this.Graphics.ShowState($"BLE {e}"); };
            this.BluetoothHandler.WifiEnabled += (s, e) => { this.Graphics.ShowState($"WIFI {e}"); };
            this.BluetoothHandler.Launched += (s, e) => { _currentState.ForceLaunch(); };

            this.BootingState = new BootingState(this);
            this.LaunchingState = new LaunchingState(this);
            this.NoBallState = new NoBallState(this);
            this.BallState = new BallState(this);
            this.ReadyState = new ReadyState(this);

            this._currentState = BootingState;
        }

        internal void Start()
        {
            _currentState.Init();
        }

        internal void SetState(State state)
        {
            var name = state.Name;
            this._currentState = state;
            this._currentState.Init();

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
