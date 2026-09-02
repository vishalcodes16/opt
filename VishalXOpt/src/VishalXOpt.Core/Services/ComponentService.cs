using System.Text.RegularExpressions;
namespace VishalXOpt.Core.Services;
public sealed record ComponentInfo(string Name,string State);
public sealed class ComponentService
{
    private readonly CommandRunner _r=new();
    public IReadOnlyList<ComponentInfo> GetFeatures(){var r=_r.RunAsync("dism.exe",new[]{"/Online","/Get-Features","/Format:Table"},TimeSpan.FromMinutes(2)).GetAwaiter().GetResult();var list=new List<ComponentInfo>();foreach(var line in r.StdOut.Split('\n')){var m=Regex.Match(line,@"^\s*([^|]+?)\s+\|\s+([^|\r]+)");if(m.Success)list.Add(new(m.Groups[1].Value.Trim(),m.Groups[2].Value.Trim()));}return list;}
    public bool SetFeature(string name,bool enable){if(string.IsNullOrWhiteSpace(name)||name.Any(c=>char.IsWhiteSpace(c)||c=='/'||c=='"'))return false;var args=enable?new[]{"/Online","/Enable-Feature","/FeatureName:"+name,"/NoRestart"}:new[]{"/Online","/Disable-Feature","/FeatureName:"+name,"/NoRestart"};return _r.RunAsync("dism.exe",args,TimeSpan.FromMinutes(5)).GetAwaiter().GetResult().ExitCode==0;}
}
