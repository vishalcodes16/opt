# Vishal X Opt — Implementation Status

This build moves the project beyond UI placeholders and wires the main modules to Windows-backed services.

## Implemented
- WPF/MVVM navigation and gaming-style 3D dashboard.
- Real Windows system snapshot: memory, CPU sampling, disk usage, Secure Boot, active power plan, adapter latency.
- Power plan listing/activation and Ultimate Performance creation with detected GUID.
- Windows restore-point creation through Windows System Restore API.
- Scan-first cleanup targets with size calculation and safe-user-temp cleanup.
- Real device enumeration through pnputil.
- MSI support detection and registry-backed MSI toggle where the driver exposes the expected property, with backup/verification/logging.
- Data-driven registry tweaks with backup + verification (mouse acceleration, Game Mode, Game DVR, transparency, HAGS definition).
- Real Autorun enumeration from Windows Run keys/startup folder; protected classifications are not automatically disabled.
- Real Scheduled Task inventory and enable/disable without deleting tasks.
- Real Windows Service inventory.
- Real network adapter enumeration and latency tests.
- Windows feature enumeration through DISM with confirmation-gated feature change operations.
- UWP/AppX enumeration and confirmation-gated removal through the PowerShell SDK.
- Running process inventory.
- WinUtil official-source launch and explicit remote-script confirmation path.
- Settings persistence.
- Structured logs and JSON backups under the user LocalAppData/CommonAppData application folders.
- Before/after style system measurements for CPU/RAM/disk/network.

## Hardware/vendor-dependent
- Vendor-specific overclock/undervolt controls require a supported vendor API; the project contains the monitoring/profile surface but does not invent vendor control APIs.
- Device affinity-policy editing uses dynamic topology validation; vendor/device-specific writes still depend on supported Windows policy APIs.
- Advanced/legacy registry definitions are available as explicit settings, but unsupported values are not invented.
- FPS/1% low frame-time capture now supports an optional PresentMon.exe integration and reports unavailable when the telemetry source is absent.

## Build limitation in this environment
The current execution environment does not contain the .NET SDK or Windows runtime, so a Windows-native compile/run verification cannot be performed here. Build and test on Windows with the .NET 8 SDK.
