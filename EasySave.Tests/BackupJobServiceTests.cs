using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
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

        public BackupJobServiceIntegrationTests()
        {
            // Créer des répertoires temporaires pour les tests
            _sourceDirectory = Path.Combine(Path.GetTempPath(), $"SourceDir_{Guid.NewGuid()}");
            _targetDirectory = Path.Combine(Path.GetTempPath(), $"TargetDir_{Guid.NewGuid()}");
            _logFilePath = Path.Combine(Path.GetTempPath(), $"LogDir_{Guid.NewGuid()}");

            Directory.CreateDirectory(_sourceDirectory);
            Directory.CreateDirectory(_targetDirectory);

            // Initialiser LoggerService avec un fichier de log temporaire unique
            _loggerService = new LoggerService(_logFilePath , LoggerDLL.Models.LogType.LogTypeEnum.JSON);

            // Initialiser BackupJobService avec LoggerService
            _backupJobService = new BackupJobService(_loggerService);
        }

        public void Dispose()
        {
            // Nettoyer les répertoires et fichiers temporaires après les tests
            if (Directory.Exists(_sourceDirectory))
                Directory.Delete(_sourceDirectory, true);

            if (Directory.Exists(_targetDirectory))
                Directory.Delete(_targetDirectory, true);

            if (Directory.Exists(_logFilePath))
                Directory.Delete(_logFilePath, true);
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldCopyFiles_WhenBackupTypeIsFull()
        {
            var job = new BackupConfig("Backup1", _sourceDirectory, _targetDirectory, BackupType.Full);
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");


            await _backupJobService.ExecuteBackupAsync(job);


            Assert.True(File.Exists(Path.Combine(_targetDirectory, "file1.txt")));
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldLogFileTransfer_WhenFileIsCopied()
        {
            var job = new BackupConfig("Backup1", _sourceDirectory, _targetDirectory, BackupType.Full);
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");

            await _backupJobService.ExecuteBackupAsync(job);

            string logFilePath = Path.Combine(_logFilePath, DateTime.Now.ToString("d-MM-yyyy") + ".json");
            var logContent = File.ReadAllText(logFilePath);
            Assert.Contains("file1.txt", logContent);
            Assert.Contains("FileTransferTime", logContent);
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldNotCopyFiles_WhenBackupTypeIsDifferentialAndFileIsUnchanged()
        {
            var job = new BackupConfig("Backup1", _sourceDirectory, _targetDirectory, BackupType.Differential);
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");
            File.WriteAllText(Path.Combine(_targetDirectory, "file1.txt"), "Test content");

            await _backupJobService.ExecuteBackupAsync(job);

            string logFilePath = Path.Combine(_logFilePath, DateTime.Now.ToString("d-MM-yyyy") + ".json");
            var logContent = File.ReadAllText(logFilePath);

            Assert.True(File.Exists(Path.Combine(_targetDirectory, "file1.txt")));
            Assert.DoesNotContain("\"file1.txt\"", logContent);
        }

        [Fact]
        public async Task ExecuteBackupAsync_ShouldInvokeBackupJobFullStateChanged_WhenBackupStartsAndEnds()
        {

            var job = new BackupConfig("Backup1", _sourceDirectory, _targetDirectory, BackupType.Full);
            File.WriteAllText(Path.Combine(_sourceDirectory, "file1.txt"), "Test content");

            var states = new List<BackupJobFullState>();
            _backupJobService.BackupJobFullStateChanged += (sender, state) => states.Add(state);

            await _backupJobService.ExecuteBackupAsync(job);
            Assert.Equal(3, states.Count); // Début (ACTIVE), lors de la copie des fichiers et à la fin (END)
            Assert.Equal(BackupJobState.ACTIVE, states[0].State);
            Assert.Equal(BackupJobState.END, states[2].State);
        }
    }
}