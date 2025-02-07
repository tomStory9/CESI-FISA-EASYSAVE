using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySaveBusiness.Services
{
    public class BackupFullStateLogger
    {
        private readonly string _logFilePath;

        public BackupFullStateLogger(string logFilePath)
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                throw new ArgumentException("Log file path cannot be null or empty.", nameof(logFilePath));
            }

            _logFilePath = logFilePath;
        }

        public void LogBackupFullState(List<BackupJobFullState> states)
        {
            if (states == null)
            {
                throw new ArgumentNullException(nameof(states), "States cannot be null.");
            }

            if (states.Count == 0)
            {
                throw new ArgumentException("States list cannot be empty.", nameof(states));
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var jsonString = JsonSerializer.Serialize(states, options);
            File.WriteAllText(_logFilePath, jsonString);
        }
    }
}