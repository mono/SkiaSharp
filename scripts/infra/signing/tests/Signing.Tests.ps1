$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression
Import-Module (Join-Path $PSScriptRoot '../SkiaSharp.Signing.psm1') -Force

function New-ArchiveBytes {
    param(
        [Parameter(Mandatory)]
        [hashtable] $Entries,

        [switch] $AddSignature
    )

    $memory = [IO.MemoryStream]::new()
    $archive = [IO.Compression.ZipArchive]::new(
        $memory,
        [IO.Compression.ZipArchiveMode]::Create,
        $true)
    try {
        foreach ($path in @($Entries.Keys | Sort-Object)) {
            $entry = $archive.CreateEntry($path)
            $stream = $entry.Open()
            try {
                $value = $Entries[$path]
                $bytes = if ($value -is [byte[]]) {
                    $value
                } else {
                    [Text.Encoding]::UTF8.GetBytes([string] $value)
                }
                $stream.Write($bytes, 0, $bytes.Length)
            } finally {
                $stream.Dispose()
            }
        }

        if ($AddSignature) {
            $signature = $archive.CreateEntry('.signature.p7s')
            $stream = $signature.Open()
            try {
                $bytes = [Text.Encoding]::UTF8.GetBytes('test-signature')
                $stream.Write($bytes, 0, $bytes.Length)
            } finally {
                $stream.Dispose()
            }
        }
    } finally {
        $archive.Dispose()
    }

    $bytes = $memory.ToArray()
    $memory.Dispose()
    return ,$bytes
}

function New-TestPackage {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [switch] $Signed,

        [switch] $Tampered
    )

    $suffix = if ($Signed) { '-signed' } else { '' }
    $nested = New-ArchiveBytes -Entries @{
        'lib/net10.0/HarfBuzzSharp.Subset.dll' = "nested-managed$suffix"
        'content/keep-nested.txt' = 'keep-nested'
    } -AddSignature:$Signed

    $entries = @{
        'lib/net10.0/SkiaSharp.dll' = "managed$suffix"
        'lib/net10.0/SkiaSharp.Views.dll' = "views$suffix"
        'lib/uap10.0/SkiaSharp.Views.WinUI.Native.winmd' = "winmd$suffix"
        'lib/net10.0/HarfBuzzSharp.dll' = "harfbuzz$suffix"
        'runtimes/win-x64/native/libSkiaSharp.dll' = "native-skia$suffix"
        'runtimes/win-x64/native/libHarfBuzzSharp.dll' = "native-harfbuzz$suffix"
        'runtimes/win-x64/native/libEGL.dll' = "egl$suffix"
        'runtimes/win-x64/native/libGLESv2.dll' = "gles$suffix"
        'runtimes/win-x64/native/zlib1.dll' = "zlib$suffix"
        'runtimes/osx/native/libSkiaSharp.dylib' = "mac-skia$suffix"
        'runtimes/osx/native/libHarfBuzzSharp.dylib' = "mac-harfbuzz$suffix"
        'tools/payload.nupkg' = $nested
        'content/keep.txt' = if ($Tampered) { 'tampered' } else { 'keep' }
    }

    [IO.File]::WriteAllBytes(
        $Path,
        (New-ArchiveBytes -Entries $entries -AddSignature:$Signed))
}

$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-signing-tests-$([Guid]::NewGuid().ToString('N'))"
$unsigned = Join-Path $testRoot 'unsigned'
$signed = Join-Path $testRoot 'signed'
$generated = Join-Path $testRoot 'generated'
$signList = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../package/SignList.xml'))
$verifier = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../verify-signed-packages.ps1'))

try {
    New-Item $unsigned -ItemType Directory -Force | Out-Null
    New-Item $signed -ItemType Directory -Force | Out-Null
    New-TestPackage (Join-Path $unsigned 'SkiaSharp.Test.1.0.0.nupkg')
    New-TestPackage (Join-Path $signed 'SkiaSharp.Test.1.0.0.nupkg') -Signed

    $inventory = @(Get-SkiaSharpPackageInventory $unsigned)
    $policy = Get-SkiaSharpSigningPolicy $inventory $signList
    $props = Join-Path $generated 'Signing.props'
    Write-SkiaSharpArcadeSigningProps $policy $props

    [xml] $generatedProps = Get-Content $props -Raw
    $fileSignInfo = @($generatedProps.SelectNodes(
        "/*[local-name()='Project']/*[local-name()='ItemGroup']/*[local-name()='FileSignInfo']"))
    $fileNames = @($fileSignInfo | ForEach-Object { $_.GetAttribute('Include') })
    if (-not ($fileNames -contains 'SkiaSharp.dll')) {
        throw 'Generated Arcade policy did not include SkiaSharp.dll.'
    }
    if (-not ($fileNames -contains 'libSkiaSharp.dylib')) {
        throw 'Generated Arcade policy did not include libSkiaSharp.dylib.'
    }
    if (-not ($fileNames -contains 'SkiaSharp.Views.WinUI.Native.winmd')) {
        throw 'Generated Arcade policy did not include SkiaSharp.Views.WinUI.Native.winmd.'
    }
    $nestedPayloadPath = 'SkiaSharp.Test.1.0.0.nupkg!/tools/payload.nupkg!/lib/net10.0/HarfBuzzSharp.Subset.dll'
    if (-not ($inventory.Path -contains $nestedPayloadPath)) {
        throw 'Recursive package inventory did not include the nested NuGet payload.'
    }
    $policyPaths = @($policy.Files | ForEach-Object { $_.Paths } | ForEach-Object { $_ })
    if (-not ($policyPaths -contains $nestedPayloadPath)) {
        throw 'Signing policy did not classify the nested NuGet payload.'
    }

    & $verifier `
        -OriginalDirectory $unsigned `
        -SignedDirectory $signed `
        -SignListPath $signList `
        -RequireSignatures

    New-TestPackage (Join-Path $signed 'SkiaSharp.Test.1.0.0.nupkg') -Signed -Tampered
    $tamperDetected = $false
    try {
        & $verifier `
            -OriginalDirectory $unsigned `
            -SignedDirectory $signed `
            -SignListPath $signList `
            -RequireSignatures
    } catch {
        $tamperDetected = $true
    }
    if (-not $tamperDetected) {
        throw 'Payload verification did not detect a modified unsigned entry.'
    }

    $unknownInventory = @($inventory) + [pscustomobject]@{
        RootPackage = 'SkiaSharp.Test.1.0.0.nupkg'
        Path = 'SkiaSharp.Test.1.0.0.nupkg!/lib/Unknown.dll'
        Name = 'Unknown.dll'
        Extension = '.dll'
        Length = 1
        Sha256 = '00'
        IsContainer = $false
        IsSignatureMetadata = $false
    }
    $unclassifiedDetected = $false
    try {
        Get-SkiaSharpSigningPolicy $unknownInventory $signList | Out-Null
    } catch {
        $unclassifiedDetected = $true
    }
    if (-not $unclassifiedDetected) {
        throw 'Signing policy did not reject an unclassified DLL.'
    }

    Write-Host 'Signing policy and payload tests passed.'
} finally {
    if (Test-Path $testRoot) {
        Remove-Item $testRoot -Recurse -Force
    }
}
