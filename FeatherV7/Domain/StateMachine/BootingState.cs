using Meadow;
using UnitsNet;
using System;
using System.Collections.Generic;
using System.Text;
using FeatherV7.Domain.Models;

namespace FeatherV7.Domain.StateMachine
{
    internal class BootingState : State
    {
        public override string Name => "Booting";

        public BootingState(BallShooterMachine ballShooterMachine)
            : base(ballShooterMachine)
        {
            Resolver.Log.Debug("Booting");
        }

        internal override void Init()
        {
            BallShooterMachine.Graphics.ShowState("Booting");
            BallShooterMachine.Led.ShowBooting();
            BallShooterMachine.Graphics.Init();

            BallShooterMachine.SetState(BallShooterMachine.ReadyState);
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
        }

        internal override void UpdateDistanceToObject(Length distance)
        {

        }

        internal override void ForceLaunch()
        {
            BallShooterMachine.Graphics.ShowState("FORBIDDEN");
        }
    }
}
