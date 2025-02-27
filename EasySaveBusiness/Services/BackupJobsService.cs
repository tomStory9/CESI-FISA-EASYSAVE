using EasySaveBusiness.Models;
using LoggerDLL.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasySaveBusiness.Services
{
    public class BackupJobsService
    {
        public event EventHandler<List<BackupJobFullState>>? BackupJobFullStatesChanged;

        public Dictionary<int, BackupJobService> BackupJobs { get; }

        public List<BackupJobFullState> BackupJobFullStates { get; private set; }
        private ManualResetEvent Systemmre { get; }
        private IServiceProvider _serviceProvider { get; }

        private System.Timers.Timer _backupJobFullStateBroadcastTimer;
        private bool _shouldBroadcastBackupJobFullState;

        public BackupJobsService(
            EasySaveConfigService backupConfigService,
            LoggerService loggerService,
            FileProcessingService fileProcessingService,
            WorkAppMonitorService workAppMonitorService,
            ManualResetEvent systemmre,
            IServiceProvider serviceProvider
        )
        {
            _serviceProvider = serviceProvider;
            Systemmre = systemmre;

            BackupJobs = backupConfigService.BackupConfigs
                .Select(backupConfig => CreateBackupJob(backupConfig))
                .ToDictionary(x => x.Key, x => x.Value);

            UpdateBackupJobFullStates();

            backupConfigService.BackupConfigAdded += OnBackupConfigAdded;
            backupConfigService.BackupConfigRemoved += OnBackupConfigRemoved;
            backupConfigService.BackupConfigEdited += OnBackupConfigEdited;

            _backupJobFullStateBroadcastTimer = new(500);
            _backupJobFullStateBroadcastTimer.Elapsed += (sender, e) => BroadcastBackupJobFullStateIfNeeded();
            _backupJobFullStateBroadcastTimer.Start();
        }

        private KeyValuePair<int, BackupJobService> CreateBackupJob(BackupConfig backupConfig)
        {
            var job = _serviceProvider.GetRequiredService<BackupJobService>();
            job.Init(backupConfig);
            job.BackupJobFullStateChanged += OnBackupJobFullStateChanged;
            return new KeyValuePair<int, BackupJobService>(backupConfig.Id, job);
        }

        private void OnBackupJobFullStateChanged(object? sender, BackupJobFullState e)
        {
            Console.WriteLine($"Backup job full state changed to {e.State}");
            _shouldBroadcastBackupJobFullState = true;
        }

        private void BroadcastBackupJobFullStateIfNeeded()
        {
            if (_shouldBroadcastBackupJobFullState)
            {
                UpdateBackupJobFullStates();
                _shouldBroadcastBackupJobFullState = false;
            }
        }

        private void OnBackupConfigAdded(object? sender, BackupConfig config)
        {
            var job = CreateBackupJob(config).Value;
            BackupJobs.Add(config.Id, job);
            UpdateBackupJobFullStates();
        }

        private void OnBackupConfigRemoved(object? sender, int id)
        {
            if (BackupJobs.TryGetValue(id, out var job))
            {
                job.BackupJobFullStateChanged -= OnBackupJobFullStateChanged;
                BackupJobs.Remove(id);
                UpdateBackupJobFullStates();
            }
        }

        private void OnBackupConfigEdited(object? sender, BackupConfig config)
        {
            if (BackupJobs.TryGetValue(config.Id, out var job))
            {
                job.BackupJobFullStateChanged -= OnBackupJobFullStateChanged;
                BackupJobs.Remove(config.Id);
            }
            var newJob = CreateBackupJob(config).Value;
            BackupJobs.Add(config.Id, newJob);
            UpdateBackupJobFullStates();
        }

        private void UpdateBackupJobFullStates()
        {
            BackupJobFullStates = BackupJobs.Values.Select(job => job.FullState).ToList();
            BackupJobFullStatesChanged?.Invoke(this, BackupJobFullStates);
        }
    }
}