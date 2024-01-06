using DisplayTest.Domain.StateMachine;
using FeatherV7.Domain.Interfaces;
using FeatherV7.Domain.Models;
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
        public readonly IBluetoothHandler bluetoothHandler;

        public int CountDownValueInSeconds = 0;
        private State _currentState;

        public BallShooterMachine(IShooterDisplay graphics, IShooterSpeaker speaker, IShooterTrigger trigger, IShooterLed led, IBluetoothHandler bluetoothHandler)
        {
            this.Graphics = graphics;
            this.Speaker = speaker;
            this.Trigger = trigger;
            this.Led = led;

            this.bluetoothHandler = bluetoothHandler;
            this.bluetoothHandler.ButtonClicked += (s, e) => { this.Graphics.ShowState($"Button {e}"); };
            this.bluetoothHandler.RotationUpdated += (s, e) => { _currentState.SetLaunchDelay(TimeSpan.FromSeconds(e)); };
            this.bluetoothHandler.BlePaired += (s, e) => { this.Graphics.ShowState($"BLE {e}"); };
            this.bluetoothHandler.WifiEnabled += (s, e) => { this.Graphics.ShowState($"WIFI {e}"); };

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
            this._currentState = state;
            this._currentState.Init();
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
            bluetoothHandler.UpdateRotation(TimeSpan.FromSeconds(CountDownValueInSeconds));
        }

        internal void UpdateDistanceToObject(Length distance)
        {
            if (distance > Length.Zero)
                Resolver.Log.Debug($"{distance.Centimeters} cm");

            _currentState.UpdateDistanceToObject(distance);
            bluetoothHandler.UpdateDistance(distance);
        }

        internal void HandleButtonClicked()
        {
            Resolver.Log.Debug("Button clicked");
            bluetoothHandler.RotationClicked(true);
        }

        internal void HandleButtonReleased()
        {
            Resolver.Log.Debug("Button clicked");
            bluetoothHandler.RotationClicked(false);
        }
    }
}
