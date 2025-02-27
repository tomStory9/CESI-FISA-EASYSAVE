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
    public interface IEasySaveController
    {
        public IView View { protected get; set; }

        public Task Init();

        public Task AddBackupConfig(BackupConfig config);

        public Task EditBackupConfig(BackupConfig config);
        public Task OverrideEasySaveConfig(EasySaveConfig config);

        public Task OverrideBackupConfigs(List<BackupConfig> configs);

        public Task RemoveBackupConfig(int id);

        public Task StartBackupJob(int id);

        public Task PauseBackupJob(int id);

        public Task StopBackupJob(int id);

        protected void OnBackupJobFullStateChanged(object? sender, List<BackupJobFullState> backupJobFullStates);
    }
}
