using AsyncAwaitBestPractices;
using Meadow;
using UnitsNet;
using System;
using System.Threading;
using System.Threading.Tasks;
using CommonFeather;
using FeatherV7.Domain.Models;

namespace FeatherV7.Domain.StateMachine
{
    internal class LaunchingState : State
    {
        public override string Name => "Launching";
        private bool _isLaunching = false;

        private CancellationTokenSource _cancellationTokenSource;

        public LaunchingState(BallShooterMachine ballShooterMachine)
            : base(ballShooterMachine)
        {
            Resolver.Log.Debug("Launching");
        }

        internal override void Init()
        {
            if (_isLaunching)
                return;

            _cancellationTokenSource = new CancellationTokenSource();

            BallShooterMachine.Graphics.ShowState("Launching");
            BallShooterMachine.Led.ShowLaunching();
            StartCountDownSequenceAsync(_cancellationTokenSource.Token).SafeFireAndForget();
        }

        internal override void SetLaunchDelay(TimeSpan delay)
        {
            BallShooterMachine.Graphics.ShowRotatorScreen(delay);
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
            var totalSeconds = BallShooterMachine.CountDownValueInSeconds;

            var task = Task.Run(async () =>
            {
                try
                {
                    _isLaunching = true;

                    for (int i = totalSeconds; i > 0; i--)
                    {
                        BallShooterMachine.Speaker.PlayClickAsync().SafeFireAndForget();
                        BallShooterMachine.Graphics.ShowCountDownSequenceScreen(i);
                        BallShooterMachine.BluetoothHandler.UpdateStatus($"Launch in {i}...");

                        if (cancellationToken.IsCancellationRequested)
                        {
                            Resolver.Log.Error("Launch was cancelled.");

                            BallShooterMachine.BluetoothHandler.UpdateStatus("Launch cancelled");
                            BallShooterMachine.BluetoothHandler.LaunchTriggered(false);
                            BallShooterMachine.Graphics.ShowCancel();
                            BallShooterMachine.Led.ShowError();
                            BallShooterMachine.Speaker.PlayWarningAsync().SafeFireAndForget();

                            await Task.Delay(TimeSpan.FromSeconds(2));

                            BallShooterMachine.SetState(BallShooterMachine.ReadyState);

                            return;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    BallShooterMachine.BluetoothHandler.UpdateStatus("Ball away!");
                    BallShooterMachine.BluetoothHandler.LaunchTriggered(true);
                    BallShooterMachine.Graphics.ShowBoom();
                    BallShooterMachine.Speaker.PlayLaunchAsync().SafeFireAndForget();
                    BallShooterMachine.Trigger.ShootAsync().SafeFireAndForget();

                    Resolver.Log.Info("Launch successfull!");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    BallShooterMachine.SetState(BallShooterMachine.ReadyState);
                    _cancellationTokenSource.Dispose();

                    _isLaunching = false;
                }

            }, cancellationToken);

            await task;
        }

        internal override void ForceLaunch()
        {
            BallShooterMachine.Graphics.ShowState("FORBIDDEN");
        }
    }
}
