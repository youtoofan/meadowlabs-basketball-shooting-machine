﻿using DisplayTest.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnitsNet;

namespace DisplayTest.Domain.StateMachine
{
    internal abstract class State
    {
        protected BallShooterMachine BallShooterMachine { get; private set; }

        public State(BallShooterMachine ballShooterMachine)
        {
            BallShooterMachine = ballShooterMachine;
        }

        internal abstract void Init();

        internal abstract void SetLaunchDelay(TimeSpan delay);

        internal abstract void UpdateDistanceToObject(Length distance);
        
    }
}
