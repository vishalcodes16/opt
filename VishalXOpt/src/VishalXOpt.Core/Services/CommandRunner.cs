using System.Diagnostics;
namespace VishalXOpt.Core.Services;
public sealed class CommandRunner
{
    public async Task<(int ExitCode,string StdOut,string StdErr)> RunAsync(string exe, IEnumerable<string> args, TimeSpan timeout, CancellationToken token=default)
    {
        var psi = new ProcessStartInfo(exe) { UseShellExecute=false, CreateNoWindow=true, RedirectStandardOutput=true, RedirectStandardError=true };
        foreach (var arg in args) psi.ArgumentList.Add(arg);
        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Unable to start {exe}.");
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token);
        linked.CancelAfter(timeout);
        var stdout = p.StandardOutput.ReadToEndAsync(linked.Token);
        var stderr = p.StandardError.ReadToEndAsync(linked.Token);
        await p.WaitForExitAsync(linked.Token);
        return (p.ExitCode, await stdout, await stderr);
    }
}
