using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using LoggerDLL.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Collections;


namespace EasySaveBusiness.Tests
{
    public class FileProcessingServiceTests : IDisposable
    {
        private readonly string _sourceDirectory;
        private readonly string _targetDirectory;
        private readonly LoggerService _loggerService;
        private readonly DifferentialBackupVerifierService _differentialBackupVerifierService;
        private readonly BackupJobService _backupJobService;
        private readonly FileProcessingService _fileProcessingService;
        private readonly WorkAppMonitorService _workAppMonitorService;
        private readonly IsNetworkUsageExceededService _isNetworkUsageExceeded;
        private readonly SortBackupFileService _sortBackupFileService;
        private readonly IsRunningWorkAppService _isRunningWorkAppService;
        private readonly EasySaveConfigService _easySaveBackupConfig;
        private readonly string _logFilePath = Path.Combine(Path.GetTempPath(), "backup_full_state_log.json");
        private readonly string _workApp = "notepad.exe";

        public FileProcessingServiceTests()
        {
            _sourceDirectory = Path.Combine(Path.GetTempPath(), $"SourceDir_{Guid.NewGuid()}");
            _targetDirectory = Path.Combine(Path.GetTempPath(), $"TargetDir_{Guid.NewGuid()}");
            Directory.CreateDirectory(_sourceDirectory);
            Directory.CreateDirectory(_targetDirectory);
            ManualResetEvent systemMre = new ManualResetEvent(false);
            _loggerService = new LoggerService(_logFilePath, LoggerDLL.Models.LogType.LogTypeEnum.JSON);
            _differentialBackupVerifierService = new DifferentialBackupVerifierService();
            _isRunningWorkAppService = new IsRunningWorkAppService();
            _isNetworkUsageExceeded = new IsNetworkUsageExceededService();
            _sortBackupFileService = new SortBackupFileService();
            _easySaveBackupConfig = new EasySaveConfigService();
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = _sourceDirectory,
                TargetDirectory = _targetDirectory,
                Type = BackupType.Full,
                Encrypted = false
            };

            var easySaveConfig = new EasySaveConfig(new List<BackupConfig> { backupConfig }, _workApp, new List<string>(), 1024, "eth0", LoggerDLL.Models.LogType.LogTypeEnum.JSON, 1024 * 1024 * 5, null);
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            _fileProcessingService = new FileProcessingService(_loggerService, _differentialBackupVerifierService);
            _backupJobService = new BackupJobService(_loggerService, _easySaveBackupConfig, _fileProcessingService, _sortBackupFileService, _isRunningWorkAppService, _isNetworkUsageExceeded, systemMre);
         _backupJobService.Init(backupConfig);
        }

        public void Dispose()
        {
            if (Directory.Exists(_sourceDirectory))
                Directory.Delete(_sourceDirectory, true);

            if (Directory.Exists(_targetDirectory))
                Directory.Delete(_targetDirectory, true);
        }

        [Fact]
        public async Task ProcessFileAsync_ShouldCopyFile_WhenFileNeedsToBeCopied()
        {
            // Arrange
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = _sourceDirectory,
                TargetDirectory = _targetDirectory,
                Type = BackupType.Full,
                Encrypted = false
            };
            var filePath = Path.Combine(_sourceDirectory, "file1.txt");
            File.WriteAllText(filePath, "Test content");

            var lockEvent = new ManualResetEventSlim();
            var lockObject = new object();

            // Act
            await _fileProcessingService.ProcessFileAsync(backupConfig, filePath, 0, 0, 1, 1024, lockEvent, lockObject);

            // Assert
            var destinationFilePath = Path.Combine(_targetDirectory, "file1.txt");
            Assert.True(File.Exists(destinationFilePath));
            Assert.Equal("Test content", File.ReadAllText(destinationFilePath));
        }

        [Fact]
        public async Task ProcessFileAsync_ShouldHandleBigFileProcessing()
        {
            // Arrange
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = _sourceDirectory,
                TargetDirectory = _targetDirectory,
                Type = BackupType.Full,
                Encrypted= false
            };
            var filePath = Path.Combine(_sourceDirectory, "bigfile1.txt");
            File.WriteAllText(filePath, new string('a', 1024 * 1024 * 10)); // 10 MB file

            var lockEvent = new ManualResetEventSlim();
            var lockObject = new object();

            // Act
            await _fileProcessingService.ProcessFileAsync(backupConfig, filePath, 0, 0, 1, 1024 * 1024 * 10, lockEvent, lockObject);

            // Assert
            var destinationFilePath = Path.Combine(_targetDirectory, "bigfile1.txt");
            Assert.True(File.Exists(destinationFilePath));
            Assert.Equal(new string('a', 1024 * 1024 * 10), File.ReadAllText(destinationFilePath));
        }

        [Fact]
        public async Task ProcessFileAsync_ShouldHandleConcurrentAccessAndLocks()
        {
            // Arrange
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = _sourceDirectory,
                TargetDirectory = _targetDirectory,
                Type = BackupType.Full,
                Encrypted = false
            };
            var filePath1 = Path.Combine(_sourceDirectory, "file1.txt");
            var filePath2 = Path.Combine(_sourceDirectory, "file2.txt");
            var bigFilePath = Path.Combine(_sourceDirectory, "bigfile.txt");

            File.WriteAllText(filePath1, "Test content 1");
            File.WriteAllText(filePath2, "Test content 2");
            File.WriteAllText(bigFilePath, new string('a', 1024 * 1024 * 10)); // 10 MB file

            var lockEvent = new ManualResetEventSlim();
            var lockObject = new object();

            // Act
            var tasks = new Task[]
            {
                Task.Run(() => _fileProcessingService.ProcessFileAsync(backupConfig, filePath1, 0, 0, 3, 1024 * 1024 * 10 + 1024 * 2, lockEvent, lockObject)),
                Task.Run(() => _fileProcessingService.ProcessFileAsync(backupConfig, filePath2, 0, 0, 3, 1024 * 1024 * 10 + 1024 * 2, lockEvent, lockObject)),
                Task.Run(() => _fileProcessingService.ProcessFileAsync(backupConfig, bigFilePath, 0, 0, 3, 1024 * 1024 * 10 + 1024 * 2, lockEvent, lockObject))
            };

            await Task.WhenAll(tasks);

            // Assert
            var destinationFilePath1 = Path.Combine(_targetDirectory, "file1.txt");
            var destinationFilePath2 = Path.Combine(_targetDirectory, "file2.txt");
            var destinationBigFilePath = Path.Combine(_targetDirectory, "bigfile.txt");

            Assert.True(File.Exists(destinationFilePath1));
            Assert.True(File.Exists(destinationFilePath2));
            Assert.True(File.Exists(destinationBigFilePath));

            Assert.Equal("Test content 1", File.ReadAllText(destinationFilePath1));
            Assert.Equal("Test content 2", File.ReadAllText(destinationFilePath2));
            Assert.Equal(new string('a', 1024 * 1024 * 10), File.ReadAllText(destinationBigFilePath));
        }
        [Fact]
        public async Task ProcessEncryptedFileAsync_ShouldEncryptFileCorrectly()
        {
            // Arrange
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = _sourceDirectory,
                TargetDirectory = _targetDirectory,
                Type = BackupType.Full,
                Encrypted = true
            };
            var filePath = Path.Combine(_sourceDirectory, "file1.txt");
            File.WriteAllText(filePath, "Test content");

            var lockEvent = new ManualResetEventSlim();
            var lockObject = new object();
            var encryptionKey = "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2b";


            // Act
            await _fileProcessingService.ProcessEncryptedFileAsync(backupConfig, filePath, 0, 0, 1, 1024, lockEvent, lockObject, encryptionKey);

            // Assert
            var encryptedFilePath = Path.Combine(_targetDirectory, "file1.txt");
            Assert.True(File.Exists(encryptedFilePath));
            Assert.NotEqual("Test content", File.ReadAllText(encryptedFilePath)); // Ensure the content is encrypted
        }

        [Fact]
        public async Task RestoreEncryptedFileAsync_ShouldDecryptFileCorrectly()
        {
            // Arrange
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = _sourceDirectory,
                TargetDirectory = _targetDirectory,
                Type = BackupType.Full,
                Encrypted = true
            };
            var filePath = Path.Combine(_targetDirectory, "file1.txt");
            File.WriteAllText(filePath, "Encrypted content"); // Simulate encrypted content

            var lockEvent = new ManualResetEventSlim();
            var lockObject = new object();
            var encryptionKey = "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2b";


            // Act
            await _fileProcessingService.RestoreEncryptedFileAsync(backupConfig, filePath, 0, 0, 1, 1024, lockEvent, lockObject, encryptionKey);

            // Assert
            var decryptedFilePath = Path.Combine(_sourceDirectory, "file1.txt");
            Assert.True(File.Exists(decryptedFilePath));
            Assert.Equal("Test content", File.ReadAllText(decryptedFilePath)); // Ensure the content is decrypted correctly
        }
    }
}
