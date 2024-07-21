using FeatherV7.Domain.Interfaces;
using Meadow;
using Meadow.Foundation.Relays;
using Meadow.Hardware;
using Meadow.Peripherals.Relays;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FeatherV7.Domain.Models
{

    internal class Trigger : IShooterTrigger
    {
        private IDigitalOutputPort OutputPort { get; }

        public Trigger(IPin pin)
        {
            OutputPort = pin.CreateDigitalOutputPort(true);
        }

        public async Task ShootAsync()
        {
            OutputPort.State = false;
            await Task.Delay(TimeSpan.FromSeconds(1));
            OutputPort.State = true;
        }
    }
}
