using DisplayTest.Domain.Models;
using Meadow;
using UnitsNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisplayTest.Domain.StateMachine
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
            this.BallShooterMachine.Graphics.ShowState("Booting");
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
            this.BallShooterMachine.Graphics.ShowState("FORBIDDEN");
        }
    }
}
