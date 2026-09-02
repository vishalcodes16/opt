using Microsoft.Win32;
using VishalXOpt.Core.Models;
namespace VishalXOpt.Core.Services;
public sealed record TweakState(TweakDefinition Definition,string CurrentValue,string RecommendedValue,bool Supported);
public sealed class TweakService
{
    private readonly RegistryService _registry = new();
    private readonly BackupRestoreService _backup = new();
    private readonly LoggingService _log = new();
    public IReadOnlyList<TweakDefinition> Definitions { get; } = new List<TweakDefinition>
    {
        new(){Id="input.mouse-acceleration",Name="Mouse acceleration",Category="Basic",Description="Disable Windows pointer acceleration for a more consistent raw-input style pointer feel.",Risk="MODERATE",RegistryHive="CurrentUser",RegistryPath="Control Panel\\Mouse",ValueName="MouseSpeed",EnabledValue=1,DisabledValue=0,RecommendedValue=0},
        new(){Id="gaming.game-mode",Name="Game Mode",Category="Gaming",Description="Enable Windows automatic Game Mode behavior where supported.",Risk="SAFE",RegistryHive="CurrentUser",RegistryPath="Software\\Microsoft\\GameBar",ValueName="AllowAutoGameMode",EnabledValue=1,DisabledValue=0,RecommendedValue=1},
        new(){Id="gaming.game-dvr",Name="Background Game Recording",Category="Gaming",Description="Disable background Game DVR capture to reduce unnecessary recording workload.",Risk="MODERATE",RegistryHive="CurrentUser",RegistryPath="Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR",ValueName="AppCaptureEnabled",EnabledValue=1,DisabledValue=0,RecommendedValue=0},
        new(){Id="gaming.hags",Name="Hardware-accelerated GPU scheduling",Category="Gaming",Description="Request HAGS enablement when the current Windows build and GPU driver support it.",Risk="ADVANCED",RequiresAdmin=true,RequiresRestart=true,RegistryHive="LocalMachine",RegistryPath="SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers",ValueName="HwSchMode",EnabledValue=2,DisabledValue=1,RecommendedValue=2},
        new(){Id="custom.transparency",Name="Transparency effects",Category="Customization",Description="Disable visual transparency for a simpler desktop rendering path.",Risk="SAFE",RegistryHive="CurrentUser",RegistryPath="Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",ValueName="EnableTransparency",EnabledValue=1,DisabledValue=0,RecommendedValue=0}
    };
    public IReadOnlyList<TweakState> Detect(){return Definitions.Select(d=>{try{if(!Enum.TryParse<RegistryHive>(d.RegistryHive,out var h))return new TweakState(d,"Unknown",Val(d.RecommendedValue),false);var v=_registry.Read(h,d.RegistryPath!,d.ValueName!);return new TweakState(d,v?.ToString()??"(not set)",Val(d.RecommendedValue),true);}catch{return new TweakState(d,"Unavailable",Val(d.RecommendedValue),false);}}).ToList();}
    public async Task<OperationResult> ApplyAsync(TweakDefinition d,CancellationToken token=default){if(d.RegistryPath is null||d.ValueName is null||d.RegistryHive is null)return OperationResult.Fail("This tweak has no concrete setting mapping.");if(!Enum.TryParse<RegistryHive>(d.RegistryHive,out var hive))return OperationResult.Fail("Unsupported registry hive.");try{var old=_registry.Read(hive,d.RegistryPath,d.ValueName);var existed=_registry.ValueExists(hive,d.RegistryPath,d.ValueName);await _backup.SaveAsync(new BackupRecord{TweakId=d.Id,Timestamp=DateTime.UtcNow,Hive=d.RegistryHive,Path=d.RegistryPath,ValueName=d.ValueName,ValueType=TypeName(old),OldValue=old?.ToString(),NewValue=Val(d.RecommendedValue),ValueExisted=existed},token);object nv=d.RecommendedValue??throw new InvalidOperationException("Missing recommended value.");_registry.Write(hive,d.RegistryPath,d.ValueName,nv);var verify=_registry.Read(hive,d.RegistryPath,d.ValueName);var ok=Val(verify)==Val(nv);await _log.LogAsync(ok?"Information":"Error",d.Category,"Apply Tweak",$"{d.Name}: {Val(old)} -> {Val(verify)}",token);return ok?OperationResult.Ok($"{d.Name} applied.",d.RequiresRestart):OperationResult.Fail($"{d.Name} verification failed.");}catch(Exception ex){await _log.LogAsync("Error",d.Category,"Apply Tweak",ex.Message,token);return OperationResult.Fail(ex.Message);}}
    private static string Val(object? x)=>x?.ToString()??"(null)";
    private static string TypeName(object? x)=>x switch{int=>"DWord",uint=>"DWord",long=>"QWord",ulong=>"QWord",byte[]=>"Binary",_=>"String"};
}
