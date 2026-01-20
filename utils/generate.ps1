param(
    [string]$Config
)

$ErrorActionPreference = "Stop"

$projects = @(
    @{ Json="libSkiaSharp.json";            Root="externals/skia";                                  Output="SkiaSharp/SkiaApi.generated.cs"                  },
    @{ Json="libSkiaSharp.Skottie.json";    Root="externals/skia";                                  Output="SkiaSharp.Skottie/SkottieApi.generated.cs"       },
    @{ Json="libSkiaSharp.SceneGraph.json"; Root="externals/skia";                                  Output="SkiaSharp.SceneGraph/SceneGraphApi.generated.cs" },
    @{ Json="libSkiaSharp.Resources.json";  Root="externals/skia";                                  Output="SkiaSharp.Resources/ResourcesApi.generated.cs"   },
    @{ Json="libHarfBuzzSharp.json";        Root="externals/skia/third_party/externals/harfbuzz";   Output="HarfBuzzSharp/HarfBuzzApi.generated.cs"          } # you may have to revert harfbuzz in Deps to 2.8.2 or 63e15eac4f443fa53565d1e4fb9611cdd7814f28
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

New-Item -ItemType Directory -Force -Path "output/generated/" | Out-Null

dotnet build utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj

foreach ($proj in $projects) {
    $json = $proj.Json;
    $output = $proj.Output;
    $root = $proj.Root;
    $filename = Split-Path $output -Leaf

    $cmd = "dotnet run --no-build --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config binding/$json --root $root --output binding/$output"
    Write-Host $cmd
    Invoke-Expression $cmd
    if (!$?) {
        exit $LASTEXITCODE
    }

    Copy-Item -Path binding/$output -Destination output/generated/$filename -Force
}
