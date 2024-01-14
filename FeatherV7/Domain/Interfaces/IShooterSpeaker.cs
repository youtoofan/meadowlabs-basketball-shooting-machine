using System.Threading.Tasks;

namespace FeatherV7.Domain.Interfaces
{
    internal interface IShooterSpeaker
    {
        Task PlayClickAsync();
        Task PlayBuzzAsync();
        Task PlayOkAsync();
        Task PlayWarningAsync();
        Task PlayLaunchAsync();
    }
}
