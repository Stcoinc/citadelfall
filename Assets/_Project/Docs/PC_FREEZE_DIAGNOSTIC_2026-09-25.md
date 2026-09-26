# Windows Freeze Diagnostic — 2026-09-25

## Incident summary

- The PC froze and required a hard reset on 2026-09-25.
- Windows booted again at `22:47:31` America/Bogota.
- Windows recorded Kernel-Power event 41 at `22:47:42` and EventLog event 6008 at `22:47:53`, confirming an unclean shutdown.
- Kernel-Power reported `BugcheckCode=0`, `WHEABootErrorCount=0`, and no long-power-button flag. No new minidump, memory dump, or LiveKernelReport was created for this incident.
- Therefore Windows did not capture a blue-screen cause. A hard lock, power interruption, firmware/CPU instability, or reset can all produce this evidence.

## Evidence checked

- Windows System and Application event logs around the incident and for the prior seven days.
- Kernel-Power event 41 raw fields and recent unexpected-shutdown history.
- Windows Error Reporting archive, `C:\Windows\Minidump`, `C:\Windows\MEMORY.DMP`, and `C:\Windows\LiveKernelReports`.
- WHEA hardware-error, NVIDIA/display-reset, disk, NTFS, storage-controller, and resource-exhaustion events.
- Physical-disk health, current RAM/pagefile state, GPU temperature and VRAM, motherboard/BIOS, CPU, and memory-module configuration.
- Official Intel and Gigabyte support information current on 2026-09-25.

## Findings

1. **No direct crash signature was captured.** There was no bugcheck, new dump, WHEA hardware error, NVIDIA display reset, disk/controller error, NTFS error, or resource-exhaustion event associated with the freeze.
2. **The strongest actionable risk is the old motherboard firmware.** The machine uses a Gigabyte `Z790 D DDR4`, Intel `i7-13700K`, and BIOS `F9` dated 2023-12-14 (Windows date display). Gigabyte's later BIOS releases include:
   - F11: Intel Default Settings and microcode `0x12B` for 13th/14th-generation instability.
   - F12: specifically addresses random Windows shutdown while idling.
   - F13: microcode `0x12F` for further stability improvement.
   - F16: latest stable release shown on the support page at inspection time, dated 2026-06-30. F17a is a beta-suffixed release and is not the first choice for recovery.
3. Intel recommends Intel Default Settings and the latest BIOS containing microcode `0x12F` or later for affected 13th/14th-generation desktop processors.
4. This is not the first unclean shutdown recorded: Windows has twelve additional bugcheck-zero Kernel-Power 41 events between 2026-06-14 and 2026-09-15. These entries do not prove a hardware defect because manual resets and power loss look the same, but they make the incident a recurring pattern rather than a single isolated log entry.
5. `Windows.Media.Capture.AppCaptureManager` / `BcastDVRUserService` timed out repeatedly before the reset. The same error occurs 24–62 times per day across most of the prior two weeks, so it is chronic background noise rather than persuasive evidence of this freeze's cause. Game DVR and audio capture are enabled.
6. Both SSDs report `Healthy`; NTFS checked the mounted volumes as healthy after boot. Current NVIDIA state was normal (`39 C`, about `22 W`) and the RTX 4060 Ti driver was `591.86`.
7. The system has 64 GB (2 x 32 GB Corsair DDR4-3200) running at 3200 MT/s and 1.2 V. No memory-diagnostic result or recent resource-exhaustion event was present.

## Recommended action order

1. **Back up important work first.** A BIOS update is a firmware operation and should not be interrupted.
2. **Physically confirm the motherboard revision printed on the board** (for example `REV: 1.0`) before downloading firmware. Windows reports the product name but returns the revision as `x.x`.
3. From the official Gigabyte support page for the confirmed revision, update from F9 to the latest stable BIOS (F16 was the stable release visible during this investigation). Do not use firmware for `Z790 D`, `Z790 D WIFI`, or another revision/model.
4. After flashing, load Optimized Defaults / Intel Default Settings. Re-enable only settings that are actually required. Confirm Windows boots normally before restoring any custom tuning.
5. If another freeze occurs after the firmware update, temporarily disable Windows **Settings > Gaming > Captures > Record what happened** (and Game Bar background capture if unused), then test again. This targets the repeated capture-service timeouts but is secondary to firmware.
6. If freezes continue, run Windows Memory Diagnostic or preferably an extended bootable memory test, then test the PC at BIOS defaults with any CPU undervolt/overclock and XMP disabled. Record the exact time and whether audio continued, the mouse moved, or the display alone stopped; that distinguishes display hangs from full system locks.
7. If a future incident creates a dump or WHEA/display event, preserve it before cleanup and correlate it with the exact incident time.

## Read-only PowerShell checks used

```powershell
Get-CimInstance Win32_OperatingSystem
Get-CimInstance Win32_BaseBoard
Get-CimInstance Win32_BIOS
Get-CimInstance Win32_Processor
Get-CimInstance Win32_PhysicalMemory
Get-WinEvent -FilterHashtable @{ LogName = 'System'; Id = 41 }
Get-PhysicalDisk
nvidia-smi --query-gpu=name,driver_version,temperature.gpu,power.draw,memory.used,memory.total --format=csv,noheader
```

## External services and changes

- Official reference pages consulted: Intel processor-instability guidance and Gigabyte Z790 D DDR4 Rev. 1.0 support/BIOS history.
- No cloud service, Firebase project, external library, Windows setting, driver, BIOS, or application was changed.
- The investigation was diagnostic-only apart from adding this repository documentation.

