using System.ServiceProcess;
namespace VishalXOpt.Core.Services;
public sealed record ServiceInfo(string Name,string DisplayName,string Status,string StartType,string CanStop);
public sealed class ServiceManager
{
    private readonly CommandRunner _runner=new();
    public IReadOnlyList<ServiceInfo> GetServices(){var list=new List<ServiceInfo>();foreach(var s in ServiceController.GetServices().OrderBy(x=>x.DisplayName)){string start="Unknown";try{start=GetStartType(s.ServiceName);}catch{}list.Add(new(s.ServiceName,s.DisplayName,s.Status.ToString(),start,s.CanStop.ToString()));s.Dispose();}return list;}
    public bool SetStartup(string name,string mode){if(!new[]{"auto","demand","disabled"}.Contains(mode,StringComparer.OrdinalIgnoreCase))return false;try{return _runner.RunAsync("sc.exe",new[]{"config",name,$"start={mode}"},TimeSpan.FromSeconds(10)).GetAwaiter().GetResult().ExitCode==0;}catch{return false;}}
    public bool Start(string name){try{return _runner.RunAsync("sc.exe",new[]{"start",name},TimeSpan.FromSeconds(20)).GetAwaiter().GetResult().ExitCode==0;}catch{return false;}}
    public bool Stop(string name){try{return _runner.RunAsync("sc.exe",new[]{"stop",name},TimeSpan.FromSeconds(20)).GetAwaiter().GetResult().ExitCode==0;}catch{return false;}}
    private string GetStartType(string name){var r=_runner.RunAsync("sc.exe",new[]{"qc",name},TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();var m=System.Text.RegularExpressions.Regex.Match(r.StdOut, @"START_TYPE\s*:\s*\d+\s+(\w+)",System.Text.RegularExpressions.RegexOptions.IgnoreCase);return m.Success?m.Groups[1].Value:"Unknown";}
}
