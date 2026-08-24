[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $OriginalDirectory,

    [Parameter(Mandatory)]
    [string] $StagedDirectory,

    [string] $OutputPath = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

function Get-PackageMap {
    param(
        [Parameter(Mandatory)]
        [string] $Directory,

        [Parameter(Mandatory)]
        [string] $Description
    )

    $resolved = (Resolve-Path $Directory).Path
    $packages = @(Get-ChildItem $resolved -Filter '*.nupkg' -File -Recurse)
    if ($packages.Count -eq 0) {
        throw "No $Description transport packages were found in '$resolved'."
    }

    $map = [System.Collections.Generic.Dictionary[string, IO.FileInfo]]::new(
        [StringComparer]::Ordinal)
    $caseInsensitiveNames = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    foreach ($package in $packages) {
        if (-not $package.Name.StartsWith('_', [StringComparison]::Ordinal)) {
            throw "$Description package '$($package.Name)' is not an underscore-prefixed transport package."
        }
        if ($package.Name.EndsWith('.symbols.nupkg', [StringComparison]::OrdinalIgnoreCase)) {
            throw "$Description transport package '$($package.Name)' must not be a symbol package."
        }
        if (-not $caseInsensitiveNames.Add($package.Name)) {
            throw "$Description transport packages contain a duplicate or case-colliding name: $($package.Name)"
        }
        $map.Add($package.Name, $package)
    }
    return $map
}

function Assert-Unsigned {
    param(
        [Parameter(Mandatory)]
        [IO.FileInfo] $Package
    )

    $stream = [IO.File]::OpenRead($Package.FullName)
    $archive = [IO.Compression.ZipArchive]::new(
        $stream,
        [IO.Compression.ZipArchiveMode]::Read,
        $false)
    try {
        if ($archive.GetEntry('.signature.p7s')) {
            throw "Transport package '$($Package.Name)' must remain unsigned."
        }
    } finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

$original = Get-PackageMap $OriginalDirectory 'original'
$staged = Get-PackageMap $StagedDirectory 'staged'
$nameDifference = Compare-Object @($original.Keys) @($staged.Keys) -CaseSensitive
if ($nameDifference) {
    $nameDifference | Format-Table | Out-String | Write-Host
    throw 'The staged transport package set differs from the original package set.'
}

foreach ($name in $original.Keys) {
    Assert-Unsigned $original[$name]
    Assert-Unsigned $staged[$name]

    $originalHash = (Get-FileHash $original[$name].FullName -Algorithm SHA256).Hash
    $stagedHash = (Get-FileHash $staged[$name].FullName -Algorithm SHA256).Hash
    if ($originalHash -cne $stagedHash) {
        throw "Unsigned transport package '$name' changed while staging or signing product packages."
    }
}

$result = [ordered]@{
    formatVersion = 1
    verifiedAtUtc = [DateTime]::UtcNow.ToString('O')
    packageCount = $original.Count
    packagesRemainUnsigned = $true
    packagesRemainByteIdentical = $true
}
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $outputDirectory = Split-Path $OutputPath -Parent
    New-Item $outputDirectory -ItemType Directory -Force | Out-Null
    $result | ConvertTo-Json | Set-Content $OutputPath -Encoding utf8NoBOM
}

Write-Host "Verified $($original.Count) unsigned, byte-identical transport package(s)."
