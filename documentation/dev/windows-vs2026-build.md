# Building with Visual Studio 2026

Visual Studio 2026 (18.x), MSVC v145 and matching Spectre libraries are the Windows
build baseline. Apply `source/.vsconfig` to an existing IDE through Visual Studio
Installer:

```powershell
./scripts/infra/native/windows/install-components.ps1
```

Use `-InstallationPath` to select a particular installation. The script verifies
that it is an 18.x IDE and that every declared component is installed. The
`-BuildTools` switch selects the SKU-specific `build-tools.vsconfig`. To install a
fresh VS 2026 Build Tools instance, also supply a new `-InstallationPath`.

Both profiles request the latest x86/x64/ARM64 compiler and matching Spectre
libraries, LLVM, CMake/Ninja, and Windows SDKs 22621, 26100 and 28000. Native
HarfBuzz and ANGLE use SDK 28000; the existing WinUI projection uses 22621.
An installed SDK version does not itself set the minimum runtime OS. MSVC v145
supports Windows 10 / Server 2016 and later, not Windows 7, 8 or 8.1.
The WinUI project's declared minimum remains Windows 10 build 17763; API usage
and dependency requirements can impose a higher minimum. Hosted validation does
not verify deployment on that oldest declared Windows build.
There is no VS 2022/v143 compiler fallback or .NET SDK downgrade.
The native WinUI project uses C++20 standard coroutines; the deprecated C++17
`/await` mode is not enabled.

Select the Installer-provided LLVM and Ninja instead of installing standalone
copies:

```powershell
./scripts/infra/native/windows/select-vs.ps1
dotnet tool restore
dotnet cake native/windows/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.28000.0
dotnet cake native/winui/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.22621.0
dotnet cake native/winui-angle/build.cake --configuration=Release --arch=x64 `
  --vsinstall="$env:VS_INSTALL" --windowsSdkVersion=10.0.28000.0
```

Initialize `externals/skia` before building and provide the SDK pinned by
`global.json`, the MAUI Windows workload, Python, Git and PowerShell. Prefer the
VS Installer SDK/MAUI components; the validation workflow inventories and uses
the hosted image's existing .NET SDK and Python without running their standalone
installers. The Installer's optional Python 3.9 component is out of support and
is deliberately not requested.

Dependency restoration still needs the network: NuGet, Skia's `git-sync-deps`,
and ANGLE's revision-pinned GN and Chromium LLVM/resource compiler downloads
are not Visual Studio Installer components. ANGLE's resource preprocessing still
requires its own LLVM even when MSVC compiles the library. Those upstream tools
are retained rather than substituted with unrelated compiler revisions.
`scripts/VERSIONS.txt` pins both `chromium/8099` and its exact source revision.
Its upstream toolchain supports VS 2026/v145 and SDK 28000 without compatibility
patches. The SkiaSharp hardening config and DLL PDB naming adjustment are applied
before generating Ninja files. The latter changes the linker's `/PDB` argument
and GN's declared outputs together: renaming after linking would leave the DLL's
embedded debug reference pointing to `.dll.pdb`. The build checks that each DLL
references its packaged `.pdb` filename. Import libraries retain upstream names.
Windows App SDK header generation includes the local transitive IDL imports
of UI/Graphics APIs, including Microsoft.Foundation; MIDL processing an imported
IDL does not itself generate that import's header. Required generation fails
rather than creating stubs. WINMD conversion writes ABI IDLs into a separate
directory before replacing package IDLs; pre-existing source MIDL 3 files are
not valid conversion stamps or inputs for the ABI-header generation step.

GN is not a Visual Studio Installer component or a documented preinstalled tool
on the hosted VS 2026 image. Official latest Windows binaries are available from
Google's build infrastructure, but GN has no universal stable release or promised
backward compatibility. The default follows the revision in ANGLE's `DEPS`, as
recommended by GN's own distribution guidance, rather than an untracked system
binary. This is a reproducibility choice, not a different ANGLE-specific GN fork.

`Validate - Windows Components` validates VS 2026 on `windows-2025-vs2026` for
x86, x64 and ARM64, for both Build Tools and the full IDE. It exports before/after
Installer configurations and tool provenance, builds native Skia/HarfBuzz,
WinUI/projection and ANGLE, and builds the managed Windows integrations and
source benchmark on x64. The x64 jobs also exercise ANGLE with D3D11 WARP and
run the unfiltered .NET 10 console solution. Missing required GPU capabilities
are failures, not implicit skips.
The console suite uses WGL and Vulkan independently of ANGLE's D3D11 WARP
smoke test. Its x64 validation reuses the Windows test pipeline's pinned Mesa
OpenGL and SwiftShader Vulkan provisioning. These software drivers are not
available through Visual Studio Installer; downloads are hash-verified by the
existing repository helpers.

The public native Azure image is `windows.vs2026.amd64.open` and the internal
image is `windows.vs2026.amd64`. Hosted Windows tests and source benchmarks use
`windows-2025-vs2026`. The shared Azure bootstrapper provisions VS components and
exports Installer-provided LLVM/Ninja paths instead of installing standalone
LLVM, 7-Zip, Ninja or old Windows SDKs. Existing .NET workload provisioning remains
necessary for repository-pinned SDK/workload combinations across platforms.
