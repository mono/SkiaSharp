param(
    [string]$Config
)

$ErrorActionPreference = "Stop"

$projects = @(
    @{ Json="libSkiaSharp.json";            Root="externals/skia";                                  Output="SkiaSharp/Generated"            },
    @{ Json="libSkiaSharp.Skottie.json";    Root="externals/skia";                                  Output="SkiaSharp.Skottie/Generated"    },
    @{ Json="libSkiaSharp.SceneGraph.json"; Root="externals/skia";                                  Output="SkiaSharp.SceneGraph/Generated" },
    @{ Json="libSkiaSharp.Resources.json";  Root="externals/skia";                                  Output="SkiaSharp.Resources/Generated"  },
    @{ Json="libHarfBuzzSharp.json";        Root="externals/skia/third_party/externals/harfbuzz";   Output="HarfBuzzSharp/Generated"        }
)

# Filter to specific config if provided
if ($Config) {
    $configName = Split-Path $Config -Leaf
    $projects = $projects | Where-Object { $_.Json -eq $configName }
    if ($projects.Count -eq 0) {
        Write-Error "Config not found: $Config. Valid options: libSkiaSharp.json, libSkiaSharp.Skottie.json, libSkiaSharp.SceneGraph.json, libSkiaSharp.Resources.json, libHarfBuzzSharp.json"
        exit 1
    }
}

dotnet build utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj

$failed = $false
foreach ($proj in $projects) {
    $json = $proj.Json;
    $output = $proj.Output;
    $root = $proj.Root;

    $runArgs = @("run", "--no-build", "--no-launch-profile",
              "--project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj",
              "--", "generate",
              "--config", "binding/$json",
              "--root", $root,
              "--output", "binding/$output")
    Write-Host "dotnet $($runArgs -join ' ')"
    & dotnet @runArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Generation failed for $json with exit code $LASTEXITCODE"
        $failed = $true
        continue
    }

}

if ($failed) {
    Write-Host "ERROR: One or more generation steps failed"
    exit 1
}
