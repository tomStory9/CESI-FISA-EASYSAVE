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
        private BackupConfig BackupConfig { get; set; }
        private EasySaveConfig EasySaveConfig { get; }
        private FileProcessingService FileProcessingService { get; }
        private IsNetworkUsageExceededService _isNetworkUsageExceedService { get; }
        private IsRunningWorkAppService _isRunningWorkAppService { get; }
        private SortBackupFileService SortBackupFileService { get; }
        private BackupJobFullState _FullState;
        private ManualResetEvent Systemmre { get; }
        private ManualResetEvent Usermre { get; }
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

        public BackupJobService(
            LoggerService loggerService,
            EasySaveConfigService easySaveConfigService,
            FileProcessingService fileProcessingService,
            SortBackupFileService sortBackupFileService,
            IsRunningWorkAppService isRunningWorkAppService,
            IsNetworkUsageExceededService isNetworkUsageExceedService,
            ManualResetEvent systemmre
            
        )
        {
            LoggerService = loggerService;
            EasySaveConfig = easySaveConfigService.EasySaveConfig;
            FileProcessingService = fileProcessingService;
            SortBackupFileService = sortBackupFileService;
            _isRunningWorkAppService = isRunningWorkAppService;
            _isNetworkUsageExceedService = isNetworkUsageExceedService;
            Systemmre = systemmre;
            Usermre = new ManualResetEvent(false);

            FileProcessingService.FileProcessed += OnFileProcessed;
        }

        private void OnFileProcessed(object? sender, FileProcessedEventData e)
        {
            UpdateBackupJobFullState(
                FullState with
                {
                    NbFilesLeftToDo = e.NbFilesLeftToDo,
                    Progression = e.Progression,
                    SourceFilePath = e.SourceFilePath,
                    TargetFilePath = e.TargetFilePath
                }
            );
        }

        public void Init(BackupConfig backupConfig)
        {
            BackupConfig = backupConfig;
            FullState = BackupJobFullState.FromBackupConfig(backupConfig);
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
                FullState = FullState with { State = BackupJobState.ACTIVE };
                Usermre.Set();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            switch ()
            {
                default:
                    Task.Run(() => ExecuteBackupAsync(_cancellationTokenSource.Token));
                    break;
            }
            
        }

        public void Pause()
        {
            if (FullState.State != BackupJobState.ACTIVE)
            {
                throw new Exception("Backup job is not running");
            }

            FullState = FullState with { State = BackupJobState.PAUSED };
        }
        private void SystemPause()
        {
            if (FullState.State != BackupJobState.ACTIVE)
            {
                throw new Exception("Backup job is not running");
            }
            FullState = FullState with { State = BackupJobState.SYSTEM_PAUSED };
        }
        public void Stop()
        {
            if (FullState.State == BackupJobState.STOPPED)
            {
                throw new Exception("Backup job is not running");
            }

            FullState = FullState with { State = BackupJobState.STOPPED };
            _cancellationTokenSource?.Cancel();
        }

        private async Task ExecuteRestoreAsync(CancellationToken cancellationToken)
        {
             Console.WriteLine($"Executing backup: {BackupConfig.Name}");

            var files = Directory.GetFiles(BackupConfig.SourceDirectory, "*", SearchOption.AllDirectories);
            SortBackupFileService.SortFile(EasySaveConfig.PriorityFileExtension, files);
            long totalFilesSize = files.Sum(f => new FileInfo(f).Length);
            long totalFiles = files.Length;

            FullState = FullState with
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
                if(FullState.State == BackupJobState.PAUSED)
                {
                    Usermre.WaitOne();
                }

                if (
                    false
                    // _isRunningWorkAppService.IsRunning(EasySaveConfig.WorkApp)
                    // || _isNetworkUsageExceedService.IsNetworkUsageLimitExceeded(EasySaveConfig.NetworkInterfaceName, EasySaveConfig.NetworkKoLimit)
                )
                {
                    SystemPause();
                    Systemmre.WaitOne();
                }
                else
                {
                    /* FullState = FullState with
                    { State = BackupJobState.ACTIVE }; */
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    FullState = FullState with
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

                    FullState = FullState with
                    {
                        State = BackupJobState.STOPPED,
                    };
                    return;
                }

                lock (lockObject)
                {
                    if (IsNetworkUsageExceededService.IsBigFileProcessing)
                    {
                        if (file.Length >= EasySaveConfig.SizeLimit)
                        {
                            if (files.Length <= i + 1)
                            {
                                SystemPause();
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
                        IsNetworkUsageExceededService.IsBigFileProcessing = true;
                    }
                }
                if (BackupConfig.Encrypted)
                {
                    await FileProcessingService.RestoreEncryptedFileAsync(BackupConfig, file, completedFiles, completedSize, totalFiles, totalFilesSize, lockEvent, lockObject, EasySaveConfig.Key);
                }
                else
                {
                    await FileProcessingService.RestoreFileAsync(BackupConfig, file, completedFiles, completedSize, totalFiles, totalFilesSize, lockEvent, lockObject);
                }
                completedFiles++;
                completedSize += new FileInfo(file).Length;
                i++;
            }

            FullState = BackupJobFullState.FromBackupConfig(BackupConfig);

            Console.WriteLine($"Backup {BackupConfig.Name} completed.");
        }

        private async Task ExecuteBackupAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Executing backup: {BackupConfig.Name}");

            var files = Directory.GetFiles(BackupConfig.SourceDirectory, "*", SearchOption.AllDirectories);
            SortBackupFileService.SortFile(EasySaveConfig.PriorityFileExtension, files);
            long totalFilesSize = files.Sum(f => new FileInfo(f).Length);
            long totalFiles = files.Length;

            FullState = FullState with
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
                if(FullState.State == BackupJobState.PAUSED)
                {
                    Usermre.WaitOne();
                }

                if (
                    false
                    // _isRunningWorkAppService.IsRunning(EasySaveConfig.WorkApp)
                    // || _isNetworkUsageExceedService.IsNetworkUsageLimitExceeded(EasySaveConfig.NetworkInterfaceName, EasySaveConfig.NetworkKoLimit)
                )
                {
                    SystemPause();
                    Systemmre.WaitOne();
                }
                else
                {
                    /* FullState = FullState with
                    { State = BackupJobState.ACTIVE }; */
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    FullState = FullState with
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

                    FullState = FullState with
                    {
                        State = BackupJobState.STOPPED,
                    };
                    return;
                }

                lock (lockObject)
                {
                    if (IsNetworkUsageExceededService.IsBigFileProcessing)
                    {
                        if (file.Length >= EasySaveConfig.SizeLimit)
                        {
                            if (files.Length <= i + 1)
                            {
                                SystemPause();
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
                        IsNetworkUsageExceededService.IsBigFileProcessing = true;
                    }
                }
                if (BackupConfig.Encrypted)
                {
                    await FileProcessingService.ProcessEncryptedFileAsync(BackupConfig, file, completedFiles, completedSize, totalFiles, totalFilesSize, lockEvent, lockObject, EasySaveConfig.Key);
                }
                else
                {
                    await FileProcessingService.ProcessFileAsync(BackupConfig, file, completedFiles, completedSize, totalFiles, totalFilesSize, lockEvent, lockObject);
                }
                completedFiles++;
                completedSize += new FileInfo(file).Length;
                i++;
            }

            FullState = BackupJobFullState.FromBackupConfig(BackupConfig);

            Console.WriteLine($"Backup {BackupConfig.Name} completed.");
        }

        public void UpdateBackupJobFullState(BackupJobFullState newState)
        {
            FullState = newState;
        }
    }
}