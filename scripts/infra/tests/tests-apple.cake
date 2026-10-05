DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// APPLE TESTS — iOS, Mac Catalyst (build + dotnet test execution)
////////////////////////////////////////////////////////////////////////////////////////////////////

var IOS_SIMULATOR_NAME = Argument("iosSimulator", EnvironmentVariable("IOS_SIMULATOR_NAME") ?? "iPhone 16");

void RecordIosSimulatorRuntime(string udid, string devicesJson, string runtimesJson)
{
    using var devices = System.Text.Json.JsonDocument.Parse(devicesJson);
    var runtimeIdentifier = devices.RootElement.GetProperty("devices").EnumerateObject()
        .Single(runtime => runtime.Value.EnumerateArray().Any(device =>
            string.Equals(device.GetProperty("udid").GetString(), udid, StringComparison.OrdinalIgnoreCase)))
        .Name;
    if (!runtimeIdentifier.StartsWith("com.apple.CoreSimulator.SimRuntime.iOS-", StringComparison.Ordinal))
        throw new Exception($"The selected test simulator is not an iOS runtime: {runtimeIdentifier}");

    using var runtimes = System.Text.Json.JsonDocument.Parse(runtimesJson);
    var selectedRuntime = runtimes.RootElement.GetProperty("runtimes").EnumerateArray()
        .Single(runtime => runtime.GetProperty("identifier").GetString() == runtimeIdentifier);
    var version = selectedRuntime.GetProperty("version").GetString();
    if (!Version.TryParse(version, out _))
        throw new Exception($"The selected iOS simulator runtime has an invalid version: {version}");

    Information("Selected iOS simulator runtime: {0} ({1}), identifier: {2}, UDID: {3}",
        version, selectedRuntime.GetProperty("buildversion").GetString(), runtimeIdentifier, udid);
    System.Console.WriteLine($"##vso[task.setvariable variable=IOS_TEST_RUNTIME_VERSION]{version}");
}

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .Does (() =>
{
    // Create a unique simulator for this test run (matches DeviceRunners CI pattern)
    var simulatorName = $"SkiaSharp-Tests-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
    Information("Creating iOS simulator: {0} (device type: {1})...", simulatorName, IOS_SIMULATOR_NAME);

    // Create simulator and capture UDID from JSON output
    RunProcess("dotnet", $"apple simulator create \"{simulatorName}\" --device-type \"{IOS_SIMULATOR_NAME}\" --format json", out var createStdout);

    try
    {
        var createJson = string.Join("", createStdout);
        var udid = System.Text.Json.JsonDocument.Parse(createJson).RootElement.GetProperty("udid").GetString();
        Information("  Created simulator with UDID: {0}", udid);
        RunProcess("xcrun", "simctl list devices --json", out var devicesStdout);
        RunProcess("xcrun", "simctl list runtimes --json", out var runtimesStdout);
        RecordIosSimulatorRuntime(udid, string.Join("", devicesStdout), string.Join("", runtimesStdout));

        // Boot by UDID
        DotNetTool($"apple simulator boot \"{udid}\" --wait");
        Information("  Simulator booted");

        FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
        DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.ios/{DATE_TIME_STR}";

        // Pass the simulator UDID to DeviceRunners so it targets the correct device
        var properties = new Dictionary<string, string> {
            { "DeviceRunnersDevice", udid },
        };

        RunDeviceRunnersTest(csproj, results, configuration: "Debug", framework: "net10.0-ios", noBuild: SKIP_BUILD, properties: properties);
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

    RunDeviceRunnersTest(csproj, results, configuration: "Debug", framework: "net10.0-maccatalyst", noBuild: SKIP_BUILD);
});

Task ("Default")
    .IsDependentOn ("tests-ios")
    .IsDependentOn ("tests-maccatalyst");

RunTarget(TARGET);
