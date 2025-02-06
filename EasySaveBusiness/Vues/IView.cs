using EasySaveBusiness.Models;
using EasySaveBusiness.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Vues
{
    public interface IView
    {
        public void Init();
        public UserChoice GetUserChoice();
        public void DisplayMessage(string message);
        public void DisplayError(string errorMessage);
        public void DisplayBackupConfigs(Dictionary<int, BackupConfig> backupConfigs);
        public void DisplayBackupJobFullState(BackupJobFullState backupJobFullState);
    }
}
