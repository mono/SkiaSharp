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
 * [Windows Device Tests](#windows-device-tests)
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
- [Visual Studio 2026](https://visualstudio.microsoft.com/vs/)
   - Open `source/SkiaSharpSource.slnx` to install the components in `source/.vsconfig`
   - The profile includes the .NET desktop, MAUI and UWP workloads, and the required Windows SDK

**macOS Dependencies:**
- macOS supported by the selected Xcode
- [Xcode](https://developer.apple.com/xcode/) matching the installed Apple workloads; see [Xcode 27](#xcode-27)
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

## Windows Device Tests

On Windows, run the MAUI device test app after preparing the native libraries:

```powershell
dotnet cake --target=tests-windows --skipExternals=all
```

This builds and runs `SkiaSharp.Tests.Devices` for
`net10.0-windows10.0.19041.0` as an MSIX x64 app with a self-contained
Windows App SDK runtime. `dotnet test` uses DeviceRunners and its bundled
winapp tool to register and launch the loose MSIX build output; no separate
publish or certificate-signing step is required. It writes TRX results under
`output/logs/testlogs/SkiaSharp.Tests.Devices.windows/`.
The **Windows (MAUI)** CI job runs the same target and publishes test results
and visual-regression images.

## Native Building

### Dependencies

In addition to a few extra dependencies, the [Managed-Only build dependencies](#dependencies) are still required.

**Windows Dependencies:**
 - [Managed-Only build dependencies](#dependencies)
 - [Python 3](https://www.python.org/downloads/)
    - Make sure the path to `python` is in the `PATH` environment variable
 - [Visual Studio 2026](https://visualstudio.microsoft.com/vs/)
    - Open `source/SkiaSharpSource.slnx` to install the components in `source/.vsconfig`
    - Or configure an existing installation from PowerShell:

```powershell
./scripts/infra/native/windows/select-vs.ps1
./scripts/infra/native/windows/install-components.ps1
```

The profiles include MSVC v145 and matching Spectre libraries for x86, x64 and
ARM64, LLVM, CMake, Ninja, and the required Windows SDK and managed workloads.
Use `--vsinstall` or `VS_INSTALL` to select a specific installation. MSVC v145
supports Windows 10 and Server 2016 or later.

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

### Xcode 27

Xcode 27.0 requires macOS 26.6 or later. For managed Apple applications, use
.NET SDK 10.0.401, workload set **10.0.401.1** (Apple packs **27.0.10722**)
and MAUI **10.0.110** or a compatible later servicing release. These are separate
version pins; installing the SDK alone does not update workloads. See the
[Apple workload release](https://github.com/dotnet/macios/releases/tag/dotnet-10.0.1xx-xcode27.0-10722).
Xcode 27.1 is not interchangeable with 27.0.

To keep the machine's shared SDK and workloads unchanged, provision a
repository-local SDK, then install the required workloads into that SDK:

```sh
use_installed_dotnet_cli=false ./eng/common/dotnet.sh --info
./.dotnet/dotnet workload install ios tvos macos maccatalyst maui \
  --version 10.0.401.1 --configfile nuget.config
```

Allow sufficient free space for the SDK, workload packs, Skia checkout and native
build outputs. Select Xcode per process rather than changing `xcode-select`:

```sh
export DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer
xcodebuild -version
./.dotnet/dotnet workload --info
./.dotnet/dotnet cake --target=externals-macos --arch=arm64
./.dotnet/dotnet cake --target=externals-ios
./.dotnet/dotnet cake --target=externals-tvos
./.dotnet/dotnet cake --target=externals-maccatalyst --arch=arm64
```

The iOS and tvOS packaging targets require the complete device and simulator
architecture set, including Intel simulators. Omit `--arch=arm64` from the
macOS/Catalyst commands to include their Intel slices. Native deployment
minimums are macOS 12, iOS 15, tvOS 15 and Mac Catalyst 15. Catalyst 14 maps to
macOS 11, which Xcode 27 rejects; Catalyst 15 maps to macOS 12. These minimums apply
to both native libraries and cannot be lowered by an application's managed TFM.
The .NET 10 Apple 27 application runtime additionally requires macOS 14 and
Catalyst 17; iOS/tvOS simulator applications require version 16 or later.
An SDK version, a deployment minimum and the build host OS are different settings.

After rebuilding natives, rebuild the test hosts and run the complete suite so
their output directories contain the newly built libraries:

```sh
./.dotnet/dotnet test tests/SkiaSharp.Tests.Console.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

Do not use `externals-download` for native changes or disable Xcode-version
validation to work around an incompatible workload. Existing .NET 9 targets
still need their matching older Apple toolchain; Xcode 27 has no .NET 9 Apple
workload. The existing CI Xcode 26.3/workload 10.0.202 path is retained for that
coverage; use the local pairing above to validate Xcode 27 without changing the
CI pins. The current macOS 15/Sequoia CI images cannot run Xcode 27.

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