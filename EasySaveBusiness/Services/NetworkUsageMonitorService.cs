﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class NetworkUsageMonitorService
    {
        public event EventHandler? NetWorkUsageExceed;

        private readonly int _limit;
        private readonly IsNetworkUsageExceededService _isNetworkUsageExceeded = new IsNetworkUsageExceededService();
        private readonly string _interfaceName;
        private CancellationTokenSource? _cancellationTokenSource;
        private ManualResetEvent Systemmre { get; }
        public NetworkUsageMonitorService(EasySaveConfigService easySaveConfigService, ManualResetEvent systemmre)
        {
            _interfaceName = easySaveConfigService.EasySaveConfig.NetworkInterfaceName;
            _limit = easySaveConfigService.EasySaveConfig.SizeLimit;
            Systemmre = systemmre;
        }

        public void StartMonitoring()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => MonitorNetworkLimit(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task MonitorNetworkLimit(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isNetworkUsageExceeded.IsNetworkUsageLimitExceeded(_interfaceName,_limit))
                {
                  Systemmre.Set();
                }

                await Task.Delay(1000, cancellationToken); // Check every second
            }
        }

    }
}
