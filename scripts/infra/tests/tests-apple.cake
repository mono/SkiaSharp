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
    // Create a unique simulator for this test run (matches DeviceRunners CI pattern)
    var simulatorName = $"SkiaSharp-Tests-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
    Information("Creating iOS simulator: {0} (device type: {1})...", simulatorName, IOS_SIMULATOR_NAME);

    // Create simulator and capture UDID from JSON output
    IEnumerable<string> createStdout;
    var createExitCode = StartProcess("dotnet", new ProcessSettings {
        Arguments = $"apple simulator create \"{simulatorName}\" --device-type \"{IOS_SIMULATOR_NAME}\" --format json",
        RedirectStandardOutput = true,
    }, out createStdout);
    if (createExitCode != 0)
        throw new Exception($"Failed to create simulator (exit code {createExitCode})");

    var createJson = string.Join("", createStdout);
    var udid = System.Text.Json.JsonDocument.Parse(createJson).RootElement.GetProperty("udid").GetString();
    Information("  Created simulator with UDID: {0}", udid);

    // Boot by UDID
    DotNetTool($"apple simulator boot \"{udid}\" --wait");
    Information("  Simulator booted");

    try
    {
        FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
        DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.ios/{DATE_TIME_STR}";

        // Pass the simulator UDID to DeviceRunners so it targets the correct device
        var properties = new Dictionary<string, string> {
            { "DeviceRunnersDevice", udid },
        };

        RunDeviceRunnersTest(csproj, results, framework: "net10.0-ios", properties: properties);
    }
    finally
    {
        // Always clean up the simulator
        Information("Deleting simulator: {0}", simulatorName);
        try
        {
            DotNetTool($"apple simulator delete --force \"{simulatorName}\"");
        }
        catch (Exception ex)
        {
            Warning($"Failed to delete simulator: {ex.Message}");
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
