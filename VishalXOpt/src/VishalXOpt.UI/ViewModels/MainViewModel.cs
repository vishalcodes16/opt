using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using VishalXOpt.Core.Models;
using VishalXOpt.Core.Services;
using VishalXOpt.Core.Interfaces;
using System.Diagnostics;

namespace VishalXOpt.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string selectedPage = "Home";
    [ObservableProperty] private string statusMessage = "Ready.";
    [ObservableProperty] private bool isBusy = false;
    [ObservableProperty] private SystemSnapshot? snapshot;
    [ObservableProperty] private double cpuTemp = double.NaN;
    public ObservableCollection<TweakState> Tweaks { get; } = new();
    public ObservableCollection<AutorunEntry> Autoruns { get; } = new();
    public ObservableCollection<TaskInfo> Tasks { get; } = new();
    public ObservableCollection<DeviceInfo> Devices { get; } = new();
    public ObservableCollection<NetworkAdapterInfo> NetworkAdapters { get; } = new();
    public ObservableCollection<ComponentInfo> Components { get; } = new();
    public ObservableCollection<PingResult> PingResults { get; } = new();
    public ObservableCollection<string> Logs { get; } = new();
    public IReadOnlyList<string> Profiles => AppServices.Optimizer.ProfileNames;
    public string RamText => Snapshot is null ? "Detecting…" : $"{RamUsedPercent:0}% / {Snapshot.TotalRamBytes / 1024d / 1024d / 1024d:0.0} GB";
    public string AdminText => Snapshot?.IsAdmin == true ? "Administrator" : "Standard user";
    public string CpuTempText => double.IsNaN(CpuTemp) ? "N/A" : $"{CpuTemp:0}°C";
    public int RamUsedPercent => Snapshot is null ? 0 : (int)(Snapshot.UsedRamBytes * 100.0 / Snapshot.TotalRamBytes);
    public int CpuUsagePercent => Snapshot?.CpuUsagePercent ?? 0;
    public int DiskUsagePercent => Snapshot?.DiskUsagePercent ?? 0;
    public string PowerPlan => Snapshot?.ActivePowerPlan ?? "";
    [RelayCommand] public void Navigate(string page) { SelectedPage = page; StatusMessage = $"{page} ready"; if (page == "Autoruns") ScanAutoruns(); if (page == "Tasks") ScanTasks(); if (page == "Devices" || page == "MSI Utility" || page == "Interrupts") ScanDevices(); if (page == "Network Adapters") ScanNetwork(); if (page == "Tweaks" || page == "Windows Tweaker" || page == "Gaming / FPS") DetectTweaks(); if (page == "Components") ScanComponents(); if (page == "Tools") ScanProcesses(); }
    [RelayCommand] public void Scan() => Refresh();
    [RelayCommand] public void Optimize() { SelectedPage = "Optimizer"; StatusMessage = "Choose a profile and preview its supported changes before applying."; }
    [RelayCommand] public async Task ApplyProfileAsync(string? profile) { if (string.IsNullOrWhiteSpace(profile)) { StatusMessage = "Choose an optimization profile first."; return; } var preview = AppServices.Optimizer.Preview(profile); if (preview.Operations.Count == 0) { StatusMessage = "Default profile makes no changes."; return; } var detail = string.Join("\n", preview.Operations.Select(x => $"• {x.Name}: {x.CurrentValue} → {x.RecommendedValue}")); var adminNote = preview.RequiresAdmin && !new AdminService().IsAdministrator() ? "\n\nSome operations require administrator privileges and will be skipped. Relaunch as administrator to include them." : ""; if (MessageBox.Show($"Apply '{preview.Profile}'?\n\n{detail}{adminNote}\n\nEach successful registry change is backed up and verified.", "Optimizer • Confirm changes", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) { StatusMessage = "Optimizer changes cancelled."; return; } await RunBusy(async () => { var result = await AppServices.Optimizer.ApplyAsync(profile, new AdminService().IsAdministrator()); StatusMessage = $"{result.Profile}: {result.Applied} applied, {result.Skipped} skipped." + (result.RestartRequired ? " Restart required." : ""); MessageBox.Show(string.Join("\n", result.Messages), "Optimizer • Results", MessageBoxButton.OK, result.Skipped == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning); }); DetectTweaks(); Refresh(); }
    [RelayCommand] public void Gaming() { SelectedPage = "Gaming / FPS"; StatusMessage = "Gaming controls are hardware/version aware and preview-first."; }
    [RelayCommand] public async Task RestoreAsync() { IsBusy = true; StatusMessage = "Restoring last saved registry states…"; try { var r = await ((BackupRestoreService)AppServices.Backups).RestoreAllAsync(); StatusMessage = $"Restore finished: {r.success} restored, {r.failed} failed."; } catch (Exception ex) { StatusMessage = $"Restore error: {ex.Message}"; } finally { IsBusy = false; } }
    [RelayCommand] public void ApplyBalancedPower() => ApplyPower("Balanced");
    private async void Refresh() { IsBusy = true; StatusMessage = "Detecting system state…"; try { Snapshot = await AppServices.Computer.GetSnapshotAsync(); } catch { } finally { IsBusy = false; } }
    private async void ScanAutoruns() { IsBusy = true; try { var entries = await AppServices.StartupManager.GetAutorialsAsync(); Autoruns.Clear(); foreach (var e in entries) Autoruns.Add(e); } finally { IsBusy = false; } }
    private async void ScanTasks() { IsBusy = true; try { var items = await AppServices.Tasks.GetTasksAsync(); Tasks.Clear(); foreach (var t in items) Tasks.Add(t); } finally { IsBusy = false; } }
    private void ScanDevices() { IsBusy = true; try { var items = AppServices.Devices.GetAllDevices(); Devices.Clear(); foreach (var d in items) Devices.Add(d); } finally { IsBusy = false; } }
    private void ScanNetwork() { IsBusy = true; try { var items = AppServices.Devices.GetNetworkAdapters(); NetworkAdapters.Clear(); foreach (var n in items) NetworkAdapters.Add(n); } finally { IsBusy = false; } }
    private void ScanComponents() { IsBusy = true; try { var items = AppServices.Devices.GetComponents(); Components.Clear(); foreach (var c in items) Components.Add(c); } finally { IsBusy = false; } }
    private void ScanProcesses() { IsBusy = true; try { Logs.Clear(); var processes = AppServices.Processes.GetProcessList(); foreach (var p in processes.Take(20)) Logs.Add($"{p.ProcessName} ({p.ProcessId})"); } finally { IsBusy = false; } }
    private void DetectTweaks() { Tweaks.Clear(); var states = AppServices.Tweaks.Detect(); foreach (var s in states) Tweaks.Add(s); }
    private async Task RunBusy(Func<Task> action) { IsBusy = true; try { await action(); } finally { IsBusy = false; } }
}