## Table of Contents

 * [Preparation](#preparation)
 * [Managed-Only Building](#managed-only-building)
    * [Dependencies](#dependencies)
    * [Preparation](#preparation-1)
    * [Making Changes](#making-changes)
    * [Building](#building)
 * [Native Building](#native-building)
    * [Dependencies](#dependencies-1)
 * [Generating Documentation](#generating-documentation)

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

**Windows Dependencies:**
 - Windows 10 Fall Creators Update _(build 16299 / version 1709)_
 - [Visual Studio 2019+](https://visualstudio.microsoft.com/vs/)
    - .NET desktop development
    - Universal Windows Platform development
       - Windows 10 SDK (10.0.16299)
       - Windows 10 SDK (10.0.10240)
    - Mobile development with .NET
       - Android SDK platform for API level 19 _(KitKat / version 4.4)_
    - .NET Core cross-platform development
    - Individual components
       - .NET Framework 4.6.2 SDK
       - .NET Framework 4.6.2 targeting pack
 - [GTK# 2.12](https://xamarin.azureedge.net/GTKforWindows/Windows/gtk-sharp-2.12.45.msi)
 - Cake .NET Core Tool
    - Run `dotnet tool install -g cake.tool`

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
 - [Python 2.7](https://www.python.org/downloads/)
    - Make sure the path to `python` is in the `PATH` environment variable, or the full path to `python` is in the `PYTHON_EXE` environment variable.
 - [Visual Studio 2017+](https://visualstudio.microsoft.com/vs/)
    - Desktop development with C++
       - Windows 10 SDK (10.0.16299)
       - [Windows 10 SDK (10.0.10240)](https://go.microsoft.com/fwlink/p/?LinkId=619296)
       - Windows 8.1 SDK and UCRT SDK
       - VC++ 2015.3 v14.00 (v140) toolset for desktop
    - Individual components
       - Visual C++ compilers and libraries for ARM
       - Visual C++ compilers and libraries for ARM64
       - Visual C++ runtime for UWP
       - Android NDK r15+ or [manually](https://developer.android.com/ndk/downloads)
          - Make sure the path to the root is in the `ANDROID_NDK_ROOT` or `ANDROID_NDK_HOME` environment variables.
 - [OpenJDK 13.0.2+](http://jdk.java.net/archive/)
 - [Tizen Studio 3.6+ (either the IDE or the CLI)](https://developer.tizen.org/development/tizen-studio/download)
    - Using CLI: Install the `MOBILE-4.0` and `MOBILE-4.0-NativeAppDevelopment` packages
    - Using IDE: Install the `4.0 Mobile` | `Native app. development (IDE)`
    - Make sure the root is in the `TIZEN_STUDIO_HOME` environment variable, or at `~/tizen-studio`.
    - Tizen Studio requires Java 8/9/10 to be installed and available in the `PATH` environment variable.
 - Clang 9+
    - Run `.\scripts\install-llvm.ps1`
    - Set `LLVM_HOME` to the path of the install
 - nano-api-scan .NET Core Tool
    - Run `dotnet tool install -g nano-api-scan`

**Linux Dependencies**
 - Python 2
 - Clang 9+
 - Mono (completed)
 - MSBuild
 - Make
 - OpenJDK 13.0.2
 - Tizen Studio 3.6+

## Generating Documentation

```
dotnet cake --target=docs-download-output [--gitSha=<git-sha> | --gitBranch=<git-branch>]
dotnet cake --target=update-docs
```