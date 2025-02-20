using EasySaveBusiness.Models;
using EasySaveBusiness.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Views
{
    public interface IView
    {
        public void Init();
        public void RefreshBackupConfigs(List<BackupConfig> backupConfigs);
        public void RefreshBackupJobFullStates(List<BackupJobFullState> backupJobFullState);
        public void DisplayMessage(string message);
        public void DisplayError(string errorMessage);
    }
}
