DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "tests-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// MAC CATALYST TESTS — DeviceRunners execution without a simulator
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .Description ("Run all Mac Catalyst tests.")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.maccatalyst/{DATE_TIME_STR}";

    RunDeviceRunnersTest(csproj, results, configuration: "Debug", framework: "net10.0-maccatalyst", noBuild: SKIP_BUILD);
});

RunTarget(TARGET);
