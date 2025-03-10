﻿using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasySaveBusiness.Services
{
    public class EasySaveConfigService
    {
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
        private static readonly string ConfigPath = Path.Combine(AppDataPath, "config.json");
        public List<BackupConfig> BackupConfigs { get; private set; } = [];
        public EasySaveConfig EasySaveConfig { get; private set; } = EasySaveConfig.Defaults;

        public event EventHandler<BackupConfig>? BackupConfigAdded;
        public event EventHandler<int>? BackupConfigRemoved;
        public event EventHandler<BackupConfig>? BackupConfigEdited;

        public EasySaveConfigService()
        {
            Init();
        }

        private void Init()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    EasySaveConfig = JsonSerializer.Deserialize<EasySaveConfig>(json, new JsonSerializerOptions
                    {
                        IncludeFields = true
                    }) ?? EasySaveConfig;
                    BackupConfigs = EasySaveConfig.BackupConfigs ?? [];
                }
                else
                {
                    EasySaveConfig = EasySaveConfig.Defaults;
                    BackupConfigs = new List<BackupConfig>();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize easySave configurations.", ex);
            }
        }

        public void AddBackupConfig(BackupConfig config)
        {
            if (BackupConfigs.Count() == 5)
            {
                throw new InvalidOperationException("You can't add more than 5 backup configs.");
            }
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Backup configuration cannot be null.");
            }

            if (BackupConfigs.Any(bc => bc.Id == config.Id))
            {
                throw new InvalidOperationException($"Backup job with ID {config.Id} already exists.");
            }

            BackupConfigs.Add(config);
            BackupConfigAdded?.Invoke(this, config);
            Save();
        }

        public void EditBackupConfig(BackupConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Backup configuration cannot be null.");
            }
            var existingConfig = BackupConfigs.FirstOrDefault(bc => bc.Id == config.Id);
            if (existingConfig == null)
            {
                throw new KeyNotFoundException($"Backup job with ID {config.Id} not found.");
            }
            BackupConfigs.Remove(existingConfig);
            BackupConfigs.Add(config);
            BackupConfigEdited?.Invoke(this, config);
            Save();
        }

        public void OverrideBackupConfigs(List<BackupConfig> configs)
        {
            BackupConfigs = configs;
            Save();
        }

        public void RemoveBackupConfig(int id)
        {   
            var config = BackupConfigs.FirstOrDefault(bc => bc.Id == id) ?? null;
            if (config == null)
            {
                throw new KeyNotFoundException($"Backup job with ID {id} not found.");
            }

            BackupConfigs.Remove(config);
            BackupConfigRemoved?.Invoke(this, id);
            Save();
        }

        public void OverrideEasySaveConfig(EasySaveConfig config)
        {
            EasySaveConfig = config;
            Save();
        }

        private void Save()
        {
            try
            {
                if (!Directory.Exists(AppDataPath))
                {
                    Directory.CreateDirectory(AppDataPath);
                }
                EasySaveConfig = new EasySaveConfig(
                    BackupConfigs,
                    EasySaveConfig.WorkApp,
                    EasySaveConfig.PriorityFileExtension,
                    EasySaveConfig.NetworkKoLimit,
                    EasySaveConfig.NetworkInterfaceName,
                    EasySaveConfig.LogType,
                    EasySaveConfig.SizeLimit,
                    EasySaveConfig.Key
                );
                string json = JsonSerializer.Serialize(EasySaveConfig, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true
                });

                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save backup configurations.", ex);
            }
        }
    }
}
