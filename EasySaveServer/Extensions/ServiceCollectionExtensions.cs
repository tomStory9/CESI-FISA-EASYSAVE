﻿using EasySaveBusiness.Controllers;
using EasySaveBusiness.Services;
using LoggerDLL.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection services)
        {
            var logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "logs");
            var fullStatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "state.json");
            var workAppName = "caca.exe";
            ManualResetEvent Systemmre = new ManualResetEvent(false);


            services.AddSingleton<EasySaveConfigService>();
            services.AddSingleton(provider => new LoggerService(logPath, LoggerDLL.Models.LogType.LogTypeEnum.JSON));
            services.AddSingleton<DifferentialBackupVerifierService>();
            services.AddSingleton<SortBackupFileService>();
            services.AddSingleton<IsRunningWorkAppService>();
            services.AddSingleton<IsNetworkUsageExceededService>();
            services.AddTransient(provider => new BackupJobService(
                provider.GetRequiredService<LoggerService>(),
                provider.GetRequiredService<EasySaveConfigService>(),
                provider.GetRequiredService<FileProcessingService>(),
                provider.GetRequiredService<SortBackupFileService>(),
                provider.GetRequiredService<IsRunningWorkAppService>(),
                provider.GetRequiredService<IsNetworkUsageExceededService>(),
                Systemmre
            ));
            services.AddSingleton(provider => new FileProcessingService(
                provider.GetRequiredService<LoggerService>(),
                provider.GetRequiredService<DifferentialBackupVerifierService>()
            ));
            services.AddSingleton(provider => new WorkAppMonitorService(workAppName, Systemmre));
            services.AddSingleton(provider => new BackupFullStateLogger(fullStatePath));
            services.AddSingleton(provider => new BackupJobsService(
                provider.GetRequiredService<EasySaveConfigService>(),
                provider.GetRequiredService<LoggerService>(),
                provider.GetRequiredService<FileProcessingService>(),
                provider.GetRequiredService<WorkAppMonitorService>(),
                Systemmre,
                provider
            ));
            services.AddSingleton(provider => new NetworkUsageMonitorService(
                provider.GetRequiredService<EasySaveConfigService>(),
                Systemmre
            ));
            services.AddSingleton<IEasySaveController>(provider => new EasySaveController(
                provider.GetRequiredService<EasySaveConfigService>(),
                provider.GetRequiredService<BackupJobsService>(),
                provider.GetRequiredService<LoggerService>(),
                provider.GetRequiredService<BackupFullStateLogger>(),
                provider.GetRequiredService<NetworkUsageMonitorService>(),
                provider.GetRequiredService<WorkAppMonitorService>()
            ));
        }
    }
}
