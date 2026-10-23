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

// Offline checks that mirror what the service validates, so mistakes fail here instead of at submission.
// The service still runs the authoritative validation (`onefuzz onefuzzconfig validate`, and again on submit).
Task("onefuzz-validate")
    .Description("Strictly validate the generated OneFuzz configs against the drop.")
    .IsDependentOn("onefuzz-prepare")
    .Does(() =>
{
    var outputPath = ROOT_PATH.Combine("output/onefuzz").FullPath;
    var errors = new List<string>();
    var warnings = new List<string>();

    var entryRequired = new[] { "JobNotificationEmail", "Fuzzer", "OneFuzzJobs", "JobDependencies" };
    var entryRecommended = new[] { "Skip", "FuzzerTimeoutInSeconds", "MinAvailableMemoryMB", "SdlWorkItemId" };
    var entryKnown = entryRequired.Concat(entryRecommended).ToHashSet();
    var fuzzerRequired = new[] { "$type", "Dll", "Class", "Method", "FuzzingTargetBinaries" };
    var jobRequired = new[] { "ProjectName", "TargetName" };
    var jobKnown = jobRequired.Concat(new[] { "SeedCorpusContainer", "FuzzingTargetOptions" }).ToHashSet();

    void CheckObject(JsonNode node, string path, string[] required, ISet<string> known)
    {
        if (node is not JsonObject obj)
        {
            errors.Add($"{path}: expected an object.");
            return;
        }
        foreach (var name in required)
        {
            if (obj[name] is null)
                errors.Add($"{path}: missing required '{name}'.");
        }
        foreach (var property in obj)
        {
            if (!known.Contains(property.Key))
                warnings.Add($"{path}: unrecognised property '{property.Key}'.");
        }
    }

    // Methods are checked against the harness source, so a typo can never reach the service.
    var harnessMethods = Regex
        .Matches(System.IO.File.ReadAllText(ROOT_PATH.CombineWithFilePath("tests/SkiaSharp.Fuzzing/FuzzTargets.cs").FullPath), @"public static void (\w+)\(ReadOnlySpan<byte>")
        .Select(m => m.Groups[1].Value)
        .ToHashSet();
    if (harnessMethods.Count == 0)
        errors.Add("Unable to find any fuzz target methods in FuzzTargets.cs.");

    var configFiles = System.IO.Directory.GetFiles(outputPath, "OneFuzzConfig.*.json");
    if (configFiles.Length == 0)
        errors.Add("No OneFuzzConfig.<platform>.json files were generated.");

    foreach (var configFile in configFiles)
    {
        var name = System.IO.Path.GetFileName(configFile);
        var text = System.IO.File.ReadAllText(configFile);

        var leftover = Regex.Matches(text, @"\{[A-Za-z]+\}").Select(m => m.Value).Distinct().ToList();
        if (leftover.Count > 0)
            errors.Add($"{name}: unreplaced template tokens {string.Join(", ", leftover)}.");

        var config = JsonNode.Parse(text)!;
        if (config["ConfigVersion"]?.GetValue<int>() != 3)
            errors.Add($"{name}: ConfigVersion must be 3.");

        var entries = config["Entries"]?.AsArray();
        if (entries is null || entries.Count == 0)
        {
            errors.Add($"{name}: no entries.");
            continue;
        }

        var targetNames = new HashSet<string>();
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i]!;
            var entryPath = $"{name} Entries[{i}]";
            CheckObject(entry, entryPath, entryRequired, entryKnown);

            foreach (var missing in entryRecommended.Where(r => entry[r] is null))
                warnings.Add($"{entryPath}: '{missing}' is not set.");

            var fuzzer = entry["Fuzzer"];
            if (fuzzer is not null)
            {
                CheckObject(fuzzer, $"{entryPath} Fuzzer", fuzzerRequired, fuzzerRequired.ToHashSet());
                if (fuzzer["$type"]?.GetValue<string>() != "libfuzzerDotNet")
                    errors.Add($"{entryPath}: Fuzzer.$type must be 'libfuzzerDotNet'.");

                var dll = fuzzer["Dll"]?.GetValue<string>();
                if (dll is not null && !System.IO.File.Exists(System.IO.Path.Combine(outputPath, dll)))
                    errors.Add($"{entryPath}: Fuzzer.Dll '{dll}' is not in the drop.");

                var method = fuzzer["Method"]?.GetValue<string>();
                if (method is not null && !harnessMethods.Contains(method))
                    errors.Add($"{entryPath}: Fuzzer.Method '{method}' does not exist on FuzzTargets.");

                foreach (var binary in fuzzer["FuzzingTargetBinaries"]?.AsArray() ?? new JsonArray())
                {
                    var file = binary!.GetValue<string>();
                    if (!System.IO.File.Exists(System.IO.Path.Combine(outputPath, file)))
                        errors.Add($"{entryPath}: FuzzingTargetBinaries '{file}' is not in the drop.");
                    else if (!System.IO.File.Exists(System.IO.Path.Combine(outputPath, System.IO.Path.ChangeExtension(file, ".pdb"))))
                        warnings.Add($"{entryPath}: '{file}' has no .pdb, so its stacks will not symbolize.");
                }
            }

            var jobs = entry["OneFuzzJobs"]?.AsArray();
            if (jobs is null || jobs.Count == 0)
                errors.Add($"{entryPath}: OneFuzzJobs is empty.");
            for (var j = 0; j < (jobs?.Count ?? 0); j++)
            {
                var jobPath = $"{entryPath} OneFuzzJobs[{j}]";
                CheckObject(jobs![j]!, jobPath, jobRequired, jobKnown);

                var target = jobs[j]!["TargetName"]?.GetValue<string>();
                if (target is not null && !targetNames.Add(target))
                    errors.Add($"{jobPath}: duplicate TargetName '{target}'.");
                if (target is not null && !Regex.IsMatch(target, "^[A-Za-z0-9._-]+$"))
                    errors.Add($"{jobPath}: TargetName '{target}' has characters OneFuzz may reject.");
                if (jobs[j]!["SeedCorpusContainer"] is null)
                    warnings.Add($"{name}: OneFuzzJobs have no SeedCorpusContainer, so the fuzzers start without a corpus.");
            }

            foreach (var dependency in entry["JobDependencies"]?.AsArray() ?? new JsonArray())
            {
                var pattern = dependency!.GetValue<string>();
                if (pattern.Contains(".."))
                    errors.Add($"{entryPath}: JobDependencies '{pattern}' escapes the drop.");
                else if (System.IO.Directory.GetFiles(outputPath, System.IO.Path.GetFileName(pattern.Replace('\\', '/')), System.IO.SearchOption.TopDirectoryOnly).Length == 0)
                    errors.Add($"{entryPath}: JobDependencies '{pattern}' matches no files.");
            }
        }
    }

    foreach (var warning in warnings.Distinct())
    {
        Warning(warning);
        if (EnvironmentVariable("TF_BUILD") is not null)
            Console.WriteLine($"##vso[task.logissue type=warning]{warning}");
    }

    if (errors.Count > 0)
        throw new Exception($"OneFuzz config validation failed:{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", errors)}");

    Information($"Validated {configFiles.Length} OneFuzz configs ({warnings.Distinct().Count()} warnings).");
});

Task("Default")
    .IsDependentOn("onefuzz-validate");

RunTarget(TARGET);
