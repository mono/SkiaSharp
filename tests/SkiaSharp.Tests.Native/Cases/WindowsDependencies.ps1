<#
Checks hard DLL imports in desktop Windows, Nano Server and WinUI artifacts, including the managed projection.
VCRUNTIME and MSVCP imports are forbidden by the static CRT policy; delay-only imports remain allowed. LLVM inspects
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
    $forbidden = @($imports.Hard | Where-Object { $_ -match 'VCRUNTIME|MSVCP' })
    if ($forbidden.Count -gt 0) {
        throw "Forbidden VCRUNTIME/MSVCP hard import in $($artifact.Path): $($forbidden -join ', ')"
    }

    Write-Host "PASS: $($artifact.Path); hard imports: $($imports.Hard -join ', ')"
}
