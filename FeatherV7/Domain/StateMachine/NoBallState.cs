using DisplayTest.Domain.Models;
using Meadow;
using UnitsNet;
using System;
using CommonFeather;

namespace DisplayTest.Domain.StateMachine
{
    internal class NoBallState : State
    {
        public NoBallState(BallShooterMachine ballShooterMachine) 
            : base(ballShooterMachine)
        {
            Resolver.Log.Debug("No Ball");
        }

        internal override void Init()
        {
            this.BallShooterMachine.Graphics.ShowState("No ball");
            this.BallShooterMachine.Led.ShowNoBall();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            this.BallShooterMachine.Graphics.ShowRotatorScreen(delay);
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            if (distance <= Length.Zero || distance > Constants.Sensors.MINIMUM_SENSOR_DISTANCE)
                return;

            this.BallShooterMachine.SetState(this.BallShooterMachine.BallState);
        }

        internal override void ForceLaunch()
        {
            this.BallShooterMachine.Graphics.ShowState("FORBIDDEN");
        }
    }
}
