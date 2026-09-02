using System.Management;

namespace VishalXOpt.Core.Services;

public sealed class RestorePointService
{
    public async Task<bool> TryCreateAsync(string description, CancellationToken token = default)
    {
        if (!OperatingSystem.IsWindows())
            return false;

        token.ThrowIfCancellationRequested();

        return await Task.Run(() =>
        {
            try
            {
                using var restoreClass = new ManagementClass(@"\\.\root\default:SystemRestore");
                using var parameters = restoreClass.GetMethodParameters("CreateRestorePoint");

                parameters["Description"] = string.IsNullOrWhiteSpace(description)
                    ? "Vishal X Opt Restore Point"
                    : description;
                parameters["RestorePointType"] = 12u;
                parameters["EventType"] = 100u;

                using var result = restoreClass.InvokeMethod(
                    "CreateRestorePoint",
                    parameters,
                    new InvokeMethodOptions());

                return result is not null && Convert.ToUInt32(result["ReturnValue"]) == 0;
            }
            catch
            {
                return false;
            }
        }, token).ConfigureAwait(false);
    }
}
