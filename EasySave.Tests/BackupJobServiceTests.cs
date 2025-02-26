using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using LoggerDLL.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class BackupJobServiceTests : IDisposable
    {
        private readonly string _sourceDirectory;
        private readonly string _targetDirectory;
        private readonly LoggerService _loggerService;
        private readonly DifferentialBackupVerifierService _differentialBackupVerifierService;
        private readonly BackupJobService _backupJobService;
        private readonly FileProcessingService _fileProcessingService;
        private readonly WorkAppMonitorService _workAppMonitorService;
        private readonly IsNetworkUsageExceeded _isNetworkUsageExceeded;
        private readonly SortBackupFileService _sortBackupFileService;
        private readonly string _logFilePath = Path.Combine(Path.GetTempPath(), "backup_full_state_log.json");
        private readonly string _workApp = "notepad.exe";

        public BackupJobServiceTests()
        {
            _sourceDirectory = Path.Combine(Path.GetTempPath(), $"SourceDir_{Guid.NewGuid()}");
            _targetDirectory = Path.Combine(Path.GetTempPath(), $"TargetDir_{Guid.NewGuid()}");
            Directory.CreateDirectory(_sourceDirectory);
            Directory.CreateDirectory(_targetDirectory);

            _loggerService = new LoggerService(_logFilePath, LoggerDLL.Models.LogType.LogTypeEnum.JSON);
            _differentialBackupVerifierService = new DifferentialBackupVerifierService();
            _workAppMonitorService = new WorkAppMonitorService(_workApp);
            _isNetworkUsageExceeded = new IsNetworkUsageExceeded();
            _sortBackupFileService = new SortBackupFileService();
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = _sourceDirectory,
                TargetDirectory = _targetDirectory,
                Type = BackupType.Full
            };

            var easySaveConfig = new EasySaveConfig(new List<BackupConfig> { backupConfig }, _workApp, LoggerDLL.Models.LogType.LogTypeEnum.JSON, new List<string>(), 1024, "eth0", 1024 * 1024 * 5);
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            _backupJobService = new BackupJobService(_loggerService, backupConfig, easySaveConfig, _fileProcessingService, _workAppMonitorService, manualResetEvent);
            _fileProcessingService = new FileProcessingService(_loggerService, _differentialBackupVerifierService, _backupJobService);
        }


        public void Dispose()
        {
            if (Directory.Exists(_sourceDirectory))
                Directory.Delete(_sourceDirectory, true);

            if (Directory.Exists(_targetDirectory))
                Directory.Delete(_targetDirectory, true);
        }
        
        [Fact]
        public async Task ExecuteBackupAsync_ShouldHandleConcurrentAccessAndLocks()
        {
            // Arrange
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = _sourceDirectory,
                TargetDirectory = _targetDirectory,
                Type = BackupType.Full
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
    }
}
