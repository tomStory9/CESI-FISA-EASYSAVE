using EasySaveBusiness.Services;
using System.Diagnostics;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class IsRunningWorkAppServiceTests
    {
        [Fact]
        public void IsRunning_ShouldReturnTrue_WhenProcessIsRunning()
        {
            // Arrange
            var service = new IsRunningWorkAppService();
            var processName = "notepad"; 

            Process.Start(processName);

            // Act
            var isRunning = service.IsRunning(processName);

            // Assert
            Assert.True(isRunning);
        }

        [Fact]
        public void IsRunning_ShouldReturnFalse_WhenProcessIsNotRunning()
        {
            // Arrange
            var service = new IsRunningWorkAppService();
            var processName = "nonexistentprocess";

            // Act
            var isRunning = service.IsRunning(processName);

            // Assert
            Assert.False(isRunning);
        }
    }
}
