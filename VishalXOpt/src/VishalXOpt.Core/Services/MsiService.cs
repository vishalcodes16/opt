using Microsoft.Win32;
using VishalXOpt.Core.Models;
namespace VishalXOpt.Core.Services;
public sealed class MsiService
{
    private readonly BackupRestoreService _backup=new(); private readonly LoggingService _log=new();
    public DeviceInfo Enrich(DeviceInfo d){var key=FindDeviceKey(d.InstanceId);if(key==null)return d with{MsiSupported=false,MsiEnabled=false,AffinityMask="Not available"};try{using var p=key.OpenSubKey("Device Parameters\\Interrupt Management\\MessageSignaledInterruptProperties");var supported=p?.GetValue("MSISupported");var limit=p?.GetValue("MessageNumberLimit");return d with{MsiSupported=true,MsiEnabled=ToInt(supported)==1,Irq=d.Irq,AffinityMask=limit?.ToString()??"Not available"};}catch{return d;}}
    public bool SetMsi(DeviceInfo d,bool enabled){var key=FindDeviceKey(d.InstanceId);if(key==null)return false;try{var path=key.Name[(key.Name.IndexOf('\\')+1)..]+"\\Device Parameters\\Interrupt Management\\MessageSignaledInterruptProperties";var hive=RegistryHive.LocalMachine;var old=new RegistryService().Read(hive,path,"MSISupported");_backup.SaveAsync(new BackupRecord{TweakId="msi."+d.InstanceId,Timestamp=DateTime.UtcNow,Hive="LocalMachine",Path=path,ValueName="MSISupported",ValueType="DWord",OldValue=old?.ToString(),NewValue=enabled?"1":"0",ValueExisted=old!=null}).GetAwaiter().GetResult();new RegistryService().Write(hive,path,"MSISupported",enabled?1:0);var verify=new RegistryService().Read(hive,path,"MSISupported");var ok=ToInt(verify)==(enabled?1:0);_log.LogAsync(ok?"Information":"Error","MSI Utility","Set MSI",$"{d.Name}: {old} -> {verify}").GetAwaiter().GetResult();return ok;}catch{return false;}}
    private static int ToInt(object? o)=>o switch{int i=>i,long l=>(int)l,uint u=>(int)u,_=>int.TryParse(o?.ToString(),out var x)?x:0};
    private static RegistryKey? FindDeviceKey(string instanceId){try{using var baseKey=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry64);var path="SYSTEM\\CurrentControlSet\\Enum\\"+instanceId;return baseKey.OpenSubKey(path,true);}catch{return null;}}
}
