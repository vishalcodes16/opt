# Windows Build Instructions

1. Install Visual Studio 2022 with Desktop development with .NET or the .NET 8 SDK.
2. Open `VishalXOpt.sln`.
3. Restore NuGet packages.
4. Validate the source and optimizer tests before publishing:

```powershell
dotnet test VishalXOpt.sln -c Release
```

5. Build the solution in Release and run `src/VishalXOpt.App` once. Review an optimizer profile before applying it; administrator-only operations are explicitly marked and skipped without elevation.
6. For a standalone x64 executable:

```powershell
dotnet publish src/VishalXOpt.App/VishalXOpt.App.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=false -o publish
```

7. Smoke-test `publish\VishalXOpt.exe` on a supported Windows x64 machine before distributing it. The executable is configured as a single-file, self-contained .NET 8 Windows application.

The environment used to prepare this archive is Linux and does not include the .NET SDK or the Windows runtime, so the Windows build itself must be executed on Windows.
