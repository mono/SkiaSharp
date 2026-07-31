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
# gdi32's ChoosePixelFormat/SetPixelFormat only reach a *registered* driver, so
# an app-local Mesa gets a pixel format it never learns about and every
# wglCreateContext afterwards returns NULL. Going through the ICD needs no
# test-code change at all -- Mesa simply becomes the machine's OpenGL.
#
# Any failure here is fatal, and the script proves the result by creating a real
# GL context in both bitnesses before it exits, so a broken deployment surfaces
# in this step instead of as a wall of failed tests two hours later. An agent
# that genuinely cannot run OpenGL must be declared with SKIASHARP_TEST_SKIP_GPU
# (see documentation/dev/gpu-test-policy.md), never inferred from a failure.

param (
    # Re-entry point used to prove a deployment: the script relaunches itself,
    # once per candidate driver and once under the in-box 32-bit PowerShell,
    # because a process can only ever bring up one OpenGL context. Neither
    # re-entry installs anything.
    [switch] $VerifyOnly,
    [string] $Driver = 'default'
)

$ErrorActionPreference = 'Stop'

# Pinned mesa-dist-win release + the SHA-256 of that immutable GitHub asset.
# Update the version and its hash together (this is CI config, bumped manually).
$mesaVersion = '26.1.3'
$mesaSha     = '6dd431f4620cea73970b13e3ffa94f721f2a3924306b8a4283c97648cdb6eb9c'

# Candidate gallium drivers, in the order they are tried; the first that brings
# a context up wins and is exported for the test step.
#
# softpipe leads, not llvmpipe: llvmpipe's LLVM shader JIT segfaults compiling
# the fragment shader Skia generates for a runtime blender, taking the whole
# test host with it. Reproduced on Mesa 25.2 (Linux, LLVM 20) and 26.1 (Windows,
# LLVM 22); softpipe runs the identical tests green on both. See #4604. The
# others are here so a host where softpipe cannot come up still has a chance,
# and so the log says what each one did.
$galliumDrivers = @('softpipe', 'llvmpipe', 'd3d12', 'default')

# ---------------------------------------------------------------------------
# The verifier: brings up a WGL context exactly as WglContext does, and reports
# what answered. Compiled into whichever bitness of PowerShell is running it.
# ---------------------------------------------------------------------------

$verifierSource = @'
using System;
using System.Runtime.InteropServices;

public static class MesaGlVerifier
{
    const string OGL = "opengl32.dll";

    [DllImport(OGL, SetLastError = true)] static extern IntPtr wglCreateContext(IntPtr hdc);
    [DllImport(OGL)] static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hrc);
    [DllImport(OGL)] static extern bool wglDeleteContext(IntPtr hrc);
    [DllImport(OGL)] static extern IntPtr wglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string n);
    [DllImport(OGL)] static extern IntPtr glGetString(uint name);

    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr CreateWindowExW(int ex, string cls, string name, int style,
        int x, int y, int w, int h, IntPtr parent, IntPtr menu, IntPtr inst, IntPtr param);
    [DllImport("gdi32.dll")] static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport("gdi32.dll", SetLastError = true)] static extern bool SetPixelFormat(IntPtr hdc, int fmt, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport(OGL)] static extern int wglChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport(OGL, SetLastError = true)] static extern bool wglSetPixelFormat(IntPtr hdc, int fmt, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport("gdi32.dll")] static extern int DescribePixelFormat(IntPtr hdc, int fmt, uint bytes, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern int GetModuleFileNameW(IntPtr module, [Out] char[] buffer, int size);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern bool GetModuleHandleExW(int flags, string name, out IntPtr module);

    [StructLayout(LayoutKind.Sequential)]
    struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize, nVersion;
        public uint dwFlags;
        public byte iPixelType, cColorBits, cRedBits, cRedShift, cGreenBits, cGreenShift, cBlueBits, cBlueShift;
        public byte cAlphaBits, cAlphaShift, cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits;
        public byte cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
        public uint dwLayerMask, dwVisibleMask, dwDamageMask;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    delegate IntPtr GetExtensionsStringArb(IntPtr dc);

    static string Str(uint v)
    {
        var p = glGetString(v);
        return p == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(p);
    }

    static string ModulePath(string name)
    {
        IntPtr module;
        if (!GetModuleHandleExW(0, name, out module) || module == IntPtr.Zero)
            return "<not loaded>";
        var buffer = new char[520];
        var n = GetModuleFileNameW(module, buffer, buffer.Length);
        return new string(buffer, 0, n);
    }

    /// <summary>
    /// Which OpenGL implementation actually answered. mesadrv.dll being loaded is
    /// the proof that the MSOGL registration took effect; the pixel-format count
    /// separates Mesa (four figures) from the in-box generic driver (a couple of
    /// dozen). Never throws -- this runs on the failure path too.
    /// </summary>
    public static string Diagnostics(IntPtr dc)
    {
        var report = "opengl32=" + ModulePath("opengl32.dll")
                   + "; mesadrv=" + ModulePath("mesadrv.dll")
                   + "; libgallium_wgl=" + ModulePath("libgallium_wgl.dll");

        if (dc != IntPtr.Zero)
        {
            var pfd = new PIXELFORMATDESCRIPTOR();
            var count = DescribePixelFormat(dc, 1, (uint)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR)), ref pfd);
            report += "; pixelFormats=" + count;
        }

        return report;
    }

    // Returns null on success, or a description of what went wrong.
    public static string Verify(out string vendor, out string renderer, out string version, out string extensions, out string diagnostics)
    {
        vendor = renderer = version = extensions = null;
        diagnostics = null;

        var hwnd = CreateWindowExW(0, "STATIC", "mesagl", 0, 0, 0, 8, 8,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (hwnd == IntPtr.Zero)
        {
            diagnostics = Diagnostics(IntPtr.Zero);
            return "CreateWindowEx failed (" + Marshal.GetLastWin32Error() + ")";
        }

        var dc = GetDC(hwnd);
        if (dc == IntPtr.Zero)
        {
            diagnostics = Diagnostics(IntPtr.Zero);
            return "GetDC failed";
        }

        var pfd = new PIXELFORMATDESCRIPTOR
        {
            nSize = (ushort)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR)),
            nVersion = 1,
            dwFlags = 0x00000004 /* PFD_DRAW_TO_WINDOW */ | 0x00000020 /* PFD_SUPPORT_OPENGL */,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
        };

        var format = ChoosePixelFormat(dc, ref pfd);
        diagnostics = Diagnostics(dc) + "; chosenFormat=" + format;
        if (format == 0)
            return "ChoosePixelFormat found no OpenGL pixel format";
        if (!SetPixelFormat(dc, format, ref pfd))
            return "SetPixelFormat failed (" + Marshal.GetLastWin32Error() + ")";

        var rc = wglCreateContext(dc);
        var path = "gdi32";
        if (rc == IntPtr.Zero)
        {
            // Same fallback the suite's Wgl bootstrap uses: GDI's SetPixelFormat
            // does not reach a Mesa ICD, so the driver never learns which format
            // was picked and wglCreateContext fails with ERROR_INVALID_PIXEL_FORMAT
            // (2000). opengl32's own wglSetPixelFormat does reach it. A format can
            // be set once per DC, so this needs a fresh window.
            var retryHwnd = CreateWindowExW(0, "STATIC", "mesagl2", 0, 0, 0, 8, 8,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (retryHwnd == IntPtr.Zero)
                return "wglCreateContext returned NULL (" + Marshal.GetLastWin32Error() + "), and the retry window could not be created";

            dc = GetDC(retryHwnd);
            format = wglChoosePixelFormat(dc, ref pfd);
            if (format == 0)
                return "wglChoosePixelFormat found no OpenGL pixel format";
            if (!wglSetPixelFormat(dc, format, ref pfd))
                return "wglSetPixelFormat failed (" + Marshal.GetLastWin32Error() + ")";

            rc = wglCreateContext(dc);
            path = "opengl32";
        }

        diagnostics = Diagnostics(dc) + "; chosenFormat=" + format + "; pixelFormatPath=" + path;
        if (rc == IntPtr.Zero)
            return "wglCreateContext returned NULL (" + Marshal.GetLastWin32Error() + ")";
        if (!wglMakeCurrent(dc, rc))
            return "wglMakeCurrent failed";

        vendor = Str(0x1F00);
        renderer = Str(0x1F01);
        version = Str(0x1F02);

        var proc = wglGetProcAddress("wglGetExtensionsStringARB");
        if (proc != IntPtr.Zero)
        {
            var getExtensions = (GetExtensionsStringArb)Marshal.GetDelegateForFunctionPointer(proc, typeof(GetExtensionsStringArb));
            extensions = Marshal.PtrToStringAnsi(getExtensions(dc));
        }

        wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
        wglDeleteContext(rc);
        return null;
    }
}
'@

function Test-GlDriver {
    # A single attempt with one gallium driver selected. Returns the driver name
    # on success and $null on failure, reporting either way. A GL context can be
    # created only once per process, so each attempt runs in its own child.
    param (
        [Parameter(Mandatory)] [string] $Driver,
        [Parameter(Mandatory)] [string] $Arch
    )

    $label = if ($Driver -eq 'default') { '<mesa default>' } else { $Driver }

    if (-not ('MesaGlVerifier' -as [type])) {
        Add-Type -TypeDefinition $verifierSource -Language CSharp | Out-Null
    }

    if ($Driver -eq 'default') { Remove-Item Env:GALLIUM_DRIVER -ErrorAction SilentlyContinue } else { $env:GALLIUM_DRIVER = $Driver }

    # The agents have no GPU, and this is the hint Mesa reads to keep a layered
    # driver (d3d12) on a software device rather than looking for hardware. It is
    # a no-op for softpipe and llvmpipe, which are software either way, and it is
    # what the Linux leg already sets.
    $env:LIBGL_ALWAYS_SOFTWARE = '1'

    $vendor = $renderer = $version = $extensions = $diagnostics = $null
    $failure = [MesaGlVerifier]::Verify(
        [ref] $vendor, [ref] $renderer, [ref] $version, [ref] $extensions, [ref] $diagnostics)

    Write-Host "  [$Arch/$label] $diagnostics"
    if ($failure) {
        Write-Host "  [$Arch/$label] FAILED: $failure"
        return $null
    }

    Write-Host "  [$Arch/$label] GL_VENDOR   : $vendor"
    Write-Host "  [$Arch/$label] GL_RENDERER : $renderer"
    Write-Host "  [$Arch/$label] GL_VERSION  : $version"

    # An unprovisioned agent answers "Microsoft Corporation" / "GDI Generic" /
    # "1.1.0", which is precisely the state this script exists to replace.
    if ($vendor -notmatch 'Mesa|VMware|Brian Paul') {
        Write-Host "  [$Arch/$label] FAILED: expected Mesa to answer, got vendor '$vendor' / renderer '$renderer'."
        return $null
    }

    # WglContext needs both: it picks a format with wglChoosePixelFormatARB and
    # renders into a pbuffer.
    foreach ($required in @('WGL_ARB_pixel_format', 'WGL_ARB_pbuffer')) {
        if ($extensions -notmatch [regex]::Escape($required)) {
            Write-Host "  [$Arch/$label] FAILED: driver does not expose $required, which WglContext requires."
            return $null
        }
    }

    Write-Host "  [$Arch/$label] WGL_ARB_pixel_format and WGL_ARB_pbuffer present"
    return $label
}

function Invoke-Verification {
    # One attempt, in this process, with the driver this invocation was given.
    $arch = if ([IntPtr]::Size -eq 8) { 'x64' } else { 'x86' }
    if (Test-GlDriver -Driver $Driver -Arch $arch) { return }
    throw "OpenGL bring-up failed for $arch."
}

# ---------------------------------------------------------------------------
# Verification re-entry, before any installation work. A process can bring up
# only one OpenGL context, so every attempt is its own child.
# ---------------------------------------------------------------------------

if ($VerifyOnly) {
    Invoke-Verification
    return
}

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
# 1803, and reads 7-Zip when it was built with liblzma — which is not guaranteed
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
        Write-Host "Installed the $($t.Arch) DirectX IL redistributable into $($t.SystemDir)\dxil.dll"
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
# Prove it, in both bitnesses. Each attempt is a child process because a process
# can bring up only one OpenGL context, and every attempt is reported whether it
# worked or not -- when none of them do, the log has to say why for each one.
# ---------------------------------------------------------------------------

Write-Host 'Verifying OpenGL (x64) ...'

$powershell64 = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
$powershell32 = Join-Path $env:SystemRoot 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
foreach ($shell in @($powershell64, $powershell32)) {
    if (-not (Test-Path $shell)) {
        throw "Missing the Windows PowerShell needed to verify the driver: $shell"
    }
}

$selected = $null
foreach ($candidate in $galliumDrivers) {
    & $powershell64 -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -VerifyOnly -Driver $candidate
    if ($LASTEXITCODE -eq 0) {
        $selected = $candidate
        break
    }
}

if ($null -eq $selected) {
    throw "No Mesa gallium driver could bring up an OpenGL context on this agent. " +
          "Tried: $($galliumDrivers -join ', '). " +
          "See the per-driver diagnostics above."
}

$selectedLabel = if ($selected -eq 'default') { '<mesa default>' } else { $selected }
Write-Host "Selected the $selectedLabel gallium driver."

Write-Host 'Verifying OpenGL (x86) ...'
& $powershell32 -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -VerifyOnly -Driver $selected
if ($LASTEXITCODE -ne 0) {
    throw "The x86 OpenGL verification failed with the $selectedLabel driver (exit $LASTEXITCODE)."
}

# Select the driver for every later step in the job. Mesa's own default needs no
# variable, and setting an empty one would override nothing usefully.
if ($selected -ne 'default') {
    Write-Host "##vso[task.setvariable variable=GALLIUM_DRIVER]$selected"
}
Write-Host "##vso[task.setvariable variable=LIBGL_ALWAYS_SOFTWARE]1"

Write-Host "Mesa OpenGL ($selectedLabel) provisioned for x64 and x86; ganesh-gl will execute."
