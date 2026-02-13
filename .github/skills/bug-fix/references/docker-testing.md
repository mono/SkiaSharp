# Docker Cross-Platform Testing

Use Docker to test SkiaSharp on Linux when the host is macOS or Windows. Docker Desktop provides both linux/amd64 (x86_64) and linux/arm64 via QEMU or native execution.

## Verified Configurations

All configurations below have been tested end-to-end.

| Image | Platform flag | SkiaSharp versions | libc | Notes |
|-------|--------------|-------------------|------|-------|
| `mcr.microsoft.com/dotnet/sdk:8.0` | `--platform linux/amd64` | 1.68.x, 2.88.x, 3.x | glibc (Debian) | **Primary choice** |
| `mcr.microsoft.com/dotnet/sdk:8.0` | `--platform linux/arm64` | 2.88.x, 3.x | glibc (Debian) | Native on Apple Silicon |
| `mcr.microsoft.com/dotnet/sdk:8.0-alpine` | `--platform linux/amd64` | 2.88.x, 3.x | musl | Uses `sh` not `bash` |

> **1.68.x has NO arm64 native.** Use `--platform linux/amd64` for 1.x testing.
> Alpine uses `sh` — replace `bash` with `sh` in `docker run` commands.

## Required System Dependencies

SkiaSharp's native library (`libSkiaSharp.so`) links against `libfontconfig`. Docker SDK images do not include it. **Install it first or you get `DllNotFoundException`.**

| Image | Install command |
|-------|----------------|
| Debian (sdk:8.0) | `apt-get update -qq && apt-get install -y -qq libfontconfig1` |
| Alpine (sdk:8.0-alpine) | `apk add --no-cache fontconfig` |

## Quick Start — One-liner Test

Verify SkiaSharp works in Docker before doing anything complex:

```bash
# Linux x64 (Debian) — works for ALL SkiaSharp versions
docker run --rm --platform linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 bash -c '
apt-get update -qq && apt-get install -y -qq libfontconfig1 2>&1 | tail -1
mkdir -p /tmp/test && cd /tmp/test
dotnet new console -n Test --framework net8.0 --no-restore 2>&1 | tail -1
cd Test
dotnet add package SkiaSharp --version VERSION --no-restore 2>&1 | tail -1
dotnet add package SkiaSharp.NativeAssets.Linux --version VERSION --no-restore 2>&1 | tail -1
cat > Program.cs << "EOF"
using SkiaSharp;
var info = new SKImageInfo(100, 100);
using var surface = SKSurface.Create(info);
var canvas = surface.Canvas;
canvas.Clear(SKColors.Red);
using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
Console.WriteLine($"SkiaSharp {typeof(SKSurface).Assembly.GetName().Version}");
Console.WriteLine($"Arch: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Surface: {surface != null}, PNG: {data.Size} bytes");
Console.WriteLine("SUCCESS");
EOF
dotnet run --runtime linux-x64 --no-self-contained 2>&1 | tail -5
'
```

Replace `VERSION` with the version from the issue (e.g., `1.68.3`, `2.88.8`, `3.116.1`).

## NativeAssets.Linux Package Versions

The `SkiaSharp.NativeAssets.Linux` package is **required** and **separate** from the main SkiaSharp package on Linux.

| SkiaSharp version | NativeAssets.Linux RIDs | Notes |
|-------------------|------------------------|-------|
| 1.68.x | `linux-x64` only | No arm64, no musl |
| 2.88.x | `linux-x64`, `linux-arm64`, `linux-musl-x64`, `linux-arm` | Full coverage |
| 3.x | Same as 2.88.x + versioned `.so` symlinks | Full coverage |

## Interactive Docker Session

For complex reproductions, use an interactive session:

```bash
docker run --rm --platform linux/amd64 -it mcr.microsoft.com/dotnet/sdk:8.0 bash

# Inside container:
apt-get update -qq && apt-get install -y -qq libfontconfig1
mkdir -p /app && cd /app
dotnet new console --framework net8.0 --no-restore
dotnet add package SkiaSharp --version <version> --no-restore
dotnet add package SkiaSharp.NativeAssets.Linux --version <version> --no-restore
# Edit Program.cs and iterate...
```

## Inspecting Native Library

```bash
# Find the native library
find ~/.nuget/packages/skiasharp.nativeassets.linux/ -name "*.so*"

# Check dependencies (DT_NEEDED entries)
readelf -d /path/to/libSkiaSharp.so | grep NEEDED

# Check for undefined symbols
nm -u /path/to/libSkiaSharp.so | head -20

# Check GLIBC version requirements
readelf -V /path/to/libSkiaSharp.so | grep GLIBC
```

## Common Issues & Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `libfontconfig.so.1: cannot open shared object file` | Missing fontconfig | `apt-get install libfontconfig1` |
| `liblibSkiaSharp: cannot open shared object file` | Wrong RID resolution | Add `--runtime linux-x64 --no-self-contained` to `dotnet run` |
| `bash: not found` (Alpine) | Alpine uses `sh` | Replace `bash` with `sh` in docker command |
| No output, hangs | QEMU emulation slow for cross-arch | Give it time (2-3x slower than native) |

## What Docker CANNOT Test

| Platform | Why | Alternative |
|----------|-----|-------------|
| Windows (WPF, WinUI, ClickOnce) | No Windows containers for GUI | Needs Windows machine |
| macOS / Mac Catalyst | No macOS Docker images | Needs macOS machine |
| iOS / Android | No mobile OS in Docker | Needs device/emulator |
| GPU rendering (Metal, OpenGL) | No GPU in Docker | Needs GPU hardware |
| Xamarin.Forms / Xamarin.Mac | Deprecated platform, no Docker support | Historical context only |

## When Docker is Unavailable

1. Document the limitation in the repro/fix JSON `blockers` array
2. Test on available platforms
3. Record `needs-platform` conclusion with specific platform needed
4. Note: "Verified on [available platform], user verification needed for [target platform]"
