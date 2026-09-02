using System.Text.Json;
using VishalXOpt.Core.Interfaces;
namespace VishalXOpt.Core.Services;
public sealed class LoggingService:ILoggingService
{
 private readonly string _dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"VishalXOpt","Logs");
 public async Task LogAsync(string level,string module,string operation,string message,CancellationToken token=default){Directory.CreateDirectory(_dir);var rec=new{timestamp=DateTimeOffset.Now,level,module,operation,message};var path=Path.Combine(_dir,$"{DateTime.Now:yyyy-MM-dd}.jsonl");await File.AppendAllTextAsync(path,JsonSerializer.Serialize(rec)+Environment.NewLine,token);}
 public IReadOnlyList<string> ReadRecent(int max=250){if(!Directory.Exists(_dir))return [];return Directory.EnumerateFiles(_dir,"*.jsonl").OrderByDescending(x=>x).SelectMany(File.ReadLines).Reverse().Take(max).Reverse().ToList();}
}
