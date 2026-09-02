using System.Diagnostics;
using System.Globalization;
namespace VishalXOpt.Core.Services;
public sealed record FrameTimeResult(bool Available,string Source,double Fps,double OnePercentLowMs,double AverageFrameTimeMs,string Message);
public sealed class FrameTimeService
{
    public string? FindPresentMon(){
        var candidates=new[]{Path.Combine(AppContext.BaseDirectory,"PresentMon.exe"),Path.Combine(AppContext.BaseDirectory,"tools","PresentMon.exe")};
        foreach(var c in candidates)if(File.Exists(c))return c;
        try{var r=new CommandRunner().RunAsync("where.exe",new[]{"PresentMon.exe"},TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();if(r.ExitCode==0){var p=r.StdOut.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();if(!string.IsNullOrWhiteSpace(p)&&File.Exists(p))return p;}}catch{}
        return null;
    }
    public async Task<FrameTimeResult> CaptureAsync(string processName,int seconds=10,CancellationToken token=default){
        var exe=FindPresentMon();if(exe is null)return new(false,"None",0,0,0,"PresentMon.exe was not found. Put a licensed PresentMon build next to VishalXOpt.exe or in PATH.");
        var csv=Path.Combine(Path.GetTempPath(),$"vxo-{Guid.NewGuid():N}.csv");
        try{
            var psi=new ProcessStartInfo(exe,$"--process_name {processName} --duration {seconds} --output_file \"{csv}\" --no_console" ){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
            using var p=Process.Start(psi);if(p is null)return new(false,"PresentMon",0,0,0,"Unable to start PresentMon.");
            await p.WaitForExitAsync(token); if(!File.Exists(csv))return new(false,"PresentMon",0,0,0,"No PresentMon output was produced.");
            var times=new List<double>();foreach(var line in File.ReadLines(csv).Skip(1)){var cols=line.Split(',');if(cols.Length<3)continue;for(int i=0;i<cols.Length;i++){if(cols[i].Contains("msBetweenPresents",StringComparison.OrdinalIgnoreCase)&&i+1<cols.Length&&double.TryParse(cols[i+1],NumberStyles.Float,CultureInfo.InvariantCulture,out var ms)&&ms>0){times.Add(ms);break;}}}
            if(times.Count==0)return new(false,"PresentMon",0,0,0,"PresentMon produced no usable frame-time samples.");
            times.Sort();var avg=times.Average();var fps=1000.0/avg;var oneCount=Math.Max(1,(int)Math.Ceiling(times.Count*0.01));var oneAvg=times.TakeLast(oneCount).Average();var oneLowFps=1000.0/oneAvg;return new(true,"PresentMon",fps,1000.0/oneLowFps,avg,$"Captured {times.Count} frame-time samples.");
        }catch(OperationCanceledException){return new(false,"PresentMon",0,0,0,"Capture cancelled.");}catch(Exception ex){return new(false,"PresentMon",0,0,0,ex.Message);}finally{try{if(File.Exists(csv))File.Delete(csv);}catch{}}
    }
}
