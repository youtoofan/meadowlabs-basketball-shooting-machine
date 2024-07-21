using Meadow;
using UnitsNet;
using System;
using CommonFeather;
using FeatherV7.Domain.Models;

namespace FeatherV7.Domain.StateMachine
{
    internal class ReadyState : State
    {
        private int _distanceCounts = 0;

        public override string Name => "Ready";

        public ReadyState(BallShooterMachine ballShooterMachine) : base(ballShooterMachine)
        {
            Resolver.Log.Debug("Ready");
        }

        internal override void Init()
        {
            BallShooterMachine.Graphics.ShowState("Ready");
            BallShooterMachine.Led.ShowReady();
            BallShooterMachine.Graphics.ShowCurrentTimeScreen(null);
            BallShooterMachine.BluetoothHandler.LaunchTriggered(false);
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            BallShooterMachine.Graphics.ShowRotatorScreen(delay);
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            if (_distanceCounts < Constants.Sensors.MINIMUM_SENSOR_DISTANCE_COUNTS)
            {
                _distanceCounts++;
                Resolver.Log.Info($"Received {_distanceCounts} of {Constants.Sensors.MINIMUM_SENSOR_DISTANCE_COUNTS} required distance updates.");
                return;
            }

            if (distance > Length.Zero && distance <= Constants.Sensors.MINIMUM_SENSOR_DISTANCE)
            {
                BallShooterMachine.SetState(BallShooterMachine.BallState);
            }
            else
            {
                BallShooterMachine.SetState(BallShooterMachine.NoBallState);
            }
        }

        internal override void ForceLaunch()
        {
            BallShooterMachine.Graphics.ShowState("FORBIDDEN");
        }
    }
}
