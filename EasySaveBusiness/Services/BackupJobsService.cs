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
        public Dictionary<int,BackupJobService> BackupJobs { get; }

        public BackupJobsService(BackupConfigService backupConfigService, LoggerService loggerService)
        {

            BackupJobs = backupConfigService.BackupConfigs.Select(backupConfig => new KeyValuePair<int, BackupJobService>(
                backupConfig.Id,
                new BackupJobService(loggerService)
            )).ToDictionary(x => x.Key, x => x.Value);
        }

    }


}
