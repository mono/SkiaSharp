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
- **MAUI workload** - Required for mobile platform targets:
  ```bash
  dotnet workload install maui
  ```
- **Cake .NET Tool** - For running build scripts:
  ```bash
  dotnet tool install -g cake.tool
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
- **MAUI workload** - `dotnet workload install maui`
- **Cake .NET Tool** - `dotnet tool install -g cake.tool`

**Windows Dependencies:**
- Windows 10/11
- [Visual Studio 2022+](https://visualstudio.microsoft.com/vs/)
   - .NET desktop development
   - .NET Multi-platform App UI development (MAUI)
   - Universal Windows Platform development
- Windows 10 SDK (latest)

**macOS Dependencies:**
- macOS 12+ (Monterey or later)
- [Xcode](https://developer.apple.com/xcode/) (latest stable)
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
 - [OpenJDK 17+](https://adoptium.net/)
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
 - OpenJDK 17+

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
targets. The normal, unfiltered suite builds desktop, WASM, and real MAUI
applications. It needs the SDK pinned by `global.json`, `wasm-tools`, the
host-supported MAUI workloads, JDK 21, and the Android SDK. macOS also needs
the pinned Xcode (currently 26.3), and Windows needs the WinUI XAML build
toolchain. No submodules, GPU, browser, emulator, device, UI execution, or native
source build is required. These tests inspect build outputs without running apps.

Download the `nuget` artifact from one exact completed SkiaSharp CI build and
place its packages in `output/nugets`. Record the build URL/commit when reporting
results. Do not combine different builds or substitute published packages for
missing artifacts. Both families require their core, `NativeAssets.Win32`,
`NativeAssets.macOS`, and `NativeAssets.Linux` packages for the desktop tests.
The WASM test additionally requires both `NativeAssets.WebAssembly` packages.
MAUI requires `SkiaSharp.HarfBuzz`, `SkiaSharp.Views.Maui.Controls`,
`SkiaSharp.Views.Maui.Core`, and `SkiaSharp.Views`, plus both families'
`NativeAssets.Android` packages on every host. Windows additionally requires
`SkiaSharp.Views.WinUI` and `SkiaSharp.NativeAssets.WinUI`; macOS additionally
requires both families' `NativeAssets.iOS` and `NativeAssets.MacCatalyst`.
All transitive SkiaSharp/HarfBuzzSharp dependencies must also be present in that
same artifact set. Missing packages, workloads, or platform tools fail tests;
they never trigger skips or public-package fallback.
Package versions are read from their nuspec metadata, not inferred from the
checkout.

Run the CI entry point from the repository root:

```sh
dotnet cake --target=tests-msbuild
```

Install `wasm-tools,maui-android` on all hosts; add `maui-windows` on Windows
or `maui-ios,maui-maccatalyst` on macOS. CI installs these using the repository
workload installer and the stable workload set (currently `10.0.202`).
The bootstrapper's existing `installPreviewSdk` / `previewWorkloads` options
also provision the preview SDK and `wasm-tools` up front using the shared
preview version variables. Although this pins the job's `global.json` to the
preview SDK, the first test pass explicitly selects `DOTNET_VERSION` for
its consumers; the second WASM-only pass selects `DOTNET_VERSION_PREVIEW`.
The test executable still targets net10.0.
Provision the Android SDK/API 36 and JDK 21 as well. The shared JDK installer
currently selects JDK 17, so these jobs select the hosted JDK 21 before building
.NET 10 Android applications. Unsupported host TFMs are
explicitly omitted from MAUI theory data, not skipped during execution:

| Host | Stable net10 consumer coverage | Stable test count |
| --- | --- | --- |
| Windows | Desktop, Mono WASM, Android APK, unpackaged WinUI executable | 25 |
| macOS | Desktop, Mono WASM, Android APK, iOS simulator app, Mac Catalyst app | 26 |
| Linux | Desktop, Mono WASM, Android APK | 24 |

To run only the WASM native-link regression (for example, a second SDK pass),
install `wasm-tools` under the selected consumer SDK and use:

```sh
dotnet cake --target=tests-msbuild --wasm=true
# Use a specific installed SDK (including preview SDKs):
dotnet cake --target=tests-msbuild --wasm=true --consumerSdkVersion=11.0.100-rc.1.26425.128
```

The runner remains net10.0, while `ConsumerSdkVersion` pins the generated
consumer's `global.json` and selects its target framework (SDK 10 -> net10.0,
SDK 11 -> net11.0). An SDK preview is permitted only when that exact SDK was
selected. A direct filtered invocation is also available:

```sh
dotnet test tests/SkiaSharp.Tests.MSBuild/SkiaSharp.Tests.MSBuild.csproj \
  -p:PackageDirectory=/absolute/path/to/nugets \
  -p:ConsumerSdkVersion=11.0.100-rc.1.26425.128 \
  -- --filter-trait Category=Wasm --report-trx --results-directory /absolute/path/to/test-results
```

For an SDK installed outside the runner's dotnet root, also pass
`-p:ConsumerDotNetHost=/absolute/path/to/dotnet`. CI uses the published RC1 SDK
and workload set `11.0.100-rc.1.26458.5`. The consumer's Microsoft dependencies
must be available on `dotnet-public`; unpublished daily SDKs can require
additional feeds and are not supported by this fixture's restore configuration.
Workload provisioning uses the existing approved sources in `nuget.config`
without overrides. MAUI restores also accept the SDK-injected local
`library-packs` directory; no additional network feed is allowed.
Missing workload packages must be mirrored to an approved
feed; do not add NuGet.org as a workaround. On 2026-10-03, RC1 workload
provisioning through the approved feeds and the default Mono WASM package
regression both passed locally on Windows after the missing workload-set,
manifest, and Emscripten packages were mirrored.

**macOS RC1 provisioning blocker (2026-10-03):** the
`Microsoft.NET.Runtime.Emscripten.6.0.2.{Python,Sdk,Node,Cache}.osx-{x64,arm64}`
packages at version `11.0.0-rc.1.26425.128` are absent from both `dotnet-public`
and `dotnet-eng`. Mirror the four packages for the CI host's SDK architecture
to an approved feed before provisioning its RC1 `wasm-tools` workload.
The Tests pipeline's default `macos-15` host is Intel/x64 and requires these
four exact package IDs, all at `11.0.0-rc.1.26425.128`:

- `Microsoft.NET.Runtime.Emscripten.6.0.2.Python.osx-x64`
- `Microsoft.NET.Runtime.Emscripten.6.0.2.Sdk.osx-x64`
- `Microsoft.NET.Runtime.Emscripten.6.0.2.Node.osx-x64`
- `Microsoft.NET.Runtime.Emscripten.6.0.2.Cache.osx-x64`

Windows/Linux equivalents have been mirrored. macOS net11 coverage remains
required and must fail provisioning until these packages are available; do
not remove coverage, add source overrides, or fall back to a different toolchain.

Or run the test project directly against a local artifact directory:

```sh
dotnet test tests/SkiaSharp.Tests.MSBuild/SkiaSharp.Tests.MSBuild.csproj \
  -p:PackageDirectory=/absolute/path/to/nugets \
  -- --report-trx --results-directory /absolute/path/to/test-results
```

`NativeAssetOutputTests.cs`, `WasmNativeAssetTests.cs`, and
`MauiNativeAssetTests.cs` contain the package references, scenarios, and
assertions. `Utils/DotNet.cs` handles isolated project creation and CLI execution;
`Utils/ArtifactPackage.cs` reads real package identities and hashes. Each fixture
has a private restore cache; every case has independent project,
intermediate, and output directories. Source mapping restricts SkiaSharp and
HarfBuzzSharp packages to the supplied artifacts, so missing packages cannot
fall back to public versions. User NuGet caches and input packages are not modified.

On Windows, Android's resource compiler and WinUI's XAML compiler can fail with
deeply nested consumer paths. For a long checkout path, pass
`-p:MSBuildTestArtifactsDirectory=<short-absolute-path>` to the direct test command.
The directory contains generated applications and retained build diagnostics.

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

`WasmNativeAssetTests.cs` builds a minimal Mono WebAssembly app in Debug with
both families' real packages and the SDK's default WASM flags. It checks the
resolved build properties (native build, exception handling, SIMD, no threads),
compares selected archive paths and hashes against the input packages, verifies
that the emcc link response names both families, and checks the linked WASM
header. This is native-link build coverage, not a trimmed publish/AOT or
browser-execution test. The SDK and packages, rather than the test, choose the
flags and archive variants. It does not claim CoreCLR WASM support.

`MauiNativeAssetTests.cs` generates an installed `dotnet new maui --no-restore`
application, preserves the platform entrypoints/manifests/resources, and
rewrites its project and shared source inside the isolated consumer directory.
The app registers the real `SKCanvasView` handler with `.UseSkiaSharp()` and
compiles drawing/shaping code against `SkiaSharp.HarfBuzz` and `HarfBuzzSharp`.
It builds one host-architecture RID per supported platform (`android-x64` or
`android-arm64`, `win-x64` or `win-arm64`, and matching iOS simulator/Mac Catalyst
RIDs). Platform native packages are selected through the real package graph,
not repository imports or explicit replacement assets.

MAUI assertions verify the exact RID restore target, all restored SkiaSharp/
HarfBuzzSharp identities and package hashes, and their local-artifact origin.
They also verify both families and the real MAUI integration assemblies were
compiler references. Android APKs must contain a manifest, DEX, and exactly one
native library per family for the selected ABI, with input-package hashes
(native stripping is disabled to preserve byte identity). Windows must produce
a PE app executable and the selected Win32 and WinUI native libraries, matching
package hashes. Apple builds must produce a native `.app` executable and both
families' Mach-O framework binaries, with `otool -L` evidence; selected cached
framework inputs match their package hashes. Final Apple binaries may be
thinned/signed by the platform build, so final hashes are not compared.
Debug application builds avoid release signing/AOT requirements, without
turning the application into a class library or bypassing native packaging.
The Windows consumer uses the packaged C++/WinRT projection; the separate
VS17/SDK `10.0.111` native-projection build guidance does not change its SDK.

TRX results, generated projects, command logs, binlogs, restore/dependency
metadata, and failed-consumer outputs are published from `output/logs/` in CI.
Successful desktop build outputs are removed after assertions; WASM and MAUI
outputs, emcc response files, and Apple link inspections are retained even on success. The consumer
diagnostics default to `output/logs/testlogs/msbuild`; override
`-p:MSBuildTestArtifactsDirectory=/absolute/path/to/diagnostics` for a direct run.
Private restore caches are not included in diagnostic artifacts.

The **MSBuild package tests** CI stage has exactly three jobs: Windows, macOS,
and Linux. Each runs the normal unfiltered .NET 10 desktop + WASM + MAUI suite,
then installs public SDK `11.0.100-rc.1.26425.128` and workload set
`11.0.100-rc.1.26458.5` with `wasm-tools`, and runs the same test project with
`--wasm=true --consumerSdkVersion=11.0.100-rc.1.26425.128`. This second pass has
one net11 WASM consumer test; the test runner and Cake remain net10. The RC1
workload install uses a separate `global.json` directory after the stable run,
so it never changes the repository SDK pin or switches SDKs before that run. In
combined CI it depends on `package`; in downstream Tests it depends on `prepare`
and downloads the exact SkiaSharp pipeline-resource run's artifact. It runs
alongside Samples without changing the prerequisites of existing source/unit/
device tests. Its failures are reported independently and still fail the pipeline.
The existing release/platform Integration suite remains a separate entry point.

## Documentation Outputs

Public API documentation is authored as `///` comments in managed source. A
managed build generates compiler XML and packages it beside matching `lib` and
`ref` assemblies. See [writing-docs.md](writing-docs.md) for the package
contract and supported package acquisition paths. The external
`mono/SkiaSharp-API-docs` repository owns ECMA/mdoc generation and Microsoft
Learn publication; this repository has no local API-reference generation
target.