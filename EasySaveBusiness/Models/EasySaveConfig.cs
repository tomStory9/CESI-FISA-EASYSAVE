using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoggerDLL.Models;
namespace EasySaveBusiness.Models
{
    public class EasySaveConfig(Dictionary<int, BackupConfig> backupConfigs,string WorkApp, LoggerDLL.Models.LogType.LogTypeEnum LogType)
    {
        public Dictionary<int, BackupConfig> BackupConfigs { get; } = backupConfigs;
        public string WorkApp { get; } = WorkApp;

        public LoggerDLL.Models.LogType.LogTypeEnum LogType { get; } = LogType;

    }
}
