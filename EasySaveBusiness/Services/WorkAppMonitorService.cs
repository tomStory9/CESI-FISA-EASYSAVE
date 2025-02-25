using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class WorkAppMonitorService
    {
        public event EventHandler? WorkAppStopped;

        private readonly string _workAppName;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly IsRunningWorkAppService _isRunningWorkAppService;

        public WorkAppMonitorService(string workAppName)
        {
            _isRunningWorkAppService = new IsRunningWorkAppService();
            _workAppName = workAppName;
        }

        public void StartMonitoring()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => MonitorWorkAppAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task MonitorWorkAppAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isRunningWorkAppService.IsRunning(_workAppName))
                {
                    WorkAppStopped?.Invoke(this, EventArgs.Empty);
                }

                await Task.Delay(1000, cancellationToken); // Check every second
            }
        }
    }
}
