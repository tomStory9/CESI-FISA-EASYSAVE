using EasySaveBusiness.Models;
using LoggerDLL.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EasySaveBusiness.Services
{
    public class FileProcessingService
    {
        private readonly LoggerService _loggerService;
        private readonly DifferentialBackupVerifierService _differentialBackupVerifierService;

        public FileProcessingService(LoggerService loggerService, DifferentialBackupVerifierService differentialBackupVerifierService)
        {
            _differentialBackupVerifierService = differentialBackupVerifierService;
            _loggerService = loggerService;
        }

        public async Task ProcessFileAsync(BackupConfig backupConfig, string file, int completedFiles, long completedSize, long totalFiles, long totalFilesSize, EventHandler<BackupJobFullState>? backupJobFullStateChanged)
        {
            string relativePath = Path.GetRelativePath(backupConfig.SourceDirectory, file);
            string destinationFile = Path.Combine(backupConfig.TargetDirectory, relativePath);
            string destinationDir = Path.GetDirectoryName(destinationFile) ?? string.Empty;

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (_differentialBackupVerifierService.VerifyDifferentialBackupAndShaDifference(backupConfig,file,destinationFile))
            {
                await Task.Run(() => File.Copy(file, destinationFile, true));
            }
            stopwatch.Stop();
            double transferTime = stopwatch.Elapsed.TotalMilliseconds;

            backupJobFullStateChanged?.Invoke(this, new BackupJobFullState(
                backupConfig.Id,
                backupConfig.Name,
                file,
                destinationFile,
                BackupJobState.ACTIVE,
                totalFiles,
                totalFilesSize,
                totalFiles - completedFiles,
                (int)((completedSize * 100) / totalFilesSize)
            ));

            _loggerService.AddLog(new
            {
                Name = backupConfig.Name,
                FileSource = file,
                FileTarget = destinationFile,
                FileTransferTime = transferTime,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }
    }
}
