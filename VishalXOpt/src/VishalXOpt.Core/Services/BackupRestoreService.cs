using System.Text.Json;
using VishalXOpt.Core.Interfaces;
using VishalXOpt.Core.Models;
namespace VishalXOpt.Core.Services;
public sealed class BackupRestoreService:IBackupRestoreService
{
    private readonly string _dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"VishalXOpt","Backups");
    public async Task SaveAsync(BackupRecord record,CancellationToken token=default){Directory.CreateDirectory(_dir); var path=Path.Combine(_dir,$"{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-{Sanitize(record.TweakId)}.json"); await File.WriteAllTextAsync(path,JsonSerializer.Serialize(record,new JsonSerializerOptions{WriteIndented=true}),token);}
    public async Task<IReadOnlyList<BackupRecord>> GetAsync(CancellationToken token=default){if(!Directory.Exists(_dir))return [];var list=new List<BackupRecord>();foreach(var f in Directory.EnumerateFiles(_dir,"*.json")){try{var x=JsonSerializer.Deserialize<BackupRecord>(await File.ReadAllTextAsync(f,token));if(x!=null)list.Add(x);}catch{}}return list.OrderByDescending(x=>x.Timestamp).ToList();}
    public async Task<(int success,int failed)> RestoreAllAsync(CancellationToken token=default){var all=await GetAsync(token);var reg=new RegistryService();int ok=0,bad=0;foreach(var r in all){try{if(!Enum.TryParse<Microsoft.Win32.RegistryHive>(r.Hive,out var hive)){bad++;continue;}if(!r.ValueExisted){reg.DeleteValue(hive,r.Path,r.ValueName);}else{reg.Write(hive,r.Path,r.ValueName,ConvertValue(r.OldValue,r.ValueType));}ok++;}catch{bad++;}}return(ok,bad);}
    private static object ConvertValue(string? v,string t)=>t switch{"DWord"=>uint.TryParse(v,out var n)?n:v??"","QWord"=>ulong.TryParse(v,out var q)?q:v??"",_=>v??""};
    private static string Sanitize(string s)=>new string(s.Where(c=>char.IsLetterOrDigit(c)||c=='-'||c=='_').ToArray());
}
