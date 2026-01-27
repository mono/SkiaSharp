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
 * [Generating Documentation](#generating-documentation)

## Prerequisites

Before building SkiaSharp, ensure you have:

- **.NET 8 SDK** - The repository uses `global.json` to pin the SDK version
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
- **.NET 8 SDK** - Pinned via `global.json`
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

If you need a specific build, you can specify the commit SHA from the git history:

```
> dotnet cake --target=externals-download --gitSha=<git-sha>
```

If you want the latest from a specific branch, you can also pass the branch name:

```
> dotnet cake --target=externals-download --gitBranch=<git-branch>
```


### Making Changes

Once that is complete, you should be able to now start working on some code. You can open the `source/SkiaSharpSource.sln` solution (or one of the platform variants) and start making changes. If you are going to be working with unit tests, or don't need to work on all the platform projects, you can open the `tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln` solution.

The **`SkiaSharpSource.sln`** solution is primarily for working with platform-specific bits, and then you can compile to make sure everything is working. The **`SkiaSharp.Desktop.Tests.sln`** solution is for testing that changes to the API are still working as expected.

### Building

Once you are finished making changes, you can run the `tests` target and make sure that the tests will pass on CI. There is also the `samples` and `nuget` targets. By adding the `--skipExternals=all` argument, you can let the bootstrapper know that it should _not_ build any native bits, but rather use the bits that were downloaded.

```
> dotnet cake --target=everything --skipExternals=all
```

## Native Building

### Dependencies

In addition to a few extra dependencies, the [Managed-Only build dependencies](#dependencies) are still required.

**Windows Dependencies:**
 - [Managed-Only build dependencies](#dependencies)
 - [Python 3](https://www.python.org/downloads/)
    - Make sure the path to `python` is in the `PATH` environment variable
 - [Visual Studio 2022+](https://visualstudio.microsoft.com/vs/)
    - Desktop development with C++
       - Windows 10/11 SDK (latest)
       - MSVC v143+ C++ build tools
    - Individual components
       - C++ compilers and libraries for ARM64
       - Android NDK (via Visual Studio Installer or [manually](https://developer.android.com/ndk/downloads))
          - Make sure the path to the root is in the `ANDROID_NDK_ROOT` or `ANDROID_NDK_HOME` environment variables
 - [OpenJDK 17+](https://adoptium.net/)
 - Clang/LLVM
    - Run `.\scripts\install-llvm.ps1`
    - Set `LLVM_HOME` to the path of the install

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
dotnet cake --target=externals-ios-simulator --arch=arm64

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

## Generating Documentation

```
dotnet cake --target=docs-download-output [--gitSha=<git-sha> | --gitBranch=<git-branch>]
dotnet cake --target=update-docs
```