using AsyncAwaitBestPractices;
using FeatherV7.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnitsNet;

namespace FeatherV7.Domain.StateMachine
{
    internal sealed class ErrorState : State
    {
        public ErrorState(BallShooterMachine ballShooterMachine) : base(ballShooterMachine)
        {
        }

        public override string Name => "ErrorState";

        internal override void ForceLaunch()
        {
            BallShooterMachine.Graphics.ShowState("ERROR");
            BallShooterMachine.Led.ShowError();
            BallShooterMachine.Speaker.PlayWarningAsync().SafeFireAndForget();
        }

        internal override void Init()
        {
            BallShooterMachine.Graphics.ShowState("ERROR");
            BallShooterMachine.Led.ShowError();
            BallShooterMachine.Speaker.PlayWarningAsync().SafeFireAndForget();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            BallShooterMachine.Graphics.ShowState("ERROR");
            BallShooterMachine.Led.ShowError();
            BallShooterMachine.Speaker.PlayWarningAsync().SafeFireAndForget();
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            BallShooterMachine.Graphics.ShowState("ERROR");
            BallShooterMachine.Led.ShowError();
            BallShooterMachine.Speaker.PlayWarningAsync().SafeFireAndForget();
        }
    }
}
