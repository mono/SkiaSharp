$ErrorActionPreference = "Stop"

$projects = @(
    @{ Json="libSkiaSharp.json";            Root="externals/skia";                                  Output="SkiaSharp/SkiaApi.generated.cs"                  },
    @{ Json="libSkiaSharp.Skottie.json";    Root="externals/skia";                                  Output="SkiaSharp.Skottie/SkottieApi.generated.cs"       },
    @{ Json="libSkiaSharp.SceneGraph.json"; Root="externals/skia";                                  Output="SkiaSharp.SceneGraph/SceneGraphApi.generated.cs" },
    @{ Json="libSkiaSharp.Resources.json";  Root="externals/skia";                                  Output="SkiaSharp.Resources/ResourcesApi.generated.cs"   },
    @{ Json="libHarfBuzzSharp.json";        Root="externals/skia/third_party/externals/harfbuzz";   Output="HarfBuzzSharp/HarfBuzzApi.generated.cs"          }
)

New-Item -ItemType Directory -Force -Path "output/generated/" | Out-Null

foreach ($proj in $projects) {
    $json = $proj.Json;
    $output = $proj.Output;
    $root = $proj.Root;
    $filename = Split-Path $output -Leaf

    dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config binding/$json --root $root --output binding/$output
    if (!$?) {
        exit $LASTEXITCODE
    }

    Copy-Item -Path binding/$output -Destination output/generated/$filename -Force
}
