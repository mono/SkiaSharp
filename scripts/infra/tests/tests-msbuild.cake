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

    if (!SKIP_BUILD)
        RunDotNetBuild(project, properties: properties);

    RunDotNetTest(project, results);
});

RunTarget(TARGET);
