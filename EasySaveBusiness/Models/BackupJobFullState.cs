using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveBusiness.Models
{
    public class BackupJobFullState(
        int id,
        string name,
        string sourceFilePath,
        string targetFilePath,
        BackupJobState state,
        long totalFilesToCopy,
        long totalFilesSize,
        long nbFilesLeftToDo,
        int progression
    ) {
        public int Id { get; } = id;
        public string Name { get; } = name;
        public string SourceFilePath { get; } = sourceFilePath;
        public string TargetFilePath { get; } = targetFilePath;
        public BackupJobState State { get; } = state;
        public long TotalFilesToCopy { get; } = totalFilesToCopy;
        public long TotalFilesSize { get; } = totalFilesSize;
        public long NbFilesLeftToDo { get; } = nbFilesLeftToDo;
        public int Progression { get; } = progression;

        public static BackupJobFullState Default(
            int id,
            string name,
            string sourceFilePath,
            string targetFilePath
        )
        {
            return new BackupJobFullState(
                id,
                name,
                sourceFilePath,
                targetFilePath,
                BackupJobState.STOPPED,
                0,
                0,
                0,
                0
            );
        }
    }
}
