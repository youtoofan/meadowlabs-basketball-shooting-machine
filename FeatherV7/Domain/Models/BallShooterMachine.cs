using DisplayTest.Domain.StateMachine;
using Meadow;
using Meadow.Peripherals;
using Meadow.Units;
using System;

namespace DisplayTest.Domain.Models
{
    internal sealed class BallShooterMachine
    {
        public static readonly Length MINIMUM_SENSOR_DISTANCE = new Length(10, Length.UnitType.Centimeters);
        public static readonly int MINIMUM_SENSOR_DISTANCE_COUNTS = 5;
        public static readonly int MINIMUM_COUNTDOWN_SECONDS = 1;
        public static readonly TimeSpan SENSOR_DISTANCE_READ_FREQUENCY = TimeSpan.FromMilliseconds(500);

        public readonly State BootingState;
        public readonly State LaunchingState;
        public readonly State NoBallState;
        public readonly State BallState;
        public readonly State ReadyState;
        
        public readonly IShooterDisplay Graphics;
        public readonly IShooterSpeaker Speaker;
        public readonly IShooterTrigger Trigger;
        public readonly IShooterLed Led;

        public int CountDownValueInSeconds = 0;
        private State _currentState;

        public BallShooterMachine(IShooterDisplay graphics, IShooterSpeaker speaker, IShooterTrigger trigger, IShooterLed led)
        {
            this.Graphics = graphics;
            this.Speaker = speaker;
            this.Trigger = trigger;
            this.Led = led;

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
        }

        internal void UpdateDistanceToObject(Length distance)
        {
            if (distance > Length.Zero)
                Resolver.Log.Debug($"{distance.Centimeters} cm");

            _currentState.UpdateDistanceToObject(distance);
        }

        internal void HandleButtonClicked()
        {
            Resolver.Log.Debug("Button clicked");
        }
    }
}
