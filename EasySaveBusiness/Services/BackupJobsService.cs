using EasySaveBusiness.Models;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    class BackupJobsService
    {
        public event EventHandler<List<BackupJobFullState>>? BackupJobFullStatesChanged;

        public Dictionary<int, BackupJobService> BackupJobs { get; }

        public List<BackupJobFullState> BackupJobFullStates
        {
            get
            {
                return [.. BackupJobs.Values.Select(job => job.FullState)];
            }
            private set
            {
                BackupJobFullStatesChanged?.Invoke(this, value);
            }
        }

        public BackupJobsService(BackupConfigService backupConfigService, LoggerService loggerService, EasySaveConfig easySaveConfig)
        {
            BackupJobs = backupConfigService.BackupConfigs.Select(backupConfig =>
            {
                var job = new BackupJobService(loggerService, backupConfig, easySaveConfig);
                job.BackupJobFullStateChanged += OnBackupJobFullStateChanged;
                return new KeyValuePair<int, BackupJobService>(backupConfig.Id, job);
            }).ToDictionary(x => x.Key, x => x.Value);
        }

        private void OnBackupJobFullStateChanged(object? sender, BackupJobFullState e)
        {
            BackupJobFullStates = [.. BackupJobs.Values.Select(job => job.FullState)];
            BackupJobFullStatesChanged?.Invoke(this, BackupJobFullStates);
        }
    }
}
