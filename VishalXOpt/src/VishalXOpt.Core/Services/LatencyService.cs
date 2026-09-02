using System.Net.NetworkInformation;
namespace VishalXOpt.Core.Services;
public sealed record PingResult(string Host,bool Success,double Ms,string Message);
public sealed class LatencyService
{
    public IReadOnlyList<PingResult> Test(params string[] hosts){var l=new List<PingResult>();using var p=new Ping();foreach(var h in hosts){try{var r=p.Send(h,1500);l.Add(new(h,r.Status==IPStatus.Success,r.Status==IPStatus.Success?r.RoundtripTime:-1,r.Status.ToString()));}catch(Exception ex){l.Add(new(h,false,-1,ex.Message));}}return l;}
}
