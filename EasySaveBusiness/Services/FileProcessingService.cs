using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using LoggerDLL.Services;
using System.Diagnostics;

public class FileProcessingService
{
    private readonly LoggerService _loggerService;
    private readonly DifferentialBackupVerifierService _differentialBackupVerifierService;
    private readonly BackupJobService _backupJobService;

    public FileProcessingService(LoggerService loggerService, DifferentialBackupVerifierService differentialBackupVerifierService, BackupJobService backupJobService)
    {
        _differentialBackupVerifierService = differentialBackupVerifierService;
        _loggerService = loggerService;
        _backupJobService = backupJobService;
    }

    public async Task ProcessFileAsync(BackupConfig backupConfig, string file, int completedFiles, long completedSize, long totalFiles, long totalFilesSize, ManualResetEventSlim lockEvent, object lockObject)
    {
        string relativePath = Path.GetRelativePath(backupConfig.SourceDirectory, file);
        string destinationFile = Path.Combine(backupConfig.TargetDirectory, relativePath);
        string destinationDir = Path.GetDirectoryName(destinationFile) ?? string.Empty;

        if (!Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (_differentialBackupVerifierService.VerifyDifferentialBackupAndShaDifference(backupConfig, file, destinationFile))
        {
            using (FileStream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (FileStream destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
        }

        stopwatch.Stop();
        double transferTime = stopwatch.Elapsed.TotalMilliseconds;

        lock (lockObject)
        {
            if (IsNetworkUsageExceeded.IsBigFileProcessing)
            {
                lockEvent.Set();
                IsNetworkUsageExceeded.IsBigFileProcessing = false;
            }
        }

        var newState = _backupJobService.FullState with
        {
            NbFilesLeftToDo = totalFiles - completedFiles,
            Progression = (int)((completedSize * 100) / totalFilesSize),
            SourceFilePath = file,
            TargetFilePath = destinationFile
        };

        _backupJobService.UpdateBackupJobFullState(newState);
    }

}
