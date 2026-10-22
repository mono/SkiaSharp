# Building with Visual Studio 2026

Visual Studio 2026, MSVC v145 and Windows SDK 28000 are the Windows build
baseline. Open `source/SkiaSharpSource.slnx` to let Visual Studio apply
`source/.vsconfig`, or configure an existing VS 2026 installation from PowerShell:

```powershell
./scripts/infra/native/windows/select-vs.ps1
./scripts/infra/native/windows/install-components.ps1
```

`select-vs.ps1` selects an installed VS 2026 IDE or Build Tools product. It does
not install Visual Studio or require components. It prefers Enterprise,
Professional, Community and then Build Tools. `install-components.ps1` runs
Visual Studio Installer once with the selected installation and profile.

The profiles install:

- MSVC v145 for x86, x64 and ARM64, including matching Spectre libraries
- matching preview compiler/Spectre components used by VS 2026 Insiders images
- LLVM, CMake and Ninja from Visual Studio Installer
- Windows SDKs 22621, 26100 and 28000
- managed desktop, MAUI, Android, MSIX and UWP build components

## Native builds

Initialize `externals/skia`, restore the repository tools, and run:

```powershell
dotnet tool restore
dotnet cake native/windows/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.28000.0
dotnet cake native/winui/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.22621.0
dotnet cake native/winui-angle/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.28000.0
```

Git, PowerShell, Python 3.12, the SDK pinned by `global.json`, and repository
.NET workloads remain separate prerequisites. Python must be installed outside
Visual Studio. NuGet, Skia dependency synchronization, and ANGLE's
revision-pinned GN, LLVM and resource-compiler downloads require network access.

## Compatibility

MSVC v145 supports Windows 10 and Server 2016 or later. It does not support
Windows 7, 8 or 8.1. Installing SDK 28000 does not by itself raise an
application's minimum OS. Native WinUI continues to declare Windows 10 build
17763 as its minimum, but deployment on that oldest build is not validated here.

ANGLE is pinned in `scripts/VERSIONS.txt`. Its build uses the GN revision from
ANGLE's `DEPS`, generates Windows App SDK ABI headers from the requested SDK,
and verifies each DLL's embedded PDB filename. Required generation fails when
the pinned upstream layout changes.

## CI validation

Normal Windows CI selects the VS 2026 product already installed on the agent and
applies the matching profile. It does not install an IDE.
