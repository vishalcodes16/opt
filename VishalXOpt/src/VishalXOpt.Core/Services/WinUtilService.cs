using System.Diagnostics;
namespace VishalXOpt.Core.Services;
public sealed class WinUtilService
{
    public const string Command = "iwr -useb https://christitus.com/win | iex";
    public bool OpenOfficialSource(){try{Process.Start(new ProcessStartInfo("https://christitus.com/win"){UseShellExecute=true});return true;}catch{return false;}}
    public Task<(int,string,string)> RunConfirmedAsync(CancellationToken token=default)=>new CommandRunner().RunAsync("powershell.exe",new[]{"-NoProfile","-ExecutionPolicy","Bypass","-Command",Command},TimeSpan.FromMinutes(15),token).ContinueWith(t=>(t.Result.ExitCode,t.Result.StdOut,t.Result.StdErr),token);
}
