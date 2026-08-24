$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

function New-TestPackage {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [switch] $Signed,

        [string] $Payload = 'payload'
    )

    $stream = [IO.File]::Create($Path)
    $archive = [IO.Compression.ZipArchive]::new(
        $stream,
        [IO.Compression.ZipArchiveMode]::Create,
        $false)
    try {
        $entry = $archive.CreateEntry('content/payload.txt')
        $writer = [IO.StreamWriter]::new($entry.Open())
        try {
            $writer.Write($Payload)
        } finally {
            $writer.Dispose()
        }

        if ($Signed) {
            $signature = $archive.CreateEntry('.signature.p7s')
            $writer = [IO.StreamWriter]::new($signature.Open())
            try {
                $writer.Write('signature')
            } finally {
                $writer.Dispose()
            }
        }
    } finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

function Assert-Rejected {
    param(
        [Parameter(Mandatory)]
        [string] $Description,

        [Parameter(Mandatory)]
        [scriptblock] $Action
    )

    $rejected = $false
    try {
        & $Action
    } catch {
        $rejected = $true
    }
    if (-not $rejected) {
        throw "$Description was not rejected."
    }
}

$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-unsigned-transport-tests-$([Guid]::NewGuid().ToString('N'))"
$original = Join-Path $testRoot 'original'
$staged = Join-Path $testRoot 'staged'
$output = Join-Path $testRoot 'verification/result.json'
$verifier = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../verify-unsigned-transport-packages.ps1'))

try {
    New-Item $original -ItemType Directory -Force | Out-Null
    New-Item $staged -ItemType Directory -Force | Out-Null
    foreach ($name in @(
        '_NuGets.0.0.0-commit.abc.1.nupkg'
        '_NativeAssets.android.0.0.0-commit.abc.1.nupkg')) {
        New-TestPackage (Join-Path $original $name)
        Copy-Item (Join-Path $original $name) (Join-Path $staged $name)
    }

    & $verifier `
        -OriginalDirectory $original `
        -StagedDirectory $staged `
        -OutputPath $output
    $result = Get-Content $output -Raw | ConvertFrom-Json
    if ($result.packageCount -ne 2 -or
        -not $result.packagesRemainUnsigned -or
        -not $result.packagesRemainByteIdentical) {
        throw 'Unsigned transport verification result is incomplete.'
    }

    New-TestPackage (Join-Path $staged '_NuGets.0.0.0-commit.abc.1.nupkg') -Payload 'changed'
    Assert-Rejected 'A modified transport package' {
        & $verifier -OriginalDirectory $original -StagedDirectory $staged
    }
    Copy-Item `
        (Join-Path $original '_NuGets.0.0.0-commit.abc.1.nupkg') `
        (Join-Path $staged '_NuGets.0.0.0-commit.abc.1.nupkg') `
        -Force

    New-TestPackage (Join-Path $staged '_NativeAssets.android.0.0.0-commit.abc.1.nupkg') -Signed
    Assert-Rejected 'A signed transport package' {
        & $verifier -OriginalDirectory $original -StagedDirectory $staged
    }
    Copy-Item `
        (Join-Path $original '_NativeAssets.android.0.0.0-commit.abc.1.nupkg') `
        (Join-Path $staged '_NativeAssets.android.0.0.0-commit.abc.1.nupkg') `
        -Force

    Remove-Item (Join-Path $staged '_NuGets.0.0.0-commit.abc.1.nupkg')
    Assert-Rejected 'A missing staged transport package' {
        & $verifier -OriginalDirectory $original -StagedDirectory $staged
    }

    Write-Host 'Unsigned transport package tests passed.'
} finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
