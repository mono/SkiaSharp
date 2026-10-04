# Building SkiaSharp

This guide covers building SkiaSharp on Windows and macOS.

## Table of Contents

 * [Prerequisites](#prerequisites)
 * [Preparation](#preparation)
 * [Managed-Only Building](#managed-only-building)
    * [Dependencies](#dependencies)
    * [Preparation](#preparation-1)
    * [Making Changes](#making-changes)
    * [Building](#building)
 * [Native Building](#native-building)
    * [Dependencies](#dependencies-1)
 * [MSBuild Package-Consumer Tests](#msbuild-package-consumer-tests)
 * [Documentation Outputs](#documentation-outputs)

## Prerequisites

Before building SkiaSharp, ensure you have:

- **.NET SDK pinned by the repository** - See `global.json` for the required version
- **Pinned workloads** - Use the shared installer from the repository root:
  ```bash
  pwsh ./scripts/infra/managed/install-dotnet-workloads.ps1 -WorkloadSetVersion 10.0.401
  ```
- **OpenJDK 21** and the Android SDK - Required for Android targets. The shared
  JDK installer defaults to Microsoft OpenJDK `21.0.10+7` and reuses an existing
  `JAVA_HOME_21_X64` installation.
- **Cake .NET Tool** - For running build scripts:
  ```bash
  dotnet tool install -g cake.tool
  ```

CI provisions SDK `10.0.401` with workload set `10.0.401`. Opt-in preview jobs
select SDK `11.0.100-rc.1.26425.128` and workload set
`11.0.100-rc.1.26458.5`. An empty `previewWorkloads` uses the shared installer's
host-supported defaults; an explicit comma-separated list overrides them.
Linux installs `android`, `wasm-tools`, and `maui-android`; Windows and macOS
also install `macos`, `ios`, `tvos`, `maccatalyst`, and aggregate `maui`.

Both pinned Apple workload sets require **Xcode 26.6 and macOS 26.2+ (Tahoe)**:
see the [.NET 10 release](https://github.com/dotnet/macios/releases/tag/dotnet-10.0.1xx-xcode26.5-10318)
and [.NET 11 RC1 release](https://github.com/dotnet/macios/releases/tag/dotnet-11.0.1xx-rc1-12193).
The .NET 10 release's `xcode26.5` tag does not describe its required Xcode.
Public managed Complete and Tests jobs use `AcesShared` with
`ImageOverride -equals ACES_VM_SharedPool_Tahoe`; internal managed Package jobs
use `macos-26`. Native jobs retain Sequoia / `macos-15` and Xcode 26.3.
Android provisioning installs platform APIs `21,35,36,37.0`. Android 37 uses
the SDK package ID `platforms;android-37.0`; `platforms;android-37` is not published.

Workload installation uses only the approved `dotnet-public` and `dotnet-eng`
sources in `nuget.config`. Missing packs are provisioning blockers: request
mirroring rather than adding sources or overriding the installer's feeds.

Validate the shared installers without changing installed tools or workloads:

```powershell
pwsh ./scripts/infra/managed/tests/Provisioning.Tests.ps1
```

## Preparation

Building a complete SkiaSharp is actually pretty simple, you just need to install a few dependencies. 

To get started with any type of development, you will have to fork and then clone SkiaSharp. If you are not going to be making changes, you can clone the main repository:

```
> git clone https://github.com/mono/SkiaSharp
```

Once the source is on your machine, you can get started with building. There are a few ways in which to get started, depending on what you are going to do.

## Managed-Only Building

In many cases, you just want to fix a bug in the managed code. If this is the case, you can just download the native bits from CI, and then work from there. 

### Dependencies

**All Platforms:**
- **.NET SDK pinned by the repository** - See `global.json` for the required version
- **Pinned workloads** - Use the shared installer described in [Prerequisites](#prerequisites)
- **Cake .NET Tool** - `dotnet tool install -g cake.tool`

**Windows Dependencies:**
- Windows 10/11
- [Visual Studio 2022+](https://visualstudio.microsoft.com/vs/)
   - .NET desktop development
   - .NET Multi-platform App UI development (MAUI)
   - Universal Windows Platform development
- Windows 10 SDK (latest)

**macOS Dependencies:**
- macOS 26.2+ (Tahoe) for the pinned Apple workloads
- [Xcode 26.6](https://developer.apple.com/xcode/)
- Command Line Tools: `xcode-select --install`

### Preparation

The latest master build bits can be downloaded by running the `externals-download` target:

```
> dotnet cake --target=externals-download
```

To use a promoted build from a specific branch, pass the branch name:

```
> dotnet cake --target=externals-download --gitBranch=<git-branch>
```


### Making Changes

Once that is complete, you should be able to now start working on some code. You can open the `source/SkiaSharpSource.slnx` solution (or one of the platform variants) and start making changes. If you are going to be working with unit tests, or don't need to work on all the platform projects, you can open the `tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.slnx` solution.

The **`SkiaSharpSource.slnx`** solution is primarily for working with platform-specific bits, and then you can compile to make sure everything is working. The **`SkiaSharp.Desktop.Tests.slnx`** solution is for testing that changes to the API are still working as expected.

### Building

Once you are finished making changes, you can run the `tests` target and make sure that the tests will pass on CI. There is also the `samples` and `nuget` targets. By adding the `--skipExternals=all` argument, you can let the bootstrapper know that it should _not_ build any native bits, but rather use the bits that were downloaded.

```
> dotnet cake --target=Everything --skipExternals=all
```

## Native Building

### Dependencies

In addition to a few extra dependencies, the [Managed-Only build dependencies](#dependencies) are still required.

**Windows Dependencies:**
 - [Managed-Only build dependencies](#dependencies)
 - [Python 3](https://www.python.org/downloads/)
    - Make sure the path to `python` is in the `PATH` environment variable
 - [Visual Studio 2022 or 2026](https://visualstudio.microsoft.com/vs/)
    - Desktop development with C++
       - Windows 10/11 SDK (latest)
       - MSVC v143 C++ build tools and matching Spectre-mitigated libraries for the architectures you build
    - Individual components
       - C++ compilers and libraries for ARM64
       - For WinUI native builds, C++ (v143) Universal Windows Platform tools from VS 2022
          - In VS 2022 Build Tools, select **WinUI application development build tools** and its optional C++ tools
       - Android NDK (via Visual Studio Installer or [manually](https://developer.android.com/ndk/downloads))
          - Make sure the path to the root is in the `ANDROID_NDK_ROOT` or `ANDROID_NDK_HOME` environment variables
 - [OpenJDK 21](https://learn.microsoft.com/java/openjdk/download)
 - Clang/LLVM
    - Run `.\scripts\install-llvm.ps1`
    - Set `LLVM_HOME` to the path of the install

If you have multiple Visual Studio installations, use `--vsinstall` or set
`VS_INSTALL` to select one with the v143 tools and matching Spectre libraries.
Use `--windowsSdkVersion` if you need a specific installed Windows SDK.

**macOS Dependencies:**
 - [Managed-Only build dependencies](#dependencies)
 - Xcode Command Line Tools
 - Python 3

**Linux Dependencies:**
 - Python 3
 - Clang 14+
 - Make
 - OpenJDK 21

### Building Native Libraries

Build native libraries for specific platforms using Cake targets:

```bash
# macOS (Apple Silicon)
dotnet cake --target=externals-macos --arch=arm64

# macOS (Intel)
dotnet cake --target=externals-macos --arch=x64

# iOS (device)
dotnet cake --target=externals-ios

# iOS Simulator
dotnet cake --target=externals-ios --arch=arm64

# Android (ARM64)
dotnet cake --target=externals-android --arch=arm64

# Android (x86_64 for emulator)
dotnet cake --target=externals-android --arch=x64

# Windows (x64)
dotnet cake --target=externals-windows --arch=x64

# Linux (requires Docker)
dotnet cake --target=externals-linux --arch=x64
```

> **Tip:** Native builds can take 10-30 minutes depending on your machine. Only build for platforms you need to test.

## MSBuild Package-Consumer Tests

`tests/SkiaSharp.Tests.MSBuild` tests real packed SkiaSharp and HarfBuzzSharp
packages using isolated .NET consumers. It does not build the bindings, load
native libraries into the test runner, or use the repository's native-copy
targets. The normal, unfiltered suite builds desktop and real MAUI applications.
It needs the SDK pinned by `global.json`, the host-supported MAUI workloads,
JDK 21, and the Android SDK. macOS also needs the pinned Xcode 26.6, and Windows
needs the WinUI XAML build toolchain. No submodules, GPU, browser, emulator,
device, UI execution, or native source build is required. These tests inspect
build outputs without running apps.

Download the `nuget` artifact from one exact completed SkiaSharp CI build and
place its packages in `output/nugets`. Record the build URL/commit when reporting
results. Do not combine different builds or substitute published packages for
missing artifacts. Both families require their core, `NativeAssets.Win32`,
`NativeAssets.macOS`, and `NativeAssets.Linux` packages for the desktop tests.
MAUI requires `SkiaSharp.HarfBuzz`, `SkiaSharp.Views.Maui.Controls`,
`SkiaSharp.Views.Maui.Core`, and `SkiaSharp.Views`, plus both families'
`NativeAssets.Android` packages on every host. Windows additionally requires
`SkiaSharp.Views.WinUI` and `SkiaSharp.NativeAssets.WinUI`; macOS additionally
requires both families' `NativeAssets.iOS` and `NativeAssets.MacCatalyst`.
All transitive SkiaSharp/HarfBuzzSharp dependencies must also be present in that
same artifact set. Missing packages, workloads, or platform tools fail tests;
they never trigger skips or public-package fallback. Package versions are read
from their nuspec metadata, not inferred from the checkout.

Run the CI entry point from the repository root:

```sh
dotnet cake --target=tests-msbuild
```

CI uses the shared workload installer's default host-supported list and pinned
workload set, without a `dotnetWorkloads` or `previewWorkloads` override. Stable
and preview are separate bootstrapper jobs on each host; preview jobs use the
existing `installPreviewSdk` option. Both SDKs run the same unfiltered desktop
and MAUI suite, including Android on every host. The SDK building the harness
selects the generated consumers' SDK: the existing `NETCoreSdkVersion` runtime
setting derives all consumer TFMs and the MAUI template's `--framework`.
Only SDK 10 and SDK 11 are supported; no separate consumer SDK version override
is required. The test executable itself still targets net10.0.

Android consumers target API 36 with SDK 10 and API 37 with SDK 11. Their minimum
OS versions are respectively 21 and 24, matching the selected Android workload.
Both SDKs use JDK 21 through the normal bootstrapper provisioning. Unsupported
host TFMs are omitted from MAUI theory data, not skipped during execution:

| Host | Stable net10 / preview net11 consumer coverage | Test count per SDK |
| --- | --- | --- |
| Windows | Desktop, Android APK, unpackaged WinUI executable | 24 |
| macOS | Desktop, Android APK, iOS simulator app, Mac Catalyst app | 25 |
| Linux | Desktop, Android APK | 23 |

Or run the test project directly against a local artifact directory:

```sh
dotnet test tests/SkiaSharp.Tests.MSBuild/SkiaSharp.Tests.MSBuild.csproj \
  -p:PackageDirectory=/absolute/path/to/nugets \
  -- --report-trx --results-directory /absolute/path/to/test-results
```

For a preview diagnostic, select the exact preview SDK in `global.json` in the
working directory used to build the harness. If it is installed outside the
runner's dotnet root, also pass `-p:ConsumerDotNetHost=/absolute/path/to/dotnet`.
Run the built net10.0 test assembly with a net10-capable host; the generated
consumers still use the SDK and host recorded at harness build time. Do not
build the same harness configuration concurrently with different SDK settings.

Consumer Microsoft dependencies must be available on `dotnet-public`; the
SDK-injected local `library-packs` directory is also allowed for MAUI restores.
Workload provisioning uses only `dotnet-public` and `dotnet-eng` from the
repository `nuget.config`. Missing packs are provisioning blockers: request
mirroring, not a NuGet.org fallback or source override.

`NativeAssetOutputTests.cs` and `MauiNativeAssetTests.cs` contain the package
references, scenarios, and assertions. `Utils/DotNet.cs` handles isolated
project creation and CLI execution; `Utils/ArtifactPackage.cs` reads real
package identities and hashes. Each fixture has a private restore cache;
every case has independent project, intermediate, and output directories.
Source mapping restricts SkiaSharp and
HarfBuzzSharp packages to the supplied artifacts, so missing packages cannot
fall back to public versions. User NuGet caches and input packages are not modified.

WinUI's XAML compiler still imposes `MAX_PATH` on referenced assemblies, even
with OS long paths enabled. Private caches use compact directories under the
host temp directory, rather than nesting package paths below the diagnostic
root. Fixture run IDs are compact too. On Windows with a long checkout path,
use `-p:MSBuildTestArtifactsDirectory=<short-absolute-path>` for generated
applications and diagnostics; `TEMP`/`TMP` can select a short private-cache
parent for a local diagnostic. Each fixture removes only its own cache on
disposal. Do not share caches with user restores or bypass package assertions.

For each family, build and publish first verify the default package includes
Win32/macOS native assets and **no Linux native assets**. With an explicit
`NativeAssets.Linux` reference, the nine-case matrix below verifies that Linux
assets are included and that RID selection behaves as expected.

For each family, the suite tests `build`, `publish`, and `publish -r linux-x64`
against three project configurations:

| Project configuration | Build/publish without a CLI RID | Publish with `-r linux-x64` |
| --- | --- | --- |
| No RID | All native variants under `runtimes/` | Linux x64 native assets beside the app |
| `RuntimeIdentifier=linux-arm64` | Linux arm64 native assets beside the app | Linux x64 overrides the project RID |
| `RuntimeIdentifiers=linux-x64;linux-arm64` | All native variants under `runtimes/` | Linux x64 native assets beside the app |

Plural `RuntimeIdentifiers` are restore targets, **not an output allow-list**.
The tests compare native paths and hashes with the actual input packages,
rather than accepting only a successful MSBuild exit code.
Linux assets are explicitly referenced; this suite does not change package
dependencies or filtering behavior. No fake packages or mock CLI are used.

`MauiNativeAssetTests.cs` generates an installed `dotnet new maui --no-restore`
application with the SDK-derived `--framework net10.0` or `net11.0`, preserves
the platform entrypoints/manifests/resources, and rewrites only the project and
shared source inside the isolated consumer directory. The app registers the
real `SKCanvasView` handler with `.UseSkiaSharp()` and compiles drawing/shaping
code against `SkiaSharp.HarfBuzz` and `HarfBuzzSharp`. It builds one
host-architecture RID per supported platform (`android-x64` or `android-arm64`,
`win-x64` or `win-arm64`, and matching iOS simulator/Mac Catalyst RIDs).
Platform native packages are selected through the real package graph, not
repository imports or explicit replacement assets.

MAUI assertions verify the exact RID restore target, all restored SkiaSharp/
HarfBuzzSharp identities and SHA512 package hashes, and their local-artifact
origin. They also verify both families and the real MAUI integration assemblies
were compiler references. Android APKs must contain a manifest, DEX, and exactly
one native library per family for the selected ABI, matching input-package
SHA256 hashes (native stripping is disabled to preserve byte identity).
Windows must produce a PE app executable and the selected Win32 and WinUI
native libraries with matching package hashes. Apple builds must produce a
native `.app` executable and both families' Mach-O framework binaries with
`otool -L` evidence; selected cached framework inputs match package hashes.
Final Apple binaries may be thinned/signed, so final hashes are not compared.
Debug builds avoid release signing/AOT requirements without turning the app
into a class library or bypassing native packaging. This is package/build
coverage, not execution of the app's native libraries.

TRX results, generated projects, command logs, binlogs, restore/dependency
metadata, and failed-consumer outputs are published from `output/logs/` in CI.
Successful desktop build outputs are removed after assertions; MAUI outputs
and Apple link inspections are retained even on success. The consumer diagnostics
default to `output/logs/testlogs/msbuild`; override
`-p:MSBuildTestArtifactsDirectory=/absolute/path/to/diagnostics` for a direct run.
Private restore caches are not included in diagnostic artifacts.

The **MSBuild package tests** CI stage has a three-host by two-SDK matrix:
Windows, macOS, and Linux, each with stable and preview jobs. The host loop uses
the existing agent objects and their `pool.os` values. Every job uses
`target: tests-msbuild` and the bootstrapper's normal SDK/workload provisioning.
Stable jobs run the unfiltered net10 desktop and MAUI suite; preview jobs use
`installPreviewSdk: true` and default host-supported preview workloads for the
identical unfiltered net11 consumers. There are no category filters, new skips,
custom SDK/workload install steps, JDK selection tasks, or extra Cake invocations.
In combined CI it depends on `package`; in downstream Tests it depends on `prepare`
and downloads the exact SkiaSharp pipeline-resource run's artifact. It runs
alongside Samples without changing the prerequisites of existing source/unit/
device tests. Its failures are reported independently and still fail the pipeline.
The existing release/platform Integration suite remains a separate entry point.

Preview CI verification remains blocked by approved-feed mirror gaps in the
full default workload list. Windows requires
`Microsoft.iOS.Windows.Sdk.net10.0_27.0` and
`Microsoft.MacCatalyst.Sdk.net10.0_27.0` at `27.0.10539-xcode27.0`. macOS also
requires the Emscripten 6.0.2 `Python/Sdk/Node/Cache.osx-{x64,arm64}` packs at
`11.0.0-rc.1.26425.128` for its SDK architecture. Those dependencies do not
change the pinned net11 Xcode selection. Targeted local Android/MAUI Windows
installations can diagnose consumer builds but do not validate CI's required
default workload installation. Keep CI coverage intact until the missing
packages are mirrored to an approved feed.

## Documentation Outputs

Public API documentation is authored as `///` comments in managed source. A
managed build generates compiler XML and packages it beside matching `lib` and
`ref` assemblies. See [writing-docs.md](writing-docs.md) for the package
contract and supported package acquisition paths. The external
`mono/SkiaSharp-API-docs` repository owns ECMA/mdoc generation and Microsoft
Learn publication; this repository has no local API-reference generation
target.