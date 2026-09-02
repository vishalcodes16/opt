namespace VishalXOpt.Core.Models;
public sealed record SystemSnapshot(
    string WindowsVersion,
    string Build,
    string Architecture,
    int LogicalProcessors,
    ulong TotalRamBytes,
    string PowerPlan,
    bool IsAdmin,
    string SecureBoot,
    string MachineType,
    ulong AvailableRamBytes,
    double CpuUsagePercent,
    double DiskUsagePercent,
    double NetworkLatencyMs);
