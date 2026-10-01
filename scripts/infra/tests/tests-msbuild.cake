DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

Task ("Default")
    .Description ("Test MSBuild consumers of the already-packed NuGets.")
    .Does (() =>
{
    var project = ROOT_PATH.CombineWithFilePath("tests/SkiaSharp.Tests.MSBuild/SkiaSharp.Tests.MSBuild.csproj");
    var results = ROOT_PATH.Combine($"output/logs/testlogs/msbuild/{DATE_TIME_STR}");
    EnsureDirectoryExists(results);

    var properties = new Dictionary<string, string> {
        { "PackageDirectory", OUTPUT_NUGETS_PATH.FullPath },
        { "MSBuildTestArtifactsDirectory", results.FullPath },
    };
    var consumerSdkVersion = Argument("consumerSdkVersion", "");
    if (!string.IsNullOrEmpty(consumerSdkVersion))
        properties["ConsumerSdkVersion"] = consumerSdkVersion;

    if (!SKIP_BUILD)
        RunDotNetBuild(project, properties: properties);

    var filter = Argument("wasm", false)
        ? "--filter-trait"
        : "--filter-not-trait";
    RunDotNetTest(project, results, runnerArguments: new[] { filter, "Category=Wasm" });
});

RunTarget(TARGET);
