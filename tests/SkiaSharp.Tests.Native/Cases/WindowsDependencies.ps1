<#
Checks hard DLL imports in desktop Windows, Nano Server, WinUI and UWP artifacts, including the managed projections.
VCRUNTIME and MSVCP imports are forbidden by the static CRT policy; delay-only imports remain allowed. Native UWP
AppContainer artifacts must instead import the store CRT (VCRUNTIME140_APP) and never the desktop CRT. LLVM inspects
the PE import tables directly, so this check does not require a Windows host.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot
Assert-Tool llvm-readobj

# The static CRT policy excludes hard VCRUNTIME/MSVCP imports; delay-only imports remain allowed.
foreach ($artifact in Get-WindowsArtifacts $NativeRoot) {
    Assert-Artifact $artifact.Path
    $imports = Get-PeImports $artifact.Path
    $crt = @($imports.Hard | Where-Object { $_ -match 'VCRUNTIME|MSVCP' })
    if ($artifact.AppCrt) {
        $forbidden = @($crt | Where-Object { $_ -notmatch '_APP\.dll$' })
        if (@($crt | Where-Object { $_ -match '^VCRUNTIME140_APP\.dll$' }).Count -eq 0) {
            throw "Missing store CRT (VCRUNTIME140_APP.dll) hard import in $($artifact.Path)"
        }
    } else {
        $forbidden = $crt
    }
    if ($forbidden.Count -gt 0) {
        throw "Forbidden VCRUNTIME/MSVCP hard import in $($artifact.Path): $($forbidden -join ', ')"
    }

    Write-Host "PASS: $($artifact.Path); hard imports: $($imports.Hard -join ', ')"
}
