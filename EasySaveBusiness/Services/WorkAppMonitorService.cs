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
        private ManualResetEvent Systemmre { get; }
        public WorkAppMonitorService(string workAppName, ManualResetEvent systemmre)
        {
            _isRunningWorkAppService = new IsRunningWorkAppService();
            _workAppName = workAppName;
            Systemmre = systemmre;
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
                    Systemmre.Set();
                }

                await Task.Delay(1000, cancellationToken); // Check every second
            }
        }
    }
}
