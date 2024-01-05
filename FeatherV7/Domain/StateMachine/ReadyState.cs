using DisplayTest.Domain.Models;
using Meadow;
using Meadow.Logging;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisplayTest.Domain.StateMachine
{
    internal class ReadyState : State
    {
        private int _distanceCounts = 0;

        public ReadyState(BallShooterMachine ballShooterMachine) : base(ballShooterMachine)
        {
            Resolver.Log.Debug("Ready");
        }

        internal override void Init()
        {
            this.BallShooterMachine.Graphics.ShowState("Ready");
            this.BallShooterMachine.Led.ShowReady();
            this.BallShooterMachine.Graphics.ShowCurrentTimeScreen();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            this.BallShooterMachine.Graphics.ShowRotatorScreen(delay);
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            if (_distanceCounts < BallShooterMachine.MINIMUM_SENSOR_DISTANCE_COUNTS)
            {
                _distanceCounts++;
                Resolver.Log.Info($"Received {_distanceCounts} of {BallShooterMachine.MINIMUM_SENSOR_DISTANCE_COUNTS} required distance updates.");
                return;
            }

            if (distance > Length.Zero && distance <= BallShooterMachine.MINIMUM_SENSOR_DISTANCE)
            {
                this.BallShooterMachine.SetState(this.BallShooterMachine.BallState);
            }
            else
            {
                this.BallShooterMachine.SetState(this.BallShooterMachine.NoBallState);
            }
        }
    }
}
