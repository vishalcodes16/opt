using System.Management.Automation;
namespace VishalXOpt.Core.Services;
public sealed record AppPackageInfo(string Name,string FullName,string Publisher,string Version);
public sealed class DebloatService
{
    public IReadOnlyList<AppPackageInfo> GetPackages(){if(!OperatingSystem.IsWindows())return [];try{using var ps=PowerShell.Create();ps.AddScript("Get-AppxPackage | Select-Object Name,PackageFullName,Publisher,Version");var r=ps.Invoke();return r.Select(x=>new AppPackageInfo(x.Properties["Name"]?.Value?.ToString()??"",x.Properties["PackageFullName"]?.Value?.ToString()??"",x.Properties["Publisher"]?.Value?.ToString()??"",x.Properties["Version"]?.Value?.ToString()??"" )).OrderBy(x=>x.Name).ToList();}catch{return [];}}
    public bool Remove(string fullName){if(string.IsNullOrWhiteSpace(fullName)||fullName.Any(c=>char.IsWhiteSpace(c)||c is '\'' or '"' or ';' or '|'))return false;try{using var ps=PowerShell.Create();ps.AddCommand("Remove-AppxPackage").AddParameter("Package",fullName);ps.Invoke();return ps.HadErrors==false;}catch{return false;}}
}
