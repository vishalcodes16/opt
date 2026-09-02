using LibreHardwareMonitor.Hardware;
using VishalXOpt.Core.Interfaces;
namespace VishalXOpt.Core.Services;
public sealed class HardwareMonitorService:IHardwareMonitorService, IDisposable
{
    private readonly Computer _computer;
    public HardwareMonitorService(){_computer=new Computer{IsCpuEnabled=true,IsGpuEnabled=true,IsMemoryEnabled=true,IsMotherboardEnabled=true,IsControllerEnabled=true,IsBatteryEnabled=true};try{_computer.Open();}catch{}}
    public IReadOnlyList<(string Name,float Value,string Unit)> GetSensors(){var list=new List<(string,float,string)>();foreach(var hw in _computer.Hardware){try{hw.Update();foreach(var s in hw.Sensors)if(s.Value is float v)list.Add(($"{hw.Name} / {s.Name}",v,s.SensorType.ToString()));}catch{}}return list;}
    public void Dispose(){try{_computer.Close();}catch{}}
}
