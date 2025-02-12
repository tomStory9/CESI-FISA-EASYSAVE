﻿using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySaveBusiness.Services
{
    public class BackupConfigService
    {
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
        private static readonly string ConfigPath = Path.Combine(AppDataPath, "config.json");

        public Dictionary<int, BackupConfig> BackupConfigs { get; private set; } = new Dictionary<int, BackupConfig>();

        public BackupConfigService()
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
                    BackupConfigs = JsonSerializer.Deserialize<Dictionary<int, BackupConfig>>(json) ?? new Dictionary<int, BackupConfig>();
                }
                else
                {
                    BackupConfigs = new Dictionary<int, BackupConfig>();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize backup configurations.", ex);
            }
        }

        public void AddBackupConfig(int id, BackupConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Backup configuration cannot be null.");
            }

            if (BackupConfigs.ContainsKey(id))
            {
                throw new InvalidOperationException($"Backup job with ID {id} already exists.");
            }

            BackupConfigs.Add(id, config);
            Save();
        }

        public void RemoveBackupConfig(int id)
        {
            if (!BackupConfigs.Remove(id))
            {
                throw new KeyNotFoundException($"Backup job with ID {id} not found.");
            }

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

                string json = JsonSerializer.Serialize(BackupConfigs, new JsonSerializerOptions
                {
                    WriteIndented = true
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