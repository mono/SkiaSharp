DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// APPLE TESTS — iOS, Mac Catalyst (build + dotnet test execution)
////////////////////////////////////////////////////////////////////////////////////////////////////

var IOS_SIMULATOR_NAME = Argument("iosSimulator", EnvironmentVariable("IOS_SIMULATOR_NAME") ?? "iPhone 16");

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .Does (() =>
{
    // Boot the iOS simulator before running tests
    Information("Booting iOS simulator: {0}...", IOS_SIMULATOR_NAME);
    DotNetTool($"apple simulator boot \"{IOS_SIMULATOR_NAME}\" --wait");
    Information("  Simulator booted");

    try
    {
        FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
        DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.ios/{DATE_TIME_STR}";

        RunDeviceRunnersTest(csproj, results, framework: "net10.0-ios");
    }
    finally
    {
        // Always attempt to shut down the simulator
        Information("Shutting down iOS simulator: {0}", IOS_SIMULATOR_NAME);
        try
        {
            DotNetTool($"apple simulator shutdown \"{IOS_SIMULATOR_NAME}\"");
        }
        catch (Exception ex)
        {
            Warning($"Failed to shutdown simulator: {ex.Message}");
        }
    }
});

Task ("tests-maccatalyst")
    .Description ("Run all Mac Catalyst tests.")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.maccatalyst/{DATE_TIME_STR}";

    RunDeviceRunnersTest(csproj, results, framework: "net10.0-maccatalyst");
});

Task ("Default")
    .IsDependentOn ("tests-ios")
    .IsDependentOn ("tests-maccatalyst");

RunTarget(TARGET);
