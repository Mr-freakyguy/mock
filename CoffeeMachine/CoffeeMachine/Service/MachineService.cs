using CoffeeMachine.Enum;
using CoffeeMachine.Logger;
using CoffeeMachine.Model;

namespace CoffeeMachine.Service
{
    public class MachineService
    {
        private readonly ILogger _logger;
        private CancellationTokenSource? _cts;

        public Machine _Machine { get; }
        public event Action<MachineStatus, int>? CountdownTick;
        public event Action<string>? NotificationRaised;

        public MachineService(Machine machine, ILogger logger)
        {
            _Machine = machine;
            _logger = logger;

            _Machine.StatusChanged += status => _ = _logger.WriteAsync(LogLevel.INFO, $"Status changed to {status}");
            _Machine.LidChanged += HandleLidChanged;
        }

        private void HandleLidChanged(LidStatus lid)
        {
            _ = _logger.WriteAsync(LogLevel.INFO, $"Lid toggled to {lid}");

            if (lid == LidStatus.Open && _Machine.SystemStatus != MachineStatus.SafeMode)
            {
                _cts?.Cancel();
                _ = _logger.WriteAsync(LogLevel.ERROR, "Lid opened - shifting to Safe mode");
                _Machine.SystemStatus = MachineStatus.SafeMode;
            }
        }

        public async Task InitializeAsync()
        {
            _Machine.SystemStatus = MachineStatus.SafeMode;
            _Machine.LidStatus = LidStatus.Open;
            await _logger.WriteAsync(LogLevel.INFO, "Coffee machine Initialized");
        }

        public Task ToggleLidAsync()
        {
            _Machine.LidStatus = _Machine.LidStatus == LidStatus.Open ? LidStatus.Closed : LidStatus.Open;
            return Task.CompletedTask;
        }

        public async Task ResetMachineAsync()
        {
            if (_Machine.LidStatus == LidStatus.Open)
            {
                RaiseNotification("Cannot reset: lid is open.");
                await _logger.WriteAsync(LogLevel.ERROR, "Reset failed: lid open");
                return;
            }

            _Machine.SystemStatus = MachineStatus.Ready;
        }

        public async Task StartBrewSequenceAsync()
        {
            if (_Machine.SystemStatus != MachineStatus.Ready || _Machine.LidStatus != LidStatus.Closed)
            {
                RaiseNotification("Cannot start brew: machine must be Ready with the lid closed.");
                return;
            }

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                _Machine.SystemStatus = MachineStatus.PreHeat;
                await RunCountdownAsync(10, MachineStatus.PreHeat, token);

                _Machine.SystemStatus = MachineStatus.PreInfusion;
                await RunCountdownAsync(10, MachineStatus.PreInfusion, token);

                _Machine.SystemStatus = MachineStatus.Brewing;
                await Task.Delay(Timeout.Infinite, token);
            }
            catch (OperationCanceledException)
            {
                // Expected: Stop or SimulateError cancels the token to end the sequence
            }
        }

        public async Task StopBrewSequenceAsync()
        {
            if (_Machine.SystemStatus is not (MachineStatus.PreHeat or MachineStatus.PreInfusion or MachineStatus.Brewing))
            {
                RaiseNotification("No active brew sequence to stop.");
                return;
            }

            _cts?.Cancel();
            await _logger.WriteAsync(LogLevel.WARN, "Brew sequence stopped by user");
            _Machine.SystemStatus = MachineStatus.Ready;
        }

        public async Task SimulateErrorAsync()
        {
            if (_Machine.SystemStatus != MachineStatus.Brewing)
            {
                RaiseNotification("Error simulation is only allowed while Brewing.");
                return;
            }

            _cts?.Cancel();
            await _logger.WriteAsync(LogLevel.ERROR, "Simulated machine error during brewing");
            _Machine.SystemStatus = MachineStatus.SafeMode;
        }

        public Task<IEnumerable<string>> ReadLogLinesAsync() => _logger.ReadAllAsync();

        private async Task RunCountdownAsync(int seconds, MachineStatus state, CancellationToken token)
        {
            for (int remaining = seconds; remaining > 0; remaining--)
            {
                CountdownTick?.Invoke(state, remaining);
                await Task.Delay(1000, token);
            }
            CountdownTick?.Invoke(state, 0);
        }

        private void RaiseNotification(string message) => NotificationRaised?.Invoke(message);
    }
}
