# Updating .NET Version in SkiaSharp

This checklist documents every file that needs updating when bumping the .NET SDK version (e.g., .NET 10 → .NET 11).

## Terminology

| Property | Purpose | Example |
|----------|---------|---------|
| **TFMBase** | Lowest .NET for class libraries only (no platform TFMs) | `net6.0` |
| **TFMPrevious** | Previous .NET with full platform support | `net9.0` |
| **TFMCurrent** | Current .NET with full platform support | `net10.0` |
| **TPV\*Previous** | Target Platform Versions for TFMPrevious | `TPViOSPrevious=18.0` |
| **TPV\*Current** | Target Platform Versions for TFMCurrent | `TPViOSCurrent=18.0` |

## Upgrade Checklist

### 1. SDK & Workloads

- [ ] **`global.json`** — Update `sdk.version` to the new SDK feature band (e.g., `10.0.100`)
- [ ] **`scripts/azure-templates-variables.yml`** — Update `DOTNET_WORKLOAD_VERSION` to match the new SDK version
- [ ] **`scripts/install-dotnet-workloads.ps1`** — Review Tizen script URL (Samsung repo may update)

### 2. Central Build Props

- [ ] **`source/SkiaSharp.Build.props`** — This is the most critical file:
  - Shift TFMBase ← TFMPrevious (if dropping oldest base)
  - Shift TFMPrevious ← TFMCurrent
  - Set TFMCurrent to the new .NET version
  - Update all TPV\*Previous values (copy from old TPV\*Current)
  - Set new TPV\*Current values (check workload manifests: `dotnet workload list`)
  - Sections to update: BasicTargetFrameworks, PlatformTargetFrameworks, Windows, MAUI, MAUI App, Uno, DefineConstants

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

- [ ] `scripts/azure-templates-variables.yml` — DOTNET_VERSION, XCODE_VERSION, EMSCRIPTEN_VERSION, test device versions
- [ ] `scripts/azure-templates-stages-native-wasm.yml` — Add new .NET emscripten entry
- [ ] `scripts/azure-templates-jobs-bootstrapper.yml` — Review workload install step

### 10. NuGet & Feeds

- [ ] `nuget.config` — Remove old preview feeds, keep nuget.org + dotnet-public + dotnet-eng

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
```

## Workload Pinning

Workloads are pinned via the `DOTNET_WORKLOAD_VERSION` pipeline variable, which is passed to `install-dotnet-workloads.ps1` as `--version`. This uses the .NET SDK workload sets feature (`dotnet workload install --version <version>`) for reproducible builds. The workload version is NOT set in `global.json` because native builds (which skip SDK/workload install) would fail if the pinned version isn't pre-installed on the agent.

**Exception:** Tizen is not an official workload — it uses Samsung's custom install scripts from `Samsung/Tizen.NET` repository.
