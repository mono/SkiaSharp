param(
    [Parameter(Mandatory = $true)]
    [string] $SourceDirectory,

    [Parameter(Mandatory = $true)]
    [string] $DestinationDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

$source = [IO.Path]::GetFullPath($SourceDirectory)
$destination = [IO.Path]::GetFullPath($DestinationDirectory)

if (-not (Test-Path $source -PathType Container)) {
    throw "Transport package source does not exist: $source"
}

$packages = @(Get-ChildItem $source -Filter '*.nupkg' -File -Recurse)
if ($packages.Count -eq 0) {
    throw "No transport packages were found in '$source'."
}

foreach ($package in $packages) {
    if (-not $package.Name.StartsWith('_', [StringComparison]::Ordinal)) {
        throw "Transport package '$($package.Name)' must use an underscore-prefixed package ID."
    }
    if ($package.Name.EndsWith('.symbols.nupkg', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Symbol package '$($package.Name)' cannot be staged as a transport package."
    }

    $stream = [IO.File]::OpenRead($package.FullName)
    $archive = [IO.Compression.ZipArchive]::new(
        $stream,
        [IO.Compression.ZipArchiveMode]::Read,
        $false)
    try {
        if ($archive.GetEntry('.signature.p7s')) {
            throw "Transport package '$($package.Name)' must remain unsigned."
        }
    } finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

Remove-Item $destination -Recurse -Force -ErrorAction Ignore
New-Item $destination -ItemType Directory -Force | Out-Null
Copy-Item $packages.FullName $destination

Write-Host "Staged $($packages.Count) unsigned transport package(s) for Arcade publishing."
