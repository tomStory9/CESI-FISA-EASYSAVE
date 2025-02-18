﻿using EasySaveBusiness.Models;
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

        public BackupJobService(LoggerService loggerService, BackupConfig backupConfig, EasySaveConfig easySaveConfig)
        {
            LoggerService = loggerService;
            BackupConfig = backupConfig;
            EasySaveConfig = easySaveConfig;
            FullState = BackupJobFullState.Default(backupConfig.Id, backupConfig.Name, backupConfig.SourceDirectory, backupConfig.TargetDirectory);
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

                if (BackupConfig.Type == BackupType.Differential && Process.GetProcessesByName(EasySaveConfig.WorkApp).Length > 0)
                {
                    LoggerService.AddLog(new
                    {
                        Type = "Error",
                        Description = "Backup job stopped because work app has been launched"
                    });

                    UpdateFullState(BackupJobState.STOPPED, totalFiles, totalFilesSize, totalFiles - completedFiles, (int)((completedSize * 100) / totalFilesSize));
                    return;
                }

                await ProcessFileAsync(file, completedFiles, completedSize, totalFiles, totalFilesSize);
                completedFiles++;
                completedSize += new FileInfo(file).Length;
                i++;
            }

            UpdateFullState(BackupJobState.STOPPED, totalFiles, totalFilesSize, 0, 100);

            Console.WriteLine($"Backup {BackupConfig.Name} completed.");
        }

        private async Task ProcessFileAsync(string file, int completedFiles, long completedSize, long totalFiles, long totalFilesSize)
        {
            string relativePath = Path.GetRelativePath(BackupConfig.SourceDirectory, file);
            string destinationFile = Path.Combine(BackupConfig.TargetDirectory, relativePath);
            string destinationDir = Path.GetDirectoryName(destinationFile) ?? string.Empty;

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (BackupConfig.Type == BackupType.Full || !File.Exists(destinationFile) ||
                File.GetLastWriteTime(file) > File.GetLastWriteTime(destinationFile))
            {
                await Task.Run(() => File.Copy(file, destinationFile, true));
            }
            stopwatch.Stop();
            double transferTime = stopwatch.Elapsed.TotalMilliseconds;

            UpdateFullState(BackupJobState.ACTIVE, totalFiles, totalFilesSize, totalFiles - completedFiles, (int)((completedSize * 100) / totalFilesSize), file, destinationFile);

            LoggerService.AddLog(new
            {
                Name = BackupConfig.Name,
                FileSource = file,
                FileTarget = destinationFile,
                FileTransferTime = transferTime,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }

        private void UpdateFullState(BackupJobState state, long totalFiles = 0, long totalFilesSize = 0, long nbFilesLeftToDo = 0, int progression = 0, string sourceFilePath = "", string targetFilePath = "")
        {
            FullState = new BackupJobFullState(
                BackupConfig.Id,
                BackupConfig.Name,
                sourceFilePath,
                targetFilePath,
                state,
                totalFiles,
                totalFilesSize,
                nbFilesLeftToDo,
                progression
            );
        }
    }
}
