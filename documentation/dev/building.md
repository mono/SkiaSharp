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
- **Pinned workloads** - Use `DOTNET_WORKLOAD_VERSION` from the
  [build tool versions](../../scripts/azure-templates-variables.yml) in place of
  `VERSION`, then run the shared installer from the repository root:
  ```bash
  pwsh ./scripts/infra/managed/install-dotnet-workloads.ps1 -WorkloadSetVersion VERSION
  ```
- **OpenJDK 21** and the Android SDK - Required for Android targets. Use
  [`install-openjdk.ps1`](../../scripts/infra/managed/install-openjdk.ps1)
  to install the required JDK or reuse `JAVA_HOME_21_X64`.
- **Cake .NET Tool** - For running build scripts:
  ```bash
  dotnet tool install -g cake.tool
  ```

Use the approved sources in [`nuget.config`](../../nuget.config) for workload
installation. Request mirroring for missing packages rather than adding sources.

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
- A macOS version supported by the required Xcode
- [Xcode](https://developer.apple.com/xcode/) matching `XCODE_VERSION` in the
  [build tool versions](../../scripts/azure-templates-variables.yml)
- Command Line Tools: `xcode-select --install`
- For tvOS storyboard compilation, install the matching simulator runtime:
  ```bash
  xcodebuild -downloadPlatform tvOS -buildVersion "$(xcrun --sdk appletvsimulator --show-sdk-version)"
  ```
  See [additional Xcode components](https://developer.apple.com/documentation/xcode/downloading-and-installing-additional-xcode-components).

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
packages by building isolated desktop, WASM, and host-supported MAUI applications.
These tests inspect outputs without running the apps or loading native
libraries into the runner.

Install the [prerequisites](#prerequisites), including `wasm-tools`, MAUI
workloads and Android tools; Apple builds need Xcode, and Windows builds need
the WinUI toolchain. Download the complete `nuget` artifact from one SkiaSharp
CI build into `output/nugets`, then run from the repository root:

```sh
dotnet cake --target=tests-msbuild
```

To use another artifact directory or inspect test results, run the project
directly:

```sh
dotnet test tests/SkiaSharp.Tests.MSBuild/SkiaSharp.Tests.MSBuild.csproj \
  -p:PackageDirectory=/absolute/path/to/nugets \
  -- --report-trx --results-directory /absolute/path/to/test-results
```

Use packages from the same build, including transitive dependencies; do not
substitute public packages for missing artifacts. Restores use private caches
and restrict SkiaSharp/HarfBuzzSharp to the supplied artifacts.

The SDK selected when building the harness also selects the consumer SDK and
target frameworks. For an SDK outside the runner's dotnet installation, pass
`-p:ConsumerDotNetHost=/absolute/path/to/dotnet` and execute the built test
assembly with a host supporting its target framework.

Diagnostics are retained under `output/logs/testlogs/msbuild`. On Windows,
use short paths for `-p:MSBuildTestArtifactsDirectory=<absolute-path>` and
`TEMP`/`TMP` if platform tools hit path-length limits.

See the [MSBuild test stage](../../scripts/azure-templates-stages-msbuild.yml)
for CI configuration and the [test sources](../../tests/SkiaSharp.Tests.MSBuild)
for coverage.

**WASM native-link regression:**

For a WASM-only diagnostic, install `wasm-tools` and supply both families'
core and `NativeAssets.WebAssembly` packages. The test links a Mono WebAssembly
app with the SDK's default flags; it does not run the app in a browser.

```sh
dotnet cake --target=tests-msbuild --wasm=true
```

Run the unfiltered suite for final validation. Emscripten toolchain changes
require a native source build, not `externals-download`.

## Documentation Outputs

Public API documentation is authored as `///` comments in managed source. A
managed build generates compiler XML and packages it beside matching `lib` and
`ref` assemblies. See [writing-docs.md](writing-docs.md) for the package
contract and supported package acquisition paths. The external
`mono/SkiaSharp-API-docs` repository owns ECMA/mdoc generation and Microsoft
Learn publication; this repository has no local API-reference generation
target.