using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveBusiness.Models;
namespace EasySaveBusiness.Services
{
    class BackupStateManagerService
    {
        private BackupFullStateLogger BackupFullStateLogger { get; }
        private BackupConfigService BackupConfigService { get; }
        public List<BackupJobFullState> BackupJobFullStates { get; }

        public BackupStateManagerService(BackupFullStateLogger backupFullStateLogger, BackupConfigService backupConfigService)
        {
            BackupFullStateLogger = backupFullStateLogger;
            BackupConfigService = backupConfigService;

            // map backup job full states
            BackupJobFullStates = backupConfigService.BackupConfigs.Select((i, backupConfig) => new BackupJobFullState(backupConfig.Name);
        }
    }
}
