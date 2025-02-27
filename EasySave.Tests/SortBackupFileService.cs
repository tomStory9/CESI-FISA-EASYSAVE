using EasySaveBusiness.Services;
using System.Collections.Generic;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class SortBackupFileServiceTests
    {
        [Fact]
        public void SortFile_ShouldSortFilesBasedOnPriorityExtensions()
        {
            // Arrange
            var service = new SortBackupFileService();
            var priorityExtensions = new List<string> { "txt", "jpg", "png" };
            var files = new string[]
            {
                "file1.doc",
                "file2.txt",
                "file3.jpg",
                "file4.png",
                "file5.txt",
                "file6.doc"
            };

            // Act
            var sortedFiles = service.SortFile(priorityExtensions, files);

            // Assert
            var expectedOrder = new List<string>
            {
                "file2.txt",
                "file5.txt",
                "file3.jpg",
                "file4.png",
                "file1.doc",
                "file6.doc"
            };
            Assert.Equal(expectedOrder, sortedFiles);
        }

        [Fact]
        public void SortFile_ShouldHandleFilesWithNoPriorityExtensions()
        {
            // Arrange
            var service = new SortBackupFileService();
            var priorityExtensions = new List<string> { "txt", "jpg" };
            var files = new string[]
            {
                "file1.doc",
                "file2.txt",
                "file3.jpg",
                "file4.png",
                "file5.txt",
                "file6.doc"
            };

            // Act
            var sortedFiles = service.SortFile(priorityExtensions, files);

            // Assert
            var expectedOrder = new List<string>
            {
                "file2.txt",
                "file5.txt",
                "file3.jpg",
                "file1.doc",
                "file4.png",
                "file6.doc"
            };
            Assert.Equal(expectedOrder, sortedFiles);
        }
    }
}
