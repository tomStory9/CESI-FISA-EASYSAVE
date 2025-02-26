using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

        [JsonConstructor]
        public EasySaveConfig(
            List<BackupConfig> backupConfigs,
            string workApp,
            List<string> priorityFileExtension,
            int networkKoLimit,
            string networkInterfaceName,
            LoggerDLL.Models.LogType.LogTypeEnum logType,
            int sizeLimit,
            string key
        )
        {
            BackupConfigs = backupConfigs;
            WorkApp = workApp;
            LogType = logType;
            PriorityFileExtension = priorityFileExtension;
            NetworkKoLimit = networkKoLimit;
            NetworkInterfaceName = networkInterfaceName;
            SizeLimit = sizeLimit;
            Key = key;
        }

        public static EasySaveConfig Defaults => new EasySaveConfig(
            new List<BackupConfig>(),
            "notepad.exe",
            new List<string> { ".txt" },
            1000,
            "Ethernet",
            LoggerDLL.Models.LogType.LogTypeEnum.JSON,
            1000,
            "password"
        );
    }

}
