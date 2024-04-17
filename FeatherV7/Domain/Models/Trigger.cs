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

    internal class Trigger :  IShooterTrigger
    {
        private IDigitalOutputPort OutputPort { get; }

        public Trigger(IPin pin) 
        {
            this.OutputPort = pin.CreateDigitalOutputPort(false);
            this.OutputPort.State = false;
        }

        public async Task ShootAsync()
        {
            this.OutputPort.State = true;
            await Task.Delay(TimeSpan.FromSeconds(1));
            this.OutputPort.State = false;
        }
    }
}
