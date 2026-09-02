using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using VishalXOpt.Core.Interfaces;
using VishalXOpt.Core.Models;
namespace VishalXOpt.Core.Services;
public sealed class WindowsInfoService : IWindowsInfoService
{
    [DllImport("kernel32.dll")]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
    [DllImport("kernel32.dll")]
    private static extern void GetSystemTimes(out long idle, out long kernel, out long user);
    [StructLayout(LayoutKind.Sequential)] private struct MEMORYSTATUSEX { public uint dwLength; public uint dwMemoryLoad; public ulong ullTotalPhys,ullAvailPhys,ullTotalPageFile,ullAvailPageFile,ullTotalVirtual,ullAvailVirtual,ullAvailExtendedVirtual; }
    private static (long idle,long kernel,long user) CpuTimes(){ GetSystemTimes(out var i,out var k,out var u); return(i,k,u); }
    public SystemSnapshot GetSnapshot()
    {
        var mem = new MEMORYSTATUSEX{dwLength=(uint)Marshal.SizeOf<MEMORYSTATUSEX>()};
        GlobalMemoryStatusEx(ref mem);
        var p = Environment.OSVersion.Version;
        var admin = new AdminService().IsAdministrator();
        var secure = "Unknown";
        try { using var k=Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\SecureBoot\\State"); secure = k?.GetValue("UEFISecureBootEnabled")?.ToString()=="1" ? "Enabled":"Disabled"; } catch { }
        var cpu = SampleCpuUsage();
        var disk = GetDiskUsage();
        var ping = GetPing();
        return new($"Windows {p.Major}",p.Build.ToString(),RuntimeInformation.OSArchitecture.ToString(),Environment.ProcessorCount,mem.ullTotalPhys,SafePowerPlan(),admin,secure,Environment.MachineName,mem.ullAvailPhys,cpu,disk,ping);
    }
    private static double SampleCpuUsage(){ if(!OperatingSystem.IsWindows()) return 0; var a=CpuTimes(); Thread.Sleep(120); var b=CpuTimes(); var idle=b.idle-a.idle; var total=(b.kernel-a.kernel)+(b.user-a.user); return total<=0?0:Math.Clamp(100d*(1d-idle/(double)total),0,100); }
    private static double GetDiskUsage(){ try{var d=new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)!); return d.TotalSize==0?0:100d*(1d-d.AvailableFreeSpace/(double)d.TotalSize);}catch{return 0;} }
    private static double GetPing(){ try{using var p=new Ping(); var r=p.Send("1.1.1.1",1200); return r.Status==IPStatus.Success?r.RoundtripTime:-1;}catch{return -1;} }
    private static string SafePowerPlan(){try{return new PowerCfgService().GetActivePlan();}catch{return "Unknown";}}
}
