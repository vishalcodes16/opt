using System.Diagnostics;
using System.Text.RegularExpressions;
using VishalXOpt.Core.Interfaces;
using VishalXOpt.Core.Models;
namespace VishalXOpt.Core.Services;
public sealed class DeviceService : IDeviceService
{
    public IReadOnlyList<DeviceInfo> GetDevices()
    {
        if (!OperatingSystem.IsWindows()) return [];
        try
        {
            var psi = new ProcessStartInfo("pnputil.exe", "/enum-devices /connected")
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            using var p = Process.Start(psi);
            if (p is null) return [];
            var output = p.StandardOutput.ReadToEnd(); p.WaitForExit(5000);
            var blocks = Regex.Split(output, @"\r?\n\r?\n");
            var result = new List<DeviceInfo>();
            foreach (var block in blocks)
            {
                var name = Match(block, "Device Description");
                if (string.IsNullOrWhiteSpace(name)) continue;
                var instance = Match(block, "Instance ID");
                var manufacturer = Match(block, "Manufacturer");
                var driver = Match(block, "Driver Name");
                result.Add(new DeviceInfo(name, manufacturer, instance, driver, "Not available", null, false, false, "Not available"));
            }
            return result.OrderBy(d => d.Name).ToList();
        }
        catch { return []; }
    }
    static string Match(string block, string key)
    {
        var m = Regex.Match(block, @"^\s*" + Regex.Escape(key) + @"\s*:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value.Trim() : "Not available";
    }
}
