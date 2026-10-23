using System.Text.Json;
using System.Text.Json.Nodes;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// ONEFUZZ - prepare the drop shared by every OneFuzz submission
////////////////////////////////////////////////////////////////////////////////////////////////////

// When set, every OneFuzz target name is prefixed/suffixed so validation runs never collide with the production targets.
var ONEFUZZ_VALIDATION_BUILD_ID = Argument("validationBuildId", EnvironmentVariable("ONEFUZZ_VALIDATION_BUILD_ID"));

Task("onefuzz-prepare")
    .Description("Publish and instrument the OneFuzz harness into ./output/onefuzz.")
    .Does(() =>
{
    string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

    var fuzzingRoot = ROOT_PATH.Combine("tests/SkiaSharp.Fuzzing").FullPath;
    var outputPath = ROOT_PATH.Combine("output/onefuzz").FullPath;

    if (System.IO.Directory.Exists(outputPath))
        System.IO.Directory.Delete(outputPath, recursive: true);

    RunProcess("dotnet", $"publish {Quote(System.IO.Path.Combine(fuzzingRoot, "SkiaSharp.Fuzzing.csproj"))} -c Release -f net10.0 -o {Quote(outputPath)} -p:AllTargetFrameworks=net10.0");

    // The drop is shared, so it must carry the native libraries of every OneFuzz platform.
    foreach (var requiredFile in new[]
    {
        "SkiaSharp.Fuzzing.dll",
        "SkiaSharp.dll",
        "HarfBuzzSharp.dll",
        "SkiaSharp.Resources.dll",
        "SkiaSharp.Skottie.dll",
        "SkiaSharp.SceneGraph.dll",
        "SharpFuzz.Common.dll",
        "libSkiaSharp.dll",
        "libHarfBuzzSharp.dll",
        "libSkiaSharp.so",
        "libHarfBuzzSharp.so"
    })
    {
        if (!System.IO.File.Exists(System.IO.Path.Combine(outputPath, requiredFile)))
            throw new Exception($"The OneFuzz drop is missing required file: {requiredFile}");
    }

    var harness = System.IO.Path.Combine(outputPath, "SkiaSharp.Fuzzing.dll");
    var seeds = System.IO.Path.Combine(outputPath, "seeds");
    RunProcess("dotnet", $"{Quote(harness)} --write-seeds {Quote(seeds)}");
    RunProcess("dotnet", $"{Quote(harness)} --smoke {Quote(seeds)}");

    var instrumentation = new Dictionary<string, string[]>
    {
        ["SkiaSharp.dll"] = new[]
        {
            "SkiaSharp.SKBitmap",
            "SkiaSharp.SKCodec",
            "SkiaSharp.SKFontManager",
            "SkiaSharp.SKFrontBufferedManagedStream",
            "SkiaSharp.SKManagedStream",
            "SkiaSharp.SKPicture"
        },
        ["HarfBuzzSharp.dll"] = new[]
        {
            "HarfBuzzSharp.Buffer",
            "HarfBuzzSharp.Face",
            "HarfBuzzSharp.Font"
        },
        ["SkiaSharp.Resources.dll"] = new[]
        {
            "SkiaSharp.Resources.CachingResourceProvider",
            "SkiaSharp.Resources.DataUriResourceProvider",
            "SkiaSharp.Resources.FileResourceProvider"
        },
        ["SkiaSharp.Skottie.dll"] = new[]
        {
            "SkiaSharp.Skottie.Animation",
            "SkiaSharp.Skottie.AnimationBuilder"
        }
    };

    foreach (var item in instrumentation)
    {
        var arguments = string.Join(" ", new[] { System.IO.Path.Combine(outputPath, item.Key) }.Concat(item.Value).Select(Quote));
        RunProcess("dotnet", $"tool run sharpfuzz -- {arguments}");
    }

    // Each submission ships its own config; the submit job selects one as OneFuzzConfig.json.
    foreach (var configSource in System.IO.Directory.GetFiles(fuzzingRoot, "OneFuzzConfig.*.json"))
    {
        var config = JsonNode.Parse(System.IO.File.ReadAllText(configSource)) ?? throw new Exception($"Failed to load OneFuzz config: {configSource}");
        if (!string.IsNullOrWhiteSpace(ONEFUZZ_VALIDATION_BUILD_ID))
        {
            foreach (var entry in config["Entries"]?.AsArray() ?? new JsonArray())
            {
                foreach (var job in entry?["OneFuzzJobs"]?.AsArray() ?? new JsonArray())
                {
                    var targetName = job?["TargetName"]?.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(targetName))
                        job["TargetName"] = $"validation-{targetName}-{ONEFUZZ_VALIDATION_BUILD_ID}";
                }
            }
        }

        System.IO.File.WriteAllText(
            System.IO.Path.Combine(outputPath, System.IO.Path.GetFileName(configSource)),
            config.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    Information($"Prepared the OneFuzz drop at '{outputPath}'.");
});

Task("Default")
    .IsDependentOn("onefuzz-prepare");

RunTarget(TARGET);
