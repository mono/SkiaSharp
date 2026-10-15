<#
Reproduces Linux symbol preemption by loading conflicting HarfBuzz functions globally before each shipped library.
A deliberately preemptible control proves the probe detects the problem, then normal and nodeps artifacts must both
work with and without the conflict. Only matching Linux/glibc artifacts execute, in isolated native child processes.
Other operating systems explicitly skip this runtime-only case before requiring a compiler.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/../Helpers.psm1"

# Require glibc and a supported host before loading any artifacts.
if (-not $IsLinux) {
    Write-Output 'SKIP: HarfBuzz symbol binding requires a Linux/glibc host.'
    exit 77
}

Assert-NativeRoot $NativeRoot
Assert-Tool getconf
Assert-Readelf
$libc = & getconf GNU_LIBC_VERSION
if ($libc -notmatch '^glibc ') {
    throw "Unsupported host libc: $libc"
}

switch ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture) {
    X64 {
        $arch = 'x64'
        $machine = 'Advanced Micro Devices X86-64'
    }
    Arm64 {
        $arch = 'arm64'
        $machine = 'AArch64'
    }
    default {
        throw "Unsupported host architecture: $([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)"
    }
}

$compiler = if (Test-Path Env:CC) {
    $env:CC
} else {
    'cc'
}
if ($compiler -cnotmatch '^[a-zA-Z0-9_.+-]+$') {
    throw 'CC must be a compiler name, not a command or path.'
}
Assert-Tool $compiler
Remove-Item Env:LD_PRELOAD, Env:LD_AUDIT -ErrorAction SilentlyContinue

# Check both artifacts before executing any native probe.
foreach ($variant in 'linux', 'linuxnodeps') {
    $library = Join-Path $NativeRoot "$variant/$arch/libHarfBuzzSharp.so"
    Write-Host "Checking $variant ($arch, ${libc}): $library"
    Assert-Artifact $library

    $header = (& llvm-readelf -h $library) -join "`n"
    if ($header -notmatch '(?m)^\s*Class:\s+ELF64\s*$') {
        throw "Expected ELF64: $library"
    }
    if ($header -notmatch "(?m)^\s*Machine:\s+$([regex]::Escape($machine))\s*$") {
        throw "Artifact architecture does not match ${arch}: $library"
    }
    if ($header -notmatch '(?m)^\s*Type:\s+DYN\s') {
        throw "Expected shared library: $library"
    }

    $dynamic = (& llvm-readelf -d $library) -join "`n"
    if (-not $dynamic.Contains('Shared library: [libc.so.6]')) {
        throw "Artifact is not glibc-linked: $library"
    }
    if ($dynamic.Contains('musl')) {
        throw "Artifact depends on musl: $library"
    }
}

# Build the loader, conflicting symbols, and deliberately preemptible control.
$scratch = Join-Path ([System.IO.Path]::GetTempPath()) ("harfbuzz-symbol-binding-" + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
try {
    $source = Join-Path $PSScriptRoot 'probe.c'
    $probe = Join-Path $scratch 'probe'
    $interposer = Join-Path $scratch 'interposer.so'
    $control = Join-Path $scratch 'control.so'
    $flags = @('-std=c11', '-O0', '-Wall', '-Wextra', '-Werror')
    & $compiler @flags $source -ldl -o $probe
    & $compiler @flags -shared -fPIC -DINTERPOSER $source -o $interposer
    & $compiler @flags -shared -fPIC -fsemantic-interposition -DNEGATIVE_CONTROL $source -o $control

    # Prove the interposer detects an actual binding failure, including its diagnostic.
    Write-Host 'Control: intentionally preemptible fake HarfBuzz alone'
    & $probe $control

    Write-Host 'Control: interposer must terminate fake HarfBuzz with exit 86'
    $nativeErrors = $PSNativeCommandUseErrorActionPreference
    try {
        $PSNativeCommandUseErrorActionPreference = $false
        & $probe $control $interposer 2>&1 | Tee-Object -Variable controlOutput
        $status = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativeErrors
    }

    if ($status -ne 86) {
        throw "Control returned $status, expected 86"
    }
    $diagnostic = 'FAIL: bundled HarfBuzz called the global hb_font_set_funcs'
    if (($controlOutput -join "`n") -notmatch [regex]::Escape($diagnostic)) {
        throw 'Control exited 86 without the interposer diagnostic'
    }
    Write-Host 'PASS: negative control was interposed (exit 86)'

    # Both shipped variants must work with and without globally loaded conflicting symbols.
    foreach ($variant in 'linux', 'linuxnodeps') {
        $library = Join-Path $NativeRoot "$variant/$arch/libHarfBuzzSharp.so"
        Write-Host "${variant}: bundled library alone"
        & $probe $library
        Write-Host "${variant}: conflicting functions loaded globally first"
        & $probe $library $interposer
    }
} finally {
    Remove-Item -LiteralPath $scratch -Recurse -Force
}
