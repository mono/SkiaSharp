# Updating .NET Version in SkiaSharp

This checklist documents every file that needs updating when bumping the .NET SDK version (e.g., .NET 10 → .NET 11).

## Terminology

| Property | Purpose | Example |
|----------|---------|---------|
| **TFMBase** | Lowest .NET for class libraries only (no platform TFMs) | `net6.0` |
| **TFMPrevious** | Previous .NET with full platform support | `net9.0` |
| **TFMCurrent** | Current .NET with full platform support | `net10.0` |
| **TPV\*Previous** | Target Platform Versions for TFMPrevious | `TPViOSPrevious=18.0` |
| **TPV\*Current** | Target Platform Versions for TFMCurrent | `TPViOSCurrent=26.0` |

> **Important:** Starting with .NET 10, Apple TPVs use Xcode 26 unified SDK versioning. iOS, MacCatalyst, tvOS, and macOS all use `26.0` (not the OS version like `18.0` or `15.0`). Check for valid TPVs with `dotnet new console -f net10.0-ios` and observe the error message listing valid versions.

## Upgrade Checklist

### 1. SDK & Workloads

- [ ] **`global.json`** — Update `sdk.version` to the new SDK feature band (e.g., `10.0.100`). Use `"rollForward": "latestPatch"` to accept any patch version available on CI agents.
- [ ] **`scripts/azure-templates-variables.yml`** — Update `DOTNET_VERSION` and `DOTNET_WORKLOAD_VERSION` to match the new SDK version
- [ ] **`scripts/install-dotnet-workloads.ps1`** — Review Tizen script URL (Samsung repo may update)

> **Note:** Do NOT set `workloadVersion` in `global.json`. Native builds skip SDK install but still read global.json, causing failures if the pinned workload version isn't pre-installed.

### 2. Central Build Props

- [ ] **`source/SkiaSharp.Build.props`** — This is the most critical file:
  - Shift TFMBase ← TFMPrevious (if dropping oldest base)
  - Shift TFMPrevious ← TFMCurrent
  - Set TFMCurrent to the new .NET version
  - Update all TPV\*Previous values (copy from old TPV\*Current)
  - Set new TPV\*Current values (check workload manifests: `dotnet workload list`)
  - Update **SupportedOSPlatformVersion** minimums (check workload manifests for new minimums)
  - Sections to update: BasicTargetFrameworks, PlatformTargetFrameworks, Windows, MAUI, MAUI App, Uno, DefineConstants

> **SupportedOSPlatformVersion:** Each .NET version may raise the minimum supported OS versions. For .NET 10: iOS/tvOS minimum is 12.2, MacCatalyst minimum is 15.0, macOS minimum is 12.0. These are enforced by the workloads and will cause build errors if too low.

### 3. NativeAssets Platform Projects (14 files)

All use `$(TFMPrevious)-platform$(TPVPrevious);$(TFMCurrent)-platform$(TPVCurrent)` pattern.

- [ ] `binding/SkiaSharp.NativeAssets.Android/SkiaSharp.NativeAssets.Android.csproj`
- [ ] `binding/SkiaSharp.NativeAssets.iOS/SkiaSharp.NativeAssets.iOS.csproj`
- [ ] `binding/SkiaSharp.NativeAssets.MacCatalyst/SkiaSharp.NativeAssets.MacCatalyst.csproj`
- [ ] `binding/SkiaSharp.NativeAssets.tvOS/SkiaSharp.NativeAssets.tvOS.csproj`
- [ ] `binding/SkiaSharp.NativeAssets.Tizen/SkiaSharp.NativeAssets.Tizen.csproj`
- [ ] `binding/SkiaSharp.NativeAssets.macOS/SkiaSharp.NativeAssets.macOS.csproj` *(also has BasicTargetFrameworks)*
- [ ] `binding/HarfBuzzSharp.NativeAssets.Android/HarfBuzzSharp.NativeAssets.Android.csproj`
- [ ] `binding/HarfBuzzSharp.NativeAssets.iOS/HarfBuzzSharp.NativeAssets.iOS.csproj`
- [ ] `binding/HarfBuzzSharp.NativeAssets.MacCatalyst/HarfBuzzSharp.NativeAssets.MacCatalyst.csproj`
- [ ] `binding/HarfBuzzSharp.NativeAssets.tvOS/HarfBuzzSharp.NativeAssets.tvOS.csproj`
- [ ] `binding/HarfBuzzSharp.NativeAssets.Tizen/HarfBuzzSharp.NativeAssets.Tizen.csproj`
- [ ] `binding/HarfBuzzSharp.NativeAssets.macOS/HarfBuzzSharp.NativeAssets.macOS.csproj`

### 4. Source Projects

- [ ] `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj` — Update TFM list and add PackageReference for new `Microsoft.AspNetCore.Components.Web` version

### 5. Test Projects

- [ ] `tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj` — Uses `$(MauiTargetFrameworksAppCurrent)`
- [ ] `tests/SkiaSharp.Tests.Integration/SkiaSharp.Tests.Integration.csproj` — Hardcoded TFM
- [ ] `tests/SkiaSharp.Tests.Integration/Tests/LinuxConsoleTests.cs` — Hardcoded TFM in string template
- [ ] `tests/SkiaSharp.Tests.Integration/Tests/Maui*Tests.cs` — Hardcoded TFMs in `TargetFramework` property

### 6. Cake Build Scripts

- [ ] `build.cake` — 4 hardcoded TFMs in test tasks (~lines 285, 333, 365, 397)
- [ ] `scripts/cake/UtilsManaged.cake` — Framework check list (add new `netX.0`)
- [ ] `scripts/cake/UpdateDocs.cake` — Apple/Android ref package names include TFM+TPV (e.g., `Microsoft.iOS.Ref.net10.0_18.0`)
- [ ] `native/winui/build.cake` — WinUI Projection output path uses `$(WindowsTargetFrameworksPrevious)` 

### 7. Utility Projects

- [ ] `utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj`
- [ ] `utils/WasmTestRunner/WasmTestRunner.csproj`
- [ ] `utils/NativeLibraryMiniTest/docker/NativeLibraryMiniTest.csproj`

### 8. Sample Projects

- [ ] All `samples/Basic/*/SkiaSharpSample.csproj` — 16 files with hardcoded TFMs
- [ ] `samples/Basic/UnoPlatform/SkiaSharpSample/Properties/launchSettings.json`

### 9. Pipeline YAML

- [ ] `scripts/azure-templates-variables.yml` — DOTNET_VERSION, DOTNET_WORKLOAD_VERSION, XCODE_VERSION, EMSCRIPTEN_VERSION, test device versions
- [ ] `scripts/azure-templates-stages-native-wasm.yml` — Add new .NET emscripten entry
- [ ] `scripts/azure-templates-jobs-bootstrapper.yml` — Review workload install step

### 10. Docker Images

- [ ] All Dockerfiles in `scripts/Docker/*/Dockerfile` — Update `FROM mcr.microsoft.com/dotnet/sdk:X.0` to new version
  - debian10-amd64, debian11-amd64, debian11-arm64, debian12-amd64
  - fedora40-amd64, ubuntu18.04-amd64, ubuntu20.04-amd64, ubuntu22.04-amd64
  - alpine-amd64, alpine-arm64, alpine-x86

### 11. NuGet & Feeds

- [ ] `nuget.config` — Remove old preview feeds, keep dotnet-public + dotnet-eng + test-device-runners

> **Note:** `nuget.org` is a disallowed source in the SkiaSharp CI pipeline. If you encounter missing package restore errors during development, you can temporarily add nuget.org to work through issues, but it **must be removed before merging**. Request mirroring for any missing packages.

## Pre-Merge Checklist

Before merging a .NET upgrade PR, verify these items:

- [ ] **`nuget.config`** — Must NOT contain `nuget.org` source (disallowed in CI)
- [ ] **`scripts/azure-pipelines-complete.yml`** — `buildExternals` parameter must be reset to `'latest'` (not a specific build ID)
- [ ] **All CI stages pass** — Tests, samples, API diff, and package stages must be green
- [ ] **Documentation updated** — `documentation/updating-dotnet-version.md` reflects any new learnings

## Known Issues & Breaking Changes

When upgrading .NET versions, watch for these common issues:

### Floating-Point Precision Changes
.NET 9 changed `System.Numerics.Matrix4x4.CreateFromAxisAngle` to go through `Quaternion`, producing slightly different floating-point results. Tests using exact float comparisons may need tolerance adjustments. The `AssertSimilar` helper in `tests/Tests/SkiaSharp/SKTest.cs` uses `Math.Round()` (not truncation) to handle this.

### Apple TPV Version Numbering  
Starting with .NET 10, Apple workloads use **Xcode 26 unified SDK versioning**. The TPV is `26.0` for all Apple platforms, not the OS version numbers like `18.0` (iOS), `15.0` (MacCatalyst), etc. Build errors like `NETSDK1140: 18.0 is not a valid TargetPlatformVersion for iOS` indicate this issue.

### MAUI Breaking Changes
Check the MAUI release notes for API changes. Common issues:
- Namespace/type removals (e.g., `Microsoft.Maui.Hosting.Compatibility` removed in .NET 10)
- New minimum OS versions
- Changes to workload dependencies

### Tizen Workload
Tizen is not an official Microsoft workload. Samsung may lag behind on .NET version support. Check https://github.com/Samsung/Tizen.NET for compatibility before upgrading.

## Files That Auto-Update (no manual changes needed)

These use MSBuild properties from `SkiaSharp.Build.props`:

- All projects using `$(BasicTargetFrameworks)` — NativeAssets.Linux, Win32, WebAssembly, etc.
- All projects using `$(WindowsTargetFrameworks)` — NativeAssets.WinUI, NanoServer, Views.WinUI
- All projects using `$(MauiTargetFrameworks)` — Views.Maui.Core, Views.Maui.Controls
- All projects using `$(UnoTargetFrameworks)` — Views.Uno.WinUI, Skia, Wasm
- All projects using `$(TFMCurrent)` — Benchmarks, test console projects, Direct3D
- `binding/NativeAssets.Build.targets` — Uses `$(TFMCurrent)`
- `native/winui/.../SkiaSharp.Views.WinUI.Native.Projection.csproj` — Uses `$(WindowsTargetFrameworksPrevious)`

## Files That Are Safe (no changes needed)

- `IsTargetFrameworkCompatible('net7.0')` conditions in binding csproj files — floor check
- `IncludeNativeAssets.*.targets` — `VersionGreaterThanOrEquals('9.0')` covers future versions
- `.sln` / `.slnf` files — don't encode TFMs
- `samples/Gallery/` — Legacy samples, not updated

## How to Test a Preview .NET Version (e.g., .NET 11 Preview)

Since platform workloads only support 2 versions at a time, testing a preview means shifting the TFM chain:

1. Create a branch
2. Follow the full upgrade checklist above, setting:
   - `TFMPrevious` ← old `TFMCurrent` (e.g., `net10.0`)
   - `TFMCurrent` ← the preview version (e.g., `net11.0`)
   - `global.json` SDK version ← preview SDK (e.g., `11.0.100-preview.1`)
   - `global.json` `allowPrerelease` ← `true`
   - `DOTNET_VERSION` ← preview SDK version
   - `DOTNET_WORKLOAD_VERSION` ← preview workload set version
3. Build and test on the branch
4. Merge when the new .NET version goes GA

There is no side-by-side preview mechanism — the `DOTNET_VERSION` in the pipeline IS the SDK version, preview or not.

## How to Verify TPVs

After installing the new SDK, check actual workload TPVs:

```bash
dotnet workload list
# Then check manifest files in:
# ~/.dotnet/sdk-manifests/<version>/

# Or try to create a project and observe the error for valid TPVs:
dotnet new console -f net10.0-ios
# Error will list valid TPVs like: 26.0, 26.2
```

## Workload Pinning

Workloads are pinned via the `DOTNET_WORKLOAD_VERSION` pipeline variable, which is passed to `install-dotnet-workloads.ps1` as `-WorkloadVersion`. This uses the .NET SDK workload sets feature (`dotnet workload install --version <version>`) for reproducible builds. 

**Why not use `workloadVersion` in `global.json`?** Native builds (which skip SDK/workload install) still read `global.json`. If the pinned workload version isn't pre-installed on the agent, the build fails immediately. By passing the version through the pipeline variable, we control when workload pinning applies.

**Exception:** Tizen is not an official workload — it uses Samsung's custom install scripts from `Samsung/Tizen.NET` repository.

## CI Troubleshooting

### Reusing Native Artifacts
To speed up CI iteration when debugging managed code issues, set the `buildExternals` parameter to a previous build ID that has successful native stages. This skips native compilation and downloads artifacts from the specified build instead.

### SDK Version Mismatch
If CI agents don't have the exact SDK version in `global.json`, use `"rollForward": "latestPatch"` to accept any patch version in the same feature band (e.g., `10.0.100` accepts `10.0.102`).

### Workload Install Failures
If `dotnet workload restore` fails with "no project found", the pipeline uses explicit `dotnet workload install` with a list of workloads instead.
