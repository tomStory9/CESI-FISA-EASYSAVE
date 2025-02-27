using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class BackupConfigServiceTests
    {
        private readonly EasySaveConfigService _service;
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
        private static readonly string ConfigPath = Path.Combine(AppDataPath, "config.json");

        public BackupConfigServiceTests()
        {
            // Clean up configuration files before each test
            if (Directory.Exists(AppDataPath))
            {
                Directory.Delete(AppDataPath, true);
            }

            _service = new EasySaveConfigService();
        }

        [Fact]
        public void AddBackupConfig_ShouldAddConfig_WhenConfigDoesNotExist()
        {
            // Arrange
            var config = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = "C:/Source",
                TargetDirectory = "D:/Target",
                Type = BackupType.Full,
                Encrypted = false

            };

            // Act
            _service.AddBackupConfig(config);

            // Assert
            Assert.Contains(config, _service.BackupConfigs);
            Assert.Equal(config.Name, _service.BackupConfigs.First(c => c.Id == 1).Name);
        }

        [Fact]
        public void AddBackupConfig_ShouldThrowException_WhenConfigAlreadyExists()
        {
            // Arrange
            var config = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = "C:/Source",
                TargetDirectory = "D:/Target",
                Type = BackupType.Full,
                Encrypted = false
            };
            _service.AddBackupConfig(config);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _service.AddBackupConfig(config));
            Assert.Equal($"Backup job with ID 1 already exists.", exception.Message);
        }

        [Fact]
        public void RemoveBackupConfig_ShouldRemoveConfig_WhenConfigExists()
        {
            // Arrange
            var config = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = "C:/Source",
                TargetDirectory = "D:/Target",
                Type = BackupType.Full,
                Encrypted = false
            };
            _service.AddBackupConfig(config);

            // Act
            _service.RemoveBackupConfig(1);

            // Assert
            Assert.DoesNotContain(config, _service.BackupConfigs);
        }

        [Fact]
        public void RemoveBackupConfig_ShouldThrowException_WhenConfigDoesNotExist()
        {
            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _service.RemoveBackupConfig(1));
            Assert.Equal($"Backup job with ID 1 not found.", exception.Message);
        }

        [Fact]
        public void Save_ShouldCreateConfigFile_WhenConfigIsAdded()
        {
            // Arrange
            var config = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = "C:/Source",
                TargetDirectory = "D:/Target",
                Type = BackupType.Full,
                Encrypted = false
            };
            _service.AddBackupConfig(config);

            // Assert
            Assert.True(File.Exists(ConfigPath));
            var json = File.ReadAllText(ConfigPath);
            var configs = JsonSerializer.Deserialize<EasySaveConfig>(json);
            Assert.Contains(config, configs.BackupConfigs);
            Assert.Equal(config.Name, configs.BackupConfigs.First(c => c.Id == 1).Name);
        }
    }
}
