<#
Load conflicting HarfBuzz functions globally before creating a font through each shipped glibc library.
An intentionally preemptible control proves the collision is detected. Each load order runs in a fresh native
process, never inside xUnit. Normal and nodeps libraries must both pass with and without the global conflict.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/../Helpers.psm1"

# Require the runtime host and exact tools before loading any native artifacts.
if (!$IsLinux) {
    Write-Output 'SKIP: HarfBuzz symbol binding requires a Linux/glibc host.'
    exit 77
}

Assert-NativeRoot $NativeRoot
Assert-Tool getconf
Assert-Tool llvm-readobj
Assert-Tool gcc
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

Remove-Item Env:LD_PRELOAD, Env:LD_AUDIT -ErrorAction SilentlyContinue

# Validate both exact artifact paths before executing the probes.
foreach ($variant in 'linux', 'linuxnodeps') {
    $library = Join-Path $NativeRoot "$variant/$arch/libHarfBuzzSharp.so"
    Write-Host "Checking $variant ($arch, ${libc}): $library"
    Assert-Artifact $library

    $header = (& llvm-readobj --elf-output-style=GNU --file-headers $library) -join "`n"
    if ($header -notmatch '(?m)^\s*Class:\s+ELF64\s*$' -or
        $header -notmatch '(?m)^\s*Data:\s+2.s complement, little endian\s*$' -or
        $header -notmatch "(?m)^\s*Machine:\s+$([regex]::Escape($machine))\s*$" -or
        $header -notmatch '(?m)^\s*Type:\s+DYN\s') {
        throw "Expected a little-endian ELF64 shared library matching ${arch}: $library"
    }

    $dynamic = (& llvm-readobj --elf-output-style=GNU --dynamic-table $library) -join "`n"
    if (!$dynamic.Contains('Shared library: [libc.so.6]') -or $dynamic.Contains('musl')) {
        throw "Expected a glibc-linked artifact: $library"
    }
}

# Build separate loader, interposer and negative-control sources.
$scratch = Join-Path ([IO.Path]::GetTempPath()) ("harfbuzz-symbol-binding-" + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
try {
    $probe = Join-Path $scratch 'probe'
    $interposer = Join-Path $scratch 'interposer.so'
    $control = Join-Path $scratch 'control.so'
    $flags = @('-std=c11', '-O0', '-Wall', '-Wextra', '-Werror')
    & gcc @flags "$PSScriptRoot/probe.c" -ldl -o $probe
    & gcc @flags -shared -fPIC "$PSScriptRoot/interposer.c" -o $interposer
    & gcc @flags -shared -fPIC -fsemantic-interposition "$PSScriptRoot/control.c" -o $control

    # Prove the negative control fails for the intended reason, not an unrelated loader error.
    Write-Host 'Control: intentionally preemptible library alone'
    & $probe $control

    Write-Host 'Control: interposer must terminate the preemptible library with exit 86'
    try {
        $PSNativeCommandUseErrorActionPreference = $false
        & $probe $control $interposer 2>&1 | Tee-Object -Variable controlOutput
        $status = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $true
    }

    if ($status -ne 86) {
        throw "Control returned $status, expected 86"
    }
    $diagnostic = 'FAIL: bundled HarfBuzz called the global hb_font_set_funcs'
    if (($controlOutput -join "`n") -notmatch [regex]::Escape($diagnostic)) {
        throw 'Control exited 86 without the interposer diagnostic'
    }
    Write-Host 'PASS: negative control was interposed (exit 86)'

    # Exercise the produced libraries with both isolated load orders.
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
