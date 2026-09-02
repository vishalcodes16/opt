using System.Diagnostics;
namespace VishalXOpt.Core.Services;
public sealed record ProcessInfo(int Id,string Name,double CpuPercent,long WorkingSetMb,string Path);
public sealed class ProcessService
{
    public IReadOnlyList<ProcessInfo> GetProcesses(){var list=new List<ProcessInfo>(); foreach(var p in Process.GetProcesses().OrderBy(x=>x.ProcessName)){try{var path="";try{path=p.MainModule?.FileName??"";}catch{} list.Add(new(p.Id,p.ProcessName,0,p.WorkingSet64/1024/1024,path));}catch{} p.Dispose();} return list.OrderByDescending(x=>x.WorkingSetMb).Take(250).ToList();}
}
