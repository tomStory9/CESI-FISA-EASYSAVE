using System;
using System.IO;
using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class DifferentialBackupVerifierServiceTests
    {
        [Fact]
        public void VerifyDifferentialBackupAndShaDifference_FullBackup_ReturnsTrue()
        {
            // Arrange
            var service = new DifferentialBackupVerifierService();
            var config = new BackupConfig
            {
                Id = 1,
                Name = "TestBackup",
                SourceDirectory = "/source",
                TargetDirectory = "/target",
                Type = BackupType.Full
            };
            string file1 = "testfile1.txt";
            string file2 = "testfile2.txt";

            // Create dummy files
            File.WriteAllText(file1, "content");
            File.WriteAllText(file2, "content");

            // Act
            bool result = service.VerifyDifferentialBackupAndShaDifference(config, file1, file2);

            // Assert
            Assert.True(result);

            // Cleanup
            File.Delete(file1);
            File.Delete(file2);
        }

        [Fact]
        public void VerifyDifferentialBackupAndShaDifference_DifferentialBackup_SameContent_ReturnsTrue()
        {
            // Arrange
            var service = new DifferentialBackupVerifierService();
            var config = new BackupConfig
            {
                Id = 1,
                Name = "TestBackup",
                SourceDirectory = "/source",
                TargetDirectory = "/target",
                Type = BackupType.Differential
            };
            string file1 = "testfile1.txt";
            string file2 = "testfile2.txt";

            // Create dummy files with the same content
            File.WriteAllText(file1, "content");
            File.WriteAllText(file2, "content");

            // Act
            bool result = service.VerifyDifferentialBackupAndShaDifference(config, file1, file2);

            // Assert
            Assert.True(result);

            // Cleanup
            File.Delete(file1);
            File.Delete(file2);
        }

        [Fact]
        public void VerifyDifferentialBackupAndShaDifference_DifferentialBackup_DifferentContent_ReturnsFalse()
        {
            // Arrange
            var service = new DifferentialBackupVerifierService();
            var config = new BackupConfig
            {
                Id = 1,
                Name = "TestBackup",
                SourceDirectory = "/source",
                TargetDirectory = "/target",
                Type = BackupType.Differential
            };
            string file1 = "testfile1.txt";
            string file2 = "testfile2.txt";

            // Create dummy files with different content
            File.WriteAllText(file1, "content1");
            File.WriteAllText(file2, "content2");

            // Act
            bool result = service.VerifyDifferentialBackupAndShaDifference(config, file1, file2);

            // Assert
            Assert.False(result);

            // Cleanup
            File.Delete(file1);
            File.Delete(file2);
        }

        [Fact]
        public void VerifyDifferentialBackupAndShaDifference_File2DoesNotExist_ReturnsTrue()
        {
            // Arrange
            var service = new DifferentialBackupVerifierService();
            var config = new BackupConfig
            {
                Id = 1,
                Name = "TestBackup",
                SourceDirectory = "/source",
                TargetDirectory = "/target",
                Type = BackupType.Differential
            };
            string file1 = "testfile1.txt";
            string file2 = "nonexistentfile.txt";

            // Create dummy file
            File.WriteAllText(file1, "content");

            // Act
            bool result = service.VerifyDifferentialBackupAndShaDifference(config, file1, file2);

            // Assert
            Assert.True(result);

            // Cleanup
            File.Delete(file1);
        }
    }

}
