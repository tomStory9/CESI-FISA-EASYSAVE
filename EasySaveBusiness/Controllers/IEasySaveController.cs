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

        public void Init();

        public void AddBackupConfig(BackupConfig config);

        public void EditBackupConfig(BackupConfig config);

        public void OverrideBackupConfigs(List<BackupConfig> configs);

        public void RemoveBackupConfig(int id);

        public void StartBackupJob(int id);

        protected void OnBackupJobFullStateChanged(object? sender, List<BackupJobFullState> backupJobFullStates);
    }
}
