using System.Text.RegularExpressions;
namespace VishalXOpt.Core.Services;
public sealed record TaskInfo(string Name,string Status,string Author,string TaskToRun);
public sealed class ScheduledTaskService
{
    public IReadOnlyList<TaskInfo> GetTasks(){var r=new CommandRunner().RunAsync("schtasks.exe",new[]{"/query","/fo","LIST","/v"},TimeSpan.FromSeconds(20)).GetAwaiter().GetResult(); var list=new List<TaskInfo>(); foreach(var block in Regex.Split(r.StdOut,"(?:\r?\n){2,}")){var name=Get(block,"TaskName:"); if(string.IsNullOrWhiteSpace(name))continue; list.Add(new(name,Get(block,"Status:"),Get(block,"Author:"),Get(block,"Task To Run:")));} return list.OrderBy(x=>x.Name).ToList();}
    public bool SetEnabled(string name,bool enabled){var args=new List<string>{"/change","/tn",name,enabled?"/enable":"/disable"}; var r=new CommandRunner().RunAsync("schtasks.exe",args,TimeSpan.FromSeconds(10)).GetAwaiter().GetResult(); return r.ExitCode==0;}
    private static string Get(string b,string key){var m=Regex.Match(b,"^\\s*"+Regex.Escape(key)+"\\s*(.*)$",RegexOptions.Multiline|RegexOptions.IgnoreCase);return m.Success?m.Groups[1].Value.Trim():"";}
}
