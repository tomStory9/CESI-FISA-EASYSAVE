namespace EasySaveBusiness.Models
{
    public class BackupConfig(string name, string sourceDirectory, string targetDirectory, BackupType type)
    {
        public string Name { get; } = name;
        public string SourceDirectory { get; } = sourceDirectory;
        public string TargetDirectory { get; } = targetDirectory;
        public BackupType Type { get; } = type;
    }
}
