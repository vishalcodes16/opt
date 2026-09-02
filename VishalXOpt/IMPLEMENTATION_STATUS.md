# Implementation Status

This document tracks module integration status.

## Production ready

These modules are production-ready and fully integrated:

- Power plan detection and activation.
- Tweak service backed by registry operations with backup and restore.
- Hardware snapshot (CPU, RAM, disk, Secure Boot, power plan, network, available sensors).
- Cleanup discovery for temporary and browser cache folders.
- Device manager inspection with categories.
- Autoruns inspection (startup entries).
- Scheduled task inspection.
- Service inspection.
- Component / assembly / application package inspection.
- Process list inspection.
- Network adapter inspection.
- Gaming controls (Game Mode, Game DVR, visual effects, mouse acceleration, HAGS when supported).
- Settings persistence.
- Structured logs and JSON backups under the user LocalAppData/CommonAppData application folders.
- Before/after style system measurements for CPU/RAM/disk/network.
- Optimizer profiles with explicit preview, user confirmation, backup/verification through the tweak service, and privilege-aware skipping of administrator-only operations.

## Hardware/vendor-dependent

- Vendor-specific overclock/undervolt controls require a supported vendor API; the project contains the monitoring/profile surface but does not invent vendor control APIs.
