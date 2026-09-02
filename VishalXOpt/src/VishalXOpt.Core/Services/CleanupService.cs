using VishalXOpt.Core.Models;
namespace VishalXOpt.Core.Services;
public sealed class CleanupService
{
    public IReadOnlyList<CleanupItem> Scan()
    {
        var temp=Environment.GetEnvironmentVariable("TEMP");
        var win=Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var local=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var candidates=new[]{
            new CleanupItem("User Temp",temp??"",0,true,Category:"Windows"),
            new CleanupItem("Windows Temp",Path.Combine(win,"Temp"),0,true,Category:"Windows"),
            new CleanupItem("Thumbnail Cache",Path.Combine(local,"Microsoft","Windows","Explorer"),0,true,Category:"Windows"),
            new CleanupItem("Chrome Cache",Path.Combine(local,"Google","Chrome","User Data"),0,false,Category:"Browser"),
            new CleanupItem("Edge Cache",Path.Combine(local,"Microsoft","Edge","User Data"),0,false,Category:"Browser"),
            new CleanupItem("Firefox Profile Cache",Path.Combine(local,"Mozilla","Firefox","Profiles"),0,false,Category:"Browser")};
        return candidates.Where(x=>Directory.Exists(x.Path)).Select(x=>x with {SizeBytes=CalculateSize(x.Path)}).ToList();
    }
    public async Task<long> DeleteAsync(IEnumerable<CleanupItem> items,CancellationToken token=default){long freed=0; foreach(var item in items.Where(x=>x.Selected&&x.Safe)){token.ThrowIfCancellationRequested(); freed += await DeleteContentsAsync(item.Path,token);} return freed;}
    private static async Task<long> DeleteContentsAsync(string path,CancellationToken token){long total=0; IEnumerable<string> files; try{files=Directory.EnumerateFiles(path,"*",SearchOption.AllDirectories);}catch{return 0;} foreach(var f in files.ToList()){token.ThrowIfCancellationRequested(); try{var len=new FileInfo(f).Length; File.SetAttributes(f,FileAttributes.Normal); File.Delete(f); total+=len;}catch{} } await Task.CompletedTask; return total;}
    private static long CalculateSize(string path){long total=0;try{foreach(var f in Directory.EnumerateFiles(path,"*",SearchOption.AllDirectories)){try{total+=new FileInfo(f).Length;}catch{}}}catch{}return total;}
}
