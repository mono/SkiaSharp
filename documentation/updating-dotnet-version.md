# Updating the .NET SDK Version

This guide documents the process for updating SkiaSharp to target a new .NET SDK version. Use this when a new major .NET version is released (e.g., .NET 10 → .NET 11).

## Overview

SkiaSharp's build system is centralized in `source/SkiaSharp.Build.props`, which defines three TFM (Target Framework Moniker) tiers:

| Property | Purpose | Example |
|----------|---------|---------|
| `TFMPrevious` | Oldest supported .NET version (all TFMs including platform) | `net9.0` |
| `TFMCurrent` | Primary shipping .NET version (all TFMs) | `net10.0` |
| `TFMNext` | Preview/upcoming .NET version (empty when not in use) | *(empty)* |

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

#### a) TFM Versions (lines ~61-66)

```xml
<TFMPrevious>net9.0</TFMPrevious>  <!-- was net8.0, shift to previous -->
<TFMCurrent>net10.0</TFMCurrent>   <!-- now the current shipping version -->
<TFMNext></TFMNext>                 <!-- empty until next preview -->
```

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

### 8. Sample Projects — `samples/`

Update all hardcoded TFMs in sample `.csproj` files. These are standalone projects that don't use the centralized TFM variables.

### 9. Test Projects — `tests/`

- `tests/SkiaSharp.Tests.Integration/` has a hardcoded TFM that needs updating
- Other test projects use `$(TFMCurrent)` and update automatically

## Build Architecture

The TFM flow works as follows:

```
SkiaSharp.Build.props
  ├── TFMCurrent (net10.0) → BasicTargetFrameworksCurrent (netstandard2.0;netstandard2.1;net462;net10.0)
  │                        → PlatformTargetFrameworksCurrent (net10.0-ios18.0;net10.0-android36.0;...)
  │
  ├── TFMPrevious (net9.0) → BasicTargetFrameworksPrevious (net9.0)
  │                        → PlatformTargetFrameworksPrevious (net9.0-ios18.0;net9.0-android35.0;...)
  │
  └── TFMNext ("")         → (empty, nothing generated)

Combined:
  BasicTargetFrameworks = Current + Previous + Next
  PlatformTargetFrameworks = Current + Previous + Next
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

- **CI pipelines:** The GitHub Actions workflows in this repo don't contain .NET SDK version references — they use the global.json. If Azure Pipelines are added, they would need updating.
- **Workloads:** Platform builds (iOS, Android, etc.) require workloads to be installed: `dotnet workload install maui`
- **Backward compatibility:** The `netstandard2.0` and `netstandard2.1` targets ensure compatibility with older .NET versions that don't have specific TFM support.
- **Windows TFMs** always include the Windows SDK version (e.g., `10.0.19041.0`) unlike other platforms.
