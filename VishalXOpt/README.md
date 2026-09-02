# Vishal X Opt

Vishal X Opt is a Windows desktop utility for inspecting system state and applying **explicit, user-confirmed** optimization actions. It is a .NET 8 WPF application designed around a safety-first workflow:

**Detect → Backup → Preview → Confirm → Apply → Verify → Measure → Undo**

> **Important:** This repository contains source code, not a prebuilt executable. Build and test it on a supported Windows x64 machine before using it on a primary system.

## Features

- Windows system snapshot: CPU, memory, disk, Secure Boot, power plan, network latency, and available hardware sensor readings.
- Power-plan inspection and activation, including an Ultimate Performance creation flow.
- Safe cleanup discovery for user temporary files and browser cache locations.
- Device, startup entry, scheduled task, service, component, process, and network-adapter inspection.
- Registry-backed tweaks with backup, write verification, structured logging, and restore support.
- Gaming controls for Game Mode, Game DVR, HAGS (when supported), visual effects, mouse acceleration, and optional PresentMon frame-time capture.
- Optimizer profiles with an itemized preview before any settings are changed.

## Optimizer profiles

The Optimizer page does not make changes until the user selects **Apply** and confirms the displayed plan. Every profile shows its current and recommended values before confirmation.

| Profile | Planned settings | Notes |
| --- | --- | --- |
| **Default** | None | A no-op inspection profile; it does not modify the system. |
| **Optimal** | Mouse acceleration, Game Mode, Game DVR, transparency effects | Conservative, user-level settings. |
| **Maximum** | Optimal settings plus HAGS | HAGS is applied only when supported and running as administrator. |
| **Gaming / FPS Maximum** | Mouse acceleration, Game Mode, Game DVR, HAGS | Does not claim or guarantee an FPS increase. |

Successful registry changes are backed up and verified. Unsupported operations and administrator-only actions from a standard-user session are reported as skipped rather than silently attempted. Use **Restore** to replay recorded registry backups when appropriate.

## Safety and limitations

- Review the preview carefully and create a Windows restore point before applying changes you may want to reverse.
- Hardware, driver, Windows edition, and policy settings determine what can be detected or changed. An unavailable setting is not fabricated.
- Some changes require administrator privileges, a sign-out, or a restart. HAGS and other hardware-dependent settings may have no measurable benefit on a particular system.
- The optional FPS and 1% low measurement feature requires a licensed `PresentMon.exe` next to `VishalXOpt.exe`, in `tools`, or on `PATH`.
- The optional WinUtil action downloads and runs a remote PowerShell script only after an explicit confirmation. Inspect the shown source before approving it.

## Build and test on Windows

### Prerequisites

- Windows x64
- .NET 8 SDK, or Visual Studio 2022 with **Desktop development with .NET**

### Validate the source

From the `VishalXOpt` directory:

```powershell
dotnet restore VishalXOpt.sln
dotnet test VishalXOpt.sln -c Release
dotnet build VishalXOpt.sln -c Release --no-restore
```

### Publish a standalone executable

```powershell
dotnet publish src/VishalXOpt.App/VishalXOpt.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:PublishTrimmed=false `
  -o publish
```

Run `publish\VishalXOpt.exe` and smoke-test system detection, profile preview, an approved low-risk action, and restore behavior before distribution. For the detailed Windows build checklist, see [BUILD_WINDOWS.md](BUILD_WINDOWS.md).

## Project layout

- `src/VishalXOpt.App` — WPF executable host and dynamically constructed main views.
- `src/VishalXOpt.UI` — view models, UI services, controls, and reusable views.
- `src/VishalXOpt.Core` — Windows service integrations, backup/restore, logging, models, and safety helpers.
- `src/VishalXOpt.Modules.Optimizer` — profile definitions and preview/apply orchestration.
- `tests` — xUnit test projects.
- `docs` — additional project and security notes.

## License and security

Read [SECURITY.md](SECURITY.md) before reporting security-sensitive issues. This software changes Windows settings only after user action; evaluate changes in a test environment and use it at your own risk.