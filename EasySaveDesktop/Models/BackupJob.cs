using CommunityToolkit.Mvvm.ComponentModel;
using EasySaveBusiness.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveDesktop.Models
{
    public record BackupJob : BackupJobFullState
    {
        public bool IsChecked { get; init; } = false;

        public static BackupJob FromBackupJobFullState(BackupJobFullState backupJobFullState)
        {
            return new BackupJob
            {
                Id = backupJobFullState.Id,
                Name = backupJobFullState.Name,
                SourceDirectory = backupJobFullState.SourceDirectory,
                TargetDirectory = backupJobFullState.TargetDirectory,
                Type = backupJobFullState.Type,
                SourceFilePath = backupJobFullState.SourceFilePath,
                TargetFilePath = backupJobFullState.TargetFilePath,
                State = backupJobFullState.State,
                TotalFilesToCopy = backupJobFullState.TotalFilesToCopy,
                TotalFilesSize = backupJobFullState.TotalFilesSize,
                NbFilesLeftToDo = backupJobFullState.NbFilesLeftToDo,
                Progression = backupJobFullState.Progression,
            };
        }
    }
}
