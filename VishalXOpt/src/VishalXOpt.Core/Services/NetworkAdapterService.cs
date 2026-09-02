using System.Net.NetworkInformation;
namespace VishalXOpt.Core.Services;
public sealed record AdapterInfo(string Name,string Description,OperationalStatus Status, long Speed,string Mac,string Type,string Ips);
public sealed class NetworkAdapterService
{
    public IReadOnlyList<AdapterInfo> GetAdapters(){return NetworkInterface.GetAllNetworkInterfaces().Select(n=>new AdapterInfo(n.Name,n.Description,n.OperationalStatus,n.Speed,n.GetPhysicalAddress().ToString(),n.NetworkInterfaceType.ToString(),string.Join(", ",n.GetIPProperties().UnicastAddresses.Select(x=>x.Address)))).OrderBy(x=>x.Name).ToList();}
}
