# Building with Visual Studio 2026

Keep the VS 2022 developer profile in `source/.vsconfig` unchanged. For an existing
full Visual Studio 2026 installation, apply the version-specific profile through
Visual Studio Installer:

```powershell
./scripts/infra/native/windows/install-components.ps1 -VisualStudioVersion 2026
```

Use `-InstallationPath` to select a particular installation. The script verifies
that it is an 18.x IDE and that every declared component is installed. The
`-BuildTools` profile remains VS 2022-only.

The VS 2026 profile preserves MSVC v143 14.44 and its matching x86/x64/ARM64
Spectre libraries. It uses the version-specific UWP v143 component and Windows
SDKs 22621 and 26100; VS 2026 does not offer the old 19041 SDK. Use an explicit
SDK override for native builds rather than requiring that older SDK.

Select the Installer-provided LLVM and Ninja instead of installing standalone
copies:

```powershell
$env:VS_INSTALL = 'C:\Program Files\Microsoft Visual Studio\18\Enterprise'
$env:LLVM_HOME = "$env:VS_INSTALL\VC\Tools\Llvm\x64"
$env:NINJA_EXE = "$env:VS_INSTALL\Common7\IDE\CommonExtensions\Microsoft\CMake\Ninja\ninja.exe"
dotnet tool restore
dotnet cake native/windows/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.26100.0
dotnet cake native/winui/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.22621.0
dotnet cake native/winui-angle/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.26100.0
```

Initialize `externals/skia` before building and provide the SDK pinned by
`global.json`, the MAUI Windows workload, Python, Git and PowerShell. Prefer the
VS Installer SDK/MAUI components; the validation workflow inventories and uses
the hosted image's existing .NET SDK and Python without running their standalone
installers. The Installer's optional Python 3.9 component is out of support and
is deliberately not requested.

Dependency restoration still needs the network: NuGet, Skia's `git-sync-deps`,
and ANGLE's existing revision-pinned Chromium LLVM/resource compiler downloads
are not Visual Studio Installer components. The ANGLE build retains those
upstream tools, rather than silently substituting unrelated compiler revisions.
Chromium 6275 predates VS 2026: the build adapts its toolchain scripts to pass
the selected v143 version to `vcvarsall` and locate the installed ARM64 v143
redistributable, instead of selecting the latest v145 redistributable by mistake.
Missing components or unexpected upstream script contents fail explicitly.

`Validate - Windows Components` validates VS 2026 on `windows-2025-vs2026` for
x86, x64 and ARM64, alongside the existing VS 2022 checks. It exports before/after
Installer configurations and tool provenance, builds native Skia/HarfBuzz,
WinUI/projection and ANGLE, and builds the managed Windows integrations and
source benchmark on x64. VS 2026 uses the repository SDK for WinUI rather than
the temporary SDK downgrade required by MSBuild 17.
