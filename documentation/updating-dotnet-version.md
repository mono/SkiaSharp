# Updating the .NET SDK Version

This guide documents the process for updating SkiaSharp to target a new .NET SDK version. Use this when a new major .NET version is released (e.g., .NET 10 → .NET 11).

## Overview

SkiaSharp's build system is centralized in `source/SkiaSharp.Build.props`, which defines four TFM (Target Framework Moniker) tiers:

| Property | Purpose | Example |
|----------|---------|---------|
| `TFMBase` | Oldest .NET version for class library support (base TFM only, no platforms) | `net6.0` |
| `TFMPrevious` | Previous .NET version with full platform support (all TFMs) | `net9.0` |
| `TFMCurrent` | Primary shipping .NET version (all TFMs) | `net10.0` |
| `TFMNext` | Preview/upcoming .NET version (empty when not in use) | *(empty)* |

- **TFMBase** provides backward compatibility for class libraries (e.g., a net6.0 project consuming SkiaSharp). It only adds the base TFM — no platform-specific TFMs like `-android` or `-ios`.
- **TFMPrevious** provides full platform support (net9.0-android, net9.0-ios, etc.) for apps still on the previous .NET version.
- **TFMCurrent** is the primary shipping version with all platform TFMs.
- **TFMNext** is for preview versions of upcoming .NET releases.

These properties flow to all `.csproj` files via MSBuild variables like `$(AllTargetFrameworks)`, `$(PlatformTargetFrameworks)`, `$(MauiTargetFrameworks)`, etc.

## Files That Need Updating

### 1. `global.json` — SDK Version Pin

Update the SDK version to the new .NET release:

```json
{
  "sdk": {
    "version": "10.0.100",
    "allowPrerelease": false,
    "rollForward": "latestMinor"
  }
}
```

The `rollForward: latestMinor` setting allows using any 10.0.x patch version.

### 2. `source/SkiaSharp.Build.props` — Core TFM Configuration

This is the **most important file**. Update these sections:

#### a) TFM Versions (lines ~61-68)

```xml
<TFMBase>net6.0</TFMBase>          <!-- oldest class library support -->
<TFMPrevious>net9.0</TFMPrevious>  <!-- previous version with platform TFMs -->
<TFMCurrent>net10.0</TFMCurrent>   <!-- primary shipping version -->
<TFMNext></TFMNext>                 <!-- empty until next preview -->
```

When upgrading (e.g., .NET 10 → .NET 11):
- Keep TFMBase as-is (unless dropping old .NET support entirely)
- Move TFMCurrent to TFMPrevious
- Set TFMCurrent to the new .NET version

#### b) Target Platform Versions (TPV)

Each tier has platform-specific TPV values. When updating:
- Move Current values to Previous
- Set new Current values for the new .NET version defaults
- Update Next values for the anticipated next version

```xml
<!-- Previous (net9.0 era) -->
<TPVAndroidPrevious>35.0</TPVAndroidPrevious>
<TPViOSPrevious>18.0</TPViOSPrevious>
<!-- ... -->

<!-- Current (net10.0 era) -->
<TPVAndroidCurrent>36.0</TPVAndroidCurrent>
<TPViOSCurrent>18.0</TPViOSCurrent>
<!-- ... -->
```

#### c) Platform Target Frameworks

The file defines `PlatformTargetFrameworksCurrent`, `PlatformTargetFrameworksPrevious`, and `PlatformTargetFrameworksNext`. These generate the full TFM strings like `net10.0-android36.0`.

**Important:** `PlatformTargetFrameworksPrevious` must be defined so that projects targeting the previous .NET version (e.g., `net9.0-android`) can find compatible assemblies in the NuGet packages.

#### d) MAUI Workaround

There's a workaround for MAUI (https://github.com/dotnet/maui/pull/24263) that clears and restores TPV values. When adding Previous platform TFMs, ensure the Previous TPVs are also cleared/restored in this section.

#### e) MAUI Target Frameworks

Add `MauiTargetFrameworksPrevious` alongside `MauiTargetFrameworksCurrent` and `MauiTargetFrameworksNext`. Update `MauiTargetFrameworks` to include Previous.

#### f) Uno Platform Target Frameworks

Add Previous to `UnoTargetFrameworksReference` and create `UnoTargetFrameworksPrevious`.

### 3. `nuget.config` — NuGet Package Feeds

Add the new version-specific feed and remove old ones:

```xml
<add key="dotnet10" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet10/nuget/v3/index.json" />
```

Remove feeds for .NET versions no longer supported (e.g., dotnet6, dotnet7).

### 4. `build.cake` — Hardcoded TFMs in Build Tasks

Search for hardcoded TFM strings and update them:

```csharp
var tfm = "net10.0";           // was "net8.0"
var tfm = "net10.0-android";   // was "net8.0-android"
var tfm = "net10.0-ios";       // was "net8.0-ios"
var tfm = "net10.0-maccatalyst"; // was "net8.0-maccatalyst"
```

### 5. `scripts/cake/UtilsManaged.cake` — Framework Detection

Update the framework detection logic to include the new version:

```csharp
d.Equals("net10.0")  // add alongside existing net6.0, net7.0, etc.
```

### 6. Blazor Views — `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj`

Add a new `Microsoft.AspNetCore.Components.Web` package reference for the new version:

```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="10.0.0" Condition="$(TargetFramework.StartsWith('net10.0'))" />
```

Remove references for versions no longer in `TFMPrevious`/`TFMCurrent`/`TFMNext`.

### 7. `scripts/cake/UpdateDocs.cake` — Documentation Reference Assemblies

Update platform reference assembly package names and TFMs:

```csharp
await AddDep("Microsoft.iOS.Ref.net10.0_18.0", "net10.0");
await AddDep("Microsoft.MacCatalyst.Ref.net10.0_18.0", "net10.0");
```

### 8. Azure Pipeline Variables — `scripts/azure-templates-variables.yml`

Update the .NET SDK version and related workload configuration:

```yaml
DOTNET_VERSION: '10.0.102'           # was '8.0.304'
DOTNET_WORKLOAD_SOURCE: ''           # update to .NET 10 MAUI workload source
DOTNET_WORKLOAD_TIZEN: ''            # update to .NET 10 Tizen workload version
XCODE_VERSION: '16.2'                # update to Xcode version matching .NET 10
XCODE_VERSION_NATIVE: '16.2'
IOS_TEST_DEVICE_VERSION: 18.2        # update to match current iOS version
ANDROID_TEST_DEVICE_VERSION: 35      # update to match current Android API level
ANDROID_PLATFORM_VERSIONS: 21,35     # update max API level
```

### 9. WASM Native Stages — `scripts/azure-templates-stages-native-wasm.yml`

If a new Emscripten version is needed for the new .NET version, add a section:

```yaml
# .NET 10
- 3.1.56:
  displayName: 3.1.56
  version: 3.1.56
  features: _wasmeh,st
# ... (threading, SIMD variants)
```

### 10. Sample Projects — `samples/`

Update all hardcoded TFMs in sample `.csproj` files. These are standalone projects that don't use the centralized TFM variables.

### 11. Test Projects — `tests/`

- `tests/SkiaSharp.Tests.Integration/` has a hardcoded TFM that needs updating
- Other test projects use `$(TFMCurrent)` and update automatically

## Build Architecture

The TFM flow works as follows:

```
SkiaSharp.Build.props
  ├── TFMBase (net6.0)     → BasicTargetFrameworksBase (net6.0)
  │                        → WindowsTargetFrameworksBase (net6.0-windows10.0.19041.0)
  │                        → (no platform TFMs — class library support only)
  │
  ├── TFMPrevious (net9.0) → BasicTargetFrameworksPrevious (net9.0)
  │                        → PlatformTargetFrameworksPrevious (net9.0-ios18.0;net9.0-android35.0;...)
  │
  ├── TFMCurrent (net10.0) → BasicTargetFrameworksCurrent (netstandard2.0;netstandard2.1;net462;net10.0)
  │                        → PlatformTargetFrameworksCurrent (net10.0-ios18.0;net10.0-android36.0;...)
  │
  └── TFMNext ("")         → (empty, nothing generated)

Combined:
  BasicTargetFrameworks = Current + Base + Previous + Next
  PlatformTargetFrameworks = Current + Previous + Next   (no Base — Base has no platform TFMs)
  AllTargetFrameworks = Basic + Platform
```

Projects use these combined variables:
- `binding/SkiaSharp/SkiaSharp.csproj` → `$(AllTargetFrameworks)` 
- `source/SkiaSharp.Views/SkiaSharp.Views.csproj` → `$(PlatformTargetFrameworks)`
- `source/SkiaSharp.Views.Maui/` → `$(MauiTargetFrameworks)`
- `tests/SkiaSharp.Tests.Console/` → `$(TFMCurrent)`

## Verification Steps

After making changes:

1. **Check SDK version:** `dotnet --version` should show the new SDK
2. **Restore:** `dotnet restore binding/SkiaSharp/SkiaSharp.csproj`
3. **Build net current:** `dotnet build binding/SkiaSharp/SkiaSharp.csproj -f net10.0`
4. **Build net previous:** `dotnet build binding/SkiaSharp/SkiaSharp.csproj -f net9.0`
5. **Build netstandard:** `dotnet build binding/SkiaSharp/SkiaSharp.csproj -f netstandard2.0`
6. **Build tests:** `dotnet build tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj -f net10.0`
7. **Build Blazor:** `dotnet build source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj -f net10.0`

## WebAssembly Native Assets

The WebAssembly native asset targets (`binding/*/buildTransitive/*.targets`) use version-based conditions that handle forward compatibility:

```xml
<!-- net9.0+ -->
<NativeFileReference ... Condition="$([MSBuild]::VersionGreaterThanOrEquals($(TargetFrameworkVersion), '9.0'))" />
```

This automatically covers net10.0+, so no changes are needed unless a new WASM binary version is added.

## Notes

- **Azure Pipelines:** The Azure DevOps pipeline templates are in `scripts/azure-templates-*.yml`. The key file is `scripts/azure-templates-variables.yml` which defines `DOTNET_VERSION`, workload sources, Xcode versions, and test device versions. The bootstrapper template (`azure-templates-jobs-bootstrapper.yml`) uses these variables to install the SDK.
- **Workloads:** Platform builds (iOS, Android, etc.) require workloads to be installed: `dotnet workload install maui`
- **Backward compatibility:** The `netstandard2.0`, `netstandard2.1`, and `TFMBase` (net6.0) targets ensure compatibility with older .NET versions that don't have specific TFM support.
- **Windows TFMs** always include the Windows SDK version (e.g., `10.0.19041.0`) unlike other platforms.
- **WebAssembly targets** use `VersionGreaterThanOrEquals` conditions that automatically handle new .NET versions without changes.
