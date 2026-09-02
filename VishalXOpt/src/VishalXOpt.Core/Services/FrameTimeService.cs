using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VishalXOpt.Core.Models;

namespace VishalXOpt.Core.Services;

public sealed class FrameTimeService
{
    public string? FindPresentMon() => null;
    public async Task<FrameTimeResult> CaptureAsync(string processName,int seconds=10,CancellationToken token=default){
        if (string.IsNullOrWhiteSpace(processName) || processName.IndexOfAny(['\r', '\n', '"']) >= 0)
            return new(false,"PresentMon",0,0,0,"Enter a valid game process name.");
        seconds = Math.Clamp(seconds, 1, 300);
        var exe=FindPresentMon();if(exe is null)return new(false,"None",0,0,0,"PresentMon.exe was not found. Put a licensed PresentMon build next to VishalXOpt.exe or in PATH.");
        var csv=Path.Combine(Path.GetTempPath(),$"vxo-{Guid.NewGuid():N}.csv");
        try{
            var psi=new ProcessStartInfo(exe){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
            psi.ArgumentList.Add("--process_name");psi.ArgumentList.Add(processName.Trim());psi.ArgumentList.Add("--duration");psi.ArgumentList.Add(seconds.ToString(CultureInfo.InvariantCulture));psi.ArgumentList.Add("--output_file");psi.ArgumentList.Add(csv);psi.ArgumentList.Add("--no_console");
            using var p=Process.Start(psi);if(p is null)return new(false,"PresentMon",0,0,0,"Unable to start PresentMon.");
            await p.WaitForExitAsync(token); if(!File.Exists(csv))return new(false,"PresentMon",0,0,0,"No PresentMon output was produced.");
            return ParseCsv(File.ReadLines(csv));
        }catch(OperationCanceledException){return new(false,"PresentMon",0,0,0,"Capture cancelled.");}catch(Exception ex){return new(false,"PresentMon",0,0,0,ex.Message);}finally{try{if(File.Exists(csv))File.Delete(csv);}catch{}}
    }

    public static FrameTimeResult ParseCsv(IEnumerable<string> lines)
    {
        var rows = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        if (rows.Count < 2) return new(false, "PresentMon", 0, 0, 0, "PresentMon output did not contain frame-time rows.");
        var headers = rows[0].Split(',');
        var frameTimeIndex = Array.FindIndex(headers, header => header.Trim().Equals("MsBetweenPresents", StringComparison.OrdinalIgnoreCase));
        if (frameTimeIndex < 0) return new(false, "PresentMon", 0, 0, 0, "PresentMon output did not contain the MsBetweenPresents column.");
        var times = new List<double>();
        foreach (var row in rows.Skip(1))
        {
            var columns = row.Split(',');
            if (columns.Length <= frameTimeIndex) continue;
            if (double.TryParse(columns[frameTimeIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds) && milliseconds > 0)
                times.Add(milliseconds);
        }
        if (times.Count == 0) return new(false, "PresentMon", 0, 0, 0, "PresentMon produced no usable frame-time samples.");
        times.Sort();
        var average = times.Average();
        var onePercentCount = Math.Max(1, (int)Math.Ceiling(times.Count * 0.01));
        var onePercentLowFrameTime = times.TakeLast(onePercentCount).Average();
        return new(true, "PresentMon", 1000.0 / average, onePercentLowFrameTime, average, $"Captured {times.Count} frame-time samples.");
    }
}
