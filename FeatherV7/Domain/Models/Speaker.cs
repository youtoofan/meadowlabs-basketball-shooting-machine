using AsyncAwaitBestPractices;
using FeatherV7.Domain.Interfaces;
using Meadow.Foundation.Audio;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace FeatherV7.Domain.Models
{

    internal sealed class Speaker : PiezoSpeaker, IShooterSpeaker
    {
        public Speaker(IPin pin) : base(pin)
        {
        }

        public async Task PlayBuzzAsync()
        {
            await PlayTone(new Frequency(150f), TimeSpan.FromMilliseconds(10));
        }

        public async Task PlayClickAsync()
        {
            await PlayTone(new Frequency(1000f), TimeSpan.FromMilliseconds(10));
        }

        public async Task PlayLaunchAsync()
        {
            await PlayTone(new Frequency(123.47f), TimeSpan.FromMilliseconds(50));
        }

        public async Task PlayOkAsync()
        {
            await PlayTone(new Frequency(100f), TimeSpan.FromMilliseconds(50));
        }

        public async Task PlayWarningAsync()
        {
            await PlayTone(new Frequency(650f), TimeSpan.FromMilliseconds(40));
            await PlayTone(new Frequency(900f), TimeSpan.FromMilliseconds(60));
            await PlayTone(new Frequency(650f), TimeSpan.FromMilliseconds(40));
            await PlayTone(new Frequency(900f), TimeSpan.FromMilliseconds(60));
        }
    }
}
