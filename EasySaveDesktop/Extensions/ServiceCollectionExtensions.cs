using EasySaveBusiness.Controllers;
using EasySaveBusiness.Services;
using EasySaveDesktop.ViewModels;
using LoggerDLL.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveDesktop.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection services)
        {
            var logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "logs");
            var fullStatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "state.json");
            var workAppName = "caca.exe";

            services.AddSingleton<EasySaveConfigService>();
            services.AddSingleton(provider => new LoggerService(logPath, LoggerDLL.Models.LogType.LogTypeEnum.JSON));
            services.AddSingleton<DifferentialBackupVerifierService>();
            services.AddSingleton(provider => new FileProcessingService(
                provider.GetRequiredService<LoggerService>(),
                provider.GetRequiredService<DifferentialBackupVerifierService>()
            ));
            services.AddSingleton(provider => new WorkAppMonitorService(workAppName));
            services.AddSingleton(provider => new BackupFullStateLogger(fullStatePath));
            services.AddSingleton(provider => new BackupJobsService(
                provider.GetRequiredService<EasySaveConfigService>(),
                provider.GetRequiredService<LoggerService>(),
                provider.GetRequiredService<FileProcessingService>(),
                provider.GetRequiredService<WorkAppMonitorService>()
            ));
            services.AddSingleton(provider => new EasySaveController(
                provider.GetRequiredService<EasySaveConfigService>(),
                provider.GetRequiredService<BackupJobsService>(),
                provider.GetRequiredService<LoggerService>(),
                provider.GetRequiredService<BackupFullStateLogger>()
            ));
            services.AddSingleton<MainWindowViewModel>();
        }
    }
}
