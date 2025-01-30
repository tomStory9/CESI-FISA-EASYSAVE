using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySave.Services
{
    class BackupConfig
    {
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
        private static readonly string ConfigPath = Path.Combine(AppDataPath, "config.json");

        public List<BackupJob> BackupJobs { get; } = [];

        public void Save()
        {
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }

            string json = JsonSerializer.Serialize(BackupJobs, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(ConfigPath, json);
        }

        public static BackupConfig Load()
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<BackupConfig>(json) ?? new BackupConfig();
            }
            return new BackupConfig();
        }
    }
}
