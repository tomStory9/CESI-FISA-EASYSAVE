using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasySaveBusiness.Services
{
    public class BackupConfigService
    {
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
        private static readonly string ConfigPath = Path.Combine(AppDataPath, "config.json");

        public Dictionary<int, BackupConfig> BackupConfigs { get; private set; } = [];

        public BackupConfigService()
        {
            Init();
        }

        private void Init()
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                BackupConfigs = JsonSerializer.Deserialize<Dictionary<int, BackupConfig>>(json) ?? [];
            }
            else
            {
                BackupConfigs = [];
            }
        }

        public void AddBackupConfig(int id, BackupConfig config)
        {
            if (BackupConfigs.ContainsKey(id))
            {
                throw new Exception("Backup job already exists");
            }

            BackupConfigs.Add(id, config);

            Save();
        }

        public void RemoveBackupConfig(int id)
        {
            if (!BackupConfigs.Remove(id))
            {
                throw new Exception("Backup job not found");
            }

            Save();
        }

        private void Save()
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
    }
}