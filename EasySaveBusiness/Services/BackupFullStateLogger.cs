using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class BackupFullStateLogger
    {
        private readonly string _logFilePath;

        public BackupFullStateLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void LogBackupFullState(List<BackupJobFullState> states)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var jsonString = JsonSerializer.Serialize(states, options);
            File.WriteAllText(_logFilePath, jsonString);
        }
    }
}
