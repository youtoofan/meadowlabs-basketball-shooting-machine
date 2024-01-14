using AsyncAwaitBestPractices;
using DisplayTest.Domain.Models;
using Meadow;
using UnitsNet;
using System;
using System.Threading;
using System.Threading.Tasks;
using CommonFeather;

namespace DisplayTest.Domain.StateMachine
{
    internal class LaunchingState : State
    {
        public override string Name => "Launching";

        private CancellationTokenSource _cancellationTokenSource;

        public LaunchingState(BallShooterMachine ballShooterMachine) 
            : base(ballShooterMachine)
        {
            Resolver.Log.Debug("Launching");
        }

        internal override void Init()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            this.BallShooterMachine.Graphics.ShowState("Launching");
            this.BallShooterMachine.Led.ShowLaunching();
            this.StartCountDownSequenceAsync(_cancellationTokenSource.Token).SafeFireAndForget();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            this.BallShooterMachine.Graphics.ShowRotatorScreen(delay);
        }

        internal override void UpdateDistanceToObject(Length distance)
        {
            if (distance <= Length.Zero || distance > Constants.Sensors.MINIMUM_SENSOR_DISTANCE)
            {
                _cancellationTokenSource.Cancel();
                return;
            }
        }

        private async Task StartCountDownSequenceAsync(CancellationToken cancellationToken)
        {
            var totalSeconds = this.BallShooterMachine.CountDownValueInSeconds;

            var task = Task.Run(async () =>
            {
                for (int i = totalSeconds ; i > 0; i--)
                {
                    this.BallShooterMachine.Speaker.PlayClickAsync().SafeFireAndForget();
                    this.BallShooterMachine.Graphics.ShowCountDownSequenceScreen(i);
                    this.BallShooterMachine.BluetoothHandler.UpdateStatus($"Launch in {i}...");

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Resolver.Log.Error("Launch was cancelled.");

                        this.BallShooterMachine.BluetoothHandler.UpdateStatus("Launch cancelled");
                        this.BallShooterMachine.BluetoothHandler.LaunchTriggered(false);
                        this.BallShooterMachine.Graphics.ShowCancel();
                        this.BallShooterMachine.Led.ShowError();
                        this.BallShooterMachine.Speaker.PlayWarningAsync().SafeFireAndForget();
                        
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        
                        this.BallShooterMachine.SetState(this.BallShooterMachine.ReadyState);

                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                this.BallShooterMachine.BluetoothHandler.UpdateStatus("Ball away!");
                this.BallShooterMachine.BluetoothHandler.LaunchTriggered(true);
                this.BallShooterMachine.Graphics.ShowBoom();
                this.BallShooterMachine.Speaker.PlayLaunchAsync().SafeFireAndForget();
                this.BallShooterMachine.Trigger.ShootAsync().SafeFireAndForget();

                Resolver.Log.Error("Launch successfull!");

            }, cancellationToken);

            await task;
            await Task.Delay(TimeSpan.FromSeconds(3));
            this.BallShooterMachine.SetState(this.BallShooterMachine.ReadyState);
        }

        internal override void ForceLaunch()
        {
            this.BallShooterMachine.Graphics.ShowState("FORBIDDEN");
        }
    }
}
