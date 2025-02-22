namespace EasySaveBusiness.Models
{
    public record BackupConfig
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public required string SourceDirectory { get; init; }
        public required string TargetDirectory { get; init; }
        public required BackupType Type { get; init; }
    }
}