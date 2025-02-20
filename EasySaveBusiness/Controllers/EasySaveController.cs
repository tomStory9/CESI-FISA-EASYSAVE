using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using EasySaveBusiness.Views;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Controllers
{
    public class EasySaveController(
                BackupConfigService backupConfigService,
                BackupJobsService backupJobsService,
                LoggerService loggerService,
                BackupFullStateLogger backupFullStateLogger
            )
    {
        public IView View { private get; set; }

        private readonly BackupConfigService _backupConfigService = backupConfigService;
        private readonly BackupJobsService _backupJobsService = backupJobsService;
        private readonly LoggerService _loggerService = loggerService;
        private readonly BackupFullStateLogger _backupFullStateLogger = backupFullStateLogger;

        public void Init()
        {
            // View.Init();
            _backupJobsService.BackupJobFullStatesChanged += OnBackupJobFullStateChanged;
            View.RefreshBackupConfigs(_backupConfigService.BackupConfigs);
            View.RefreshBackupJobFullStates(_backupJobsService.BackupJobFullStates);
        }

        public void AddBackupConfig(BackupConfig config)
        {
            var id = _backupConfigService.BackupConfigs.Count != 0
                ? _backupConfigService.BackupConfigs.Max(bc => bc.Id) + 1
                : 1;
            config.Id = id;
            _backupConfigService.AddBackupConfig(config);
        }

        public void RemoveBackupConfig(int id)
        {
            _backupConfigService.RemoveBackupConfig(id);
        }

        public void StartBackupJob(int id)
        {
            _backupJobsService.BackupJobs.First(job => job.Key == id).Value.Start();
        }

        private void OnBackupJobFullStateChanged(object? sender, List<BackupJobFullState> backupJobFullStates)
        {
            View.RefreshBackupJobFullStates(backupJobFullStates);
        }
    }
}
