using EasySaveBusiness.Models;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class BackupJobService
    {
        public event EventHandler<BackupJobFullState>? BackupJobFullStateChanged;
        private LoggerService LoggerService { get; }
        private BackupConfig BackupConfig { get; }
        private EasySaveConfig EasySaveConfig { get; }
        private BackupJobFullState _FullState;
        public BackupJobFullState FullState
        {
            get { return _FullState; }
            private set
            {
                _FullState = value;
                BackupJobFullStateChanged?.Invoke(this, value);
            }
        }

        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _backupTask;

        public BackupJobService(LoggerService loggerService, BackupConfig backupConfig, EasySaveConfig easySaveConfig, FileProcessingService fileProcessingService)
        {
            LoggerService = loggerService;
            BackupConfig = backupConfig;
            EasySaveConfig = easySaveConfig;
            FullState = BackupJobFullState.Default(backupConfig.Id, backupConfig.Name, backupConfig.SourceDirectory, backupConfig.TargetDirectory);
            FileProcessingService = fileProcessingService;
            State = BackupJobState.STOPPED;
        }

        public void Start()
        {
            if (FullState.State == BackupJobState.ACTIVE)
            {
                throw new Exception("Backup job already running");
            }

            _cancellationTokenSource = new CancellationTokenSource();
            UpdateFullState(BackupJobState.ACTIVE);
            _backupTask = Task.Run(() => ExecuteBackupAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        public void Pause()
        {
            if (FullState.State != BackupJobState.ACTIVE)
            {
                throw new Exception("Backup job is not running");
            }

            UpdateFullState(BackupJobState.PAUSED);
            _cancellationTokenSource?.Cancel();
        }

        public void Stop()
        {
            if (FullState.State == BackupJobState.STOPPED)
            {
                throw new Exception("Backup job is not running");
            }

            UpdateFullState(BackupJobState.STOPPED);
            _cancellationTokenSource?.Cancel();
        }

        private async Task ExecuteBackupAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Executing backup: {BackupConfig.Name}");

            var files = Directory.GetFiles(BackupConfig.SourceDirectory, "*", SearchOption.AllDirectories);
            long totalFilesSize = files.Sum(f => new FileInfo(f).Length);
            long totalFiles = files.Length;

            UpdateFullState(BackupJobState.ACTIVE, totalFiles, totalFilesSize, totalFiles, 0);

            int completedFiles = 0;
            long completedSize = 0;
            int i = 0;

            while (i < files.Length)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    UpdateFullState(FullState.State, totalFiles, totalFilesSize, totalFiles - completedFiles, (int)((completedSize * 100) / totalFilesSize));
                    return;
                }

                var file = files[i];

                if (BackupConfig.Type == BackupType.Differential && IsRunningWorkApp.IsRunning(EasySaveConfig.)
                {
                    LoggerService.AddLog(new
                    {
                        Type = "Error",
                        Description = "Backup job stopped because work app has been launched"
                    });

                    UpdateFullState(BackupJobState.STOPPED, totalFiles, totalFilesSize, totalFiles - completedFiles, (int)((completedSize * 100) / totalFilesSize));
                    return;
                }

                await FileProcessingService.ProcessFileAsync(BackupConfig, file, completedFiles, completedSize, totalFiles, totalFilesSize, BackupJobFullStateChanged);
                completedFiles++;
                completedSize += new FileInfo(file).Length;
                i++;
            }

            UpdateFullState(BackupJobState.STOPPED, totalFiles, totalFilesSize, 0, 100);

            Console.WriteLine($"Backup {BackupConfig.Name} completed.");
        }
    }
}
