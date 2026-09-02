using Microsoft.Win32;
using System.Diagnostics;
using System.Net.NetworkInformation;
using VishalXOpt.Core.Models;

namespace VishalXOpt.Core.Services;

public sealed record HagsState(bool Supported, bool Enabled, string CurrentValue, string Reason);
public sealed record VisualEffectsState(bool BestPerformance, bool Animations, bool Transparency, bool Shadows);
public sealed record AdapterAdvancedProperty(string AdapterName, string DisplayName, string RegistryKeyword, string DisplayValue, string CurrentValue, bool Writable);
public sealed record AffinityValidation(int LogicalProcessors, ulong Mask, bool Valid, string Description);

public sealed class WindowsTuningService
{
    private readonly RegistryService _registry = new();
    private readonly BackupRestoreService _backup = new();
    private readonly LoggingService _log = new();
    private const string GraphicsPath = "SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers";

    public HagsState GetHags()
    {
        if (!OperatingSystem.IsWindows()) return new(false, false, "Unavailable", "Windows only");
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var k = baseKey.OpenSubKey(GraphicsPath);
            var value = k?.GetValue("HwSchMode");
            var text = value?.ToString() ?? "Not set";
            if (value is null) return new(true, false, text, "Windows does not have an explicit override; default policy applies.");
            return new(true, text is "2", text, "2 = enabled, 1 = disabled, absent = Windows default.");
        }
        catch (Exception ex) { return new(false, false, "Unavailable", ex.Message); }
    }

    public async Task<OperationResult> SetHagsAsync(bool enabled, CancellationToken token = default)
    {
        try
        {
            var old = _registry.Read(RegistryHive.LocalMachine, GraphicsPath, "HwSchMode");
            await _backup.SaveAsync(new BackupRecord
            {
                TweakId = "gaming.hags",
                Timestamp = DateTime.UtcNow,
                Hive = "LocalMachine",
                Path = GraphicsPath,
                ValueName = "HwSchMode",
                ValueType = "DWord",
                OldValue = old?.ToString(),
                NewValue = enabled ? "2" : "1",
                ValueExisted = old is not null
            }, token);
            _registry.Write(RegistryHive.LocalMachine, GraphicsPath, "HwSchMode", enabled ? 2 : 1);
            var verify = _registry.Read(RegistryHive.LocalMachine, GraphicsPath, "HwSchMode")?.ToString();
            var ok = verify == (enabled ? "2" : "1");
            await _log.LogAsync(ok ? "Information" : "Error", "Gaming", "HAGS", $"{old} -> {verify}", token);
            return ok ? OperationResult.Ok("HAGS setting applied.", true) : OperationResult.Fail("HAGS verification failed.");
        }
        catch (Exception ex)
        {
            await _log.LogAsync("Error", "Gaming", "HAGS", ex.Message, token);
            return OperationResult.Fail(ex.Message);
        }
    }

    public VisualEffectsState GetVisualEffects()
    {
        try
        {
            using var k = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects");
            var visual = Convert.ToInt32(k?.GetValue("VisualFXSetting", 1));
            using var d = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop\\WindowMetrics");
            using var p = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            var trans = Convert.ToInt32(p?.GetValue("EnableTransparency", 1)) == 1;
            return new(visual == 2, visual != 2, trans, visual != 2);
        }
        catch { return new(false, true, true, true); }
    }

    public async Task<OperationResult> SetBestPerformanceAsync(CancellationToken token = default)
    {
        try
        {
            var path = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects";
            var old = _registry.Read(RegistryHive.CurrentUser, path, "VisualFXSetting");
            await _backup.SaveAsync(new BackupRecord { TweakId = "visual.best-performance", Timestamp = DateTime.UtcNow, Hive = "CurrentUser", Path = path, ValueName = "VisualFXSetting", ValueType = "DWord", OldValue = old?.ToString(), NewValue = "2", ValueExisted = old is not null }, token);
            _registry.Write(RegistryHive.CurrentUser, path, "VisualFXSetting", 2);
            var verify = _registry.Read(RegistryHive.CurrentUser, path, "VisualFXSetting")?.ToString();
            return verify == "2" ? OperationResult.Ok("Visual effects set to Best Performance.", true) : OperationResult.Fail("Visual effects verification failed.");
        }
        catch (Exception ex) { return OperationResult.Fail(ex.Message); }
    }

    public IReadOnlyList<AdapterAdvancedProperty> GetAdapterProperties(string adapterName)
    {
        if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(adapterName)) return [];
        var ps = "Get-NetAdapterAdvancedProperty -Name " + Quote(adapterName) + " | Select-Object Name,DisplayName,RegistryKeyword,DisplayValue,ValidDisplayValues";
        var list = new List<AdapterAdvancedProperty>();
        // JSON avoids brittle whitespace parsing.
        var jsonCmd = "Get-NetAdapterAdvancedProperty -Name " + Quote(adapterName) + " | Select-Object Name,DisplayName,RegistryKeyword,DisplayValue | ConvertTo-Json -Compress";
        var json = new CommandRunner().RunAsync("powershell.exe", new[] { "-NoProfile", "-NonInteractive", "-Command", jsonCmd }, TimeSpan.FromSeconds(20)).GetAwaiter().GetResult();
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json.StdOut);
            IEnumerable<System.Text.Json.JsonElement> arr = doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array ? doc.RootElement.EnumerateArray() : new[] { doc.RootElement };
            foreach (var x in arr)
            {
                list.Add(new AdapterAdvancedProperty(
                    x.TryGetProperty("Name", out var a) ? a.ToString() : adapterName,
                    x.TryGetProperty("DisplayName", out var b) ? b.ToString() : "",
                    x.TryGetProperty("RegistryKeyword", out var c) ? c.ToString() : "",
                    x.TryGetProperty("DisplayValue", out var d) ? d.ToString() : "",
                    x.TryGetProperty("DisplayValue", out var e) ? e.ToString() : "",
                    true));
            }
        }
        catch { }
        return list;
    }

    public async Task<OperationResult> SetAdapterPropertyAsync(string adapterName, string displayName, string displayValue, CancellationToken token = default)
    {
        if (!OperatingSystem.IsWindows()) return OperationResult.Fail("Windows only.");
        if (string.IsNullOrWhiteSpace(adapterName) || string.IsNullOrWhiteSpace(displayName)) return OperationResult.Fail("Adapter and property are required.");
        try
        {
            var escapedAdapter = Quote(adapterName);
            var escapedDisplay = Quote(displayName);
            var escapedValue = Quote(displayValue);
            var script = "$p=Get-NetAdapterAdvancedProperty -Name " + escapedAdapter + " -DisplayName " + escapedDisplay + "; Set-NetAdapterAdvancedProperty -Name " + escapedAdapter + " -DisplayName " + escapedDisplay + " -DisplayValue " + escapedValue + " -NoRestart; Get-NetAdapterAdvancedProperty -Name " + escapedAdapter + " -DisplayName " + escapedDisplay + " | Select-Object -ExpandProperty DisplayValue";
            var r = await new CommandRunner().RunAsync("powershell.exe", new[] { "-NoProfile", "-NonInteractive", "-Command", script }, TimeSpan.FromSeconds(30), token);
            return r.ExitCode == 0 ? OperationResult.Ok($"{displayName} updated on {adapterName}.") : OperationResult.Fail(r.StdErr.Trim().Length > 0 ? r.StdErr.Trim() : "Adapter property update failed.");
        }
        catch (Exception ex) { return OperationResult.Fail(ex.Message); }
    }

    public AffinityValidation ValidateAffinity(IEnumerable<int> selectedProcessors)
    {
        var count = Environment.ProcessorCount;
        ulong mask = 0;
        var invalid = false;
        foreach (var cpu in selectedProcessors)
        {
            if (cpu < 0 || cpu >= count || cpu >= 64) { invalid = true; continue; }
            mask |= 1UL << cpu;
        }
        return new(count, mask, !invalid && mask != 0, invalid ? "One or more processors are outside the detected topology." : $"0x{mask:X16}");
    }

    private static string Quote(string value) => "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";
}
