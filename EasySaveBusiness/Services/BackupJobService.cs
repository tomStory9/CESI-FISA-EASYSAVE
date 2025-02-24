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
        private FileProcessingService FileProcessingService { get; }
        private WorkAppMonitorService WorkAppMonitorService { get; }
        private SortBackupFileService SortBackupFileService { get; }
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

        public BackupJobService(LoggerService loggerService, BackupConfig backupConfig, EasySaveConfig easySaveConfig, FileProcessingService fileProcessingService, WorkAppMonitorService workAppMonitorService)
        {
            LoggerService = loggerService;
            BackupConfig = backupConfig;
            EasySaveConfig = easySaveConfig;
            FileProcessingService = fileProcessingService;
            WorkAppMonitorService = workAppMonitorService;
            _FullState = BackupJobFullState.FromBackupConfig(backupConfig);

            WorkAppMonitorService.WorkAppStopped += OnWorkAppStopped;
        }

        public void Start()
        {
            if (FullState.State == BackupJobState.ACTIVE)
            {
                Console.WriteLine("Backup job already running");
                // throw new Exception("Backup job already running");
            }

            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() => ExecuteBackupAsync(_cancellationTokenSource.Token));
        }

        public void Pause()
        {
            if (FullState.State != BackupJobState.ACTIVE)
            {
                throw new Exception("Backup job is not running");
            }

            _FullState = _FullState with { State = BackupJobState.PAUSED };
            _cancellationTokenSource?.Cancel();
        }

        public void Stop()
        {
            if (FullState.State == BackupJobState.STOPPED)
            {
                throw new Exception("Backup job is not running");
            }

            _FullState = _FullState with { State = BackupJobState.STOPPED };
            _cancellationTokenSource?.Cancel();
        }

        private async Task ExecuteBackupAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Executing backup: {BackupConfig.Name}");

            var files = Directory.GetFiles(BackupConfig.SourceDirectory, "*", SearchOption.AllDirectories);
            SortBackupFileService.SortFile(EasySaveConfig.PriorityFileExtension, files);
            long totalFilesSize = files.Sum(f => new FileInfo(f).Length);
            long totalFiles = files.Length;

            _FullState = _FullState with {
                State = BackupJobState.ACTIVE,
                TotalFilesToCopy = totalFiles,
                TotalFilesSize = totalFilesSize,
                NbFilesLeftToDo = totalFiles,
                Progression = 0
            };

            int completedFiles = 0;
            long completedSize = 0;
            int i = 0;

            while (i < files.Length)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _FullState = _FullState with
                    {
                        NbFilesLeftToDo = totalFiles - completedFiles,
                        Progression = (int)((completedSize * 100) / totalFilesSize)
                    };
                    return;
                }

                var file = files[i];

                if (BackupConfig.Type == BackupType.Differential && IsRunningWorkAppService.IsRunning(EasySaveConfig.WorkApp))
                {
                    LoggerService.AddLog(new
                    {
                        Type = "Error",
                        Description = "Backup job stopped because work app has been launched"
                    });

                    _FullState = _FullState with
                    {
                        State = BackupJobState.STOPPED,
                    };
                    return;
                }

                await ProcessFileAsync(BackupConfig, file, completedFiles, completedSize, totalFiles, totalFilesSize);
                completedFiles++;
                completedSize += new FileInfo(file).Length;
                i++;
            }

            _FullState = BackupJobFullState.FromBackupConfig(BackupConfig);

            Console.WriteLine($"Backup {BackupConfig.Name} completed.");
        }

        public async Task ProcessFileAsync(BackupConfig backupConfig, string file, int completedFiles, long completedSize, long totalFiles, long totalFilesSize)
        {
            string relativePath = Path.GetRelativePath(backupConfig.SourceDirectory, file);
            string destinationFile = Path.Combine(backupConfig.TargetDirectory, relativePath);
            string destinationDir = Path.GetDirectoryName(destinationFile) ?? string.Empty;

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }
            Stopwatch stopwatch = Stopwatch.StartNew();

            //if (_differentialBackupVerifierService.VerifyDifferentialBackupAndShaDifference(backupConfig,file,destinationFile))
            //{
            // File.Copy(file, destinationFile, true);
            using (FileStream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (FileStream destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
            //}
            stopwatch.Stop();
            double transferTime = stopwatch.Elapsed.TotalMilliseconds;

            _FullState = _FullState with
            {
                NbFilesLeftToDo = totalFiles - completedFiles,
                Progression = (int)((completedSize * 100) / totalFilesSize),
                SourceFilePath = file,
                TargetFilePath = destinationFile
            };
        }

        private void OnWorkAppStopped(object? sender, EventArgs e)
        {
            if (FullState.State == BackupJobState.STOPPED)
            {
                Start();
            }
        }
        public void UpdateBackupJobFullState(BackupJobFullState newState)
        {
            _FullState = newState;
            BackupJobFullStateChanged?.Invoke(this, _FullState);
        }
    }
}