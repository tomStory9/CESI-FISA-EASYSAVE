using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using EasySaveBusiness.Vues;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.ViewModels
{
    public class EasySaveViewModel(
        BackupConfigService backupConfigService,
        BackupJobService backupService,
        LoggerService loggerService
       )
    {
        private readonly BackupConfigService _backupConfigService = backupConfigService;
        private readonly BackupJobService _backupService = backupService;
        private readonly LoggerService _loggerService = loggerService;
        public IView View { private get; set; }

        public void Init()
        {
            View!.Init();

            var userChoice = View.GetUserChoice();

            userChoice switch
            {
                UserChoice.ListBackupConfigs => () =>
                {
                    View!.DisplayBackupConfigs(GetBackupConfigs());
                }
                ,
                UserChoice.AddBackupConfig addBackupConfig => () =>
                {
                    AddBackupConfig(addBackupConfig.Config);
                }
                ,
                UserChoice.RemoveBackupConfig removeBackupConfig => () =>
                {
                    RemoveBackupConfig(removeBackupConfig.Id);
                }
                ,
                UserChoice.ExecuteBackups executeBackups => () =>
                {
                    ExecuteBackups(executeBackups.Ids);
                },
                _ => () => { throw new NotImplementedException(); }

            };


        }

        public Dictionary<int, BackupConfig> GetBackupConfigs()
        {
            return _backupConfigService.BackupConfigs;
        }
        public void AddBackupConfig(BackupConfig config)
        {
            var id = _backupConfigService.BackupConfigs.Count != 0 ? _backupConfigService.BackupConfigs.Keys.Max() + 1 : 0;

            _backupConfigService.AddBackupConfig(id, config);
        }

        public void RemoveBackupConfig(int id)
        {
            _backupConfigService.RemoveBackupConfig(id);
        }

        public void ExecuteBackups(IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                var backupConfig = _backupConfigService.BackupConfigs[id] ?? throw new Exception("Unknown backup id");

                _backupService.ExecuteBackup(backupConfig);
            }
        }
    }
}
