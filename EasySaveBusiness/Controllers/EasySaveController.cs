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
            await RefreshBackupJobs();
            await RefreshEasySaveConfig();
            _workAppMonitorService.StartMonitoring();
            _networkUsageMonitorService.StartMonitoring();
        }

        public async Task AddBackupConfig(BackupConfig config)
        {
            var id = _backupConfigService.BackupConfigs.Count != 0
                ? _backupConfigService.BackupConfigs.Max(bc => bc.Id) + 1
                : 1;
            _backupConfigService.AddBackupConfig(config with { Id = id });
            await RefreshBackupJobs();
            await RefreshEasySaveConfig();
        }

        public async Task EditBackupConfig(BackupConfig config)
        {
            _backupConfigService.EditBackupConfig(config);
            await RefreshBackupJobs();
            await RefreshEasySaveConfig();
        }

        public async Task OverrideEasySaveConfig(EasySaveConfig config)
        {
            _backupConfigService.OverrideEasySaveConfig(config);
            await RefreshEasySaveConfig();
        }

        public async Task OverrideBackupConfigs(List<BackupConfig> configs)
        {
            _backupConfigService.OverrideBackupConfigs(configs);
            await RefreshBackupJobs();
            await RefreshEasySaveConfig();
        }

        public async Task RemoveBackupConfig(int id)
        {
            _backupConfigService.RemoveBackupConfig(id);
            await RefreshBackupJobs();
            await RefreshEasySaveConfig();
        }

        public async Task StartBackupJob(int id)
        {
            _backupJobsService.BackupJobs.First(job => job.Key == id).Value.Start();
            await Task.CompletedTask;
        }
        public async Task PauseBackupJob(int id)
        {
            _backupJobsService.BackupJobs.First(job => job.Key == id).Value.Pause();
            await Task.CompletedTask;
        }

        public async Task StopBackupJob(int id)
        {
            _backupJobsService.BackupJobs.First(job => job.Key == id).Value.Stop();
            await Task.CompletedTask;
        }

        public void OnBackupJobFullStateChanged(object? sender, List<BackupJobFullState> backupJobFullStates)
        {
            View.RefreshBackupJobFullStates(backupJobFullStates);
        }

        private async Task RefreshBackupJobs()
        {
            await View.RefreshBackupJobFullStates(_backupJobsService.BackupJobFullStates);
        }

        private async Task RefreshEasySaveConfig()
        {
            await View.RefreshEasySaveConfig(_backupConfigService.EasySaveConfig);
        }
    }
}
