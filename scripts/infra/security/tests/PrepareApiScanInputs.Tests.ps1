$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$scriptPath = Join-Path $repoRoot 'scripts/infra/security/prepare-apiscan-inputs.ps1'
$surrogateSource = Join-Path $repoRoot 'scripts/infra/security/APIScanSurrogates.in.xml'
$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-apiscan-tests-$([Guid]::NewGuid().ToString('N'))"
$scanRoot = Join-Path $testRoot 'scan'

function New-TestPackage {
    param(
        [Parameter(Mandatory)]
        [string] $Id,

        [Parameter(Mandatory)]
        [string[]] $Files
    )

    $version = '1.0.0-preview.1'
    $packageFolder = Join-Path $scanRoot 'packages/nuget_symbols'
    New-Item $packageFolder -ItemType Directory -Force | Out-Null
    $path = Join-Path $packageFolder "$Id.$version.symbols.nupkg"
    $stream = [IO.File]::Create($path)
    $archive = [IO.Compression.ZipArchive]::new(
        $stream,
        [IO.Compression.ZipArchiveMode]::Create)
    try {
        $nuspecEntry = $archive.CreateEntry("$Id.nuspec")
        $writer = [IO.StreamWriter]::new($nuspecEntry.Open())
        try {
            $writer.Write(
                "<package><metadata><id>$Id</id><version>$version</version></metadata></package>")
        } finally {
            $writer.Dispose()
        }

        foreach ($file in $Files) {
            $entry = $archive.CreateEntry($file)
            $writer = [IO.StreamWriter]::new($entry.Open())
            try {
                $writer.Write("test payload for $file")
            } finally {
                $writer.Dispose()
            }
        }
    } finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

function Add-NativePair {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[string]] $Files,

        [Parameter(Mandatory)]
        [string] $Runtime,

        [Parameter(Mandatory)]
        [string] $Name
    )

    $Files.Add("runtimes/$Runtime/native/$Name.dll")
    $Files.Add("runtimes/$Runtime/native/$Name.pdb")
}

try {
    $win32Files = [Collections.Generic.List[string]]::new()
    foreach ($runtime in @('win-x86', 'win-x64', 'win-arm64')) {
        Add-NativePair $win32Files $runtime 'libSkiaSharp'
    }
    New-TestPackage 'SkiaSharp.NativeAssets.Win32' $win32Files

    $harfBuzzFiles = [Collections.Generic.List[string]]::new()
    Add-NativePair $harfBuzzFiles 'win-arm64' 'libHarfBuzzSharp'
    New-TestPackage 'HarfBuzzSharp.NativeAssets.Win32' $harfBuzzFiles

    $nanoServerFiles = [Collections.Generic.List[string]]::new()
    foreach ($runtime in @('win-x86', 'win-x64', 'win-arm64')) {
        Add-NativePair $nanoServerFiles $runtime 'libSkiaSharp'
    }
    New-TestPackage 'SkiaSharp.NativeAssets.NanoServer' $nanoServerFiles

    $winUiFiles = [Collections.Generic.List[string]]::new()
    foreach ($runtime in @('win-x64', 'win-arm64')) {
        foreach ($name in @('libEGL', 'libGLESv2', 'SkiaSharp.Views.WinUI.Native')) {
            Add-NativePair $winUiFiles $runtime $name
        }
    }
    New-TestPackage 'SkiaSharp.NativeAssets.WinUI' $winUiFiles

    $msvcX86 = Join-Path $scanRoot 'native_msvc/native/windows/x86'
    $msvcX64 = Join-Path $scanRoot 'native_msvc/native/windows/x64'
    New-Item $msvcX86, $msvcX64 -ItemType Directory -Force | Out-Null
    foreach ($path in @(
        (Join-Path $msvcX86 'libskiasharp.dll')
        (Join-Path $msvcX86 'libskiasharp.pdb')
        (Join-Path $msvcX64 'libskiasharp.dll')
        (Join-Path $msvcX64 'libskiasharp.pdb')
        (Join-Path $msvcX64 'libharfbuzzsharp.dll')
        (Join-Path $msvcX64 'libharfbuzzsharp.pdb')
    )) {
        Set-Content $path 'test payload'
    }

    & $scriptPath -ScanRoot $scanRoot -SurrogateSource $surrogateSource

    $generatedSurrogates = Join-Path $scanRoot 'surrogates/APIScanSurrogates.xml'
    [xml] $configuration = Get-Content $generatedSurrogates -Raw
    $mappings = @($configuration.APIScanSurrogates.Mappings.Mapping)
    if ($mappings.Count -ne 10) {
        throw "Expected 10 API Scan mappings, found $($mappings.Count)."
    }
    if ((Get-Content $generatedSurrogates -Raw).Contains('{SOFTWARE_FOLDER}')) {
        throw 'The generated surrogate configuration still contains its path placeholder.'
    }

    $copiedSurrogates = @(Get-ChildItem (Join-Path $scanRoot 'surrogate-binaries') -File)
    if ($copiedSurrogates.Count -ne 6) {
        throw "Expected six copied WinUI surrogate files, found $($copiedSurrogates.Count)."
    }

    $emptyScanRoot = Join-Path $testRoot 'empty'
    New-Item (Join-Path $emptyScanRoot 'packages/nuget_symbols') -ItemType Directory -Force | Out-Null
    $missingInputsRejected = $false
    try {
        & $scriptPath -ScanRoot $emptyScanRoot -SurrogateSource $surrogateSource
    } catch {
        $missingInputsRejected = $_.Exception.Message -like '*No API Scan NuGet packages*'
    }
    if (-not $missingInputsRejected) {
        throw 'Missing API Scan package inputs were not rejected.'
    }

    Write-Host 'API Scan input preparation tests passed.'
} finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction Ignore
}
