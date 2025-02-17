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
        LoggerService loggerService,
        BackupFullStateLogger backupFullStateLogger,
        BackupStateManagerService backupStateManagerService
       )
    {
        private readonly BackupConfigService _backupConfigService = backupConfigService;
        private readonly BackupJobService _backupService = backupService;
        private readonly LoggerService _loggerService = loggerService;
        private readonly BackupFullStateLogger _backupFullStateLogger = backupFullStateLogger;
        private readonly BackupStateManagerService _backupStateManagerService = backupStateManagerService;
        public IView View { private get; set; }

        public async Task InitAsync()
        {
            View!.Init();

            _backupService.BackupJobFullStateChanged += OnBackupJobFullStateChanged;
            _backupService.BackupJobFullStateChanged += _backupStateManagerService.OnBackupFullStateChanged;

            var userChoice = View.GetUserChoice();

            switch (userChoice)
            {
                case UserChoice.ListBackupConfigs:
                    View!.DisplayBackupConfigs(GetBackupConfigs());
                    break;

                case UserChoice.AddBackupConfig addBackupConfig:
                    AddBackupConfig(addBackupConfig.Config);
                    break;

                case UserChoice.RemoveBackupConfig removeBackupConfig:
                    RemoveBackupConfig(removeBackupConfig.Id);
                    break;

                case UserChoice.ExecuteBackups executeBackups:
                    await ExecuteBackupsAsync(executeBackups.Ids);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public List<BackupConfig> GetBackupConfigs()
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

        public async Task ExecuteBackupsAsync(IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                var backupConfig = _backupConfigService.BackupConfigs[id] ?? throw new Exception("Unknown backup id");
                var config = _backupConfigService.EasySaveConfig;
                await _backupService.ExecuteBackupAsync(backupConfig,config);
            }
        }
        private void OnBackupJobFullStateChanged(object? sender, BackupJobFullState e)
        {
            View.DisplayBackupJobFullState(e);
        }
    }
}
