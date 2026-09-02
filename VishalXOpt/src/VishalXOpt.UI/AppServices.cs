using VishalXOpt.Core.Interfaces;
using VishalXOpt.Core.Services;
namespace VishalXOpt.UI;
public static class AppServices
{
    public static IWindowsInfoService WindowsInfo { get; } = new WindowsInfoService();
    public static IPowerPlanService Power { get; } = new PowerCfgService();
    public static ILoggingService Log { get; } = new LoggingService();
    public static IBackupRestoreService Backups { get; } = new BackupRestoreService();
    public static IDeviceService Devices { get; } = new DeviceService();
    public static CleanupService Cleanup { get; } = new CleanupService();
    public static RestorePointService RestorePoints { get; } = new RestorePointService();
    public static TweakService Tweaks { get; } = new TweakService();
    public static AutorunsService Autoruns { get; } = new AutorunsService();
    public static ServiceManager Services { get; } = new ServiceManager();
    public static ScheduledTaskService Tasks { get; } = new ScheduledTaskService();
    public static NetworkAdapterService Network { get; } = new NetworkAdapterService();
    public static ProcessService Processes { get; } = new ProcessService();
    public static MsiService Msi { get; } = new MsiService();
    public static ComponentService Components { get; } = new ComponentService();
    public static LatencyService Latency { get; } = new LatencyService();
    public static WinUtilService WinUtil { get; } = new WinUtilService();
    public static DebloatService Debloat { get; } = new DebloatService();
    public static PerformanceService Performance { get; } = new PerformanceService();
    public static FrameTimeService FrameTime { get; } = new FrameTimeService();
    public static SettingsService Settings { get; } = new SettingsService();
    public static WindowsTuningService WindowsTuning { get; } = new WindowsTuningService();
    private static readonly Lazy<IHardwareMonitorService> _hardware = new(() => new HardwareMonitorService());
    public static IHardwareMonitorService Hardware => _hardware.Value;
}
