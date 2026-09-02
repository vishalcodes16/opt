# Build on Windows

1. Install Visual Studio 2022 with Desktop development with .NET or the .NET 8 SDK.
2. Open `VishalXOpt.sln`.
3. Restore NuGet packages.
4. Build the solution in Release.
5. Run `src/VishalXOpt.App`.
6. For a standalone x64 executable:

```powershell
dotnet publish src/VishalXOpt.App/VishalXOpt.App.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=false -o publish
```

The environment used to prepare this archive is Linux and does not include the .NET SDK or the Windows runtime, so the Windows build itself must be executed on Windows.
