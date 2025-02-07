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
            var states = new List<BackupJobFullState>
            {
                new BackupJobFullState(
                    name: "Backup1",
                    sourceFilePath: "C:/Source",
                    targetFilePath: "D:/Target",
                    state: BackupJobState.ACTIVE,
                    totalFilesToCopy: 100,
                    totalFilesSize: 1024,
                    nbFilesLeftToDo: 50,
                    progression: 50
                )
            };

            // Act
            logger.LogBackupFullState(states);

            // Assert
            Assert.True(File.Exists(_logFilePath));
            var jsonContent = File.ReadAllText(_logFilePath);
            var deserializedStates = JsonSerializer.Deserialize<List<BackupJobFullState>>(jsonContent);
            Assert.NotNull(deserializedStates);
            Assert.Single(deserializedStates);
            Assert.Equal(states[0].Name, deserializedStates[0].Name);
            Assert.Equal(states[0].SourceFilePath, deserializedStates[0].SourceFilePath);
            Assert.Equal(states[0].State, deserializedStates[0].State);
            Assert.Equal(states[0].TotalFilesToCopy, deserializedStates[0].TotalFilesToCopy);
            Assert.Equal(states[0].TotalFilesSize, deserializedStates[0].TotalFilesSize);
            Assert.Equal(states[0].NbFilesLeftToDo, deserializedStates[0].NbFilesLeftToDo);
            Assert.Equal(states[0].Progression, deserializedStates[0].Progression);
        }

        [Fact]
        public void LogBackupFullState_ShouldOverwriteFile_WhenFileAlreadyExists()
        {
            // Arrange
            var logger = new BackupFullStateLogger(_logFilePath);
            var initialStates = new List<BackupJobFullState>
            {
                new BackupJobFullState(
                    name: "Backup1",
                    sourceFilePath: "C:/Source",
                    targetFilePath: "D:/Target",
                    state: BackupJobState.ACTIVE,
                    totalFilesToCopy: 100,
                    totalFilesSize: 1024,
                    nbFilesLeftToDo: 50,
                    progression: 50
                )
            };
            logger.LogBackupFullState(initialStates);

            var newStates = new List<BackupJobFullState>
            {
                new BackupJobFullState(
                    name: "Backup2",
                    sourceFilePath: "C:/Source2",
                    targetFilePath: "D:/Target2",
                    state: BackupJobState.END,
                    totalFilesToCopy: 200,
                    totalFilesSize: 2048,
                    nbFilesLeftToDo: 0,
                    progression: 100
                )
            };

            // Act
            logger.LogBackupFullState(newStates);

            // Assert
            var jsonContent = File.ReadAllText(_logFilePath);
            var deserializedStates = JsonSerializer.Deserialize<List<BackupJobFullState>>(jsonContent);
            Assert.NotNull(deserializedStates);
            Assert.Single(deserializedStates);
            Assert.Equal(newStates[0].Name, deserializedStates[0].Name);
            Assert.Equal(newStates[0].SourceFilePath, deserializedStates[0].SourceFilePath);
            Assert.Equal(newStates[0].State, deserializedStates[0].State);
            Assert.Equal(newStates[0].TotalFilesToCopy, deserializedStates[0].TotalFilesToCopy);
            Assert.Equal(newStates[0].TotalFilesSize, deserializedStates[0].TotalFilesSize);
            Assert.Equal(newStates[0].NbFilesLeftToDo, deserializedStates[0].NbFilesLeftToDo);
            Assert.Equal(newStates[0].Progression, deserializedStates[0].Progression);
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