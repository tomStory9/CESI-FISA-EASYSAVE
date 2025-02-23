using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using EasySaveBusiness.Views;
using LoggerDLL.Services;

namespace EasySaveBusiness.Controllers
{
    public class EasySaveController : IEasySaveController
    {
        public IView View { get; set; }

        private readonly EasySaveConfigService _backupConfigService;
        private readonly BackupJobsService _backupJobsService;
        private readonly LoggerService _loggerService;
        private readonly BackupFullStateLogger _backupFullStateLogger;

        public EasySaveController(
            EasySaveConfigService backupConfigService,
            BackupJobsService backupJobsService,
            LoggerService loggerService,
            BackupFullStateLogger backupFullStateLogger
        )
        {
            _backupConfigService = backupConfigService;
            _backupJobsService = backupJobsService;
            _loggerService = loggerService;
            _backupFullStateLogger = backupFullStateLogger;
        }

        public void Init()
        {
            // View.Init();
            _backupJobsService.BackupJobFullStatesChanged += OnBackupJobFullStateChanged;
            View.RefreshBackupConfigs(_backupConfigService.BackupConfigs);
            View.RefreshBackupJobFullStates(_backupJobsService.BackupJobFullStates);
        }

        public void AddBackupConfig(BackupConfig config)
        {
            var id = _backupConfigService.BackupConfigs.Count != 0
                ? _backupConfigService.BackupConfigs.Max(bc => bc.Id) + 1
                : 1;
            _backupConfigService.AddBackupConfig(config with { Id = id });
        }

        public void EditBackupConfig(BackupConfig config)
        {
            _backupConfigService.EditBackupConfig(config);
        }

        public void OverrideBackupConfigs(List<BackupConfig> configs)
        {
            _backupConfigService.OverrideBackupConfigs(configs);
        }

        public void RemoveBackupConfig(int id)
        {
            _backupConfigService.RemoveBackupConfig(id);
        }

        public void StartBackupJob(int id)
        {
            _backupJobsService.BackupJobs.First(job => job.Key == id).Value.Start();
        }

        public void OnBackupJobFullStateChanged(object? sender, List<BackupJobFullState> backupJobFullStates)
        {
            View.RefreshBackupJobFullStates(backupJobFullStates);
        }
    }
}
