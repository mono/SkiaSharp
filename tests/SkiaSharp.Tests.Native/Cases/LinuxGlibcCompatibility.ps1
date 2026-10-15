<#
Checks GLIBC symbol-version requirements in both normal and nodeps libraries against the per-architecture ceilings
declared below. This catches accidental minimum-runtime increases caused by toolchain or sysroot changes, even when
the build succeeds. Inspection is independent of the host architecture and does not load the target libraries.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot
Assert-Readelf

# Compatibility policy belongs with the test, independently of the producing toolchain.
$limits = @{
    x86 = '2.27'
    x64 = '2.27'
    arm = '2.35'
    arm64 = '2.27'
    riscv64 = '2.39'
    loongarch64 = '2.42'
}

# Inspect both libraries in normal and nodeps glibc outputs, without loading target code.
foreach ($arch in $limits.Keys | Sort-Object) {
    foreach ($variant in 'linux', 'linuxnodeps') {
        foreach ($name in 'libSkiaSharp', 'libHarfBuzzSharp') {
            $library = Join-Path $NativeRoot "$variant/$arch/$name.so"
            Assert-Artifact $library
            $output = (& llvm-readelf --wide --version-info $library) -join "`n"
            $versions = [regex]::Matches($output, 'GLIBC_([\w.]+)') |
                ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique

            foreach ($version in $versions) {
                if ($version -notmatch '^\d+(\.\d+)+$') {
                    throw "Unrecognized GLIBC requirement $version in $library"
                }
                if ([version]$version -gt [version]$limits[$arch]) {
                    throw "$library requires GLIBC $version above $($limits[$arch])"
                }
            }

            Write-Host "PASS: $library; GLIBC ceiling $($limits[$arch]); requirements: $($versions -join ', ')"
        }
    }
}
