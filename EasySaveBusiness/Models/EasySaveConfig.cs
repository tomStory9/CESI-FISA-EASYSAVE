using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveBusiness.Services;
using LoggerDLL.Models;
namespace EasySaveBusiness.Models
{
    public class EasySaveConfig
    {
        public List<BackupConfig> BackupConfigs { get; }
        public string WorkApp { get; }
        public List<string> PriorityFileExtension { get; }
        public int NetworkKoLimit { get; }
        public string NetworkInterfaceName { get; }
        public LoggerDLL.Models.LogType.LogTypeEnum LogType { get; }
        public int SizeLimit { get; }
        public string Key { get; }

        public EasySaveConfig(
            List<BackupConfig> backupConfigs,
            string workApp,
            LoggerDLL.Models.LogType.LogTypeEnum logType,
            List<string> priorityFileExtension,
            int networkKoUsageLimit,
            string networkInterfaceName,
            int sizeLimit,
            string key
        )
        {
            BackupConfigs = backupConfigs;
            WorkApp = workApp;
            LogType = logType;
            PriorityFileExtension = priorityFileExtension;
            NetworkKoLimit = networkKoUsageLimit;
            NetworkInterfaceName = networkInterfaceName;
            SizeLimit = sizeLimit;
            Key = key;
        }
    }

}
