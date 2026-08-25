[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SignedPackageDirectory,

    [Parameter(Mandatory)]
    [string] $TransportPackageDirectory,

    [Parameter(Mandatory)]
    [string] $PackageRoot,

    [Parameter(Mandatory)]
    [string] $PdbArtifactsDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-Packages {
    param(
        [Parameter(Mandatory)]
        [string] $Directory,

        [Parameter(Mandatory)]
        [string] $Description
    )

    $resolved = (Resolve-Path $Directory).Path
    $packages = @(Get-ChildItem $resolved -Filter '*.nupkg' -File -Recurse)
    if ($packages.Count -eq 0) {
        throw "No $Description NuGet packages were found in '$resolved'."
    }

    $duplicates = @(
        $packages |
            Group-Object { $_.Name.ToLowerInvariant() } |
            Where-Object Count -gt 1)
    if ($duplicates.Count -ne 0) {
        throw "$Description NuGet packages contain duplicate names: $($duplicates.Name -join ', ')"
    }

    return $packages
}

$signedPackages = @(Get-Packages $SignedPackageDirectory 'signed')
$transportPackages = @(Get-Packages $TransportPackageDirectory 'transport')

$packageRoot = [IO.Path]::GetFullPath($PackageRoot)
$shipping = Join-Path $packageRoot 'Shipping'
$nonShipping = Join-Path $packageRoot 'NonShipping'
$pdbArtifacts = [IO.Path]::GetFullPath($PdbArtifactsDirectory)

Remove-Item $packageRoot -Recurse -Force -ErrorAction Ignore
Remove-Item $pdbArtifacts -Recurse -Force -ErrorAction Ignore
New-Item $shipping -ItemType Directory -Force | Out-Null
New-Item $nonShipping -ItemType Directory -Force | Out-Null
New-Item $pdbArtifacts -ItemType Directory -Force | Out-Null

Copy-Item $signedPackages.FullName $shipping
Copy-Item $transportPackages.FullName $nonShipping

$explicitSymbolCount = 0
$pdbCount = 0
$normalPackages = @(
    Get-ChildItem $shipping -Filter '*.nupkg' -File |
        Where-Object {
            -not $_.Name.EndsWith('.symbols.nupkg', [StringComparison]::OrdinalIgnoreCase)
        })

foreach ($package in $normalPackages) {
    $symbolPath = Join-Path $shipping "$($package.BaseName).symbols.nupkg"
    if (Test-Path $symbolPath -PathType Leaf) {
        $explicitSymbolCount++
        continue
    }

    $packagePdbRoot = [IO.Path]::GetFullPath((Join-Path $pdbArtifacts $package.BaseName))
    $archive = [IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        foreach ($entry in $archive.Entries) {
            $entryPath = $entry.FullName.Replace('\', '/')
            if (-not $entry.Name.EndsWith('.pdb', [StringComparison]::OrdinalIgnoreCase) -or
                $entryPath.StartsWith('ref/', [StringComparison]::OrdinalIgnoreCase)) {
                continue
            }

            $relativePath = $entryPath.Replace('/', [IO.Path]::DirectorySeparatorChar)
            $targetPath = [IO.Path]::GetFullPath((Join-Path $packagePdbRoot $relativePath))
            if (-not $targetPath.StartsWith(
                "$packagePdbRoot$([IO.Path]::DirectorySeparatorChar)",
                [StringComparison]::OrdinalIgnoreCase)) {
                throw "PDB package path escapes its extraction root: $($entry.FullName)"
            }

            New-Item ([IO.Path]::GetDirectoryName($targetPath)) -ItemType Directory -Force | Out-Null
            $sourceStream = $entry.Open()
            $targetStream = [IO.File]::Create($targetPath)
            try {
                $sourceStream.CopyTo($targetStream)
            } finally {
                $targetStream.Dispose()
                $sourceStream.Dispose()
            }
            $pdbCount++
        }
    } finally {
        $archive.Dispose()
    }
}

if ($pdbCount -eq 0) {
    Set-Content (Join-Path $pdbArtifacts '.empty') ''
}

Write-Host "Arcade assets: $($signedPackages.Count) signed package(s), $explicitSymbolCount explicit symbol package(s), $pdbCount loose PDB(s), $($transportPackages.Count) unsigned transport package(s)."
