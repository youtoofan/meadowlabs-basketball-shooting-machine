using Meadow;
using UnitsNet;
using System;
using CommonFeather;
using FeatherV7.Domain.Models;

namespace FeatherV7.Domain.StateMachine
{
    internal sealed class NoBallState : State
    {
        public override string Name => "No ball";

        public NoBallState(BallShooterMachine ballShooterMachine)
            : base(ballShooterMachine)
        {
            Resolver.Log.Debug("No Ball");
        }

        internal override void Init()
        {
            BallShooterMachine.Graphics.ShowState("No ball");
            BallShooterMachine.Led.ShowNoBall();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            BallShooterMachine.Graphics.ShowRotatorScreen(delay);
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            BallShooterMachine.Graphics.ShowDistanceScreen(distance);

            if (distance <= Length.Zero || distance > Constants.Sensors.MINIMUM_SENSOR_DISTANCE)
                return;

            BallShooterMachine.SetState(BallShooterMachine.BallState);
        }

        internal override void ForceLaunch()
        {
            BallShooterMachine.Graphics.ShowState("FORBIDDEN");
        }
    }
}
