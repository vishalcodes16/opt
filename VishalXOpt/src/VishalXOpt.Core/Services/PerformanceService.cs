using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
namespace VishalXOpt.Core.Services;
public sealed record PerformanceSnapshot(DateTimeOffset Timestamp,double CpuPercent,double RamPercent,double DiskPercent,double PingMs,double StartupSeconds);
public sealed class PerformanceService
{
 [DllImport("kernel32.dll")] static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX x);
 [StructLayout(LayoutKind.Sequential)] struct MEMORYSTATUSEX{public uint dwLength;public uint dwMemoryLoad;public ulong ullTotalPhys,ullAvailPhys,ullTotalPageFile,ullAvailPageFile,ullTotalVirtual,ullAvailVirtual,ullAvailExtendedVirtual;}
 public PerformanceSnapshot Capture(){var mem=GetMem();var cpu=GetCpu();var disk=GetDisk();var ping=GetPing();return new(DateTimeOffset.Now,cpu,mem,disk,ping,-1);}
 public IReadOnlyList<PerformanceSnapshot> Sample(int count=5,int intervalMs=500){var list=new List<PerformanceSnapshot>();for(int i=0;i<count;i++){list.Add(Capture());if(i<count-1)Thread.Sleep(intervalMs);}return list;}
 private static double GetCpu(){if(!OperatingSystem.IsWindows())return 0;GetSystemTimes(out var i1,out var k1,out var u1);Thread.Sleep(120);GetSystemTimes(out var i2,out var k2,out var u2);var idle=i2-i1;var total=(k2-k1)+(u2-u1);return total<=0?0:Math.Clamp(100d*(1-idle/(double)total),0,100);}
 [DllImport("kernel32.dll")]static extern void GetSystemTimes(out long idle,out long kernel,out long user);
 private static double GetMem(){try{var m=new MEMORYSTATUSEX{dwLength=(uint)Marshal.SizeOf<MEMORYSTATUSEX>()};return GlobalMemoryStatusEx(ref m)?m.dwMemoryLoad:0;}catch{return 0;}}
 private static double GetDisk(){try{var d=new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)!);return d.TotalSize==0?0:100d*(1-d.AvailableFreeSpace/(double)d.TotalSize);}catch{return 0;}}
 private static double GetPing(){try{using var p=new Ping();var r=p.Send("1.1.1.1",1000);return r.Status==IPStatus.Success?r.RoundtripTime:-1;}catch{return -1;}}
}
