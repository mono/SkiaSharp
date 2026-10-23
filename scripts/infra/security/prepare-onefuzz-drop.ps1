[CmdletBinding()]
param(
    [string] $OutputPath = (Join-Path $PSScriptRoot '../../../output/onefuzz'),

    # When set, every OneFuzz target name is prefixed/suffixed so validation runs never
    # collide with the production targets.
    [string] $ValidationBuildId = $env:ONEFUZZ_VALIDATION_BUILD_ID
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$outputPath = [IO.Path]::GetFullPath($OutputPath)
$fuzzingRoot = Join-Path $repoRoot 'tests/SkiaSharp.Fuzzing'

function Invoke-Dotnet {
    # No param block: arguments such as -o must reach dotnet instead of binding to PowerShell parameters.
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "'dotnet $($args -join ' ')' failed with exit code $LASTEXITCODE."
    }
}

if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}

Push-Location $repoRoot
try {
    Invoke-Dotnet tool restore

    Invoke-Dotnet publish (Join-Path $fuzzingRoot 'SkiaSharp.Fuzzing.csproj') `
        -c Release -f net10.0 -o $outputPath '-p:AllTargetFrameworks=net10.0'

    # The drop is shared, so it must carry the native libraries of every OneFuzz platform.
    $requiredFiles = @(
        'SkiaSharp.Fuzzing.dll'
        'SkiaSharp.dll'
        'HarfBuzzSharp.dll'
        'SkiaSharp.Resources.dll'
        'SkiaSharp.Skottie.dll'
        'SkiaSharp.SceneGraph.dll'
        'SharpFuzz.Common.dll'
        'libSkiaSharp.dll'
        'libHarfBuzzSharp.dll'
        'libSkiaSharp.so'
        'libHarfBuzzSharp.so'
    )
    foreach ($file in $requiredFiles) {
        if (-not (Test-Path (Join-Path $outputPath $file) -PathType Leaf)) {
            throw "The OneFuzz drop is missing required file: $file"
        }
    }

    $harness = Join-Path $outputPath 'SkiaSharp.Fuzzing.dll'
    $seeds = Join-Path $outputPath 'seeds'
    Invoke-Dotnet $harness --write-seeds $seeds
    Invoke-Dotnet $harness --smoke $seeds

    $instrumentation = [ordered]@{
        'SkiaSharp.dll' = @(
            'SkiaSharp.SKBitmap'
            'SkiaSharp.SKCodec'
            'SkiaSharp.SKFontManager'
            'SkiaSharp.SKFrontBufferedManagedStream'
            'SkiaSharp.SKManagedStream'
            'SkiaSharp.SKPicture'
        )
        'HarfBuzzSharp.dll' = @(
            'HarfBuzzSharp.Buffer'
            'HarfBuzzSharp.Face'
            'HarfBuzzSharp.Font'
        )
        'SkiaSharp.Resources.dll' = @(
            'SkiaSharp.Resources.CachingResourceProvider'
            'SkiaSharp.Resources.DataUriResourceProvider'
            'SkiaSharp.Resources.FileResourceProvider'
        )
        'SkiaSharp.Skottie.dll' = @(
            'SkiaSharp.Skottie.Animation'
            'SkiaSharp.Skottie.AnimationBuilder'
        )
    }
    foreach ($assembly in $instrumentation.Keys) {
        Invoke-Dotnet tool run sharpfuzz -- (Join-Path $outputPath $assembly) @($instrumentation[$assembly])
    }
}
finally {
    Pop-Location
}

# Each platform ships its own config; the submit job selects one as OneFuzzConfig.json.
foreach ($configFile in Get-ChildItem $fuzzingRoot -Filter 'OneFuzzConfig.*.json') {
    $config = Get-Content $configFile.FullName -Raw | ConvertFrom-Json
    if ($ValidationBuildId) {
        foreach ($entry in $config.Entries) {
            foreach ($job in $entry.OneFuzzJobs) {
                $job.TargetName = "validation-$($job.TargetName)-$ValidationBuildId"
            }
        }
    }
    $config | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $outputPath $configFile.Name)
}

Write-Host "Prepared the OneFuzz drop at '$outputPath'."
