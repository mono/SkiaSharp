<#
Verifies that both Tizen libraries are ELF64 shared objects for their expected x86-64 or ARM64 architecture and define
the expected public SkiaSharp or HarfBuzz entry points. These static ABI checks catch incorrect or incomplete build
outputs on the Linux test host; they do not replace Tizen runtime tests.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"
Assert-NativeRoot $NativeRoot
Assert-GnuReadelf

# These are static ABI checks, not a substitute for running the Tizen runtime tests.
foreach ($artifact in Get-TizenArtifacts $NativeRoot) {
    Assert-Artifact $artifact.Path
    $header = (& readelf --file-header $artifact.Path) -join "`n"
    $machine = if ($artifact.Arches[0] -eq 'aarch64') {
        'AArch64'
    } else {
        'Advanced Micro Devices X86-64'
    }
    if ($header -notmatch '(?m)^\s*Class:\s+ELF64\s*$' -or
        $header -notmatch '(?m)^\s*Type:\s+DYN\b' -or
        $header -notmatch "(?m)^\s*Machine:\s+$([regex]::Escape($machine))\s*$") {
        throw "Expected a 64-bit Tizen shared library for $($artifact.Arches[0]): $($artifact.Path)"
    }

    $symbols = (& readelf --dyn-syms --wide $artifact.Path) -join "`n"
    $exports = if ($artifact.Name -eq 'libSkiaSharp') {
        'sk_version_get_milestone', 'sk_bitmap_new'
    } else {
        'hb_version', 'hb_buffer_create'
    }
    foreach ($export in $exports) {
        $pattern = "(?m)^\s*\d+:\s+\S+\s+\d+\s+FUNC\s+GLOBAL\s+DEFAULT\s+(?!UND\b)\S+\s+$export(?:@@?\S+)?\s*$"
        if ($symbols -notmatch $pattern) {
            throw "Missing public Tizen export '$export': $($artifact.Path)"
        }
    }

    Write-Host "PASS: $($artifact.Path) is $machine ELF64 with the expected public API."
}
