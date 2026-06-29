# Auto HDR Manager

A modern Windows 11 desktop app for managing per-game Auto HDR registry overrides. Built with WPF + [WPF-UI](https://github.com/lepoco/wpfui) (Fluent Design).

## Features

- **Registry Scanner** -- Scans `HKCU\SOFTWARE\Microsoft\Direct3D` and lists all applications with Direct3D behavior overrides
- **One-click toggle** -- Enable or disable Auto HDR for any listed application directly from the dashboard
- **10-bit support** -- Optionally upgrade buffers to 10-bit for richer color depth
- **Browse & Apply** -- Select any `.exe` and apply Auto HDR settings with a file browser
- **Full rollback** -- Remove individual entries or delete the entire registry subkey
- **Windows 11 UI** -- Mica backdrop, Fluent Design controls, dark/light theme toggle

## Requirements

- Windows 10 version 1809+ or Windows 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (to build)
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (to run, unless published self-contained)

## Build

```bash
cd AutoHdrManager
dotnet restore
dotnet build -c Release
```

The output binary will be in `AutoHdrManager\bin\Release\net8.0-windows\`.

## Publish (single-file portable)

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

Output: a single `AutoHdrManager.exe` in the publish folder.

## Registry Details

All changes are made under `HKEY_CURRENT_USER` (no admin privileges required).

| Key | Value | Description |
|-----|-------|-------------|
| `HKCU\SOFTWARE\Microsoft\Direct3D\ApplicationN\Name` | exe path | Identifies the target game |
| `HKCU\SOFTWARE\Microsoft\Direct3D\ApplicationN\D3DBehaviors` | `BufferUpgradeOverride=1` | Enables Auto HDR |
| `HKCU\SOFTWARE\Microsoft\Direct3D\ApplicationN\D3DBehaviors` | `BufferUpgradeOverride=1;BufferUpgradeEnable10Bit=1` | Enables Auto HDR + 10-bit |

## Credits

Based on [autohdr_force](https://github.com/ledoge/autohdr_force) by ledoge. Rewritten from C to C#/WPF with a modern GUI, registry scanning, and one-click management.
