using EasySaveBusiness.Models;

public class BackupConfig(int id, string name, string sourceDirectory, string targetDirectory, BackupType type)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string SourceDirectory { get; set; } = sourceDirectory;
    public string TargetDirectory { get; set; } = targetDirectory;
    public BackupType Type { get; set; } = type;
}
