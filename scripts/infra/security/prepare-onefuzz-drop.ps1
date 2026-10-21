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
$nativeLibrary = if ($Platform -eq 'windows') { 'libSkiaSharp.dll' } else { 'libSkiaSharp.so' }
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
$managedTarget = Join-Path $outputPath 'SkiaSharp.dll'
$nativeTarget = Join-Path $outputPath $nativeLibrary
$sharpFuzzCommon = Join-Path $outputPath 'SharpFuzz.Common.dll'
$seedDirectory = Join-Path $outputPath 'seeds'

$sharpFuzzCommonSource = Get-ChildItem (Split-Path $sharpFuzz -Parent) -Recurse -Force -File -Filter 'SharpFuzz.Common.dll' |
    Where-Object FullName -Match '[\\/]tools[\\/]net10\.0[\\/]any[\\/]SharpFuzz\.Common\.dll$' |
    Select-Object -First 1
if (-not $sharpFuzzCommonSource) {
    throw "Unable to find the .NET 10 SharpFuzz.Common.dll beside '$sharpFuzz'."
}
Copy-Item $sharpFuzzCommonSource.FullName $sharpFuzzCommon

foreach ($requiredFile in @($harness, $managedTarget, $nativeTarget, $sharpFuzzCommon)) {
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

& $sharpFuzz $managedTarget `
    SkiaSharp.SKCodec `
    SkiaSharp.SKFontManager `
    SkiaSharp.SKFrontBufferedManagedStream `
    SkiaSharp.SKManagedStream `
    SkiaSharp.SKPicture
if ($LASTEXITCODE -ne 0) {
    throw "SharpFuzz instrumentation failed with exit code $LASTEXITCODE."
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
