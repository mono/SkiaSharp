<#
Checks hard DLL imports in desktop Windows, Nano Server, WinUI and UWP artifacts, including the managed projections.
Static-CRT artifacts must not hard-import the C++ runtime. Native UWP AppContainer artifacts must import the store
runtime (VCRUNTIME140_APP) and only _APP C++ runtime DLLs. Delay-only imports remain allowed. LLVM inspects PE import
tables directly, so this check does not require a Windows host.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot
Assert-Tool llvm-readobj

foreach ($artifact in Get-WindowsArtifacts $NativeRoot) {
    Assert-Artifact $artifact.Path
    $imports = Get-PeImports $artifact.Path
    $runtime = @($imports.Hard | Where-Object { $_ -match '^(VCRUNTIME|MSVCP|MSVCR|CONCRT|VCCORLIB)' })
    $forbidden = if ($artifact.AppCrt) {
        @($runtime | Where-Object { $_ -notmatch '_APP\.dll$' })
    } else {
        $runtime
    }
    if ($forbidden.Count -gt 0) {
        throw "Forbidden C++ runtime hard import in $($artifact.Path): $($forbidden -join ', ')"
    }
    if ($artifact.AppCrt -and 'VCRUNTIME140_APP.dll' -notin $runtime) {
        throw "Missing store runtime (VCRUNTIME140_APP.dll) hard import in $($artifact.Path)"
    }

    Write-Host "PASS: $($artifact.Path); hard imports: $($imports.Hard -join ', ')"
}
