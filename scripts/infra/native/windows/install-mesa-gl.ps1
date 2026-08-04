# Provisions Mesa 3D as the system OpenGL driver so the ganesh-gl tests execute
# and golden-compare on agents with no GPU, mirroring what the Linux leg does
# with Mesa under Xvfb and what install-vulkan-icd.ps1 does for Vulkan.
#
# Windows Server agents have no display driver with an OpenGL ICD, so
# opengl32.dll falls back to the in-box generic implementation: OpenGL 1.1, no
# WGL_ARB_pixel_format, no WGL_ARB_pbuffer. WglContext cannot come up on that,
# and Ganesh needs GL 3.0+ regardless.
#
# The fix is the deployment Mesa itself documents for "systems without any
# OpenGL drivers" (https://docs.mesa3d.org/drivers/llvmpipe.html#windows):
#
#   * libgallium_wgl.dll -- the gallium megadriver, which is a real ICD --
#     becomes %SystemRoot%\System32\mesadrv.dll (x64) and SysWOW64\mesadrv.dll
#     (x86), so Windows resolves the bitness the same way it does for any system
#     DLL. The .NET Framework leg runs the suite once per bitness.
#   * it is registered under OpenGLDrivers\MSOGL, the fallback identifier
#     opengl32.dll consults when the display device names no ICD. WOW6432Node is
#     the view a 32-bit opengl32.dll sees as plain SOFTWARE\Microsoft.
#
# Registering an ICD rather than dropping an app-local opengl32.dll matters:
# gdi32's ChoosePixelFormat/SetPixelFormat only reach a *registered* driver.
#
# Any failure here is fatal, so a broken deployment surfaces in this step rather
# than as a wall of failed tests later. This checks that the driver is installed
# and is the version we just pinned -- proving it can actually render is the
# test suite's job, and GRContextTest.CreateDefaultContextIsValid does exactly
# that on every leg where GpuPolicy requires ganesh-gl. An agent that genuinely
# cannot run OpenGL must be declared with SKIASHARP_TEST_SKIP_GPU (see
# documentation/dev/gpu-test-policy.md), never inferred from a load failure.

$ErrorActionPreference = 'Stop'

# Pinned mesa-dist-win release + the SHA-256 of that immutable GitHub asset.
# Update the version and its hash together (this is CI config, bumped manually).
$mesaVersion = '26.1.3'
$mesaSha     = '6dd431f4620cea73970b13e3ffa94f721f2a3924306b8a4283c97648cdb6eb9c'

# softpipe, not llvmpipe: llvmpipe's LLVM shader JIT segfaults compiling the
# fragment shader Skia generates for a runtime blender, taking the whole test
# host with it. Reproduced on Mesa 25.2 (Linux, LLVM 20) and 26.1 (Windows,
# LLVM 22); softpipe runs the identical tests green on both, and its GL 3.3 is
# well past the 3.0 Ganesh needs. See #4604. The Linux leg pins the same driver.
$galliumDriver = 'softpipe'

# ---------------------------------------------------------------------------
# Download and stage.
# ---------------------------------------------------------------------------

$repoRoot = if ($env:BUILD_SOURCESDIRECTORY) {
    $env:BUILD_SOURCESDIRECTORY
} else {
    (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
}
# Staged alongside the other downloaded native deps (externals/angle,
# externals/vulkan-icd), not output/ which is for artifacts we build ourselves.
$dest = Join-Path $repoRoot 'externals\mesa-gl'
$tempDir = Join-Path $dest '_download'
New-Item -ItemType Directory -Force -Path $dest, $tempDir | Out-Null

$archiveName = "mesa3d-$mesaVersion-release-msvc.7z"
$uri = "https://github.com/pal1000/mesa-dist-win/releases/download/$mesaVersion/$archiveName"
$archive = Join-Path $tempDir $archiveName

Write-Host "Downloading Mesa $mesaVersion ..."
& (Join-Path $PSScriptRoot '..\shared\download-file.ps1') -Uri $uri -OutFile $archive

$actualSha = (Get-FileHash -Algorithm SHA256 -Path $archive).Hash.ToLowerInvariant()
if ($actualSha -ne $mesaSha) {
    throw "SHA-256 mismatch for $archiveName : expected $mesaSha, got $actualSha"
}

# The archive is 7-Zip. tar.exe is bsdtar/libarchive, in-box since Windows 10
# 1803, and reads 7-Zip when it was built with liblzma -- which is not guaranteed
# on every image, so fall back to 7-Zip itself, which the Windows agent images
# carry. Failing both is fatal rather than silent.
$extractDir = Join-Path $tempDir 'extracted'
if (Test-Path $extractDir) { Remove-Item $extractDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $extractDir | Out-Null

$extractors = @(
    @{ Name = 'tar';   Path = (Join-Path $env:SystemRoot 'System32\tar.exe'); Args = @('-xf', $archive, '-C', $extractDir) },
    @{ Name = '7-Zip'; Path = 'C:\Program Files\7-Zip\7z.exe';                Args = @('x', $archive, "-o$extractDir", '-y') }
)

$extracted = $false
foreach ($extractor in $extractors) {
    if (-not (Test-Path $extractor.Path)) {
        Write-Host "  $($extractor.Name) is not present at $($extractor.Path); trying the next extractor."
        continue
    }

    & $extractor.Path @($extractor.Args) | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Extracted $archiveName with $($extractor.Name)."
        $extracted = $true
        break
    }

    Write-Host "  $($extractor.Name) could not read the archive (exit $LASTEXITCODE); trying the next extractor."
    Remove-Item (Join-Path $extractDir '*') -Recurse -Force -ErrorAction SilentlyContinue
}

if (-not $extracted) {
    throw "No available extractor could unpack $archiveName. Tried: $(($extractors | ForEach-Object { $_.Name }) -join ', ')."
}

# ---------------------------------------------------------------------------
# Install the megadriver as an ICD, per bitness.
# ---------------------------------------------------------------------------

$targets = @(
    @{ Arch = 'x64'; SystemDir = (Join-Path $env:SystemRoot 'System32'); Key = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\OpenGLDrivers\MSOGL' },
    @{ Arch = 'x86'; SystemDir = (Join-Path $env:SystemRoot 'SysWOW64'); Key = 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows NT\CurrentVersion\OpenGLDrivers\MSOGL' }
)

foreach ($t in $targets) {
    if (-not (Test-Path $t.SystemDir)) {
        throw "Missing system directory: $($t.SystemDir)"
    }

    $src = Join-Path $extractDir "$($t.Arch)\libgallium_wgl.dll"
    if (-not (Test-Path $src)) {
        throw "Mesa $mesaVersion has no $($t.Arch)\libgallium_wgl.dll payload."
    }

    $archDest = Join-Path $dest $t.Arch
    New-Item -ItemType Directory -Force -Path $archDest | Out-Null
    Copy-Item -Path $src -Destination $archDest -Force

    Copy-Item -Path $src -Destination (Join-Path $t.SystemDir 'mesadrv.dll') -Force
    Write-Host "Installed the $($t.Arch) Mesa megadriver into $($t.SystemDir)\mesadrv.dll"

    # The d3d12 gallium driver refuses to load without DirectX IL beside it, and
    # Mesa's own system-wide deployment installs it for exactly that reason. It
    # costs a megabyte and keeps that driver available as a fallback.
    $dxil = Join-Path $extractDir "$($t.Arch)\dxil.dll"
    if (Test-Path $dxil) {
        Copy-Item -Path $dxil -Destination (Join-Path $t.SystemDir 'dxil.dll') -Force
    }

    if (-not (Test-Path $t.Key)) { New-Item -Path $t.Key -Force | Out-Null }
    New-ItemProperty -Path $t.Key -Name 'DLL'           -PropertyType String -Value 'mesadrv.dll' -Force | Out-Null
    New-ItemProperty -Path $t.Key -Name 'DriverVersion' -PropertyType DWord  -Value 1 -Force | Out-Null
    New-ItemProperty -Path $t.Key -Name 'Flags'         -PropertyType DWord  -Value 1 -Force | Out-Null
    New-ItemProperty -Path $t.Key -Name 'Version'       -PropertyType DWord  -Value 2 -Force | Out-Null
    Write-Host "Registered the $($t.Arch) ICD under $($t.Key)"
}

Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue

# ---------------------------------------------------------------------------
# Verify the deployment: the driver is where opengl32 will look for it, it is
# the build we just pinned rather than a leftover, and the registration reads
# back. Whether it renders is the suite's business, not this script's.
# ---------------------------------------------------------------------------

foreach ($t in $targets) {
    $installed = Join-Path $t.SystemDir 'mesadrv.dll'
    if (-not (Test-Path $installed)) {
        throw "The $($t.Arch) Mesa driver is not at $installed after installing it."
    }

    $version = (Get-Item $installed).VersionInfo
    Write-Host "  [$($t.Arch)] $installed -> $($version.ProductName) $($version.FileVersion)"

    if ($version.FileVersion -notlike "$mesaVersion*") {
        throw "The $($t.Arch) driver at $installed reports version '$($version.FileVersion)', not the pinned Mesa $mesaVersion."
    }

    $registered = Get-ItemProperty -Path $t.Key -Name 'DLL' -ErrorAction SilentlyContinue
    if ($registered.DLL -ne 'mesadrv.dll') {
        throw "The $($t.Arch) ICD registration under $($t.Key) reads '$($registered.DLL)' rather than mesadrv.dll."
    }
    Write-Host "  [$($t.Arch)] registered as the MSOGL OpenGL driver"
}

# Select the driver for every later step in the job. LIBGL_ALWAYS_SOFTWARE keeps
# a layered driver on a software device instead of hunting for hardware that is
# not there; the Linux leg sets the same pair.
Write-Host "##vso[task.setvariable variable=GALLIUM_DRIVER]$galliumDriver"
Write-Host "##vso[task.setvariable variable=LIBGL_ALWAYS_SOFTWARE]1"

Write-Host "Mesa $mesaVersion ($galliumDriver) provisioned for x64 and x86; ganesh-gl will execute."
