using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using VishalXOpt.Core.Models;
using VishalXOpt.Core.Services;
namespace VishalXOpt.UI.ViewModels;
public partial class MainViewModel:ObservableObject
{
 [ObservableProperty] private string selectedPage="Home";
 [ObservableProperty] private bool isSidebarCollapsed;
 [ObservableProperty] private SystemSnapshot? snapshot;
 [ObservableProperty] private string statusMessage="Ready";
 [ObservableProperty] private string powerPlan="Detecting…";
 [ObservableProperty] private double ramUsedPercent;
 [ObservableProperty] private double cpuUsagePercent;
 [ObservableProperty] private double cpuTemp=double.NaN;
 [ObservableProperty] private double gpuTemp=double.NaN;
 [ObservableProperty] private double gpuUsagePercent=double.NaN;
 [ObservableProperty] private double diskUsagePercent;
 [ObservableProperty] private double networkLatencyMs;
 [ObservableProperty] private int optimizationScore;
 [ObservableProperty] private bool isBusy;
 [ObservableProperty] private string selectedProfile="Default";
 [ObservableProperty] private string selectedAutorunName="";
 [ObservableProperty] private string selectedTaskName="";
 [ObservableProperty] private string selectedServiceName="";
 [ObservableProperty] private string selectedComponentName="";
 [ObservableProperty] private string selectedDeviceName="";
 [ObservableProperty] private bool selectedMsiEnabled;
 [ObservableProperty] private string selectedAdapterName="";
 [ObservableProperty] private string selectedServiceMode="auto";
 public ObservableCollection<string> Pages {get;}=new(){"Home","Optimizer","WinUtil","Windows Tweaker","Gaming / FPS","Power Management","Debloat","Cleanup","Privacy","Tweaks","Autoruns","Interrupts","Devices","Network Adapters","Tasks","Components","Deprecated / Advanced","MSI Utility","Tools","Settings"};
 public ObservableCollection<CleanupItem> CleanupItems{get;}=new();
 public ObservableCollection<DeviceInfo> Devices{get;}=new();
 public ObservableCollection<TweakState> TweakStates{get;}=new();
 public ObservableCollection<AutorunEntry> Autoruns{get;}=new();
 public ObservableCollection<TaskInfo> Tasks{get;}=new();
 public ObservableCollection<ServiceInfo> Services{get;}=new();
 public ObservableCollection<AdapterInfo> Adapters{get;}=new();
 public ObservableCollection<ProcessInfo> Processes{get;}=new();
 public ObservableCollection<ComponentInfo> Components{get;}=new();
 public ObservableCollection<PingResult> PingResults{get;}=new();
 public ObservableCollection<string> Logs{get;}=new();
 public IReadOnlyList<string> Profiles{get;}=new[]{"Default","Optimal","Maximum","Gaming / FPS Maximum"};
 public string RamText=>Snapshot is null?"Detecting…":$"{RamUsedPercent:0}% / {Snapshot.TotalRamBytes/1024d/1024d/1024d:0.0} GB";
 public string AdminText=>Snapshot?.IsAdmin==true?"Administrator":"Standard user";
 public string CpuTempText=>double.IsNaN(CpuTemp)?"N/A":$"{CpuTemp:0}°C";
 public string GpuTempText=>double.IsNaN(GpuTemp)?"N/A":$"{GpuTemp:0}°C";
 public string GpuUsageText=>double.IsNaN(GpuUsagePercent)?"N/A":$"{GpuUsagePercent:0}%";
 public string NetworkText=>NetworkLatencyMs<0?"N/A":$"{NetworkLatencyMs:0} ms";
 public MainViewModel(){Refresh();}
 [RelayCommand] public void ToggleSidebar()=>IsSidebarCollapsed=!IsSidebarCollapsed;
 [RelayCommand] public void Navigate(string page){SelectedPage=page;StatusMessage=$"{page} ready";if(page=="Autoruns")ScanAutoruns();if(page=="Tasks")ScanTasks();if(page=="Devices"||page=="MSI Utility"||page=="Interrupts")ScanDevices();if(page=="Network Adapters")ScanNetwork();if(page=="Tweaks"||page=="Windows Tweaker"||page=="Gaming / FPS")DetectTweaks();if(page=="Components")ScanComponents();if(page=="Tools")ScanProcesses();}
 [RelayCommand] public void Scan()=>Refresh();
 [RelayCommand] public void Optimize(){SelectedPage="Optimizer";StatusMessage="Choose a profile and preview its supported changes before applying.";}
 [RelayCommand] public void Gaming(){SelectedPage="Gaming / FPS";StatusMessage="Gaming controls are hardware/version aware and preview-first.";}
 [RelayCommand] public async Task RestoreAsync(){IsBusy=true;StatusMessage="Restoring last saved registry states…";try{var r=await ((BackupRestoreService)AppServices.Backups).RestoreAllAsync();StatusMessage=$"Restore finished: {r.success} restored, {r.failed} failed.";}catch(Exception ex){StatusMessage=$"Restore error: {ex.Message}";}finally{IsBusy=false;}}
 [RelayCommand] public void ApplyBalancedPower()=>ApplyPower("Balanced");
 [RelayCommand] public void ApplyHighPerformance()=>ApplyPower("High Performance");
 [RelayCommand] public async Task ApplyUltimatePerformanceAsync(){await RunBusy(async()=>{var ok=await ((PowerCfgService)AppServices.Power).EnsureUltimatePerformanceAsync();StatusMessage=ok?"Ultimate Performance activated.":"Unable to create/activate Ultimate Performance.";PowerPlan=AppServices.Power.GetActivePlan();});}
 [RelayCommand] public void ScanCleanup(){CleanupItems.Clear();foreach(var i in AppServices.Cleanup.Scan())CleanupItems.Add(i);StatusMessage=$"Cleanup scan found {CleanupItems.Count} categories.";}
 [RelayCommand] public async Task CleanSafeAsync(){if(CleanupItems.Count==0)ScanCleanup();await RunBusy(async()=>{var freed=await AppServices.Cleanup.DeleteAsync(CleanupItems);StatusMessage=$"Cleanup complete. Freed {freed/1024d/1024d:0.0} MB.";ScanCleanup();});}
 [RelayCommand] public async Task CreateRestorePointAsync(){await RunBusy(async()=>{var ok=await AppServices.RestorePoints.TryCreateAsync("Vishal X Opt - User Restore Point");StatusMessage=ok?"Restore point created.":"Windows did not create the restore point.";});}
 [RelayCommand] public void ScanDevices(){Devices.Clear();foreach(var d in AppServices.Devices.GetDevices())Devices.Add(AppServices.Msi.Enrich(d));StatusMessage=$"Detected {Devices.Count} connected devices.";}
 [RelayCommand] public void DetectTweaks(){TweakStates.Clear();foreach(var t in AppServices.Tweaks.Detect())TweakStates.Add(t);StatusMessage=$"Detected {TweakStates.Count} supported tweak definitions.";}
 [RelayCommand] public async Task ApplyTweakAsync(TweakState? state){if(state is null||!state.Supported){StatusMessage="Tweak is unavailable on this system.";return;}await RunBusy(async()=>{var r=await AppServices.Tweaks.ApplyAsync(state.Definition);StatusMessage=r.Message+(r.RestartRequired?" Restart required.":"");});DetectTweaks();Refresh();}
 [RelayCommand] public void ScanAutoruns(){Autoruns.Clear();foreach(var a in AppServices.Autoruns.GetEntries())Autoruns.Add(a);StatusMessage=$"Detected {Autoruns.Count} startup entries.";}
 [RelayCommand] public void DisableSelectedAutorun(){var e=Autoruns.FirstOrDefault(x=>x.Name==SelectedAutorunName);if(e is null){StatusMessage="Select an autorun entry first.";return;}if(e.Classification is "Microsoft" or "Driver" or "Security"){StatusMessage="Protected classification: inspect manually; automatic disable is blocked.";return;}StatusMessage=AppServices.Autoruns.Disable(e)?$"Disabled {e.Name}; entry moved to VishalXOptDisabled.":$"Unable to disable {e.Name}.";ScanAutoruns();}
 [RelayCommand] public void RestoreSelectedAutorun(){var e=Autoruns.FirstOrDefault(x=>x.Name==SelectedAutorunName&&!x.Enabled);if(e is null){StatusMessage="Select a disabled autorun entry first.";return;}StatusMessage=AppServices.Autoruns.Restore(e)?$"Restored {e.Name}.":$"Unable to restore {e.Name}.";ScanAutoruns();}
 [RelayCommand] public void ScanTasks(){Tasks.Clear();foreach(var t in AppServices.Tasks.GetTasks())Tasks.Add(t);StatusMessage=$"Detected {Tasks.Count} scheduled tasks.";}
 [RelayCommand] public void DisableSelectedTask(){if(string.IsNullOrWhiteSpace(SelectedTaskName)){StatusMessage="Select a task first.";return;}StatusMessage=AppServices.Tasks.SetEnabled(SelectedTaskName,false)?"Task disabled. It was not deleted.":"Unable to disable task.";ScanTasks();}
 [RelayCommand] public void EnableSelectedTask(){if(string.IsNullOrWhiteSpace(SelectedTaskName)){StatusMessage="Select a task first.";return;}StatusMessage=AppServices.Tasks.SetEnabled(SelectedTaskName,true)?"Task enabled.":"Unable to enable task.";ScanTasks();}
 [RelayCommand] public void ScanServices(){Services.Clear();foreach(var s in AppServices.Services.GetServices())Services.Add(s);StatusMessage=$"Detected {Services.Count} Windows services.";}
 [RelayCommand] public void StartSelectedService(){if(string.IsNullOrWhiteSpace(SelectedServiceName)){StatusMessage="Select a service first.";return;}StatusMessage=AppServices.Services.Start(SelectedServiceName)?$"Started {SelectedServiceName}.":"Unable to start {SelectedServiceName}.";ScanServices();}
 [RelayCommand] public void StopSelectedService(){if(string.IsNullOrWhiteSpace(SelectedServiceName)){StatusMessage="Select a service first.";return;}StatusMessage=AppServices.Services.Stop(SelectedServiceName)?$"Stopped {SelectedServiceName}.":"Unable to stop {SelectedServiceName}.";ScanServices();}
 [RelayCommand] public void SetSelectedServiceStartup(){if(string.IsNullOrWhiteSpace(SelectedServiceName)){StatusMessage="Select a service first.";return;}StatusMessage=AppServices.Services.SetStartup(SelectedServiceName,SelectedServiceMode)?$"{SelectedServiceName} startup set to {SelectedServiceMode}.":"Unable to change service startup.";ScanServices();}
 [RelayCommand] public void ScanNetwork(){Adapters.Clear();foreach(var a in AppServices.Network.GetAdapters())Adapters.Add(a);StatusMessage=$"Detected {Adapters.Count} network adapters.";}
 [RelayCommand] public void InspectSelectedAdapter(){if(string.IsNullOrWhiteSpace(SelectedAdapterName)){StatusMessage="Select a network adapter first.";return;}var props=AppServices.WindowsTuning.GetAdapterProperties(SelectedAdapterName);StatusMessage=props.Count==0?$"No advanced properties were exposed by {SelectedAdapterName}.":$"{props.Count} advanced adapter properties detected for {SelectedAdapterName}.";}
 [RelayCommand] public void ScanProcesses(){Processes.Clear();foreach(var p in AppServices.Processes.GetProcesses())Processes.Add(p);StatusMessage=$"Detected {Processes.Count} running processes.";}
 [RelayCommand] public void ScanComponents(){Components.Clear();foreach(var c in AppServices.Components.GetFeatures())Components.Add(c);StatusMessage=$"Detected {Components.Count} Windows features.";}
 [RelayCommand] public void EnableSelectedComponent(){SetComponent(true);}
 [RelayCommand] public void DisableSelectedComponent(){SetComponent(false);}
 [RelayCommand] public async Task ApplyGameModeAsync(){await ApplyNamedTweak("gaming.game-mode");}
 [RelayCommand] public async Task DisableGameDvrAsync(){await ApplyNamedTweak("gaming.game-dvr");}
 [RelayCommand] public async Task EnableHagsAsync(){await RunBusy(async()=>{var r=await AppServices.WindowsTuning.SetHagsAsync(true);StatusMessage=r.Message+(r.RestartRequired?" Restart required.":"");});}
 [RelayCommand] public async Task DisableHagsAsync(){await RunBusy(async()=>{var r=await AppServices.WindowsTuning.SetHagsAsync(false);StatusMessage=r.Message+(r.RestartRequired?" Restart required.":"");});}
 [RelayCommand] public async Task BestPerformanceVisualsAsync(){await RunBusy(async()=>{var r=await AppServices.WindowsTuning.SetBestPerformanceAsync();StatusMessage=r.Message+(r.RestartRequired?" Sign out/restart may be required.":"");});}
 [RelayCommand] public void DetectHags(){var h=AppServices.WindowsTuning.GetHags();StatusMessage=h.Supported?$"HAGS: {h.CurrentValue} — {(h.Enabled?"Enabled":"Disabled/Default")}. {h.Reason}":$"HAGS unavailable: {h.Reason}";}
 [RelayCommand] public async Task ApplyMouseAccelerationAsync(){await ApplyNamedTweak("input.mouse-acceleration");}
 [RelayCommand] public void TestInternet(){PingResults.Clear();foreach(var r in AppServices.Latency.Test("1.1.1.1","8.8.8.8"))PingResults.Add(r);StatusMessage="Internet latency test completed.";}
 [RelayCommand] public async Task CaptureFpsAsync(string? processName){if(string.IsNullOrWhiteSpace(processName)){StatusMessage="Enter a game process name, e.g. notepad or FortniteClient-Win64-Shipping.exe.";return;}await RunBusy(async()=>{StatusMessage="Capturing frame time…";var r=await AppServices.FrameTime.CaptureAsync(processName.Trim(),10);StatusMessage=r.Available?$"FPS {r.Fps:0.0} • 1% low frame time {r.OnePercentLowMs:0.0} ms • Avg frame {r.AverageFrameTimeMs:0.0} ms":r.Message;MessageBox.Show(r.Message+($"\n\nFPS: {r.Fps:0.0}\n1% low frame time: {r.OnePercentLowMs:0.0} ms\nAverage frame time: {r.AverageFrameTimeMs:0.0} ms"),"Frame Time Capture",MessageBoxButton.OK,r.Available?MessageBoxImage.Information:MessageBoxImage.Warning);});}
 [RelayCommand] public void OpenWinUtilSource(){AppServices.WinUtil.OpenOfficialSource();StatusMessage="Opened the official WinUtil source page. No remote script was executed.";}
 [RelayCommand] public async Task RunWinUtilConfirmedAsync(){if(MessageBox.Show($"This will download and execute the remote PowerShell script:\n\n{WinUtilService.Command}\n\nSource: https://christitus.com/win\n\nContinue only if you explicitly trust and have inspected the official source.","WinUtil • Remote Code Execution",MessageBoxButton.YesNo,MessageBoxImage.Warning)!=MessageBoxResult.Yes){StatusMessage="WinUtil execution cancelled.";return;}await RunBusy(async()=>{StatusMessage="Running WinUtil…";var r=await AppServices.WinUtil.RunConfirmedAsync();StatusMessage=r.Item1==0?"WinUtil finished successfully.":$"WinUtil exited with code {r.Item1}.";MessageBox.Show((r.Item2+"\n"+r.Item3).Trim(),"WinUtil Output",MessageBoxButton.OK,r.Item1==0?MessageBoxImage.Information:MessageBoxImage.Error);});}
 [RelayCommand] public void RefreshLogs(){Logs.Clear();foreach(var l in ((LoggingService)AppServices.Log).ReadRecent())Logs.Add(l);}
 [RelayCommand] public void SaveSettings(){AppServices.Settings.Save(AppServices.Settings.Load());StatusMessage="Settings saved.";}
 [RelayCommand] public void ApplyMsiSelected(){var d=Devices.FirstOrDefault(x=>x.Name==SelectedDeviceName);if(d is null||!d.MsiSupported){StatusMessage="Select a device with detected MSI support.";return;}var enabled=!d.MsiEnabled;StatusMessage=AppServices.Msi.SetMsi(d,enabled)?$"MSI {(enabled?"enabled":"disabled")} for {d.Name}. Restart may be required.":"MSI change was not applied.";ScanDevices();}
 [RelayCommand] public void ValidateAffinity(){var n=Environment.ProcessorCount;var mask=n>=63?ulong.MaxValue:(1UL<<n)-1;StatusMessage=$"Detected {n} logical processors. Valid affinity mask range: 0x{mask:X}. Device affinity remains staged until compatibility is validated.";}
 [RelayCommand] public void SaveProfile(){AppServices.Settings.Save(AppServices.Settings.Load());StatusMessage=$"Profile '{SelectedProfile}' selected. Changes are preview-first.";}
 private void SetComponent(bool enable){if(string.IsNullOrWhiteSpace(SelectedComponentName)){StatusMessage="Select a component first.";return;}if(MessageBox.Show($"{(enable?"Enable":"Disable")} Windows feature?\n\n{SelectedComponentName}\n\nThis uses DISM and may require a restart. Continue?","Windows Components • Confirm",MessageBoxButton.YesNo,MessageBoxImage.Warning)!=MessageBoxResult.Yes){StatusMessage="Component change cancelled.";return;}var ok=AppServices.Components.SetFeature(SelectedComponentName,enable);StatusMessage=ok?$"{SelectedComponentName} {(enable?"enabled":"disabled")}. Restart may be required.":$"Unable to {(enable?"enable":"disable")} {SelectedComponentName}.";ScanComponents();}
 private async Task ApplyNamedTweak(string id){var state=AppServices.Tweaks.Detect().FirstOrDefault(x=>x.Definition.Id==id);if(state is null){StatusMessage="Tweak definition not found.";return;}await ApplyTweakAsync(state);}
 private async Task RunBusy(Func<Task> action){if(IsBusy)return;IsBusy=true;try{await action();}catch(Exception ex){StatusMessage=$"Operation failed: {ex.Message}";}finally{IsBusy=false;}}
 private void ApplyPower(string plan){try{var before=AppServices.Power.GetActivePlan();var ok=AppServices.Power.SetPlanAsync(plan).GetAwaiter().GetResult();PowerPlan=AppServices.Power.GetActivePlan();StatusMessage=ok?$"Power plan changed {before} → {PowerPlan}.":"Unable to activate requested power plan.";}catch(Exception ex){StatusMessage=$"Power plan error: {ex.Message}";}}
 public void Refresh(){try{Snapshot=AppServices.WindowsInfo.GetSnapshot();PowerPlan=Snapshot.PowerPlan;RamUsedPercent=Math.Clamp(100d*(1d-Snapshot.AvailableRamBytes/(double)Math.Max(1UL,Snapshot.TotalRamBytes)),0,100);CpuUsagePercent=Snapshot.CpuUsagePercent;DiskUsagePercent=Snapshot.DiskUsagePercent;NetworkLatencyMs=Snapshot.NetworkLatencyMs;OptimizationScore=CalculateScore(Snapshot);try{var sensors=AppServices.Hardware.GetSensors();var cpu=sensors.Where(s=>s.Unit.Equals("Temperature",StringComparison.OrdinalIgnoreCase)&&s.Name.Contains("CPU",StringComparison.OrdinalIgnoreCase)).Select(s=>(double?)s.Value).FirstOrDefault(x=>x.HasValue);var gpu=sensors.Where(s=>s.Unit.Equals("Temperature",StringComparison.OrdinalIgnoreCase)&&s.Name.Contains("GPU",StringComparison.OrdinalIgnoreCase)).Select(s=>(double?)s.Value).FirstOrDefault(x=>x.HasValue);CpuTemp=cpu??double.NaN;GpuTemp=gpu??double.NaN;var load=sensors.Where(s=>s.Name.Contains("GPU",StringComparison.OrdinalIgnoreCase)&&s.Unit.Equals("Load",StringComparison.OrdinalIgnoreCase)).Select(s=>(double?)s.Value).FirstOrDefault(x=>x.HasValue);GpuUsagePercent=load??double.NaN;}catch{CpuTemp=double.NaN;GpuTemp=double.NaN;GpuUsagePercent=double.NaN;}OnPropertyChanged(nameof(RamText));OnPropertyChanged(nameof(AdminText));OnPropertyChanged(nameof(CpuTempText));OnPropertyChanged(nameof(GpuTempText));OnPropertyChanged(nameof(GpuUsageText));OnPropertyChanged(nameof(NetworkText));StatusMessage="System scan complete.";}catch(Exception ex){StatusMessage=$"Scan unavailable: {ex.Message}";}}
 private static int CalculateScore(SystemSnapshot s){int score=50;if(s.LogicalProcessors>=4)score+=10;if(s.TotalRamBytes>=16UL*1024*1024*1024)score+=10;if(s.IsAdmin)score+=5;if(s.SecureBoot=="Enabled")score+=10;if(s.DiskUsagePercent<90)score+=5;if(s.NetworkLatencyMs>=0&&s.NetworkLatencyMs<60)score+=5;return Math.Clamp(score,0,100);}
}
