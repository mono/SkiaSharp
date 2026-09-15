$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

. (Join-Path $PSScriptRoot '../add-transport-build-tag.ps1')

function New-Package {
    param(
        [Parameter(Mandatory)]
        [string] $Directory,

        [Parameter(Mandatory)]
        [string] $FileName,

        [Parameter(Mandatory)]
        [string] $Id,

        [Parameter(Mandatory)]
        [string] $Version
    )

    $path = Join-Path $Directory $FileName
    $archive = [IO.Compression.ZipFile]::Open($path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $entry = $archive.CreateEntry("$Id.nuspec")
        $writer = [IO.StreamWriter]::new($entry.Open())
        try {
            $writer.Write("<package xmlns=`"http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd`"><metadata><id>$Id</id><version>$Version</version></metadata></package>")
        } finally {
            $writer.Dispose()
        }
    } finally {
        $archive.Dispose()
    }
}

function Assert-Throws {
    param(
        [Parameter(Mandatory)]
        [scriptblock] $Action,

        [Parameter(Mandatory)]
        [string] $Message
    )

    try {
        & $Action
    } catch {
        if ($_.Exception.Message -notlike "*$Message*") {
            throw "Expected error containing '$Message', received '$($_.Exception.Message)'."
        }
        return
    }
    throw "Expected an error containing '$Message'."
}

$root = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-transport-build-tag-$([Guid]::NewGuid().ToString('N'))"
try {
    New-Item $root -ItemType Directory -Force | Out-Null

    New-Package $root '_NuGets.0.0.0-branch.feature.alpha-1.177.nupkg' '_NuGets' '0.0.0-branch.feature.alpha-1.177'
    $tag = Get-TransportBuildTag -PackageDirectory $root
    if ($tag.Tag -ne 'Transport - branch.feature.alpha-1.177') {
        throw "Unexpected branch transport tag: '$($tag.Tag)'."
    }

    Remove-Item (Join-Path $root '*') -Force
    New-Package $root '_NuGets.0.0.0-pr.123.177.nupkg' '_NuGets' '0.0.0-pr.123.177'
    $tag = Get-TransportBuildTag -PackageDirectory $root
    if ($tag.Tag -ne 'Transport - pr.123.177') {
        throw "Unexpected PR transport tag: '$($tag.Tag)'."
    }

    Remove-Item (Join-Path $root '*') -Force
    New-Package $root '_NuGets.Dependencies.1.0.0.0-branch.main.177.nupkg' '_NuGets.Dependencies.1' '0.0.0-branch.main.177'
    Assert-Throws { Get-TransportBuildTag -PackageDirectory $root } 'Expected exactly one _NuGets transport package'

    New-Package $root '_NuGets.0.0.0-branch.main.177.nupkg' '_NuGets' '0.0.0-branch.main.177'
    New-Package $root '_NuGets.duplicate.0.0.0-branch.main.177.nupkg' '_NuGets' '0.0.0-branch.main.177'
    Assert-Throws { Get-TransportBuildTag -PackageDirectory $root } 'Expected exactly one _NuGets transport package'

    Remove-Item (Join-Path $root '*') -Force
    New-Package $root '_NuGets.1.2.3.nupkg' '_NuGets' '1.2.3'
    Assert-Throws { Get-TransportBuildTag -PackageDirectory $root } 'has unsupported version'

    Write-Host 'Transport build tag tests passed.'
} finally {
    Remove-Item $root -Recurse -Force -ErrorAction Ignore
}
