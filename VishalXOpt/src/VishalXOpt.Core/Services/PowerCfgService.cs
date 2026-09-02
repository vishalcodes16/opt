using System.Text.RegularExpressions;
using VishalXOpt.Core.Interfaces;
namespace VishalXOpt.Core.Services;
public sealed class PowerCfgService : IPowerPlanService
{
    private readonly CommandRunner _runner = new();
    public string GetActivePlan(){ var r=_runner.RunAsync("powercfg.exe",new[]{"/getactivescheme"},TimeSpan.FromSeconds(5)).GetAwaiter().GetResult(); var m=Regex.Match(r.StdOut,@"\(([^)]+)\)"); return m.Success?m.Groups[1].Value.Trim():r.StdOut.Trim(); }
    public IReadOnlyList<(string Guid,string Name,bool Active)> GetPlans(){ var r=_runner.RunAsync("powercfg.exe",new[]{"/list"},TimeSpan.FromSeconds(5)).GetAwaiter().GetResult(); var list=new List<(string,string,bool)>(); foreach(Match m in Regex.Matches(r.StdOut,@"GUID:\s*([a-f0-9-]+)\s+\(([^)]+)\)(\s*\*)?",RegexOptions.IgnoreCase)) list.Add((m.Groups[1].Value,m.Groups[2].Value.Trim(),m.Groups[3].Success)); return list; }
    public async Task<bool> SetPlanAsync(string plan,CancellationToken token=default){ var match=GetPlans().FirstOrDefault(x=>string.Equals(x.Name,plan,StringComparison.OrdinalIgnoreCase)); if(string.IsNullOrWhiteSpace(match.Guid)) return false; var r=await _runner.RunAsync("powercfg.exe",new[]{"/setactive",match.Guid},TimeSpan.FromSeconds(10),token); return r.ExitCode==0; }
    public async Task<bool> EnsureUltimatePerformanceAsync(CancellationToken token=default){ var existing=GetPlans().FirstOrDefault(x=>x.Name.Contains("Ultimate Performance",StringComparison.OrdinalIgnoreCase)); if(!string.IsNullOrWhiteSpace(existing.Guid)) return await SetPlanAsync(existing.Name,token); var r=await _runner.RunAsync("powercfg.exe",new[]{"-duplicatescheme","e9a42b02-d5df-448d-aa00-03f14749eb61"},TimeSpan.FromSeconds(10),token); if(r.ExitCode!=0) return false; var guid=Regex.Match(r.StdOut,@"([a-f0-9-]{36})",RegexOptions.IgnoreCase).Groups[1].Value; if(string.IsNullOrWhiteSpace(guid)) return false; var rename=await _runner.RunAsync("powercfg.exe",new[]{"/changename",guid,"Vishal X Opt - Ultimate Performance"},TimeSpan.FromSeconds(10),token); return rename.ExitCode==0 && (await _runner.RunAsync("powercfg.exe",new[]{"/setactive",guid},TimeSpan.FromSeconds(10),token)).ExitCode==0; }
}
