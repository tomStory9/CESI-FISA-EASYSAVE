using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Models
{
    public record BackupJobFullState : BackupConfig {
        public string? SourceFilePath { get; init; }
        public string? TargetFilePath { get; init; }
        public BackupJobState State { get; init; } = BackupJobState.STOPPED;
        public long TotalFilesToCopy { get; init; }
        public long TotalFilesSize { get; init; }
        public long NbFilesLeftToDo { get; init; }
        public int Progression { get; init; }

        public static BackupJobFullState FromBackupConfig(BackupConfig backupConfig)
        {
            return new BackupJobFullState
            {
                Id = backupConfig.Id,
                Name = backupConfig.Name,
                SourceDirectory = backupConfig.SourceDirectory,
                TargetDirectory = backupConfig.TargetDirectory,
                Type = backupConfig.Type,
            };
        }
    }
}
