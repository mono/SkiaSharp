# Building SkiaSharp

This guide covers everything you need to build SkiaSharp from source, whether you're making changes to managed code or working with native libraries.

## Table of Contents

- [Preparation](#preparation)
- [Managed-Only Building](#managed-only-building)
- [Native Building](#native-building)
- [Generating Documentation](#generating-documentation)

## Preparation

Building SkiaSharp is straightforward once you have the necessary dependencies installed. You'll need to fork and clone the repository:

```bash
git clone https://github.com/mono/SkiaSharp
cd SkiaSharp
```

There are different approaches depending on whether you need to build native code or just work with managed assemblies.

## Managed-Only Building

In many cases, you only need to fix bugs or add features in the managed C# code. If this is your case, you can download pre-built native binaries from CI and work from there.

### Dependencies

**Windows:**
- Windows 10 or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with:
  - .NET desktop development
  - Universal Windows Platform development
  - .NET Multi-platform App UI development
  - Individual components:
    - .NET Framework 4.6.2 SDK
    - .NET Framework 4.6.2 targeting pack
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later (8.0.304+ recommended)
- Cake .NET Core Tool:
  ```bash
  dotnet tool install -g cake.tool
  ```

**macOS:**
- macOS 12.0 or later
- [Xcode 15.4](https://developer.apple.com/xcode/) or later
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later (8.0.304+ recommended)
- Cake .NET Core Tool:
  ```bash
  dotnet tool install -g cake.tool
  ```

**Linux:**
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later (8.0.304+ recommended)
- Build essentials (gcc, g++, make)
- Required development libraries:
  ```bash
  sudo apt-get install libfontconfig1-dev libglu1-mesa-dev
  ```
- Cake .NET Core Tool:
  ```bash
  dotnet tool install -g cake.tool
  ```

### Downloading Pre-Built Native Libraries

Download the latest master build artifacts:

```bash
dotnet cake --target=externals-download
```

To use a specific build from a commit SHA:

```bash
dotnet cake --target=externals-download --gitSha=<commit-sha>
```

To use the latest from a specific branch:

```bash
dotnet cake --target=externals-download --gitBranch=<branch-name>
```

### Making Changes

After downloading the native libraries, you can start working on the managed code:

1. Open the main solution:
   - **Visual Studio:** `source/SkiaSharpSource.sln`
   - **VS Code/Rider:** Individual platform solutions in `source/`

2. For testing, you can use:
   - `tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln` - Desktop tests
   - Individual test projects in `tests/`

### Building and Testing

Build and test everything using Cake:

```bash
# Build managed code only (skip native builds)
dotnet cake --target=libs --skipExternals=all

# Run tests
dotnet cake --target=tests --skipExternals=all

# Build samples
dotnet cake --target=samples --skipExternals=all

# Build NuGet packages
dotnet cake --target=nuget --skipExternals=all

# Build everything
dotnet cake --target=everything --skipExternals=all
```

## Native Building

To build the native `libSkiaSharp` libraries, you'll need additional dependencies beyond the managed-only requirements.

### Additional Dependencies

**Windows:**
- All [Managed-Only dependencies](#dependencies)
- [Python 3.x](https://www.python.org/downloads/)
  - Add Python to PATH or set `PYTHON_EXE` environment variable
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with:
  - Desktop development with C++
  - Windows 10 SDK (10.0.19041.0 or later)
  - VC++ 2019 or later toolset
  - Individual components:
    - C++ ARM64 build tools
    - C++ ARM build tools
- [Android NDK r25+](https://developer.android.com/ndk/downloads)
  - Set `ANDROID_NDK_ROOT` environment variable
- [OpenJDK 17+](https://adoptium.net/)
- LLVM/Clang 15+:
  ```powershell
  .\scripts\install-llvm.ps1
  ```
  - Set `LLVM_HOME` environment variable

**macOS:**
- All [Managed-Only dependencies](#dependencies)
- [Xcode 15.4](https://developer.apple.com/xcode/) or later with command line tools
- [Python 3.x](https://www.python.org/downloads/)
- [Android NDK r25+](https://developer.android.com/ndk/downloads)
  - Set `ANDROID_NDK_ROOT` environment variable

**Linux:**
- All [Managed-Only dependencies](#dependencies)
- Python 3.x
- Clang 15+
- Make
- Additional development libraries:
  ```bash
  sudo apt-get install python3 clang ninja-build
  ```

### Building Native Libraries

Build all native libraries:

```bash
# Build all native libraries for current platform
dotnet cake --target=externals
```

Build for specific platforms:

```bash
# Windows native
dotnet cake --target=externals-windows

# macOS native
dotnet cake --target=externals-mac

# Linux native
dotnet cake --target=externals-linux

# Android
dotnet cake --target=externals-android

# iOS
dotnet cake --target=externals-ios
```

### Build Output

Native libraries are output to:
- `output/native/<platform>/<architecture>/`

For example:
- Windows x64: `output/native/windows/x64/libSkiaSharp.dll`
- Linux x64: `output/native/linux/x64/libSkiaSharp.so`
- macOS ARM64: `output/native/macos/arm64/libSkiaSharp.dylib`

## Generating Documentation

To generate API documentation:

```bash
# Download latest docs output from CI
dotnet cake --target=docs-download-output

# Or use a specific build/branch
dotnet cake --target=docs-download-output --gitSha=<commit-sha>
dotnet cake --target=docs-download-output --gitBranch=<branch-name>

# Update/generate documentation
dotnet cake --target=update-docs
```

The generated documentation will be in the `docs/` submodule directory.

## Common Build Issues

### Missing Dependencies

If you get errors about missing tools:
1. Ensure all required SDKs and tools are installed
2. Restart your terminal/IDE after installing tools
3. Check environment variables are set correctly

### Out of Disk Space

Native builds require significant disk space:
- Native build artifacts: ~5-10 GB per platform
- Full repository with all outputs: ~20-30 GB

### Build Fails on Linux

Common issues:
1. Missing development packages - install required `-dev` packages
2. Wrong compiler version - ensure you have a modern gcc/clang
3. FontConfig issues - install `libfontconfig1-dev`

## Next Steps

- [Building on Linux](building-on-linux.md) - Detailed Linux build guide
- [Creating New Libraries](creating-new-libraries.md) - Adding new platform libraries
- [Architecture Overview](../../design/architecture-overview.md) - Understanding SkiaSharp's structure
- [Adding New APIs](../../design/adding-new-apis.md) - Contributing new bindings

## Related Documentation

- [Contributing Guide](../contributing/README.md)
- [Adding New APIs](../../design/adding-new-apis.md)
- [Design Documentation](../../design/)
