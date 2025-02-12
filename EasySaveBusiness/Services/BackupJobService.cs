﻿using EasySaveBusiness.Models;
using LoggerDLL.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class BackupJobService
    {
        public event EventHandler<BackupJobFullState>? BackupJobFullStateChanged;
        private LoggerService LoggerService { get; }

        public BackupJobService(LoggerService loggerService)
        {
            LoggerService = loggerService;
        }

        public async Task ExecuteBackupAsync(BackupConfig job)
        {
            Console.WriteLine($"Executing backup: {job.Name}");

            var files = Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories);
            long totalFilesSize = files.Sum(f => new FileInfo(f).Length);
            long totalFiles = files.Length;
            BackupJobFullStateChanged?.Invoke(this, new BackupJobFullState(
                job.Name,
                "",
                "",
                BackupJobState.ACTIVE,
                totalFiles,
                totalFilesSize,
                totalFiles,
                0
            ));

            int completedFiles = 0;
            long completedSize = 0;

            foreach (var file in files)
            {
                await ProcessFileAsync(job, file, completedFiles, completedSize, totalFiles, totalFilesSize);
                completedFiles++;
                completedSize += new FileInfo(file).Length;
            }

            BackupJobFullStateChanged?.Invoke(this, new BackupJobFullState(
                job.Name,
                "",
                "",
                BackupJobState.END,
                totalFiles,
                totalFilesSize,
                0,
                100
            ));

            Console.WriteLine($"Backup {job.Name} completed.");
        }

        private async Task ProcessFileAsync(BackupConfig job, string file, int completedFiles, long completedSize, long totalFiles, long totalFilesSize)
        {
            string relativePath = Path.GetRelativePath(job.SourceDirectory, file);
            string destinationFile = Path.Combine(job.TargetDirectory, relativePath);
            string destinationDir = Path.GetDirectoryName(destinationFile) ?? string.Empty;

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            if (job.Type == BackupType.Full || !File.Exists(destinationFile) ||
                File.GetLastWriteTime(file) > File.GetLastWriteTime(destinationFile))
            {
                await Task.Run(() => File.Copy(file, destinationFile, true));
            }
            stopwatch.Stop();
            double transferTime = stopwatch.Elapsed.TotalMilliseconds;

            BackupJobFullStateChanged?.Invoke(this, new BackupJobFullState(
                job.Name,
                file,
                destinationFile,
                BackupJobState.ACTIVE,
                totalFiles,
                totalFilesSize,
                totalFiles - completedFiles,
                (int)((completedSize * 100) / totalFilesSize)
            ));

            LoggerService.AddLog(new
            {
                Name = job.Name,
                FileSource = file,
                FileTarget = destinationFile,
                FileTransferTime = transferTime,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }
    }
}
