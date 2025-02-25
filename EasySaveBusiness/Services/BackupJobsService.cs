using EasySaveBusiness.Models;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class BackupJobsService
    {
        public event EventHandler<List<BackupJobFullState>>? BackupJobFullStatesChanged;

        public Dictionary<int, BackupJobService> BackupJobs { get; }

        public List<BackupJobFullState> BackupJobFullStates { get; private set; }
        private ManualResetEvent Systemmre { get; }

        public BackupJobsService(
            EasySaveConfigService backupConfigService,
            LoggerService loggerService,
            FileProcessingService fileProcessingService,
            WorkAppMonitorService workAppMonitorService,
            ManualResetEvent systemmre
        )
        {
            BackupJobs = backupConfigService.BackupConfigs.Select(backupConfig =>
            {
                var job = new BackupJobService(loggerService, backupConfig, backupConfigService.EasySaveConfig, fileProcessingService, workAppMonitorService , Systemmre);
                job.BackupJobFullStateChanged += OnBackupJobFullStateChanged;
                return new KeyValuePair<int, BackupJobService>(backupConfig.Id, job);
            }).ToDictionary(x => x.Key, x => x.Value);

            BackupJobFullStates = [.. BackupJobs.Values.Select(job => job.FullState)];
            BackupJobFullStatesChanged?.Invoke(this, BackupJobFullStates);
            Systemmre = systemmre;
        }

        private void OnBackupJobFullStateChanged(object? sender, BackupJobFullState e)
        {
            BackupJobFullStates = [.. BackupJobs.Values.Select(job => job.FullState)];
            BackupJobFullStatesChanged?.Invoke(this, BackupJobFullStates);
        }
    }
}
