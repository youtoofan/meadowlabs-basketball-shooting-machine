using DisplayTest.Domain.Models;
using Meadow;
using UnitsNet;
using System;
using System.Collections.Generic;
using System.Text;
using CommonFeather;

namespace DisplayTest.Domain.StateMachine
{
    internal class BallState : State
    {
        public BallState(BallShooterMachine ballShooterMachine) 
            : base(ballShooterMachine)
        {
            Resolver.Log.Debug("Ball");
        }

        internal override void Init()
        {
            this.BallShooterMachine.Graphics.ShowState("Ball");
            this.BallShooterMachine.Led.ShowBall();
            this.BallShooterMachine.Speaker.PlayBuzzAsync();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            this.BallShooterMachine.Graphics.ShowRotatorScreen(delay);
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            // one final check to avoid false positive readings.
            if (distance <= Length.Zero || distance > Constants.Sensors.MINIMUM_SENSOR_DISTANCE)
            {
                Resolver.Log.Info("Ball is out-of-bounds again.");
                this.BallShooterMachine.SetState(this.BallShooterMachine.NoBallState);
            }

            Resolver.Log.Info("Correct distance confirmed.");

            if (this.BallShooterMachine.CountDownValueInSeconds >= Constants.Sensors.MINIMUM_COUNTDOWN_SECONDS)
            {
                this.BallShooterMachine.SetState(this.BallShooterMachine.LaunchingState);
            }
        }
    }
}
