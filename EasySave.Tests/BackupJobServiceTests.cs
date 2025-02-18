using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class BackupJobServiceIntegrationTests : IDisposable
    {
        private readonly string _sourceDirectory;
        private readonly string _targetDirectory;
        private readonly string _logFilePath;
        private readonly LoggerService _loggerService;
        private readonly BackupJobService _backupJobService;
        private readonly string _logDirectory;
        private readonly EasySaveConfig _easySaveConfig;

        public BackupJobServiceIntegrationTests()
        {
            // Create temporary directories for tests
            _sourceDirectory = Path.Combine(Path.GetTempPath(), $"SourceDir_{Guid.NewGuid()}");
            _targetDirectory = Path.Combine(Path.GetTempPath(), $"TargetDir_{Guid.NewGuid()}");
            _logDirectory = Path.GetTempPath();
            _logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:d-MM-yyyy}.json");
            Directory.CreateDirectory(_sourceDirectory);
            Directory.CreateDirectory(_targetDirectory);

            // Initialize LoggerService with a unique temporary log file
            _loggerService = new LoggerService(_logDirectory, LoggerDLL.Models.LogType.LogTypeEnum.JSON);

            // Initialize BackupJobService with LoggerService
            _backupJobService = new BackupJobService(_loggerService);

            // Initialize EasySaveConfig
            var backupConfigs = new Dictionary<int, BackupConfig>
            {
                { 1, new BackupConfig("Backup1", _sourceDirectory, _targetDirectory, BackupType.Full) },
                { 2, new BackupConfig("Backup2", _sourceDirectory, _targetDirectory, BackupType.Differential)   }
            };
            _easySaveConfig = new EasySaveConfig(backupConfigs, "notepad.exe", LoggerDLL.Models.LogType.LogTypeEnum.JSON);
        }

        public void Dispose()
        {
            // Clean up temporary directories and files after tests
            if (Directory.Exists(_sourceDirectory))
                Directory.Delete(_sourceDirectory, true);

            if (Directory.Exists(_targetDirectory))
                Directory.Delete(_targetDirectory, true);

            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldCopyFiles_WhenBackupTypeIsFull()
        {
            var job = _easySaveConfig.BackupConfigs[1];
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");

            await _backupJobService.ExecuteBackupAsync(job, _easySaveConfig);

            Assert.True(File.Exists(Path.Combine(_targetDirectory, "file1.txt")));
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldLogFileTransfer_WhenFileIsCopied()
        {
            var job = _easySaveConfig.BackupConfigs[1];
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");

            await _backupJobService.ExecuteBackupAsync(job, _easySaveConfig);

            var logContent = File.ReadAllText(_logFilePath);
            Assert.Contains("file1.txt", logContent);
            Assert.Contains("FileTransferTime", logContent);
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldNotCopyFiles_WhenBackupTypeIsDifferentialAndFileIsUnchanged()
        {
            var job = new BackupConfig("Backup1", _sourceDirectory, _targetDirectory, BackupType.Differential);
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");
            File.WriteAllText(Path.Combine(_targetDirectory, "file1.txt"), "Test content");

            await _backupJobService.ExecuteBackupAsync(job, _easySaveConfig);

            var logContent = File.ReadAllText(_logFilePath);

            Assert.True(File.Exists(Path.Combine(_targetDirectory, "file1.txt")));
            Assert.DoesNotContain("\"file1.txt\"", logContent);
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldInvokeBackupJobFullStateChanged_WhenBackupStartsAndEnds()
        {
            var job = _easySaveConfig.BackupConfigs[1];
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");

            var states = new List<BackupJobFullState>();
            _backupJobService.BackupJobFullStateChanged += (sender, state) => states.Add(state);

            await _backupJobService.ExecuteBackupAsync(job, _easySaveConfig);

            Assert.Equal(3, states.Count); // Start (ACTIVE), during file copy, and end (END)
            Assert.Equal(BackupJobState.ACTIVE, states[0].State);
            Assert.Equal(BackupJobState.END, states[2].State);

            _backupJobService.BackupJobFullStateChanged -= (sender, state) => states.Add(state);
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldStopBackup_WhenWorkAppLaunchedDuringDifferentialBackup()
        {
            var job = _easySaveConfig.BackupConfigs[2];
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");
            File.WriteAllText(Path.Combine(_sourceDirectory, "file2.txt"), "Test content");
            var states = new List<BackupJobFullState>();
            _backupJobService.BackupJobFullStateChanged += (sender, state) => states.Add(state);

            // Launch the work application to simulate it being active
            Process.Start("notepad.exe");

            await _backupJobService.ExecuteBackupAsync(job, _easySaveConfig);

            // Ensure the backup stops
            Assert.Equal(BackupJobState.STOPPED, states.Last().State);

            // Ensure the log is written
            var logContent = File.ReadAllText(_logFilePath);
            Assert.Contains("Backup job stopped because work app has been launched", logContent);

            // Clean up the launched process
            foreach (var process in Process.GetProcessesByName("notepad"))
            {
                process.Kill();
            }
        }
    }
}

