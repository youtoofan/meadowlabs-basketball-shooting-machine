using AsyncAwaitBestPractices;
using FeatherV7.Domain.Models;
using Meadow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace FeatherV7.Domain.StateMachine
{
    internal class SoftwareUpdateState : State
    {
        public SoftwareUpdateState(BallShooterMachine ballShooterMachine) : base(ballShooterMachine)
        {
            RegisterUpdateService();
        }

        public override string Name => "Updating";

        internal override void ForceLaunch()
        {
            this.BallShooterMachine.Graphics.ShowState("FORBIDDEN");
        }

        internal override void Init()
        {
            BallShooterMachine.Speaker.PlayWarningAsync().SafeFireAndForget();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            
        }

        private void RegisterUpdateService()
        {
            var svc = Resolver.UpdateService;

            svc.ClearUpdates();

            svc.StateChanged += (sender, updateState) =>
            {
                BallShooterMachine.Graphics.ShowState($"{updateState}");
            };

            svc.UpdateAvailable += (updateService, info) =>
            {
                BallShooterMachine.SetState(BallShooterMachine.SoftwareUpdateState);

                BallShooterMachine.Graphics.ShowState($"Update available!");

                updateService.RetrieveUpdate(info);
            };

            svc.RetrieveProgress += (updateService, info) =>
            {
                short percentage = (short)((double)info.DownloadProgress / info.FileSize * 100);

                BallShooterMachine.Graphics.ShowState($"Downloading... {percentage}%");
            };

            svc.UpdateRetrieved += async (updateService, info) =>
            {
                BallShooterMachine.Graphics.ShowState($"Update retrieved!");

                await Task.Delay(5000);

                updateService.ApplyUpdate(info);
            };

            svc.UpdateSuccess += (s, e) =>
            {
                BallShooterMachine.SetState(BallShooterMachine.BootingState);
            };

            svc.UpdateFailure += (s, e) =>
            {
                BallShooterMachine.SetState(BallShooterMachine.BootingState);
            };
        }
    }
}
