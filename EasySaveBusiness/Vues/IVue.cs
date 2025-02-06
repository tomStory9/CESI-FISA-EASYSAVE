using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Vues
{
    public interface IVue
    {
        void DisplayMessage(string message);
        void DisplayError(string errorMessage);
        void DisplayBackupConfigs(Dictionary<int, BackupConfig> backupConfigs);
        void DisplayBackupJobFullState(BackupJobFullState backupJobFullState);
    }
}
