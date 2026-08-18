[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SourceDirectory,

    [Parameter(Mandatory)]
    [string] $DestinationDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

$allowedLabels = @(
    'alpha',
    'beta',
    'nightly',
    'pr',
    'preview',
    'rc'
)

function Get-PackageIdentity {
    param([Parameter(Mandatory)][IO.FileInfo] $Package)

    $archive = [IO.Compression.ZipFile]::OpenRead($Package.FullName)
    try {
        $nuspec = @(
            $archive.Entries |
                Where-Object {
                    $_.Name.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) -and
                    $_.FullName -eq $_.Name
                }
        )
        if ($nuspec.Count -ne 1) {
            throw "Expected one root nuspec in '$($Package.Name)', found $($nuspec.Count)."
        }

        $reader = [IO.StreamReader]::new($nuspec[0].Open())
        try {
            [xml]$document = $reader.ReadToEnd()
        } finally {
            $reader.Dispose()
        }

        $idNode = $document.SelectSingleNode(
            "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='id']")
        $versionNode = $document.SelectSingleNode(
            "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='version']")
        if (-not $idNode -or -not $versionNode -or
            [string]::IsNullOrWhiteSpace($idNode.InnerText) -or
            [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
            throw "Package '$($Package.Name)' has no ID or version."
        }

        return [pscustomobject]@{
            File = $Package
            Id = $idNode.InnerText.Trim()
            Version = $versionNode.InnerText.Trim()
        }
    } finally {
        $archive.Dispose()
    }
}

$source = (Resolve-Path $SourceDirectory).Path
$packages = @(Get-ChildItem $source -Filter '*.nupkg' -File | Sort-Object Name)
if ($packages.Count -eq 0) {
    throw "No NuGet packages were found in '$source'."
}

$identities = @($packages | ForEach-Object { Get-PackageIdentity $_ })
foreach ($identity in $identities) {
    $versionWithoutMetadata = $identity.Version.Split('+')[0]
    $separator = $versionWithoutMetadata.IndexOf('-')
    if ($separator -lt 0) {
        throw "Exact stable package '$($identity.Id) $($identity.Version)' cannot enter preview BAR publishing."
    }

    $prerelease = $versionWithoutMetadata.Substring($separator + 1)
    $label = $prerelease.Split('.')[0].ToLowerInvariant()
    if ($label -notin $allowedLabels) {
        throw "Package '$($identity.Id) $($identity.Version)' has unsupported prerelease label '$label'."
    }
}

Remove-Item $DestinationDirectory -Recurse -Force -ErrorAction Ignore
New-Item $DestinationDirectory -ItemType Directory -Force | Out-Null
foreach ($identity in $identities) {
    Copy-Item $identity.File.FullName -Destination $DestinationDirectory
}

Write-Host "Staged $($identities.Count) prerelease NuGet package(s) for BAR publishing:"
$identities | ForEach-Object { Write-Host "  $($_.Id) $($_.Version)" }
