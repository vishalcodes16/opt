using Microsoft.Win32;

namespace VishalXOpt.Core.Services;

public sealed record AutorunEntry(
    string Name,
    string Command,
    string Location,
    bool Enabled,
    string Classification);

public sealed class AutorunsService
{
    private const string DisabledSubKey = "VishalXOptDisabled";

    public IReadOnlyList<AutorunEntry> GetEntries()
    {
        var list = new List<AutorunEntry>();

        var roots = new[]
        {
            (RegistryHive.CurrentUser, "Software\\Microsoft\\Windows\\CurrentVersion\\Run", "HKCU Run", RegistryView.Registry64),
            (RegistryHive.LocalMachine, "Software\\Microsoft\\Windows\\CurrentVersion\\Run", "HKLM Run", RegistryView.Registry64),
            (RegistryHive.LocalMachine, "Software\\Microsoft\\Windows\\CurrentVersion\\Run", "HKLM Run32", RegistryView.Registry32)
        };

        foreach (var (hive, path, label, view) in roots)
        {
            ReadRunKey(list, hive, path, label, view, true);
            ReadRunKey(list, hive, $"{path}\\{DisabledSubKey}", $"{label} (Disabled)", view, false);
        }

        var startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        if (Directory.Exists(startup))
        {
            foreach (var file in Directory.EnumerateFiles(startup))
            {
                list.Add(new AutorunEntry(
                    Path.GetFileNameWithoutExtension(file),
                    file,
                    "Startup Folder",
                    true,
                    "Known Application"));
            }
        }

        return list
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(x => x.Enabled)
            .ToList();
    }

    public bool Disable(AutorunEntry entry)
    {
        if (!entry.Enabled || entry.Location.Contains("Startup Folder", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!TryResolve(entry, out var hive, out var path, out var view))
            return false;

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var runKey = baseKey.OpenSubKey(path, writable: true);
            if (runKey is null)
                return false;

            var value = runKey.GetValue(entry.Name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (value is null)
                return false;

            var valueKind = runKey.GetValueKind(entry.Name);
            runKey.DeleteValue(entry.Name, throwOnMissingValue: false);

            using var disabled = baseKey.CreateSubKey($"{path}\\{DisabledSubKey}", writable: true);
            if (disabled is null)
                return false;

            disabled.SetValue(entry.Name, value, valueKind);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Restore(AutorunEntry entry)
    {
        if (entry.Enabled || !entry.Location.Contains("Disabled", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!TryResolve(entry, out var hive, out var path, out var view))
            return false;

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var disabled = baseKey.OpenSubKey($"{path}\\{DisabledSubKey}", writable: true);
            if (disabled is null)
                return false;

            var value = disabled.GetValue(entry.Name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (value is null)
                return false;

            var valueKind = disabled.GetValueKind(entry.Name);
            using var runKey = baseKey.CreateSubKey(path, writable: true);
            if (runKey is null)
                return false;

            runKey.SetValue(entry.Name, value, valueKind);
            disabled.DeleteValue(entry.Name, throwOnMissingValue: false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void ReadRunKey(
        ICollection<AutorunEntry> list,
        RegistryHive hive,
        string path,
        string label,
        RegistryView view,
        bool enabled)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(path);
            if (key is null)
                return;

            foreach (var name in key.GetValueNames())
            {
                var value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                list.Add(new AutorunEntry(
                    name,
                    value?.ToString() ?? string.Empty,
                    label,
                    enabled,
                    Classify(name)));
            }
        }
        catch
        {
            // Registry access can fail for inaccessible hives/views.
        }
    }

    private static bool TryResolve(
        AutorunEntry entry,
        out RegistryHive hive,
        out string path,
        out RegistryView view)
    {
        hive = entry.Location.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase)
            ? RegistryHive.CurrentUser
            : RegistryHive.LocalMachine;

        view = entry.Location.Contains("32", StringComparison.OrdinalIgnoreCase)
            ? RegistryView.Registry32
            : RegistryView.Registry64;

        path = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        return entry.Location.Contains("Run", StringComparison.OrdinalIgnoreCase);
    }

    private static string Classify(string name)
    {
        if (name.Contains("security", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("defender", StringComparison.OrdinalIgnoreCase))
            return "Security";

        if (name.Contains("microsoft", StringComparison.OrdinalIgnoreCase))
            return "Microsoft";

        if (name.Contains("intel", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("amd", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("nvidia", StringComparison.OrdinalIgnoreCase))
            return "Driver";

        return "Known Application";
    }
}
