using System.Text.Json;
using System.Text.Json.Nodes;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// ONEFUZZ - prepare validation drop
////////////////////////////////////////////////////////////////////////////////////////////////////

var ONEFUZZ_PLATFORM = Argument("platform", EnvironmentVariable("ONEFUZZ_PLATFORM"));
var ONEFUZZ_OUTPUT_DIRECTORY = Argument("outputDirectory", EnvironmentVariable("ONEFUZZ_OUTPUT_DIRECTORY"));
var ONEFUZZ_SHARPFUZZ_PATH = Argument("sharpFuzzPath", EnvironmentVariable("ONEFUZZ_SHARPFUZZ_PATH"));
var ONEFUZZ_CONFIGURATION = Argument("configuration", EnvironmentVariable("ONEFUZZ_CONFIGURATION") ?? "Release");
var ONEFUZZ_VALIDATION_BUILD_ID = Argument("validationBuildId", EnvironmentVariable("ONEFUZZ_VALIDATION_BUILD_ID"));

Task("onefuzz-prepare")
    .Description("Prepare the OneFuzz harness drop for Windows or Ubuntu validation runs.")
    .Does(() =>
{
    if (string.IsNullOrWhiteSpace(ONEFUZZ_PLATFORM))
        throw new Exception("The 'platform' argument is required (windows|ubuntu).");
    if (!new[] { "windows", "ubuntu" }.Contains(ONEFUZZ_PLATFORM, StringComparer.OrdinalIgnoreCase))
        throw new Exception($"Invalid platform '{ONEFUZZ_PLATFORM}'. Expected 'windows' or 'ubuntu'.");
    if (string.IsNullOrWhiteSpace(ONEFUZZ_OUTPUT_DIRECTORY))
        throw new Exception("The 'outputDirectory' argument is required.");
    if (string.IsNullOrWhiteSpace(ONEFUZZ_SHARPFUZZ_PATH))
        throw new Exception("The 'sharpFuzzPath' argument is required.");

    string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

    var projectPath = ROOT_PATH.CombineWithFilePath("tests/SkiaSharp.Fuzzing/SkiaSharp.Fuzzing.csproj").FullPath;
    var outputPath = System.IO.Path.GetFullPath(ONEFUZZ_OUTPUT_DIRECTORY);
    var sharpFuzzPath = System.IO.Path.GetFullPath(ONEFUZZ_SHARPFUZZ_PATH);
    var configSource = ROOT_PATH.CombineWithFilePath($"tests/SkiaSharp.Fuzzing/OneFuzzConfig.{ONEFUZZ_PLATFORM.ToLowerInvariant()}.json").FullPath;
    var skiaNativeLibrary = ONEFUZZ_PLATFORM.Equals("windows", StringComparison.OrdinalIgnoreCase) ? "libSkiaSharp.dll" : "libSkiaSharp.so";
    var harfBuzzNativeLibrary = ONEFUZZ_PLATFORM.Equals("windows", StringComparison.OrdinalIgnoreCase) ? "libHarfBuzzSharp.dll" : "libHarfBuzzSharp.so";

    if (!System.IO.File.Exists(sharpFuzzPath))
        throw new Exception($"The SharpFuzz tool path does not exist: {sharpFuzzPath}");
    if (System.IO.Directory.Exists(outputPath))
        throw new Exception($"The OneFuzz output directory already exists: {outputPath}");

    RunProcess("dotnet", string.Join(" ", new[]
    {
        "publish",
        Quote(projectPath),
        "-c", Quote(ONEFUZZ_CONFIGURATION),
        "-f", "net10.0",
        "-o", Quote(outputPath),
        "-p:AllTargetFrameworks=net10.0",
        "-p:IsCI=false",
        "-p:IsNetTizenSupported=false",
        "-p:IsNetTVOSSupported=false",
        "-p:IsNetMacOSSupported=false",
    }));

    var harness = System.IO.Path.Combine(outputPath, "SkiaSharp.Fuzzing.dll");
    var skiaManagedTarget = System.IO.Path.Combine(outputPath, "SkiaSharp.dll");
    var harfBuzzManagedTarget = System.IO.Path.Combine(outputPath, "HarfBuzzSharp.dll");
    var resourcesManagedTarget = System.IO.Path.Combine(outputPath, "SkiaSharp.Resources.dll");
    var skottieManagedTarget = System.IO.Path.Combine(outputPath, "SkiaSharp.Skottie.dll");
    var sceneGraphManagedTarget = System.IO.Path.Combine(outputPath, "SkiaSharp.SceneGraph.dll");
    var skiaNativeTarget = System.IO.Path.Combine(outputPath, skiaNativeLibrary);
    var harfBuzzNativeTarget = System.IO.Path.Combine(outputPath, harfBuzzNativeLibrary);
    var sharpFuzzCommonTarget = System.IO.Path.Combine(outputPath, "SharpFuzz.Common.dll");
    var seedDirectory = System.IO.Path.Combine(outputPath, "seeds");

    var sharpFuzzToolDirectory = System.IO.Path.GetDirectoryName(sharpFuzzPath) ?? throw new Exception($"Unable to determine the SharpFuzz tool directory from '{sharpFuzzPath}'.");
    var sharpFuzzCommonSource = System.IO.Directory
        .GetFiles(sharpFuzzToolDirectory, "SharpFuzz.Common.dll", System.IO.SearchOption.AllDirectories)
        .FirstOrDefault(path =>
        {
            var normalized = path.Replace('\\', '/');
            return Regex.IsMatch(normalized, @"/tools/net10\.0/any/SharpFuzz\.Common\.dll$", RegexOptions.CultureInvariant);
        });

    if (string.IsNullOrWhiteSpace(sharpFuzzCommonSource))
        throw new Exception($"Unable to find the .NET 10 SharpFuzz.Common.dll beside '{sharpFuzzPath}'.");

    System.IO.File.Copy(sharpFuzzCommonSource, sharpFuzzCommonTarget);

    foreach (var requiredFile in new[]
    {
        harness,
        skiaManagedTarget,
        harfBuzzManagedTarget,
        resourcesManagedTarget,
        skottieManagedTarget,
        sceneGraphManagedTarget,
        skiaNativeTarget,
        harfBuzzNativeTarget,
        sharpFuzzCommonTarget
    })
    {
        if (!System.IO.File.Exists(requiredFile))
            throw new Exception($"The OneFuzz drop is missing required file: {requiredFile}");
    }

    RunProcess("dotnet", $"{Quote(harness)} --write-seeds {Quote(seedDirectory)}");
    RunProcess("dotnet", $"{Quote(harness)} --smoke {Quote(seedDirectory)}");

    var instrumentationTargets = new[]
    {
        new
        {
            Assembly = skiaManagedTarget,
            Types = new[]
            {
                "SkiaSharp.SKBitmap",
                "SkiaSharp.SKCodec",
                "SkiaSharp.SKFontManager",
                "SkiaSharp.SKFrontBufferedManagedStream",
                "SkiaSharp.SKManagedStream",
                "SkiaSharp.SKPicture"
            }
        },
        new
        {
            Assembly = harfBuzzManagedTarget,
            Types = new[]
            {
                "HarfBuzzSharp.Buffer",
                "HarfBuzzSharp.Face",
                "HarfBuzzSharp.Font"
            }
        },
        new
        {
            Assembly = resourcesManagedTarget,
            Types = new[]
            {
                "SkiaSharp.Resources.CachingResourceProvider",
                "SkiaSharp.Resources.DataUriResourceProvider",
                "SkiaSharp.Resources.FileResourceProvider"
            }
        },
        new
        {
            Assembly = skottieManagedTarget,
            Types = new[]
            {
                "SkiaSharp.Skottie.Animation",
                "SkiaSharp.Skottie.AnimationBuilder"
            }
        }
    };

    foreach (var instrumentation in instrumentationTargets)
    {
        var args = string.Join(" ", new[] { Quote(instrumentation.Assembly) }.Concat(instrumentation.Types.Select(Quote)));
        RunProcess(sharpFuzzPath, args);
    }

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

    var configPath = System.IO.Path.Combine(outputPath, "OneFuzzConfig.json");
    System.IO.File.WriteAllText(configPath, config.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

    var seedSourcesPath = System.IO.Path.Combine(outputPath, "seed-sources");
    if (System.IO.Directory.Exists(seedSourcesPath))
        System.IO.Directory.Delete(seedSourcesPath, recursive: true);

    Information($"Prepared the {ONEFUZZ_PLATFORM} OneFuzz drop at '{outputPath}'.");
});

RunTarget(TARGET);
