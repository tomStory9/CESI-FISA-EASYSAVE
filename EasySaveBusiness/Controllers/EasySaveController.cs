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
        private readonly NetworkUsageMonitorService _networkUsageMonitorService;
        private readonly WorkAppMonitorService _workAppMonitorService;

        public EasySaveController(
            EasySaveConfigService backupConfigService,
            BackupJobsService backupJobsService,
            LoggerService loggerService,
            BackupFullStateLogger backupFullStateLogger,
            NetworkUsageMonitorService networkUsageMonitorService,
            WorkAppMonitorService workAppMonitorService

        )
        {
            _backupConfigService = backupConfigService;
            _backupJobsService = backupJobsService;
            _loggerService = loggerService;
            _backupFullStateLogger = backupFullStateLogger;
            _networkUsageMonitorService = networkUsageMonitorService;
            _workAppMonitorService = workAppMonitorService;
        }

        public async Task Init()
        {
            // View.Init();
            _backupJobsService.BackupJobFullStatesChanged += OnBackupJobFullStateChanged;
            await View.RefreshBackupConfigs(_backupConfigService.BackupConfigs);
            await View.RefreshBackupJobFullStates(_backupJobsService.BackupJobFullStates);
            _workAppMonitorService.StartMonitoring();
            _networkUsageMonitorService.StartMonitoring();
        }

        public async Task AddBackupConfig(BackupConfig config)
        {
            var id = _backupConfigService.BackupConfigs.Count != 0
                ? _backupConfigService.BackupConfigs.Max(bc => bc.Id) + 1
                : 1;
            _backupConfigService.AddBackupConfig(config with { Id = id });
            await View.RefreshBackupConfigs(_backupConfigService.BackupConfigs);
        }

        public async Task EditBackupConfig(BackupConfig config)
        {
            _backupConfigService.EditBackupConfig(config);
            await View.RefreshBackupConfigs(_backupConfigService.BackupConfigs);
        }

        public async Task OverrideBackupConfigs(List<BackupConfig> configs)
        {
            _backupConfigService.OverrideBackupConfigs(configs);
            await View.RefreshBackupConfigs(_backupConfigService.BackupConfigs);
        }

        public async Task RemoveBackupConfig(int id)
        {
            _backupConfigService.RemoveBackupConfig(id);
            await View.RefreshBackupConfigs(_backupConfigService.BackupConfigs);
        }

        public async Task StartBackupJob(int id)
        {
            _backupJobsService.BackupJobs.First(job => job.Key == id).Value.Start();
            await Task.CompletedTask;
        }

        public void OnBackupJobFullStateChanged(object? sender, List<BackupJobFullState> backupJobFullStates)
        {
            View.RefreshBackupJobFullStates(backupJobFullStates);
        }
    }
}
