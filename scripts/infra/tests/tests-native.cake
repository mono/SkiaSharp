DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

Task("Default")
    .Does(() =>
{
    var testAssembly = "SkiaSharp.Tests.Native";
    var project = ROOT_PATH.CombineWithFilePath($"tests/{testAssembly}/{testAssembly}.csproj");
    RunDotNetTest(project, ROOT_OUTPUT_PATH.Combine($"logs/testlogs/{testAssembly}"),
        noBuild: false, hangTimeout: null);
});

RunTarget(TARGET);
