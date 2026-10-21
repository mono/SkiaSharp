# Building with Visual Studio 2026

Visual Studio 2026, MSVC v145 and Windows SDK 28000 are the Windows build
baseline. Open `source/SkiaSharpSource.slnx` to let Visual Studio apply
`source/.vsconfig`, or configure an existing VS 2026 installation from PowerShell:

```powershell
./scripts/infra/native/windows/select-vs.ps1
./scripts/infra/native/windows/install-components.ps1 -InstallationPath $env:VS_INSTALL
```

`select-vs.ps1` selects an installed VS 2026 IDE or Build Tools product. It does
not install Visual Studio or require components. `install-components.ps1`
detects the selected product and applies the matching profile.

The profiles install:

- MSVC v145 for x86, x64 and ARM64, including matching Spectre libraries
- matching preview compiler/Spectre components used by VS 2026 Insiders images
- LLVM, CMake and Ninja from Visual Studio Installer
- Visual Studio IDE's bundled Python 3.9 runtime for native build scripts
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

Git, PowerShell, the SDK pinned by `global.json`, and repository .NET workloads
remain separate prerequisites. Visual Studio IDE's Python component is out of
support and is used only for repository native build scripts because the internal
VS 2026 image cannot download a newer runtime. Build Tools does not offer that
component and requires a separately provisioned Python. NuGet, Skia dependency
synchronization, and ANGLE's revision-pinned GN, LLVM and resource-compiler
downloads require network access.

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

`Validate - Windows Components` is the isolated profile test. It may remove
Visual Studio from a disposable GitHub runner and install a clean Build Tools
instance. It validates both profiles on x86, x64 and ARM64 by building
Skia/HarfBuzz, WinUI and ANGLE. Its x64 jobs also run a D3D11 WARP smoke test
and the unfiltered .NET 10 console suite.

The console suite provisions the repository's pinned Mesa and SwiftShader
drivers for WGL and Vulkan. Those test drivers are not Visual Studio components.
