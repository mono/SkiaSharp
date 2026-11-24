<#
Checks the ELF dependencies of both libraries across glibc, musl, nodeps and Bionic builds. Normal Skia libraries must
depend on fontconfig, while nodeps and Bionic outputs must not. This verifies the dependency policy of the produced
artifacts without attempting to execute cross-built binaries.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot
Assert-Tool llvm-readobj

foreach ($artifact in Get-LinuxArtifacts $NativeRoot) {
    Assert-Artifact $artifact.Path
    $dynamic = (& llvm-readobj --elf-output-style=GNU --dynamic-table $artifact.Path) -join "`n"
    $needed = @([regex]::Matches($dynamic, '\(NEEDED\).+\[(.+)\]') | ForEach-Object { $_.Groups[1].Value })
    $fontconfig = @($needed | Where-Object { $_ -match 'fontconfig' }).Count -gt 0

    # Normal Skia builds require fontconfig; nodeps and Bionic must not link it.
    $required = $artifact.Variant -in 'linux', 'alpine' -and $artifact.Name -eq 'libSkiaSharp'
    $excluded = $artifact.Variant -like '*nodeps' -or $artifact.Variant -eq 'bionic'
    if ($required -and !$fontconfig) {
        throw "Missing required fontconfig dependency: $($artifact.Path)"
    }
    if ($excluded -and $fontconfig) {
        throw "Unexpected fontconfig dependency: $($artifact.Path)"
    }

    Write-Host "PASS: $($artifact.Path); dependencies: $($needed -join ', ')"
}
