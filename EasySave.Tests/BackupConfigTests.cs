using EasySaveBusiness.Models;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class BackupConfigTests
    {
        [Fact]
        public void BackupConfig_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            string name = "Backup1";
            string sourceDirectory = "C:/Source";
            string targetDirectory = "D:/Target";
            BackupType type = BackupType.Full;

            // Act
            var backupConfig = new BackupConfig(name, sourceDirectory, targetDirectory, type);

            // Assert
            Assert.Equal(name, backupConfig.Name);
            Assert.Equal(sourceDirectory, backupConfig.SourceDirectory);
            Assert.Equal(targetDirectory, backupConfig.TargetDirectory);
            Assert.Equal(type, backupConfig.Type);
        }

        [Fact]
        public void BackupConfig_ShouldHaveReadOnlyProperties()
        {
            // Arrange
            string name = "Backup1";
            string sourceDirectory = "C:/Source";
            string targetDirectory = "D:/Target";
            BackupType type = BackupType.Full;

            var backupConfig = new BackupConfig(name, sourceDirectory, targetDirectory, type);

            // Act & Assert
            // Vérifiez que les propriétés sont en lecture seule (pas de setter public)
            Assert.True(backupConfig.GetType().GetProperty("Name")?.CanWrite == false);
            Assert.True(backupConfig.GetType().GetProperty("SourceDirectory")?.CanWrite == false);
            Assert.True(backupConfig.GetType().GetProperty("TargetDirectory")?.CanWrite == false);
            Assert.True(backupConfig.GetType().GetProperty("Type")?.CanWrite == false);
        }

        [Fact]
        public void BackupConfig_ShouldThrowArgumentNullException_WhenNameIsNull()
        {
            // Arrange
            string name = null!;
            string sourceDirectory = "C:/Source";
            string targetDirectory = "D:/Target";
            BackupType type = BackupType.Full;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BackupConfig(name, sourceDirectory, targetDirectory, type));
        }

        [Fact]
        public void BackupConfig_ShouldThrowArgumentNullException_WhenSourceDirectoryIsNull()
        {
            // Arrange
            string name = "Backup1";
            string sourceDirectory = null!;
            string targetDirectory = "D:/Target";
            BackupType type = BackupType.Full;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BackupConfig(name, sourceDirectory, targetDirectory, type));
        }

        [Fact]
        public void BackupConfig_ShouldThrowArgumentNullException_WhenTargetDirectoryIsNull()
        {
            // Arrange
            string name = "Backup1";
            string sourceDirectory = "C:/Source";
            string targetDirectory = null!;
            BackupType type = BackupType.Full;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BackupConfig(name, sourceDirectory, targetDirectory, type));
        }
    }
}