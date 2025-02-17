using EasySaveBusiness.Models;

public class BackupConfig
{
    public int Id { get; }
    public string Name { get; }
    public string SourceDirectory { get; }
    public string TargetDirectory { get; }
    public BackupType Type { get; }

    public BackupConfig(int id, string name, string sourceDirectory, string targetDirectory, BackupType type)
    {
        Id = id;
        Name = name;
        SourceDirectory = sourceDirectory;
        TargetDirectory = targetDirectory;
        Type = type;
    }
}
