using FeatherV7.Domain.Interfaces;
using Meadow;
using Meadow.Foundation.Relays;
using Meadow.Hardware;
using Meadow.Peripherals.Relays;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DisplayTest.Domain.Models
{

    internal class Trigger : Relay, IShooterTrigger
    {
        public Trigger(IDigitalOutputPort port) 
            : base(port, RelayType.NormallyOpen)
        {
        }

        public async Task ShootAsync()
        {
            if (this.IsOn)
            {
                Resolver.Log.Error("Relay already in ON-state");
                return;
            }

            this.Toggle();
            await Task.Delay(TimeSpan.FromSeconds(1));
            this.Toggle();
        }
    }
}
