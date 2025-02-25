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
        private IsNetworkUsageExceeded _isNetworkUsageExceed { get; }
        private IsRunningWorkAppService _isRunningWorkAppService { get; }
        private SortBackupFileService SortBackupFileService { get; }
        private BackupJobFullState _FullState;
        private ManualResetEvent Systemmre { get; }
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

        public BackupJobService(LoggerService loggerService, BackupConfig backupConfig, EasySaveConfig easySaveConfig, FileProcessingService fileProcessingService, WorkAppMonitorService workAppMonitorService, ManualResetEvent systemmre, ManualResetEvent usermre)
        {
            LoggerService = loggerService;
            BackupConfig = backupConfig;
            EasySaveConfig = easySaveConfig;
            FileProcessingService = fileProcessingService;
            WorkAppMonitorService = workAppMonitorService;
            _FullState = BackupJobFullState.FromBackupConfig(backupConfig);

            WorkAppMonitorService.WorkAppStopped += OnWorkAppStopped;
            Systemmre = systemmre;
        }

        public void Start()
        {
            if (FullState.State == BackupJobState.ACTIVE)
            {
                Console.WriteLine("Backup job already running");
                // throw new Exception("Backup job already running");
            }
            if (FullState.State == BackupJobState.PAUSED)
            {
                _FullState = _FullState with { State = BackupJobState.ACTIVE };
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
        }
        public void SystemPause()
        {
            if (FullState.State != BackupJobState.ACTIVE)
            {
                throw new Exception("Backup job is not running");
            }
            _FullState = _FullState with { State = BackupJobState.SYSTEM_PAUSED };
            Systemmre.WaitOne();
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

            _FullState = _FullState with
            {
                State = BackupJobState.ACTIVE,
                TotalFilesToCopy = totalFiles,
                TotalFilesSize = totalFilesSize,
                NbFilesLeftToDo = totalFiles,
                Progression = 0
            };

            int completedFiles = 0;
            long completedSize = 0;
            int i = 0;
            var lockEvent = new ManualResetEventSlim(true);
            object lockObject = new object();

            while (i < files.Length)
            {
                if (_isRunningWorkAppService.IsRunning(EasySaveConfig.WorkApp) || _isNetworkUsageExceed.IsNetworkUsageLimitExceeded(EasySaveConfig.NetworkInterfaceName, EasySaveConfig.NetworkKoLimit))
                {
                   SystemPause();
                }
                else
                {
                    _FullState = _FullState with
                    { State = BackupJobState.ACTIVE };
                }
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

                if (BackupConfig.Type == BackupType.Differential && _isRunningWorkAppService.IsRunning(EasySaveConfig.WorkApp))
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

                lock (lockObject)
                {
                    if (IsNetworkUsageExceeded.IsBigFileProcessing)
                    {
                        if (file.Length >= EasySaveConfig.SizeLimit)
                        {
                            if (files.Length <= i + 1)
                            {
                                // lock my backupjob until isNetworkUsageExceeded is false
                                lockEvent.Reset();
                                lockEvent.Wait(cancellationToken);
                            }
                            else
                            {
                                var BigFile = file;
                                file = files[i + 1];
                                files[i + 1] = BigFile;
                            }
                        }
                    }

                    if (file.Length >= EasySaveConfig.SizeLimit)
                    {
                        IsNetworkUsageExceeded.IsBigFileProcessing = true;
                    }
                }

                await FileProcessingService.ProcessFileAsync(BackupConfig, file, completedFiles, completedSize, totalFiles, totalFilesSize, lockEvent, lockObject);
                completedFiles++;
                completedSize += new FileInfo(file).Length;
                i++;
            }

            _FullState = BackupJobFullState.FromBackupConfig(BackupConfig);

            Console.WriteLine($"Backup {BackupConfig.Name} completed.");
        }

        public void UpdateBackupJobFullState(BackupJobFullState newState)
        {
            _FullState = newState;
            BackupJobFullStateChanged?.Invoke(this, _FullState);
        }
    }
}