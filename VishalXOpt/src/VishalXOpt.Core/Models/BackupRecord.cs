namespace VishalXOpt.Core.Models;
public sealed class BackupRecord
{
    public string TweakId { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Hive { get; set; } = "";
    public string Path { get; set; } = "";
    public string ValueName { get; set; } = "";
    public string ValueType { get; set; } = "";
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public bool ValueExisted { get; set; }
}
