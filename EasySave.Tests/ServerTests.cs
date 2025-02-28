using System;
using System.Threading;
using System.Threading.Tasks;
using EasySaveServer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EasySave.Tests
{
    public class ServerTests
    {
        private const string MutexName = "EasySaveServerSingleton"; // Must match your program's mutex

        [Fact]
        public void OnlyOneInstance_ShouldBeAllowed()
        {
            using var firstInstanceMutex = new Mutex(true, MutexName, out bool isFirstInstance);
            Assert.True(isFirstInstance, "First instance should be allowed to run.");

            // Attempt to start a second instance
            using var secondInstanceMutex = new Mutex(true, MutexName, out bool isSecondInstance);
            Assert.False(isSecondInstance, "Second instance should NOT be allowed.");
        }

        [Fact]
        public async Task StartingSecondInstance_ShouldFail()
        {
            // First instance acquires the mutex
            using var firstInstanceMutex = new Mutex(true, MutexName, out bool isFirstInstance);
            Assert.True(isFirstInstance, "First instance should start successfully.");

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddCommonServices();
                    services.AddHostedService<SocketServer>();
                });

            var host = hostBuilder.Build();

            // Start the first instance
            await host.StartAsync();

            // Attempt to start a second instance
            using var secondInstanceMutex = new Mutex(true, MutexName, out bool isSecondInstance);
            Assert.False(isSecondInstance, "Second instance should NOT be allowed.");

            await host.StopAsync();
        }
    }
}
