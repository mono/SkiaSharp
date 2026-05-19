DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// APPLE TESTS — iOS, Mac Catalyst (build + dotnet test execution)
////////////////////////////////////////////////////////////////////////////////////////////////////

var IOS_SIMULATOR_NAME = "SkiaSharpTestSim";
var IOS_DEVICE_TYPE = Argument("iosDeviceType", EnvironmentVariable("IOS_DEVICE_TYPE") ?? "com.apple.CoreSimulator.SimDeviceType.iPhone-16");
var IOS_RUNTIME = Argument("iosRuntime", EnvironmentVariable("IOS_RUNTIME") ?? "");
var simulatorUdid = "";

Setup(context =>
{
    // Boot an iOS simulator for the iOS tests
    Information("Setting up iOS simulator for tests...");

    // Find the latest available iOS runtime if not specified
    if (string.IsNullOrEmpty(IOS_RUNTIME))
    {
        // Get available runtimes - output format: "iOS 18.4 (18.4 - 22E5241o) - com.apple.CoreSimulator.SimRuntime.iOS-18-4"
        var runtimeLines = RunCommandWithOutput("xcrun", "simctl list runtimes iOS available")
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in runtimeLines.Reverse())
        {
            var match = System.Text.RegularExpressions.Regex.Match(line, @"(com\.apple\.CoreSimulator\.SimRuntime\.iOS-[\w-]+)");
            if (match.Success)
            {
                IOS_RUNTIME = match.Groups[1].Value;
                break;
            }
        }

        if (string.IsNullOrEmpty(IOS_RUNTIME))
        {
            Warning("No iOS runtimes available - iOS tests will be skipped");
            return;
        }
    }

    Information("  Device Type: {0}", IOS_DEVICE_TYPE);
    Information("  Runtime: {0}", IOS_RUNTIME);

    // Create a simulator
    var createOutput = RunCommandWithOutput("xcrun", $"simctl create \"{IOS_SIMULATOR_NAME}\" \"{IOS_DEVICE_TYPE}\" \"{IOS_RUNTIME}\"");
    simulatorUdid = createOutput.Trim();
    Information("  Created simulator: {0}", simulatorUdid);

    // Boot it
    StartProcess("xcrun", new ProcessSettings {
        Arguments = $"simctl boot \"{simulatorUdid}\""
    });
    Information("  Simulator booted");
});

Teardown(context =>
{
    if (!string.IsNullOrEmpty(simulatorUdid))
    {
        Information("Shutting down iOS simulator: {0}", simulatorUdid);
        StartProcess("xcrun", new ProcessSettings {
            Arguments = $"simctl shutdown \"{simulatorUdid}\""
        });
        StartProcess("xcrun", new ProcessSettings {
            Arguments = $"simctl delete \"{simulatorUdid}\""
        });
    }
});

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .Does (() =>
{
    if (string.IsNullOrEmpty(simulatorUdid))
    {
        Warning("No iOS simulator available - skipping iOS tests");
        return;
    }

    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.ios/{DATE_TIME_STR}";

    var properties = new Dictionary<string, string> {
        { "DeviceRunnersDevice", simulatorUdid },
    };

    RunDeviceRunnersTest(csproj, results, framework: "net10.0-ios", properties: properties);
});

Task ("tests-maccatalyst")
    .Description ("Run all Mac Catalyst tests.")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.maccatalyst/{DATE_TIME_STR}";

    // Determine the RuntimeIdentifier for Mac Catalyst based on host architecture.
    // DeviceRunners targets look for the .app in $(OutputPath) which doesn't include
    // the RID subdirectory by default. Passing RuntimeIdentifier explicitly makes
    // OutputPath include the RID, so the targets find the .app correctly.
    var rid = RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64
        ? "maccatalyst-arm64"
        : "maccatalyst-x64";

    var properties = new Dictionary<string, string> {
        { "RuntimeIdentifier", rid },
    };

    RunDeviceRunnersTest(csproj, results, framework: "net10.0-maccatalyst", properties: properties);
});

Task ("Default")
    .IsDependentOn ("tests-ios")
    .IsDependentOn ("tests-maccatalyst");

RunTarget(TARGET);

////////////////////////////////////////////////////////////////////////////////////////////////////
// HELPERS
////////////////////////////////////////////////////////////////////////////////////////////////////

string RunCommandWithOutput(string tool, string arguments)
{
    var settings = new ProcessSettings {
        Arguments = arguments,
        RedirectStandardOutput = true,
    };

    IEnumerable<string> output;
    var exitCode = StartProcess(tool, settings, out output);
    if (exitCode != 0)
        throw new Exception($"Command '{tool} {arguments}' failed with exit code {exitCode}");

    return string.Join("\n", output);
}
