﻿using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.ViewModels
{
    public class EasySaveViewModel(BackupConfigService backupConfigService, BackupJobService backupService, LoggerService loggerService)
    {
        private readonly BackupConfigService _backupConfigService = backupConfigService;
        private readonly BackupJobService _backupService = backupService;
        private readonly LoggerService _loggerService = loggerService;

        public Dictionary<int, BackupConfig> GetBackupJobs()
        {
            return _backupConfigService.BackupConfigs;
        }
        public void AddBackupJob(BackupConfig job)
        {
            var id = _backupConfigService.BackupConfigs.Count != 0 ? _backupConfigService.BackupConfigs.Keys.Max() + 1 : 0;

            _loggerService.AddLog($"Adding backup job: {job.Name}");

            _backupConfigService.AddBackupJob(id, job);
        }

        public void RemoveBackupJob(int id)
        {
            _backupConfigService.RemoveBackupJob(id);
        }
    }
}
