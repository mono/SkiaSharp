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
 * [GTK4 Tests](#gtk4-tests)
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
- **Microsoft OpenJDK 21** - Required for .NET for Android builds. Set `JAVA_HOME`
  to the JDK home and place its `bin` directory first on `PATH`.

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

Linux cross-image fontconfig downloads retry transient transfer failures up to
three times, with a 30-second connection timeout and a 300-second limit per
attempt. Downloads must succeed before the existing pinned SHA-256 checks run;
exhausted retries fail the image build rather than continuing with a missing file.

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

## GTK4 Tests

Desktop CI runs the GTK4-native test suite only on Linux, where `libgtk-4-1`
and Xvfb are already provisioned. Windows and macOS continue running all core
and GPU test hosts without requiring an additional GTK4/libepoxy installation.
GTK4 tests run serially because GTK initialization and widget access are not
thread-safe. Initial-state assertions use APIs available in Ubuntu 22.04's GTK 4.6.

With the Linux native assets and GTK4 installed, run the existing suite under a
virtual display:

```sh
xvfb-run -a dotnet test tests/SkiaSharp.Views.Gtk4.Tests/SkiaSharp.Views.Gtk4.Tests.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

## MSBuild Package-Consumer Tests

`tests/SkiaSharp.Tests.MSBuild` tests real packed SkiaSharp and HarfBuzzSharp
packages using isolated .NET console consumers. It does not build the bindings,
load native libraries into the test runner, or use the repository's native-copy
targets. Install the stable and preview SDKs selected by `DOTNET_VERSION` and
`DOTNET_VERSION_PREVIEW` in `scripts/azure-templates-variables.yml`; no mobile workloads,
submodules, GPU, browser, native source build, or native runtime dependencies are
needed. These tests inspect build/publish output without executing native code.

Download the `nuget` artifact from one exact completed SkiaSharp CI build and
place its packages in `output/nugets`. Record the build URL/commit when reporting
results. Do not combine different builds or substitute published packages for
missing artifacts. Both families require their core, `NativeAssets.Win32`,
`NativeAssets.macOS`, and `NativeAssets.Linux` packages; package versions are read
from their nuspec metadata, not inferred from the checkout.

For local runs, select the configured preview SDK in `global.json` before running
either entry point below; keep the .NET 10 runtime installed for the test runner.
Run the CI entry point from the repository root:

```sh
dotnet cake --target=tests-msbuild
```

Or run the test project directly against a local artifact directory:

```sh
dotnet test tests/SkiaSharp.Tests.MSBuild/SkiaSharp.Tests.MSBuild.csproj \
  -p:PackageDirectory=/absolute/path/to/nugets \
  -- --report-trx --results-directory /absolute/path/to/test-results
```

`NativeAssetOutputTests.cs` contains the package references, scenarios, and
assertions. `Utils/DotNet.cs` handles isolated project creation and CLI execution.
The tests share one private restore cache; every case has independent project,
intermediate, and output directories. Source mapping restricts SkiaSharp and
HarfBuzzSharp packages to the supplied artifacts, so missing packages cannot
fall back to public versions. User NuGet caches and input packages are not modified.

Single-target theory inputs explicitly cover `net10.0` and `net11.0`, independently
of the selected SDK: eight default-package rows and 36 RID rows (44 total).
For each family and framework, build and publish first verify the default package
includes Win32/macOS native assets and **no Linux native assets**. With an explicit
`NativeAssets.Linux` reference, the nine-case matrix below verifies that Linux
assets are included and that RID selection behaves as expected.

For each family and framework, the suite tests `build`, `publish`, and
`publish -r linux-x64` against three project configurations:

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

Four additional theory cases cover each family with `build` and `publish` of a
real console project declaring `TargetFrameworks=net10.0;net11.0`, for **48 tests**
in total. The outer build omits `--framework` and `-o`, validating both normal
per-framework output trees. Publish selects each framework separately with
`--framework` and distinct output directories, then rechecks both trees to catch
cross-framework overwrites. These cases validate eight framework outputs,
including restored frameworks, runtime TFM, native paths, and package hashes.
Both publish invocations retain separate command, stdout/stderr, and binlog
diagnostics; cleanup preserves restore/dependency/runtime metadata per framework.

TRX results, generated projects, command logs, binlogs, restore/dependency
metadata, and failed-consumer outputs are published from `output/logs/` in CI.
Successful build outputs are removed after assertions. The consumer diagnostics
default to `output/logs/testlogs/msbuild`; override
`-p:MSBuildTestArtifactsDirectory=/absolute/path/to/diagnostics` for a direct run.
Private restore caches are not included in diagnostic artifacts.

The **MSBuild package tests** CI stage runs three jobs on Windows, macOS, and
Linux, each installing `DOTNET_VERSION` and `DOTNET_VERSION_PREVIEW`
side-by-side, without workloads.
The runner stays on `net10.0` using the stable runtime; all consumers use the
selected .NET 11 SDK but target `net10.0` and `net11.0` independently through
explicit theory inputs or a multi-target project. Each case checks the restored
framework and consumer runtime configuration as well as native paths and hashes.
SDK provisioning uses the shared preview version; these desktop jobs do not install workloads.
The exact selected SDK and dotnet host are captured in the runner's
runtime configuration, and every consumer pins that SDK with roll-forward disabled.
In combined CI it depends on `package`; in downstream Tests it depends on `prepare`
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