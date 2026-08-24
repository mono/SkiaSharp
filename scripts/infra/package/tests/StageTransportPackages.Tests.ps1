$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

$scriptPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../stage-transport-packages.ps1'))
$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-transport-tests-$([Guid]::NewGuid().ToString('N'))"

function New-TestPackage {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [switch] $Signed
    )

    $stream = [IO.File]::Create($Path)
    $archive = [IO.Compression.ZipArchive]::new(
        $stream,
        [IO.Compression.ZipArchiveMode]::Create,
        $false)
    try {
        $entry = $archive.CreateEntry('content/payload.txt')
        $entryStream = $entry.Open()
        try {
            $bytes = [Text.Encoding]::UTF8.GetBytes('payload')
            $entryStream.Write($bytes, 0, $bytes.Length)
        } finally {
            $entryStream.Dispose()
        }

        if ($Signed) {
            $signature = $archive.CreateEntry('.signature.p7s')
            $signatureStream = $signature.Open()
            try {
                $bytes = [Text.Encoding]::UTF8.GetBytes('signature')
                $signatureStream.Write($bytes, 0, $bytes.Length)
            } finally {
                $signatureStream.Dispose()
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
        [string] $Name,

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
        throw "$Name was not rejected."
    }
}

try {
    $source = Join-Path $testRoot 'source'
    $destination = Join-Path $testRoot 'destination'
    New-Item $source -ItemType Directory -Force | Out-Null

    New-TestPackage (Join-Path $source '_NuGets.0.0.0-commit.abc.1.nupkg')
    New-TestPackage (Join-Path $source '_NativeAssets.android.0.0.0-commit.abc.1.nupkg')

    & $scriptPath -SourceDirectory $source -DestinationDirectory $destination
    $staged = @(Get-ChildItem $destination -Filter '*.nupkg' -File)
    if ($staged.Count -ne 2) {
        throw "Expected two staged transport packages, found $($staged.Count)."
    }

    New-TestPackage (Join-Path $source 'SkiaSharp.1.0.0.nupkg')
    Assert-Rejected 'A shipping package' {
        & $scriptPath -SourceDirectory $source -DestinationDirectory $destination
    }
    Remove-Item (Join-Path $source 'SkiaSharp.1.0.0.nupkg')

    New-TestPackage (Join-Path $source '_Symbols.1.0.0.symbols.nupkg')
    Assert-Rejected 'A symbol package' {
        & $scriptPath -SourceDirectory $source -DestinationDirectory $destination
    }
    Remove-Item (Join-Path $source '_Symbols.1.0.0.symbols.nupkg')

    New-TestPackage (Join-Path $source '_Signed.1.0.0.nupkg') -Signed
    Assert-Rejected 'A signed transport package' {
        & $scriptPath -SourceDirectory $source -DestinationDirectory $destination
    }

    Write-Host 'Transport package staging tests passed.'
} finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction Ignore
}
