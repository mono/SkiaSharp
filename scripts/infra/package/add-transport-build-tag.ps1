[CmdletBinding()]
param(
    [string] $PackageDirectory,

    [switch] $EmitBuildTag
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

function Get-NuGetPackageIdentity {
    param(
        [Parameter(Mandatory)]
        [IO.FileInfo] $Package
    )

    $archive = [IO.Compression.ZipFile]::OpenRead($Package.FullName)
    try {
        $nuspecEntries = @($archive.Entries | Where-Object { $_.FullName -match '^[^/]+\.nuspec$' })
        if ($nuspecEntries.Count -ne 1) {
            throw "Package '$($Package.Name)' must contain exactly one root .nuspec file; found $($nuspecEntries.Count)."
        }

        $reader = [IO.StreamReader]::new($nuspecEntries[0].Open())
        try {
            $document = [xml] $reader.ReadToEnd()
        } finally {
            $reader.Dispose()
        }

        $namespace = $document.DocumentElement.NamespaceURI
        $manager = [Xml.XmlNamespaceManager]::new($document.NameTable)
        $manager.AddNamespace('n', $namespace)
        $metadata = $document.SelectSingleNode('/n:package/n:metadata', $manager)
        if ($null -eq $metadata) {
            throw "Package '$($Package.Name)' does not contain nuspec metadata."
        }
        $id = $metadata.SelectSingleNode('n:id', $manager).InnerText
        $version = $metadata.SelectSingleNode('n:version', $manager).InnerText
        if ([string]::IsNullOrWhiteSpace($id) -or [string]::IsNullOrWhiteSpace($version)) {
            throw "Package '$($Package.Name)' has an incomplete .nuspec identity."
        }

        [pscustomobject]@{
            Path = $Package.FullName
            Id = $id
            Version = $version
        }
    } finally {
        $archive.Dispose()
    }
}

function Get-TransportBuildTag {
    param(
        [Parameter(Mandatory)]
        [string] $PackageDirectory
    )

    if (-not (Test-Path $PackageDirectory -PathType Container)) {
        throw "Transport package directory does not exist: '$PackageDirectory'."
    }

    $identities = @(
        Get-ChildItem $PackageDirectory -Filter '*.nupkg' -File |
            ForEach-Object { Get-NuGetPackageIdentity $_ })
    $transportPackages = @($identities | Where-Object Id -eq '_NuGets')
    if ($transportPackages.Count -ne 1) {
        $found = if ($identities.Count -eq 0) {
            'no NuGet packages'
        } else {
            (@($identities | ForEach-Object { "$($_.Id) $($_.Version)" }) -join ', ')
        }
        throw "Expected exactly one _NuGets transport package in '$PackageDirectory'; found $($transportPackages.Count). Available identities: $found."
    }

    $package = $transportPackages[0]
    if ($package.Version -notmatch '^0\.0\.0-(?<suffix>(?:branch|pr)\.[0-9A-Za-z][0-9A-Za-z.-]*)$') {
        throw "Transport package '$($package.Id)' has unsupported version '$($package.Version)'."
    }

    [pscustomobject]@{
        PackagePath = $package.Path
        PackageId = $package.Id
        PackageVersion = $package.Version
        Tag = "Transport - $($Matches.suffix)"
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    if ([string]::IsNullOrWhiteSpace($PackageDirectory)) {
        throw 'PackageDirectory is required when adding a transport build tag.'
    }

    $transportBuildTag = Get-TransportBuildTag -PackageDirectory $PackageDirectory
    Write-Host "Transport package identity: $($transportBuildTag.PackageId) $($transportBuildTag.PackageVersion)"
    Write-Host "Transport package path: $($transportBuildTag.PackagePath)"
    if ($EmitBuildTag) {
        Write-Host "##vso[build.addbuildtag]$($transportBuildTag.Tag)"
    }
}
