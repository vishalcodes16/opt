namespace VishalXOpt.Core.Models;
public sealed record CleanupItem(string Name, string Path, long SizeBytes, bool Safe, bool Selected = true, string Category = "Windows");
