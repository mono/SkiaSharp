DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

Task("Default")
    .Does(() =>
{
    var project = ROOT_PATH.CombineWithFilePath(
        "tests/SkiaSharp.Native.Tests/SkiaSharp.Native.Tests.csproj");
    RunDotNetTest(project, ROOT_OUTPUT_PATH.Combine("logs/testlogs/native"),
        noBuild: false, hangTimeout: null);
});

RunTarget(TARGET);
