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
- [ ] **`global.json` `tools.dotnet`** — Keep this equal to `sdk.version` and verify the selected SDK satisfies Arcade's CLI requirements for `dotnet package download`.
- [ ] **`native/winui/global.json` and `DOTNET_VERSION_WINUI`** — Keep these on the latest SDK feature band supported by the Visual Studio MSBuild used for the C++/WinRT projection. Verify the current SDK/MSBuild compatibility matrix and install this SDK side-by-side in the WinUI native jobs instead of forcing the repository SDK onto them.
- [ ] **`scripts/azure-templates-variables.yml`** — Update `DOTNET_VERSION` to the SDK patch and pin `DOTNET_WORKLOAD_VERSION` to a compatible workload set. The workload set may intentionally lag the SDK by whole feature bands when a newer set requires an unavailable Apple toolchain.
- [ ] **Managed Apple pool and `XCODE_VERSION`** — Use an agent image containing the exact Xcode recommended by the workload set. Document any intentional cross-feature-band workload pin beside `DOTNET_WORKLOAD_VERSION`, including the unavailable toolchain that requires it. Keep native Apple builds on their separately pinned Xcode.
- [ ] **`scripts/infra/managed/install-dotnet-workloads.ps1`** — Review the workload installation flow and Tizen manifest source (Samsung may update it independently).

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

> **SupportedOSPlatformVersion:** Each .NET version may raise the minimum supported OS versions. Read the installed workload manifests and update the values in `source/SkiaSharp.Build.props`; workload validation fails when these values are too low.

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
- [ ] `scripts/infra/managed/utils-managed.cake` — Framework check list (add new `netX.0`)
- [ ] `scripts/infra/docs/docs.cake` — Apple/Android ref package names include TFM+TPV (e.g., `Microsoft.iOS.Ref.net10.0_18.0`)
- [ ] `native/winui/build.cake` — WinUI Projection output path uses `$(WindowsTargetFrameworksPrevious)` 

### 7. Utility Projects

- [ ] `utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj`
- [ ] `utils/NativeLibraryMiniTest/docker/NativeLibraryMiniTest.csproj`

### 8. Sample Projects

- [ ] All `samples/Basic/*/SkiaSharpSample.csproj` — 16 files with hardcoded TFMs
- [ ] `samples/Basic/UnoPlatform/SkiaSharpSample/Properties/launchSettings.json`

### 9. Pipeline YAML

- [ ] `scripts/azure-templates-variables.yml` — DOTNET_VERSION, DOTNET_WORKLOAD_VERSION, XCODE_VERSION, EMSCRIPTEN_VERSION, test device versions
- [ ] `scripts/azure-templates-stages-native-wasm.yml` — Add new .NET emscripten entry
- [ ] `scripts/azure-templates-jobs-bootstrapper.yml` — Review workload install step

> **WASM emsdk mapping (do this whenever the new SDK bundles a new Emscripten version).** The .NET WASM SDK links apps with a specific Emscripten toolchain, and a static library built with one Emscripten version cannot be linked by a different one (the wasm object format is incompatible → link failure). Check the new SDK's bundled version (e.g. `dotnet workload list` / the `Microsoft.NET.Runtime.Emscripten.*` pack). Known mapping so far: **.NET 8 → 3.1.34, .NET 9/10 → 3.1.56, .NET 11 → 5.0.6**. When it changes for the new SDK, you must:
> 1. Add a build matrix block (all 4 `st`/`mt`/`simd`/`simd+mt` variants) for the new Emscripten version in `scripts/azure-templates-stages-native-wasm.yml`, and register its `native_wasm_<version>_*` artifacts in both merger lists in `scripts/azure-templates-stages-native-merge.yml`, so the packages ship a static library for it.
> 2. Add a `NativeFileReference` entry for the new TFM in **all four** WASM targets files, keeping each `netX.0` on the Emscripten version its SDK actually uses:
>    - `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets`
>    - `binding/HarfBuzzSharp.NativeAssets.WebAssembly/buildTransitive/HarfBuzzSharp.targets`
>    - `binding/IncludeNativeAssets.SkiaSharp.targets`
>    - `binding/IncludeNativeAssets.HarfBuzzSharp.targets`
>
> Convention for the conditions: the **newest** entry stays open-ended (`VersionGreaterThanOrEquals(TFV, 'A')`) so a future SDK that keeps the same Emscripten version keeps working with no code change (e.g. .NET 9 and .NET 10 both use 3.1.56). Only when a new SDK actually *diverges* do you close the previous entry with an upper bound (`… and VersionLessThan(TFV, 'B')`) and add a new open-ended entry for the new version — the way `net9.0`–`net10.x` was capped at `< 11.0` once .NET 11 moved to 5.0.6. The packaging globs (`**`/`*` over the version folder) pick up new version directories automatically — no nuspec/csproj change needed.

### 10. Docker Images

- [ ] Pin every `mcr.microsoft.com/dotnet/sdk` build image to the exact repository SDK patch while preserving its distro/OS suffix:
  - `scripts/infra/docs/docker/Dockerfile`
  - `scripts/infra/tests/docker/{alpine,alpine-nodeps,azurelinux,azurelinux-nodeps,nanoserver}/Dockerfile`
  - `tests/Dockerfile.linux`
- [ ] Update every native `DOTNET_SDK_VERSION` argument to the exact repository SDK patch:
  - `scripts/infra/native/android/docker/Dockerfile`
  - `scripts/infra/native/linux/docker/{alpine,bionic,glibc,glibc-x86}/Dockerfile`
  - `scripts/infra/native/tizen/docker/Dockerfile`
  - `scripts/infra/native/wasm/docker/Dockerfile`
- [ ] In the WASM Dockerfile, use `dotnet-install.sh --version ${DOTNET_SDK_VERSION}`. Do not use `--channel` with an exact SDK version.
- [ ] Keep isolated consumer/sample contexts on the floating .NET major tag so they exercise the latest servicing release:
  - `samples/Basic/DockerConsole/{linux,windows}.Dockerfile`
  - `samples/Basic/DockerWebApi/{linux,windows}.Dockerfile`
  - The generated Dockerfile string in `tests/SkiaSharp.Tests.Integration/Tests/LinuxConsoleTests.cs`
- [ ] Verify every complete MCR tag exists with `docker manifest inspect mcr.microsoft.com/dotnet/sdk:<tag>`. Verify SDKs installed by `dotnet-install.sh` have published artifacts for every host architecture used by the image.

Images that run `dotnet` against the checked-out repository must provide an SDK compatible with the root `global.json`; this includes the local docs image, CI container-test images, and `tests/Dockerfile.linux`. The sample Dockerfiles and generated Linux integration-test project build isolated contexts without the repository `global.json`, so their floating current-major SDK tags intentionally validate the latest servicing release for `TFMCurrent`.

Keep each distro/OS suffix unchanged when updating either kind of image. For example, an SDK bump should preserve suffixes such as `-noble`, `-alpine3.23`, `-azurelinux3.0`, and `-nanoserver-ltsc2022`. Runtime and ASP.NET base images are separate from the build SDK pin; do not change them as part of an SDK-only alignment unless the runtime itself is also being updated.

### 11. NuGet & Feeds

- [ ] `nuget.config` — Remove old preview feeds, keep dotnet-public + dotnet-eng + test-device-runners

> **Note:** `nuget.org` is a disallowed source in the SkiaSharp CI pipeline. If you encounter missing package restore errors during development, you can temporarily add nuget.org to work through issues, but it **must be removed before merging**. Request mirroring for any missing packages.

## Pre-Merge Checklist

Before merging a .NET upgrade PR, verify these items:

- [ ] **`nuget.config`** — Must NOT contain `nuget.org` source (disallowed in CI)
- [ ] **All CI stages pass** — Tests, samples, API diff, and package stages must be green
- [ ] **Documentation updated** — `documentation/dev/updating-dotnet-version.md` reflects any new learnings

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
- `.slnx` / `.slnf` files — don't encode TFMs
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
Native artifacts are reused automatically through content-based caching: `scripts/infra/caching/repo-deps.py` hashes the native inputs and `Cache@2` restores a matching build, which sets `CACHE_SKIP` and skips native compilation. Nothing needs to be set by hand, and there is no way to point a run at an arbitrary previous build ID — artifacts are only ever downloaded from the current run or from an exact connected pipeline run.

### SDK Version Mismatch
If CI agents don't have the exact SDK version in `global.json`, use `"rollForward": "latestPatch"` to accept any patch version in the same feature band (e.g., `10.0.100` accepts `10.0.102`).

### Workload Install Failures
If `dotnet workload restore` fails with "no project found", the pipeline uses explicit `dotnet workload install` with a list of workloads instead.
