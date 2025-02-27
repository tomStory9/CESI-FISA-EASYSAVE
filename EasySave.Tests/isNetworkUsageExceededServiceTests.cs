using EasySaveBusiness.Services;
using System.Net.NetworkInformation;
using Xunit;

namespace EasySaveBusiness.Tests
{
    public class IsNetworkUsageExceededTests
    {
        [Fact]
        public void IsNetworkUsageLimitExceeded_ShouldReturnFalse_WhenUsageIsBelowLimit()
        {
            // Arrange
            var service = new IsNetworkUsageExceededService();
            string networkInterfaceName = "Ethernet";
            long networkUsageLimit = 1000000; // 1 MB

            // Act
            bool result = service.IsNetworkUsageLimitExceeded(networkInterfaceName, networkUsageLimit);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNetworkUsageLimitExceeded_ShouldReturnTrue_WhenUsageIsAboveLimit()
        {
            // Arrange
            var service = new IsNetworkUsageExceededService();
            string networkInterfaceName = "vEthernet (Default Switch)";
            long networkUsageLimit = 1; // 1 byte

            // Act
            bool result = service.IsNetworkUsageLimitExceeded(networkInterfaceName, networkUsageLimit);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNetworkUsageLimitExceeded_ShouldReturnFalse_WhenInterfaceNotFound()
        {
            // Arrange
            var service = new IsNetworkUsageExceededService();
            string networkInterfaceName = "NonExistentInterface";
            long networkUsageLimit = 1000000; // 1 MB

            // Act
            bool result = service.IsNetworkUsageLimitExceeded(networkInterfaceName, networkUsageLimit);

            // Assert
            Assert.False(result);
        }
    }
}
