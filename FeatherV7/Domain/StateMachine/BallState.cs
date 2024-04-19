using Meadow;
using UnitsNet;
using System;
using System.Collections.Generic;
using System.Text;
using CommonFeather;
using AsyncAwaitBestPractices;
using System.Threading.Tasks;
using FeatherV7.Domain.Models;

namespace FeatherV7.Domain.StateMachine
{
    internal class BallState : State
    {
        public override string Name => "Ball detected";
        public BallState(BallShooterMachine ballShooterMachine)
            : base(ballShooterMachine)
        {
            Resolver.Log.Debug("Ball");
        }

        internal override void Init()
        {
            BallShooterMachine.Graphics.ShowState("Ball");
            BallShooterMachine.Led.ShowBall();
            BallShooterMachine.Speaker.PlayBuzzAsync();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            BallShooterMachine.Graphics.ShowRotatorScreen(delay);
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            // one final check to avoid false positive readings.
            if (distance <= Length.Zero || distance > Constants.Sensors.MINIMUM_SENSOR_DISTANCE)
            {
                Resolver.Log.Info("Ball is out-of-bounds again.");
                BallShooterMachine.SetState(BallShooterMachine.NoBallState);
            }

            if (BallShooterMachine.CountDownValueInSeconds >= Constants.Sensors.MINIMUM_COUNTDOWN_SECONDS)
            {
                Resolver.Log.Info("Correct distance confirmed.");
                BallShooterMachine.SetState(BallShooterMachine.LaunchingState);
            }
        }

        internal override async void ForceLaunch()
        {
            BallShooterMachine.Graphics.ShowBoom();
            BallShooterMachine.Speaker.PlayLaunchAsync().SafeFireAndForget();
            BallShooterMachine.Trigger.ShootAsync().SafeFireAndForget();

            Resolver.Log.Error("Launch successfull!");

            await Task.Delay(TimeSpan.FromSeconds(3));
            BallShooterMachine.SetState(BallShooterMachine.ReadyState);
        }
    }
}
