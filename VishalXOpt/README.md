# Vishal X Opt Build Fix 2

Replace these two files in the GitHub repository:

- `src/VishalXOpt.Core/Services/RestorePointService.cs`
- `src/VishalXOpt.Core/Services/AutorunsService.cs`

Then commit and rerun GitHub Actions.

The restore-point code uses the supported `ManagementClass.GetMethodParameters(string)` and `InvokeMethod(string, ManagementBaseObject, InvokeMethodOptions)` APIs. The autorun code guards the nullable registry value before calling `SetValue`.
