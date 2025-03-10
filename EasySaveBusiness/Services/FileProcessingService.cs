using EasySaveBusiness.Models;
using EasySaveBusiness.Services;
using LoggerDLL.Services;
using System.Diagnostics;

public class FileProcessingService
{
    private readonly LoggerService _loggerService;
    private readonly DifferentialBackupVerifierService _differentialBackupVerifierService;

    public event EventHandler<FileProcessedEventData>? FileProcessed;

    public FileProcessingService(
        LoggerService loggerService,
        DifferentialBackupVerifierService differentialBackupVerifierService
    )
    {
        _differentialBackupVerifierService = differentialBackupVerifierService;
        _loggerService = loggerService;
    }
    public async Task ProcessEncryptedFileAsync(BackupConfig backupConfig, string file, int completedFiles, long completedSize, long totalFiles, long totalFilesSize, ManualResetEventSlim lockEvent, object lockObject, string encryptionKey)
    {
        string relativePath = Path.GetFileName(file);
        string destinationFile = Path.Combine(backupConfig.TargetDirectory, relativePath);
        string destinationDir = Path.GetDirectoryName(backupConfig.TargetDirectory) ?? string.Empty;
        if (!Directory.Exists(backupConfig.TargetDirectory))
        {
            Directory.CreateDirectory(destinationDir);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (_differentialBackupVerifierService.VerifyDifferentialBackupAndShaDifference(backupConfig, file, destinationFile))
        {
            string cryptosoftCommand = $"CryptoSoft encrypt \"{file}\" \"{backupConfig.TargetDirectory}\" \"{encryptionKey}\"";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {cryptosoftCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Cryptosoft command failed with error: {error}");
                }
            }
        }

        stopwatch.Stop();
        double transferTime = stopwatch.Elapsed.TotalMilliseconds;

        lock (lockObject)
        {
            if (IsNetworkUsageExceededService.IsBigFileProcessing)
            {
                lockEvent.Set();
                IsNetworkUsageExceededService.IsBigFileProcessing = false;
            }
        }

        var newState = new FileProcessedEventData
        {
            NbFilesLeftToDo = totalFiles - completedFiles,
            Progression = (int)((completedSize * 100) / totalFilesSize),
            SourceFilePath = file,
            TargetFilePath = destinationFile
        };
        FileProcessed?.Invoke(this, newState);
    }
    public async Task RestoreEncryptedFileAsync(BackupConfig backupConfig, string file, int completedFiles, long completedSize, long totalFiles, long totalFilesSize, ManualResetEventSlim lockEvent, object lockObject, string encryptionKey)
    {
        string relativePath = Path.GetFileName(file);
        relativePath = relativePath.Replace(".encrypted", "");
        string destinationFile = Path.Combine(backupConfig.SourceDirectory, relativePath);
        string destinationDir = Path.GetDirectoryName(backupConfig.SourceDirectory) ?? string.Empty;
        if (!Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (_differentialBackupVerifierService.VerifyDifferentialBackupAndShaDifference(backupConfig, file, destinationFile))
        {
            string cryptosoftCommand = $"CryptoSoft decrypt \"{file}\" \"{backupConfig.SourceDirectory}\" \"{encryptionKey}\"";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {cryptosoftCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Cryptosoft command failed with error: {error}");
                }
            }
        }

        stopwatch.Stop();
        double transferTime = stopwatch.Elapsed.TotalMilliseconds;

        lock (lockObject)
        {
            if (IsNetworkUsageExceededService.IsBigFileProcessing)
            {
                lockEvent.Set();
                IsNetworkUsageExceededService.IsBigFileProcessing = false;
            }
        }

        var newState = new FileProcessedEventData
        {
            NbFilesLeftToDo = totalFiles - completedFiles,
            Progression = (int)((completedSize * 100) / totalFilesSize),
            SourceFilePath = file,
            TargetFilePath = destinationFile
        };
        FileProcessed?.Invoke(this, newState);
    }
    public async Task RestoreFileAsync(BackupConfig backupConfig, string file, int completedFiles, long completedSize, long totalFiles, long totalFilesSize, ManualResetEventSlim lockEvent, object lockObject)
    {
        string relativePath = Path.GetRelativePath(backupConfig.TargetDirectory, file);
        string destinationFile = Path.Combine(backupConfig.SourceDirectory, relativePath);
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
            if (IsNetworkUsageExceededService.IsBigFileProcessing)
            {
                lockEvent.Set();
                IsNetworkUsageExceededService.IsBigFileProcessing = false;
            }
        }

        var newState = new FileProcessedEventData
        {
            NbFilesLeftToDo = totalFiles - completedFiles,
            Progression = (int)((completedSize * 100) / totalFilesSize),
            SourceFilePath = file,
            TargetFilePath = destinationFile
        };

        FileProcessed?.Invoke(this, newState);
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
            if (IsNetworkUsageExceededService.IsBigFileProcessing)
            {
                lockEvent.Set();
                IsNetworkUsageExceededService.IsBigFileProcessing = false;
            }
        }

        var newState = new FileProcessedEventData
        {
            NbFilesLeftToDo = totalFiles - completedFiles,
            Progression = (int)((completedSize * 100) / totalFilesSize),
            SourceFilePath = file,
            TargetFilePath = destinationFile
        };

        FileProcessed?.Invoke(this, newState);
    }
}
