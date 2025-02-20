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
        private readonly BackupConfigService _service;
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
        private static readonly string ConfigPath = Path.Combine(AppDataPath, "config.json");

        public BackupConfigServiceTests()
        {
            // Nettoyer les fichiers de configuration avant chaque test
            if (Directory.Exists(AppDataPath))
            {
                Directory.Delete(AppDataPath, true);
            }

            _service = new BackupConfigService();
        }

        [Fact]
        public void AddBackupConfig_ShouldAddConfig_WhenConfigDoesNotExist()
        {
 
            var config = new BackupConfig("Backup1", "C:/Source", "D:/Target", BackupType.Full);


            _service.AddBackupConfig(1, config);


            Assert.True(_service.BackupConfigs.ContainsKey(1));
            Assert.Equal(config.Name, _service.BackupConfigs[1].Name);
        }

        [Fact]
        public void AddBackupConfig_ShouldThrowException_WhenConfigAlreadyExists()
        {

            var config = new BackupConfig("Backup1", "C:/Source", "D:/Target", BackupType.Full);
            _service.AddBackupConfig(1, config);


            var exception = Assert.Throws<InvalidOperationException>(() => _service.AddBackupConfig(1, config));
            Assert.Equal($"Backup job with ID 1 already exists.", exception.Message);
        }

        [Fact]
        public void RemoveBackupConfig_ShouldRemoveConfig_WhenConfigExists()
        {

            var config = new BackupConfig("Backup1", "C:/Source", "D:/Target", BackupType.Full);
            _service.AddBackupConfig(1, config);


            _service.RemoveBackupConfig(1);

            Assert.False(_service.BackupConfigs.ContainsKey(1));
        }

        [Fact]
        public void RemoveBackupConfig_ShouldThrowException_WhenConfigDoesNotExist()
        {
            var exception = Assert.Throws<KeyNotFoundException>(() => _service.RemoveBackupConfig(1));
            Assert.Equal($"Backup job with ID 1 not found.", exception.Message);
        }

        [Fact]
        public void Save_ShouldCreateConfigFile_WhenConfigIsAdded()
        {

            var config = new BackupConfig("Backup1", "C:/Source", "D:/Target", BackupType.Full);
            _service.AddBackupConfig(1, config);

            Assert.True(File.Exists(ConfigPath));
            var json = File.ReadAllText(ConfigPath);
            var configs = JsonSerializer.Deserialize<EasySaveConfig>(json);
            Assert.True(configs.BackupConfigs.ContainsKey(1));
            Assert.Equal(config.Name, configs.BackupConfigs[1].Name);
        }
    }
}