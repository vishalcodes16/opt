using System.Text.Json;
namespace VishalXOpt.Core.Services;
public sealed class AppSettings
{
    public string Theme {get;set;}="Dark"; public string AccentColor{get;set;}="#35A7FF"; public bool ReduceMotion{get;set;}=false; public bool Disable3D{get;set;}=false; public bool DisableTransparency{get;set;}=false; public bool StartWithWindows{get;set;}=false; public bool CreateRestorePointBeforeOptimization{get;set;}=true; public bool AutomaticBackup{get;set;}=true; public bool ConfirmDangerousOperations{get;set;}=true;
}
public sealed class SettingsService
{
 private readonly string _path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"VishalXOpt","Config","settings.json");
 public AppSettings Load(){try{if(File.Exists(_path))return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path))??new();}catch{}return new();}
 public void Save(AppSettings s){Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(s,new JsonSerializerOptions{WriteIndented=true}));}
}
