using VishalXOpt.Core.Interfaces;
using VishalXOpt.Core.Services;
using VishalXOpt.Modules.Optimizer;
namespace VishalXOpt.UI;
public static class AppServices
{
    public static BackupRestoreService Backups { get; } = new BackupRestoreService();
    public static TweakService Tweaks { get; } = new TweakService();
    public static ComputerManager Computer { get; } = new ComputerManager();
    public static PowerProfileService PowerProfiles { get; } = new PowerProfileService();
    public static AutoCleanupService AutoCleanup { get; } = new AutoCleanupService();
    public static DeviceEnumerator Devices { get; } = new DeviceEnumerator();
    public static StartupManager StartupManager { get; } = new StartupManager();
    public static TaskListService Tasks { get; } = new TaskListService();
    public static ServiceListService Services { get; } = new ServiceListService();
    public static ProcessService Processes { get; } = new ProcessService();
    public static FrameTimeService FrameTime { get; } = new FrameTimeService();
    public static SettingsService Settings { get; } = new SettingsService();
    public static WindowsTuningService WindowsTuning { get; } = new WindowsTuningService();
    public static OptimizerModule Optimizer { get; } = new OptimizerModule(Tweaks);
    private static readonly Lazy<IHardwareMonitorService> _hardware = new(() => new HardwareMonitorService());
    public static IHardwareMonitorService Hardware => _hardware.Value;
}