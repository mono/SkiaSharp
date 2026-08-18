$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$scriptPath = Join-Path $repoRoot 'scripts/infra/package/stage-prerelease-packages.ps1'
$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-prerelease-tests-$([Guid]::NewGuid().ToString('N'))"
$source = Join-Path $testRoot 'source'
$destination = Join-Path $testRoot 'destination'

function New-TestPackage {
    param(
        [Parameter(Mandatory)]
        [string] $Id,

        [Parameter(Mandatory)]
        [string] $Version
    )

    $path = Join-Path $source "$Id.$Version.nupkg"
    $stream = [IO.File]::Create($path)
    $archive = [IO.Compression.ZipArchive]::new(
        $stream,
        [IO.Compression.ZipArchiveMode]::Create)
    try {
        $entry = $archive.CreateEntry("$Id.nuspec")
        $writer = [IO.StreamWriter]::new($entry.Open())
        try {
            $writer.Write(
                "<package><metadata><id>$Id</id><version>$Version</version></metadata></package>")
        } finally {
            $writer.Dispose()
        }
    } finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

try {
    New-Item $source -ItemType Directory -Force | Out-Null
    New-TestPackage 'Package-With-Hyphen' '1.0.0-preview.1.26418.3'
    New-TestPackage 'Package.Rc' '1.0.0-rc.1.26418.3'
    New-TestPackage 'Package.StableCandidate' '1.0.0-stable.26418.3'
    New-TestPackage 'Package.Feature' '1.0.0-featurepreview-graphite.26418.3'

    & $scriptPath -SourceDirectory $source -DestinationDirectory $destination
    $copied = @(Get-ChildItem $destination -Filter '*.nupkg' -File)
    if ($copied.Count -ne 4) {
        throw "Expected four prerelease packages, found $($copied.Count)."
    }

    New-TestPackage 'Package.ExactStable' '1.0.0'
    $stableRejected = $false
    try {
        & $scriptPath -SourceDirectory $source -DestinationDirectory $destination
    } catch {
        $stableRejected = $_.Exception.Message -like '*cannot enter preview BAR publishing*'
    }
    if (-not $stableRejected) {
        throw 'Exact stable package was not rejected.'
    }

    Remove-Item (Join-Path $source 'Package.ExactStable.1.0.0.nupkg') -Force
    New-TestPackage 'Package.Unknown' '1.0.0-custom.26418.3'
    $unknownRejected = $false
    try {
        & $scriptPath -SourceDirectory $source -DestinationDirectory $destination
    } catch {
        $unknownRejected = $_.Exception.Message -like "*unsupported prerelease label 'custom'*"
    }
    if (-not $unknownRejected) {
        throw 'Unknown prerelease label was not rejected.'
    }

    Write-Host 'Prerelease package staging tests passed.'
} finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction Ignore
}
