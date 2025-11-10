# Build Optimization Guide

This document explains how SkiaSharp's native libraries are optimized and how to ensure you're building performant binaries.

## Overview

SkiaSharp consists of two layers:
1. **Native layer**: C/C++ libraries (libSkiaSharp.dll/.so/.dylib) built from Google's Skia
2. **Managed layer**: C# assemblies that P/Invoke to the native libraries

Both layers need to be built in Release configuration for optimal performance.

## Native Library Optimization

### Build Configuration

The native Skia libraries are built using the GN build system. The critical flag is `is_official_build`:

```bash
# Release build (optimized, recommended for production)
dotnet cake --target=externals --configuration=Release

# Debug build (unoptimized, for debugging only)
dotnet cake --target=externals --configuration=Debug
```

### What `is_official_build=true` Does

When building with `--configuration=Release`, the following optimizations are enabled:

#### Compiler Optimizations
- **GCC/Clang**: `-O3` flag for maximum optimization
- **MSVC**: `/O2` flag for speed optimization
- **Link-time optimizations**: `/OPT:ICF` and `/OPT:REF` on Windows

#### Preprocessor Defines
- `NDEBUG` is defined, which disables standard C/C++ assertions
- `SK_RELEASE` is defined (Skia's release mode)
- `SK_DEBUG` is NOT defined (disables Skia's debug checks)

#### What Gets Disabled in Release
- Runtime assertions (`SkASSERT`, `assert()`)
- Bounds checking in hot paths
- Debug-only validation code
- Extra diagnostic logging
- Debug C runtime libraries (Windows: `/MTd` becomes `/MT`)

### Performance Impact

Building without optimizations (Debug configuration) can result in:
- **2-10x slower rendering** for typical graphics operations
- **3-20x slower** for compute-intensive operations (e.g., image filters, path operations)
- Larger binary sizes
- Higher memory usage

## Managed Library Optimization

The C# assemblies are optimized automatically by .NET:

- **Release builds**: JIT optimizations enabled, minimal debug info
- **Debug builds**: JIT optimizations disabled, full debug info, `THROW_OBJECT_EXCEPTIONS` defined

Build with:
```bash
dotnet build -c Release
```

## Verification

Use the provided script to verify your native libraries are optimized:

```bash
pwsh ./scripts/verify-optimizations.ps1
```

This script checks for:
- Debug C runtime linkage (Windows)
- Assert symbols in the binary (Linux)
- File size heuristics
- Other debug indicators

## CI/CD Configuration

Our CI/CD pipelines build with `Configuration=Release` by default (see `scripts/azure-templates-variables.yml`). Pre-built NuGet packages are optimized.

## Common Issues

### Issue: "SkiaSharp is slow after building from source"

**Cause**: Building without specifying configuration, which uses some default that's not Release.

**Solution**:
```bash
# Always specify Release configuration explicitly
dotnet cake --target=externals --configuration=Release
dotnet build -c Release
```

### Issue: "Local builds are faster than NuGet packages"

This should **not** happen as NuGet packages are built in Release configuration. If you experience this:

1. Verify the NuGet package configuration (check binaries with verification script)
2. Ensure you're comparing like-for-like (same platform, same workload)
3. Check that JIT optimizations are enabled in your app

### Issue: "How do I know if my build is optimized?"

Run the verification script:
```bash
pwsh ./scripts/verify-optimizations.ps1
```

Look for:
- ✓ Green checkmarks = good
- ❌ Red X marks = problems that need fixing
- ⚠ Yellow warnings = informational, usually OK

## Technical Details

### GN Build System Flow

1. Cake script sets `CONFIGURATION` (default: "Release")
2. `native-shared.cake` converts to `is_official_build=true/false`
3. GN reads `is_official_build` and sets `is_debug = !is_official_build`
4. Skia's `BUILDCONFIG.gn` applies optimization configs when `!is_debug`
5. Skia's `SkLoadUserConfig.h` defines `SK_RELEASE` when `NDEBUG` is set

### File Locations

- Native build script: `scripts/cake/native-shared.cake`
- Configuration defaults: `scripts/cake/shared.cake`
- CI variables: `scripts/azure-templates-variables.yml`
- Verification script: `scripts/verify-optimizations.ps1`

## References

- [Skia Build Configuration](https://skia.org/docs/user/build/)
- [GN Reference](https://gn.googlesource.com/gn/+/main/docs/reference.md)
- [.NET Build Configuration](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build/)
