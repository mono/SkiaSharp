<#
Verifies that desktop Skia DLLs delay-load D3D12 and D3DCompiler instead of making them mandatory loader dependencies.
Both imports must be present in the delay-import table and absent from hard imports. Nano Server is intentionally
excluded because its build disables Direct3D. Windows PE files are inspected without execution.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot
Assert-Tool llvm-readobj

# NanoServer disables Direct3D; only desktop Skia DLLs require these delay imports.
foreach ($artifact in Get-WindowsArtifacts $NativeRoot | Where-Object Direct3D) {
    Assert-Artifact $artifact.Path
    $imports = Get-PeImports $artifact.Path

    foreach ($dependency in 'd3d12', 'D3DCOMPILER') {
        if (@($imports.Hard | Where-Object { $_ -like "*$dependency*" }).Count -gt 0) {
            throw "Hard import must be delay-loaded ($dependency): $($artifact.Path)"
        }
        if (@($imports.Delayed | Where-Object { $_ -like "*$dependency*" }).Count -eq 0) {
            throw "Missing required delay import ($dependency): $($artifact.Path)"
        }
    }

    Write-Host "PASS: $($artifact.Path); delay imports: $($imports.Delayed -join ', ')"
}
