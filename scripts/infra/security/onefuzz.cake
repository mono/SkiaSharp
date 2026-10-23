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

    // Each platform ships its own config, generated from one template; the submit job selects one as OneFuzzConfig.json.
    var platforms = new[]
    {
        new { Platform = "windows", NativeExtension = "dll", DependencyPath = ".\\*" },
        new { Platform = "ubuntu", NativeExtension = "so", DependencyPath = "./*" },
        new { Platform = "azurelinux", NativeExtension = "so", DependencyPath = "./*" }
    };

    var template = JsonNode.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(fuzzingRoot, "OneFuzzConfig.template.json")))!;
    foreach (var platform in platforms)
    {
        var entries = new JsonArray();
        foreach (var target in template["Targets"]!.AsArray())
        {
            var name = ONEFUZZ_VALIDATION_BUILD_ID is { Length: > 0 }
                ? $"validation-{target!["Name"]}"
                : target!["Name"]!.GetValue<string>();
            var suffix = ONEFUZZ_VALIDATION_BUILD_ID is { Length: > 0 } ? $"-{ONEFUZZ_VALIDATION_BUILD_ID}" : "";

            string Escape(string value) => JsonEncodedText.Encode(value).ToString();
            var entry = template["Entry"]!.ToJsonString()
                .Replace("{Method}", target["Method"]!.GetValue<string>())
                .Replace("{Name}", Escape(name))
                .Replace("{Platform}", platform.Platform)
                .Replace("{NativeExtension}", platform.NativeExtension)
                .Replace("{DependencyPath}", Escape(platform.DependencyPath));

            var entryNode = JsonNode.Parse(entry)!;
            if (suffix.Length > 0)
            {
                var job = entryNode["OneFuzzJobs"]![0]!;
                job["TargetName"] = job["TargetName"]!.GetValue<string>() + suffix;
            }
            entries.Add(entryNode);
        }

        var config = new JsonObject { ["ConfigVersion"] = 3, ["Entries"] = entries };
        System.IO.File.WriteAllText(
            System.IO.Path.Combine(outputPath, $"OneFuzzConfig.{platform.Platform}.json"),
            config.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    Information($"Prepared the OneFuzz drop at '{outputPath}'.");
});

Task("Default")
    .IsDependentOn("onefuzz-prepare");

RunTarget(TARGET);
