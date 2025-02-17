using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoggerDLL.Models;
namespace EasySaveBusiness.Models
{
    public class EasySaveConfig
    {
        public List<BackupConfig> BackupConfigs { get; }
        public string WorkApp { get; }
        public LoggerDLL.Models.LogType.LogTypeEnum LogType { get; }

        public EasySaveConfig(List<BackupConfig> backupConfigs, string workApp, LoggerDLL.Models.LogType.LogTypeEnum logType)
        {
            BackupConfigs = backupConfigs;
            WorkApp = workApp;
            LogType = logType;
        }
    }

}
