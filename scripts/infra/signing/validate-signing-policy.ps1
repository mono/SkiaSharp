[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $PackageDirectory,

    [Parameter(Mandatory)]
    [string] $SigningPropsPath,

    [Parameter(Mandatory)]
    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Import-Module (Join-Path $PSScriptRoot 'NuGetPayload.psm1') -Force

$inventory = @(Get-NuGetPackageInventory $PackageDirectory)
$policy = Get-ArcadeSigningPolicy $inventory $SigningPropsPath
Write-NuGetSigningManifest `
    -PackageDirectory $PackageDirectory `
    -SigningPropsPath $SigningPropsPath `
    -Inventory $inventory `
    -Policy $policy `
    -OutputPath $OutputPath

Write-Host "Validated $($policy.Files.Count) signing-policy basenames across $($inventory.Count) recursive package entries."
