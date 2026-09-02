using Microsoft.Win32;
using VishalXOpt.Core.Interfaces;
namespace VishalXOpt.Core.Services;
public sealed class RegistryService:IRegistryService
{
    public object? Read(RegistryHive hive,string path,string valueName){using var b=RegistryKey.OpenBaseKey(hive,RegistryView.Registry64);using var k=b.OpenSubKey(path);return k?.GetValue(valueName);}
    public void Write(RegistryHive hive,string path,string valueName,object value){using var b=RegistryKey.OpenBaseKey(hive,RegistryView.Registry64);using var k=b.CreateSubKey(path,true)??throw new InvalidOperationException("Registry path unavailable.");k.SetValue(valueName,value);}
    public void DeleteValue(RegistryHive hive,string path,string valueName){using var b=RegistryKey.OpenBaseKey(hive,RegistryView.Registry64);using var k=b.OpenSubKey(path,true);k?.DeleteValue(valueName,false);}
    public bool ValueExists(RegistryHive hive,string path,string valueName){using var b=RegistryKey.OpenBaseKey(hive,RegistryView.Registry64);using var k=b.OpenSubKey(path);return k?.GetValueNames().Contains(valueName,StringComparer.OrdinalIgnoreCase)==true;}
}
