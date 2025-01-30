using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType type)
    {
        public string Name { get; } = name;
        public string SourceDirectory { get; } = sourceDirectory;
        public string TargetDirectory { get; } = targetDirectory;
        public BackupType Type { get; } = type;

        public override string ToString()
        {
            return $"{Name} ({Type}) - {SourceDirectory} → {TargetDirectory}";
        }
    }
}
