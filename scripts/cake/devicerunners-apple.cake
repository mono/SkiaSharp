DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "shared.cake"

var TEST_APP = Argument("app", EnvironmentVariable("IOS_TEST_APP"));
var TEST_RESULTS = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? "");
var TEST_DEVICE = Argument("device", EnvironmentVariable("IOS_TEST_DEVICE") ?? "ios-simulator-64");
var TEST_VERSION = Argument("deviceVersion", EnvironmentVariable("IOS_TEST_DEVICE_VERSION") ?? "latest");
var TEST_SIMULATOR_DEVICE_TYPE = Argument("simulatorDeviceType", EnvironmentVariable("IOS_TEST_SIMULATOR_DEVICE_TYPE") ?? "iPhone 16");

// simulator tracking
var SIMULATOR_NAME = "";
var SIMULATOR_UDID = "";
var usingSimulator = false;
var isMacCatalyst = false;

Task("Default")
    .Does(() =>
{
    if (string.IsNullOrEmpty(TEST_APP)) {
        throw new Exception("A path to a test app is required.");
    }
    if (string.IsNullOrEmpty(TEST_RESULTS)) {
        TEST_RESULTS = TEST_APP + "-results";
    }

    Information("Test App: {0}", TEST_APP);
    Information("Test Device: {0}", TEST_DEVICE);
    Information("Test Results Directory: {0}", TEST_RESULTS);

    CleanDirectories(TEST_RESULTS);

    // determine the platform
    var device = TEST_DEVICE.Trim().ToLower();
    isMacCatalyst = device == "maccatalyst" || device.Contains("maccatalyst");

    if (isMacCatalyst) {
        RunMacCatalystTests();
    } else {
        RuniOSTests();
    }
});

void RunMacCatalystTests()
{
    Information("Running Mac Catalyst tests...");

    DotNetTool("device-runners macos test " +
        $"--app \"{TEST_APP}\" " +
        $"--results-directory \"{TEST_RESULTS}\"");
}

void RuniOSTests()
{
    SIMULATOR_NAME = $"SkiaSharp-Tests-{System.Diagnostics.Process.GetCurrentProcess().Id}";

    try {
        // create the simulator
        Information("Creating simulator: {0} ({1})...", SIMULATOR_NAME, TEST_SIMULATOR_DEVICE_TYPE);
        RunProcess("dotnet", $"apple simulator create \"{SIMULATOR_NAME}\" --device-type \"{TEST_SIMULATOR_DEVICE_TYPE}\" --format json", out var createOutput);
        var outputLines = createOutput.ToList();
        foreach (var line in outputLines) {
            Information("  {0}", line);
        }

        // parse the UDID from the JSON output
        var jsonLine = outputLines.LastOrDefault(l => l.TrimStart().StartsWith("{")) ?? "";
        var udidMatch = System.Text.RegularExpressions.Regex.Match(jsonLine, "\"udid\"\\s*:\\s*\"([^\"]+)\"");
        if (udidMatch.Success) {
            SIMULATOR_UDID = udidMatch.Groups[1].Value;
        }
        if (string.IsNullOrEmpty(SIMULATOR_UDID)) {
            throw new Exception("Failed to parse simulator UDID from output: " + string.Join("\n", outputLines));
        }
        usingSimulator = true;
        Information("Simulator UDID: {0}", SIMULATOR_UDID);

        // boot the simulator
        Information("Booting simulator...");
        DotNetTool($"apple simulator boot \"{SIMULATOR_UDID}\" --wait");

        // give the simulator time to fully start
        System.Threading.Thread.Sleep(10000);

        DotNetTool("apple simulator list");
        TakeSnapshot(TEST_RESULTS, "simulator-booted");

        // run the tests
        Information("Running iOS tests...");
        DotNetTool("device-runners ios test " +
            $"--app \"{TEST_APP}\" " +
            $"--device \"{SIMULATOR_UDID}\" " +
            $"--results-directory \"{TEST_RESULTS}\"");

        TakeSnapshot(TEST_RESULTS, "finished-tests");

    } finally {
        // cleanup the simulator
        if (usingSimulator) {
            TakeSnapshot(TEST_RESULTS, "teardown");
            try {
                DotNetTool($"apple simulator delete --force \"{SIMULATOR_NAME}\"");
            } catch {
                Warning("Failed to delete simulator.");
            }
        }
    }
}

RunTarget(TARGET);
