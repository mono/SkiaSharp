[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $OriginalDirectory,

    [Parameter(Mandatory)]
    [string] $SignedDirectory,

    [Parameter(Mandatory)]
    [string] $SigningPropsPath,

    [string] $OutputPath = '',

    [switch] $RequireSignatures
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Import-Module (Join-Path $PSScriptRoot 'NuGetPayload.psm1') -Force

function Assert-NoDuplicatePaths {
    param(
        [Parameter(Mandatory)]
        [object[]] $Inventory,

        [Parameter(Mandatory)]
        [string] $Description
    )

    $exactDuplicates = @($Inventory | Group-Object Path -CaseSensitive | Where-Object Count -gt 1)
    if ($exactDuplicates.Count -ne 0) {
        throw "$Description contains duplicate archive paths: $($exactDuplicates.Name -join ', ')"
    }

    $caseDuplicates = @(
        $Inventory |
            Group-Object { $_.Path.ToLowerInvariant() } |
            Where-Object Count -gt 1
    )
    if ($caseDuplicates.Count -ne 0) {
        throw "$Description contains case-colliding archive paths: $($caseDuplicates.Name -join ', ')"
    }
}

$originalPath = (Resolve-Path $OriginalDirectory).Path
$signedPath = (Resolve-Path $SignedDirectory).Path
$signingProps = (Resolve-Path $SigningPropsPath).Path
$originalPackages = @(Get-ChildItem $originalPath -Filter '*.nupkg' -File | Sort-Object Name)
$signedPackages = @(Get-ChildItem $signedPath -Filter '*.nupkg' -File | Sort-Object Name)

$packageDifference = Compare-Object $originalPackages.Name $signedPackages.Name -CaseSensitive
if ($packageDifference) {
    $packageDifference | Format-Table | Out-String | Write-Host
    throw 'The signed package set differs from the unsigned package set.'
}

$originalInventory = @(Get-NuGetPackageInventory $originalPath)
$signedInventory = @(Get-NuGetPackageInventory $signedPath)
Assert-NoDuplicatePaths $originalInventory 'Unsigned packages'
Assert-NoDuplicatePaths $signedInventory 'Signed packages'

$originalPayload = @($originalInventory | Where-Object { -not $_.IsSignatureMetadata })
$signedPayload = @($signedInventory | Where-Object { -not $_.IsSignatureMetadata })
$pathDifference = Compare-Object $originalPayload.Path $signedPayload.Path -CaseSensitive
if ($pathDifference) {
    $pathDifference | Format-Table | Out-String | Write-Host
    throw 'Signing changed the recursive NuGet payload structure.'
}

$policy = Get-ArcadeSigningPolicy $originalInventory $signingProps
$assignmentByPath = @{}
foreach ($file in $policy.Files) {
    foreach ($path in $file.Paths) {
        $assignmentByPath[$path] = $file
    }
}

$signedByPath = @{}
foreach ($entry in $signedInventory) {
    $signedByPath[$entry.Path] = $entry
}

$changedEntries = 0
$unchangedEntries = 0
foreach ($original in $originalPayload) {
    $signed = $signedByPath[$original.Path]
    $assignment = $assignmentByPath[$original.Path]
    $changed = $original.Sha256 -ne $signed.Sha256

    if ($assignment -and $assignment.Category -eq 'Skip') {
        if ($changed) {
            throw "Skipped package entry '$($original.Path)' changed during signing."
        }
        $unchangedEntries++
        continue
    }

    if ($assignment) {
        if ($RequireSignatures -and -not $changed) {
            throw "Expected signed package entry '$($original.Path)' was not modified."
        }
        if ($changed) {
            $changedEntries++
        } else {
            $unchangedEntries++
        }
        continue
    }

    $signedDescendants = @(
        $assignmentByPath.Keys |
            Where-Object { $_.StartsWith("$($original.Path)!/", [StringComparison]::Ordinal) }
    )
    $containsSignedPayload = $signedDescendants.Count -ne 0
    if ($changed -and -not $containsSignedPayload) {
        throw "Unsigned package entry '$($original.Path)' changed unexpectedly."
    }
    if ($changed) {
        $changedEntries++
    } else {
        $unchangedEntries++
    }
}

if ($RequireSignatures) {
    foreach ($package in $signedPackages) {
        $signaturePath = "$($package.Name)!/.signature.p7s"
        if (-not ($signedInventory.Path -contains $signaturePath)) {
            throw "Signed NuGet package '$($package.Name)' has no author signature."
        }

        $originalPackage = Join-Path $originalPath $package.Name
        $originalHash = (Get-FileHash $originalPackage -Algorithm SHA256).Hash
        $signedHash = (Get-FileHash $package.FullName -Algorithm SHA256).Hash
        if ($originalHash -eq $signedHash) {
            throw "Signed NuGet package '$($package.Name)' is byte-identical to its unsigned input."
        }
    }
}

$result = [ordered]@{
    formatVersion = 1
    verifiedAtUtc = [DateTime]::UtcNow.ToString('O')
    packageCount = $signedPackages.Count
    payloadEntryCount = $signedPayload.Count
    changedEntryCount = $changedEntries
    unchangedEntryCount = $unchangedEntries
    requiredSignatures = [bool] $RequireSignatures
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $outputDirectory = Split-Path $OutputPath -Parent
    New-Item $outputDirectory -ItemType Directory -Force | Out-Null
    $result |
        ConvertTo-Json |
        Set-Content $OutputPath -Encoding utf8NoBOM
}

Write-Host "Verified $($signedPackages.Count) signed NuGet package(s) and $($signedPayload.Count) recursive payload entries."
