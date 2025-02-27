using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class BackupFullStateLoggerTests
    {
        private readonly string _logFilePath = Path.Combine(Path.GetTempPath(), "backup_full_state_log.json");

        [Fact]
        public void LogBackupFullState_ShouldCreateFile_WhenStatesAreProvided()
        {
            // Arrange
            var logger = new BackupFullStateLogger(_logFilePath);
            var backupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = "C:/Source",
                TargetDirectory = "D:/Target",
                Type = BackupType.Full,
                Encrypted = false
            };
            var state = BackupJobFullState.FromBackupConfig(backupConfig) with
            {
                SourceFilePath = "C:/Source/file.txt",
                TargetFilePath = "D:/Target/file.txt",
                State = BackupJobState.ACTIVE,
                TotalFilesToCopy = 100,
                TotalFilesSize = 1024,
                NbFilesLeftToDo = 50,
                Progression = 50
            };
            var states = new List<BackupJobFullState> { state };

            // Act
            logger.LogBackupFullState(states);

            // Assert
            Assert.True(File.Exists(_logFilePath));
            var jsonContent = File.ReadAllText(_logFilePath);
            var deserializedStates = JsonSerializer.Deserialize<List<BackupJobFullState>>(jsonContent);
            Assert.NotNull(deserializedStates);
            Assert.Single(deserializedStates);
            Assert.Equal(state.Name, deserializedStates[0].Name);
            Assert.Equal(state.SourceFilePath, deserializedStates[0].SourceFilePath);
            Assert.Equal(state.State, deserializedStates[0].State);
            Assert.Equal(state.TotalFilesToCopy, deserializedStates[0].TotalFilesToCopy);
            Assert.Equal(state.TotalFilesSize, deserializedStates[0].TotalFilesSize);
            Assert.Equal(state.NbFilesLeftToDo, deserializedStates[0].NbFilesLeftToDo);
            Assert.Equal(state.Progression, deserializedStates[0].Progression);
        }

        [Fact]
        public void LogBackupFullState_ShouldOverwriteFile_WhenFileAlreadyExists()
        {
            // Arrange
            var logger = new BackupFullStateLogger(_logFilePath);
            var initialBackupConfig = new BackupConfig
            {
                Id = 1,
                Name = "Backup1",
                SourceDirectory = "C:/Source",
                TargetDirectory = "D:/Target",
                Type = BackupType.Full,
                Encrypted = false
            };
            var initialState = BackupJobFullState.FromBackupConfig(initialBackupConfig) with
            {
                SourceFilePath = "C:/Source/file.txt",
                TargetFilePath = "D:/Target/file.txt",
                State = BackupJobState.ACTIVE,
                TotalFilesToCopy = 100,
                TotalFilesSize = 1024,
                NbFilesLeftToDo = 50,
                Progression = 50
            };
            var initialStates = new List<BackupJobFullState> { initialState };
            logger.LogBackupFullState(initialStates);

            var newBackupConfig = new BackupConfig
            {
                Id = 2,
                Name = "Backup2",
                SourceDirectory = "C:/Source2",
                TargetDirectory = "D:/Target2",
                Type = BackupType.Full,
                Encrypted = false
            };
            var newState = BackupJobFullState.FromBackupConfig(newBackupConfig) with
            {
                SourceFilePath = "C:/Source2/file.txt",
                TargetFilePath = "D:/Target2/file.txt",
                State = BackupJobState.STOPPED,
                TotalFilesToCopy = 200,
                TotalFilesSize = 2048,
                NbFilesLeftToDo = 0,
                Progression = 100
            };
            var newStates = new List<BackupJobFullState> { newState };

            // Act
            logger.LogBackupFullState(newStates);

            // Assert
            var jsonContent = File.ReadAllText(_logFilePath);
            var deserializedStates = JsonSerializer.Deserialize<List<BackupJobFullState>>(jsonContent);
            Assert.NotNull(deserializedStates);
            Assert.Single(deserializedStates);
            Assert.Equal(newState.Name, deserializedStates[0].Name);
            Assert.Equal(newState.SourceFilePath, deserializedStates[0].SourceFilePath);
            Assert.Equal(newState.State, deserializedStates[0].State);
            Assert.Equal(newState.TotalFilesToCopy, deserializedStates[0].TotalFilesToCopy);
            Assert.Equal(newState.TotalFilesSize, deserializedStates[0].TotalFilesSize);
            Assert.Equal(newState.NbFilesLeftToDo, deserializedStates[0].NbFilesLeftToDo);
            Assert.Equal(newState.Progression, deserializedStates[0].Progression);
        }

        [Fact]
        public void LogBackupFullState_ShouldThrowArgumentNullException_WhenStatesIsNull()
        {
            // Arrange
            var logger = new BackupFullStateLogger(_logFilePath);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => logger.LogBackupFullState(null));
            Assert.Equal("states", exception.ParamName);
        }

        [Fact]
        public void LogBackupFullState_ShouldThrowArgumentException_WhenStatesIsEmpty()
        {
            // Arrange
            var logger = new BackupFullStateLogger(_logFilePath);
            var states = new List<BackupJobFullState>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => logger.LogBackupFullState(states));
            Assert.Equal("states", exception.ParamName);
            Assert.Contains("cannot be empty", exception.Message);
        }
    }
}
