namespace VishalXOpt.Core.Models;
public sealed record DeviceInfo(
    string Name,
    string Manufacturer,
    string InstanceId,
    string Driver,
    string DriverVersion,
    string? Irq,
    bool MsiSupported,
    bool MsiEnabled,
    string AffinityMask,
    string State = "Unknown",
    string Location = "Not available");
