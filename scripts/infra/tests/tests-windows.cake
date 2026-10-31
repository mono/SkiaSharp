DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "tests-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// WINDOWS DEVICE TESTS
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .Description ("Run all Windows device tests.")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.windows/{DATE_TIME_STR}";
    var tfm = "net10.0-windows10.0.19041.0";

    var properties = new Dictionary<string, string> {
        // Restrict restore too: --framework alone still restores the other platforms.
        { "TargetFramework", tfm },
        { "TargetFrameworks", tfm },
        { "Platform", "x64" },
    };

    RunDeviceRunnersTest(csproj, results, configuration: "Debug", framework: tfm, noBuild: SKIP_BUILD, properties: properties);
});

RunTarget(TARGET);
