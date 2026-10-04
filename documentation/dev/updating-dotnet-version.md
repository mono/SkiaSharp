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
- [ ] **Managed Apple pool, `XCODE_VERSION`, and `XCODE_VERSION_PREVIEW`** — Use an agent image containing the exact Xcode required by each workload set. Stable `10.0.401` and preview `11.0.100-rc.1.26458.5` both require Xcode 26.6 on Tahoe, even though the stable Apple release tag contains `xcode26.5`. Keep native Apple builds on their separately pinned Xcode 26.3 and Sequoia agents.
- [ ] **`scripts/infra/managed/install-dotnet-workloads.ps1`** — Review the workload installation flow and Tizen manifest source (Samsung may update it independently).
- [ ] **`scripts/infra/managed/install-openjdk.ps1` and `ANDROID_PLATFORM_VERSIONS`** — Match the Android workload's prerequisites. The current installer defaults to JDK `21.0.10+7`, reuses `JAVA_HOME_<major>_X64`, and CI installs platforms `21,35,36,37.0`. Use the exact SDK package suffix: Android 37 is published as `platforms;android-37.0`, not `platforms;android-37`.

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
- [ ] `source/SkiaSharp.NuGet.targets` — verify compiler XML remains packaged beside the matching `lib/<tfm>` and `ref/<tfm>` assemblies, including platform TFMs
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

> **WASM emsdk mapping (review whenever the SDK bundles a new Emscripten version).** Align both SkiaSharp and HarfBuzzSharp archive builds with the runtime workload's Emscripten toolchain and exception-handling mode. Older static libraries can be incompatible (for example, removed `saveSetjmp` / `testSetjmp` helpers cause native-link failures). Check the bundled version using [Finding .NET's Emscripten version](#finding-nets-emscripten-version) below. The current **SkiaSharp/HarfBuzzSharp archive selection** is **.NET 8 → 3.1.34, .NET 9/10 → 3.1.56, .NET 11+ → 6.0.2**. .NET 11 previews are not supported; .NET 11 RC1/RC2 use 6.0.2. A historical passing 5.0.6-on-6.0.2 smoke test is not a substitute for native-link and browser-runtime validation of the new archives. When updating the archive set, you must:
> 1. Add a build matrix block (all 4 `st`/`mt`/`simd`/`simd+mt` variants) for the new Emscripten version in `scripts/azure-templates-stages-native-wasm.yml`, and register its `native_wasm_<version>_*` artifacts in both merger lists in `scripts/azure-templates-stages-native-merge.yml`, so the packages ship a static library for it.
> 2. Add a `NativeFileReference` entry for the new TFM in **all four** WASM targets files, keeping each `netX.0` on a compatible archive set:
>    - `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets`
>    - `binding/HarfBuzzSharp.NativeAssets.WebAssembly/buildTransitive/HarfBuzzSharp.targets`
>    - `binding/IncludeNativeAssets.SkiaSharp.targets`
>    - `binding/IncludeNativeAssets.HarfBuzzSharp.targets`
>
> Convention for the conditions: the **newest** entry stays open-ended (`VersionGreaterThanOrEquals(TFV, 'A')`) so a future SDK compatible with the same archives needs no code change (e.g. .NET 9 and .NET 10 both select 3.1.56). Only when a new SDK requires different archives do you close the previous entry with an upper bound (`… and VersionLessThan(TFV, 'B')`) and add a new open-ended entry for the new version — the way `net9.0`–`net10.x` was capped at `< 11.0` once .NET 11 selected 6.0.2. Packaging and Uno inclusion retain version wildcards; clean CI builds supply only the versions produced by the current build matrix.

#### Finding .NET's Emscripten version

The upstream toolchain pin is **`EmsdkVersion` in `dotnet/runtime/eng/Versions.props`**:

- [Current development branch](https://github.com/dotnet/runtime/blob/main/eng/Versions.props) — search the file for `EmsdkVersion`.
- [.NET 11 release branch](https://github.com/dotnet/runtime/blob/release/11.0/eng/Versions.props) — replace `release/11.0` with the release branch being investigated, or use the exact runtime tag/commit for a reproducible lookup.
- For an installed SDK/workload, inspect `<dotnet-root>/sdk-manifests/<sdk-feature-band>/microsoft.net.workload.emscripten.current/<manifest-version>/WorkloadManifest.json`. The pack IDs include the toolchain version, for example `Microsoft.NET.Runtime.Emscripten.6.0.2.Sdk.win-x64`. Installed packs appear under `<dotnet-root>/packs/`; check the manifest selected by the workload, not merely every installed pack, since several toolchains can coexist.

Do not confuse `EmsdkVersion` with `MicrosoftNETRuntimeEmscriptenVersion`: the latter is the .NET workload package version, not the Emscripten compiler version. `dotnet workload list` identifies installed workloads but does not directly report the compiler version.

The historical investigation on **2026-10-01** found that [.NET 12 development at this commit](https://github.com/dotnet/runtime/blob/2c79aa97ff29df5e321b484460322283b1332fa4/eng/Versions.props) and the .NET 11 release branch both pin **6.0.2**. It rebuilt both libraries with Emscripten 6.0.2 (`st,simd`) and verified fresh .NET 11 RC2 Mono browser apps using source targets and local native-assets packages: Skia pixel output, managed stream callbacks, and HarfBuzz shaping passed in Chrome. That is historical evidence, not execution by the MSBuild regression test; other variants and .NET 12 runtime execution were not verified.

For the Windows RC1 regression on **2026-10-04**, the installed SDK was
`11.0.100-rc.1.26425.128`, while the actual selected compiler pack was
`Microsoft.NET.Runtime.Emscripten.6.0.2.Sdk.win-x64/11.0.0-rc.1.26425.128`.
The pack version is **not** the SDK version. The compiler inside that nominal
6.0.2 pack reports `emcc 6.0.3` (commit
`6ea9c28c38cdd40c1032fa04400c9d16230ee180`); record the downstream compiler
revision separately from the manifest's toolchain identity.
An unchanged real-package consumer
selected SkiaSharp 3.1.56 and HarfBuzzSharp 5.0.6 archives and failed in emcc
with undefined `saveSetjmp` / `testSetjmp`; selecting genuine 6.0.2 archives
for both families made the same default Mono native-link test pass.

#### Published workload/toolchain history

The **2026-10-01 historical audit** inspected 156 published Emscripten manifest
package versions, 70 RC2 daily manifest versions, and an installed RC2 manifest.
The table describes each workload's current target, not every legacy target
installed beside it. These are research records, not permission to add restore
or installation feeds beyond the repository's approved sources.

| .NET target / release stage | Emscripten | Manifest version(s) |
|---|---|---|
| .NET 6, published `Manifest-6.0.100` packages | 2.0.23 | `6.0.0-preview.7.21377.2` through `6.0.36` (33 packages) |
| .NET 7, published `net7.Manifest-7.0.100` packages | 3.1.12 | `7.0.0-rc.1.22424.1` through `7.0.20` (22 packages) |
| .NET 8 Preview 1-2 | 3.1.12 | `8.0.0-preview.1.23101.1`, `8.0.0-preview.2.23127.1` |
| .NET 8 Preview 3 | 3.1.30 | `8.0.0-preview.3.23172.2` |
| .NET 8 Preview 4-7, RC1-2 | 3.1.34 | `8.0.0-preview.4.23258.2` through `8.0.0-rc.2.23473.3` |
| .NET 8 GA / servicing | 3.1.34 | `8.0.0` through `8.0.31` (31 packages) |
| .NET 9 Preview 1-6 | 3.1.34 | `9.0.0-preview.1.24072.2` through `9.0.0-preview.6.24327.1` |
| .NET 9 Preview 7, RC1-2 | 3.1.56 | `9.0.0-preview.7.24373.5` through `9.0.0-rc.2.24468.8` |
| .NET 9 GA / servicing | 3.1.56 | `9.0.0` through `9.0.20` (21 packages) |
| .NET 10 Preview 1-7, RC1-2 | 3.1.56 | `10.0.0-preview.1.25077.1` through `10.0.100-rc.2.25502.107` |
| .NET 10 GA / servicing | 3.1.56 | `10.0.100` through `10.0.112` (13 packages) |
| .NET 11 Preview 1-5 | 3.1.56 | Exact versions below |
| .NET 11 Preview 6 | 5.0.6 | `11.0.100-preview.6.26359.118` |
| .NET 11 Preview 7, RC1 | 6.0.2 | `11.0.100-preview.7.26381.103`, `11.0.100-rc.1.26425.128` |
| .NET 11 RC2 daily builds | 6.0.2 | All 70 available RC2 manifests; installed `11.0.100-rc.2.26475.136` also checked |
| .NET 12 development | 6.0.2 | Source pin only; no .NET 12 workload execution verified |

The public manifest package ID is `Microsoft.NET.Workload.Emscripten.Current.Manifest-<SDK feature band>`, including the preview suffix when present (for example, [the Preview 6 package](https://www.nuget.org/packages/Microsoft.NET.Workload.Emscripten.Current.Manifest-11.0.100-preview.6/11.0.100-preview.6.26359.118)). Its `data/WorkloadManifest.json` contains the `packs` / `alias-to` entries naming the compiler, such as `Microsoft.NET.Runtime.Emscripten.5.0.6.Sdk.win-x64`. Older .NET 6/7 packages use the names shown in the table instead of `Current`.

**Workload-set version is a separate number.** Inspect `Microsoft.NET.Workloads.<SDK feature band>` and its workload-set JSON to find the Emscripten manifest version, then inspect that manifest to find the compiler version. A .NET runtime version or SDK version alone does not identify every pack in an independently pinned workload set.

| .NET 11 stage | Workload-set package version | Emscripten manifest version | Emscripten |
|---|---|---|---|
| Preview 1 | `11.100.0-preview.1.26109.8` | `11.0.100-preview.1.26104.118` | 3.1.56 |
| Preview 2 | `11.100.0-preview.2.26160.1` | `11.0.100-preview.2.26159.112` | 3.1.56 |
| Preview 3 | `11.100.0-preview.3.26214.1` | `11.0.100-preview.3.26207.106` | 3.1.56 |
| Preview 4 | `11.100.0-preview.4.26261.2` | `11.0.100-preview.4.26230.115` | 3.1.56 |
| Preview 5 | `11.100.0-preview.5.26309.3` | `11.0.100-preview.5.26302.115` | 3.1.56 |
| Preview 6 | `11.100.0-preview.6.26364.2` | `11.0.100-preview.6.26359.118` | 5.0.6 |
| Preview 7 | `11.100.0-preview.7.26410.2` | `11.0.100-preview.7.26381.103` | 6.0.2 |
| RC1 | `11.100.0-rc.1.26458.5`, `11.100.0-rc.1.26460.1` | `11.0.100-rc.1.26425.128` | 6.0.2 |
| RC2 installed daily set | `11.0.100-rc.2.26478.2` | `11.0.100-rc.2.26475.136` | 6.0.2 |

The public **NuGet package version** differs from the **CLI workload-set
version**. For RC1, `dotnet workload install wasm-tools --version 11.0.100-rc.1.26458.5` resolves package version
`11.100.0-rc.1.26458.5`. Use the former in
`DOTNET_WORKLOAD_VERSION_PREVIEW`; passing the package version to `--version`
is rejected by the SDK. The RC2 row records the installed CLI version, not a
published NuGet package version.

The installed RC2 set above comes from `sdk-manifests/11.0.100-rc.2/workloadsets/11.0.100-rc.2.26478.2/microsoft.net.workloads.workloadset.json`. It is a daily build, not a published RC2 release. Its `Current` manifest selects 6.0.2 for net11; its separate `net6`, `net7`, `net8`, `net9`, and `net10` manifests still select 2.0.23, 3.1.12, 3.1.34, 3.1.56, and 3.1.56 respectively. One installed workload set can therefore contain several toolchains; inspect the manifest for the application's target.

#### Upstream version-file changes

This is the historical audit of `src/mono/wasm/emscripten-version.txt` and its successor `src/mono/browser/emscripten-version.txt`, through the .NET 12 source snapshot above. Development commits do not necessarily correspond to a published preview; use the manifest tables for shipped workloads.

| Date (UTC) | Change | Upstream evidence |
|---|---|---|
| 2021-03-17 | Version file introduced at 2.0.12 | [dotnet/runtime#45545](https://github.com/dotnet/runtime/pull/45545) |
| 2021-06-02 | 2.0.12 to 2.0.21 | [dotnet/runtime#52870](https://github.com/dotnet/runtime/pull/52870) |
| 2021-06-24 | 2.0.21 to 2.0.23 | [dotnet/runtime#53603](https://github.com/dotnet/runtime/pull/53603) |
| 2022-02-03 | 2.0.23 to 2.0.34 | [dotnet/runtime#62499](https://github.com/dotnet/runtime/pull/62499) |
| 2022-02-18 | Reverted 2.0.34 to 2.0.23 | [dotnet/runtime#65517](https://github.com/dotnet/runtime/pull/65517) |
| 2022-03-22 | 2.0.23 to 3.1.1 | [dotnet/runtime#63894](https://github.com/dotnet/runtime/pull/63894) |
| 2022-03-28 | 3.1.1 to 3.1.7 | [dotnet/runtime#67006](https://github.com/dotnet/runtime/pull/67006) |
| 2022-07-09 | 3.1.7 to 3.1.12 | [dotnet/runtime#70693](https://github.com/dotnet/runtime/pull/70693) |
| 2023-03-11 | 3.1.12 to 3.1.30 | [dotnet/runtime#81215](https://github.com/dotnet/runtime/pull/81215) |
| 2023-04-17 | 3.1.30 to 3.1.34 | [dotnet/runtime#83998](https://github.com/dotnet/runtime/pull/83998) |
| 2024-07-15 | 3.1.34 to 3.1.56 | [dotnet/runtime#100334](https://github.com/dotnet/runtime/pull/100334) |
| 2026-06-25 | 3.1.56 to 5.0.6 | [dotnet/runtime#129299](https://github.com/dotnet/runtime/pull/129299) |
| 2026-07-15 | 5.0.6 to 6.0.2 | [dotnet/runtime#130631](https://github.com/dotnet/runtime/pull/130631) |

The file moved from `wasm` to `browser` in [dotnet/runtime#95940](https://github.com/dotnet/runtime/pull/95940) on 2023-12-19 without a version change. `EmsdkVersion` was added to `eng/Versions.props` at 3.1.34 in [dotnet/runtime#100266](https://github.com/dotnet/runtime/pull/100266) on 2024-05-09; tracing only that property misses earlier history.

#### Why the 5.0.6 archives can pass the RC2 smoke test

SkiaSharp added 5.0.6 builds in [#4459](https://github.com/mono/SkiaSharp/pull/4459), then explicitly compiled **both SkiaSharp and HarfBuzzSharp** with `WASM_LEGACY_EXCEPTIONS=0` in [#4487](https://github.com/mono/SkiaSharp/pull/4487). This selects the newer `try_table` / `throw_ref` exception instructions rather than relying on the compiler's defaults, avoiding mixed legacy/new exception instructions in the final module.

That was deliberate code-generation compatibility work, not proof that different Emscripten versions are generally interchangeable. The product now builds/selects **6.0.2 archives for both libraries** for net11.0+, with the required exception mode, threading, and SIMD variants; native-link and browser-runtime validation must use those newly compiled archives. TFM `net11.0` alone cannot distinguish Preview 6's 5.0.6 workload from Preview 7/RC1/RC2's 6.0.2 workload. Earlier .NET 11 previews are not supported by this archive selection. The historical 5.0.6-on-6.0.2 smoke result does not validate the new archives.

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

- [ ] `nuget.config` — Keep only the approved dotnet-public + dotnet-eng sources; do not add install-time source overrides

> **Note:** `nuget.org` is not an approved package source. Use only the existing approved sources in `nuget.config`, including for local validation and workload installation. Missing packages are a provisioning blocker; request mirroring to an approved feed rather than adding or overriding sources.

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

For opt-in CI jobs, `installPreviewSdk: true` installs `DOTNET_VERSION_PREVIEW`
side-by-side and selects it in the job's `global.json`, without changing the
repository's stable pin. The bootstrapper then installs
`DOTNET_WORKLOAD_VERSION_PREVIEW` using the shared workload installer.
An empty `previewWorkloads` installs its host-supported defaults; an explicit
comma-separated list overrides them. Linux defaults exclude unsupported Apple
workloads. This provisioning option does not shift the repository's TFM chain.

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

Workloads are pinned via the `DOTNET_WORKLOAD_VERSION` pipeline variable, which is passed to `install-dotnet-workloads.ps1` as `-WorkloadSetVersion`. Preview jobs use `DOTNET_WORKLOAD_VERSION_PREVIEW`. This uses the .NET SDK workload sets feature (`dotnet workload install --version <version>`) for reproducible builds.

**Why not use `workloadVersion` in `global.json`?** Native builds (which skip SDK/workload install) still read `global.json`. If the pinned workload version isn't pre-installed on the agent, the build fails immediately. By passing the version through the pipeline variable, we control when workload pinning applies.

**Exception:** Tizen is not an official workload — it uses Samsung's custom install scripts from `Samsung/Tizen.NET` repository.

## CI Troubleshooting

### Reusing Native Artifacts
Native artifacts are reused automatically through content-based caching: `scripts/infra/caching/repo-deps.py` hashes the native inputs and `Cache@2` restores a matching build, which sets `CACHE_SKIP` and skips native compilation. Nothing needs to be set by hand, and there is no way to point a run at an arbitrary previous build ID — artifacts are only ever downloaded from the current run or from an exact connected pipeline run.

### SDK Version Mismatch
If CI agents don't have the exact SDK version in `global.json`, use `"rollForward": "latestPatch"` to accept any patch version in the same feature band (e.g., `10.0.100` accepts `10.0.102`).

### Workload Install Failures
If `dotnet workload restore` fails with "no project found", the pipeline uses explicit `dotnet workload install` with a list of workloads instead.
