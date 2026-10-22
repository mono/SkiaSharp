[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('windows', 'ubuntu')]
    [string] $Platform,

    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [Parameter(Mandatory)]
    [string] $SharpFuzzPath,

    [string] $Configuration = 'Release',

    [string] $ValidationBuildId
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$project = Join-Path $repoRoot 'tests/SkiaSharp.Fuzzing/SkiaSharp.Fuzzing.csproj'
$outputPath = [IO.Path]::GetFullPath($OutputDirectory)
$skiaNativeLibrary = if ($Platform -eq 'windows') { 'libSkiaSharp.dll' } else { 'libSkiaSharp.so' }
$harfBuzzNativeLibrary = if ($Platform -eq 'windows') { 'libHarfBuzzSharp.dll' } else { 'libHarfBuzzSharp.so' }
$configSource = Join-Path $repoRoot "tests/SkiaSharp.Fuzzing/OneFuzzConfig.$Platform.json"
$sharpFuzz = (Resolve-Path $SharpFuzzPath).Path

if (Test-Path $outputPath) {
    throw "The OneFuzz output directory already exists: $outputPath"
}

& dotnet publish $project `
    -c $Configuration `
    -f net10.0 `
    -o $outputPath
if ($LASTEXITCODE -ne 0) {
    throw "Publishing the OneFuzz harness failed with exit code $LASTEXITCODE."
}

$harness = Join-Path $outputPath 'SkiaSharp.Fuzzing.dll'
$skiaManagedTarget = Join-Path $outputPath 'SkiaSharp.dll'
$harfBuzzManagedTarget = Join-Path $outputPath 'HarfBuzzSharp.dll'
$resourcesManagedTarget = Join-Path $outputPath 'SkiaSharp.Resources.dll'
$skottieManagedTarget = Join-Path $outputPath 'SkiaSharp.Skottie.dll'
$sceneGraphManagedTarget = Join-Path $outputPath 'SkiaSharp.SceneGraph.dll'
$skiaNativeTarget = Join-Path $outputPath $skiaNativeLibrary
$harfBuzzNativeTarget = Join-Path $outputPath $harfBuzzNativeLibrary
$sharpFuzzCommon = Join-Path $outputPath 'SharpFuzz.Common.dll'
$seedDirectory = Join-Path $outputPath 'seeds'

$sharpFuzzCommonSource = Get-ChildItem (Split-Path $sharpFuzz -Parent) -Recurse -Force -File -Filter 'SharpFuzz.Common.dll' |
    Where-Object FullName -Match '[\\/]tools[\\/]net10\.0[\\/]any[\\/]SharpFuzz\.Common\.dll$' |
    Select-Object -First 1
if (-not $sharpFuzzCommonSource) {
    throw "Unable to find the .NET 10 SharpFuzz.Common.dll beside '$sharpFuzz'."
}
Copy-Item $sharpFuzzCommonSource.FullName $sharpFuzzCommon

foreach ($requiredFile in @(
    $harness,
    $skiaManagedTarget,
    $harfBuzzManagedTarget,
    $resourcesManagedTarget,
    $skottieManagedTarget,
    $sceneGraphManagedTarget,
    $skiaNativeTarget,
    $harfBuzzNativeTarget,
    $sharpFuzzCommon
)) {
    if (-not (Test-Path $requiredFile -PathType Leaf)) {
        throw "The OneFuzz drop is missing required file: $requiredFile"
    }
}

& dotnet $harness --write-seeds $seedDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Generating the OneFuzz seed corpus failed with exit code $LASTEXITCODE."
}

& dotnet $harness --smoke $seedDirectory
if ($LASTEXITCODE -ne 0) {
    throw "The uninstrumented OneFuzz smoke run failed with exit code $LASTEXITCODE."
}

$instrumentationTargets = @(
    @{
        Assembly = $skiaManagedTarget
        Types = @(
            'SkiaSharp.SKBitmap',
            'SkiaSharp.SKCodec',
            'SkiaSharp.SKFontManager',
            'SkiaSharp.SKFrontBufferedManagedStream',
            'SkiaSharp.SKManagedStream',
            'SkiaSharp.SKPicture'
        )
    },
    @{
        Assembly = $harfBuzzManagedTarget
        Types = @(
            'HarfBuzzSharp.Buffer',
            'HarfBuzzSharp.Face',
            'HarfBuzzSharp.Font'
        )
    },
    @{
        Assembly = $resourcesManagedTarget
        Types = @(
            'SkiaSharp.Resources.CachingResourceProvider',
            'SkiaSharp.Resources.DataUriResourceProvider',
            'SkiaSharp.Resources.FileResourceProvider'
        )
    },
    @{
        Assembly = $skottieManagedTarget
        Types = @(
            'SkiaSharp.Skottie.Animation',
            'SkiaSharp.Skottie.AnimationBuilder'
        )
    }
)

foreach ($target in $instrumentationTargets) {
    $arguments = @($target.Assembly) + $target.Types
    & $sharpFuzz @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "SharpFuzz instrumentation failed for '$($target.Assembly)' with exit code $LASTEXITCODE."
    }
}

$config = Get-Content $configSource -Raw | ConvertFrom-Json
if ($ValidationBuildId) {
    foreach ($entry in $config.Entries) {
        foreach ($job in $entry.OneFuzzJobs) {
            $job.TargetName = "validation-$($job.TargetName)-$ValidationBuildId"
        }
    }
}

$config | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $outputPath 'OneFuzzConfig.json') -Encoding utf8

$seedSources = Join-Path $outputPath 'seed-sources'
if (Test-Path $seedSources) {
    Remove-Item $seedSources -Recurse -Force
}

Write-Host "Prepared the $Platform OneFuzz drop at '$outputPath'."
