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

    }
}
