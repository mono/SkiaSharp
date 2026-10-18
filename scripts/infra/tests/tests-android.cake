DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "tests-shared.cake"
#load "tests-shared-android.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// ANDROID TESTS — build, emulator management, and dotnet test execution
////////////////////////////////////////////////////////////////////////////////////////////////////

var TEST_RESULTS = Argument("results", EnvironmentVariable("ANDROID_TEST_RESULTS") ?? "");
var TEST_DEVICE = Argument("device", EnvironmentVariable("ANDROID_TEST_DEVICE") ?? "android-emulator-64");
var TEST_VERSION = Argument("deviceVersion", EnvironmentVariable("ANDROID_TEST_DEVICE_VERSION") ?? "36");

string OWNED_ANDROID_AVD = null;
string OWNED_ANDROID_SERIAL = null;
var DEVICE_NAME = Argument("skin", EnvironmentVariable("ANDROID_TEST_SKIN") ?? "Nexus 5X");
var DEVICE_ID = "";
var DEVICE_ARCH = "";
var usingEmulator = true;

Setup(context =>
{
    if (!TARGET.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
        System.Environment.GetCommandLineArgs().Any(arg => new[] {
            "--showdescription", "--dryrun", "--noop", "--whatif",
        }.Contains(arg.Split('=')[0], StringComparer.OrdinalIgnoreCase)))
        return;

    if (string.IsNullOrEmpty(TEST_RESULTS)) {
        TEST_RESULTS = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.Android/{DATE_TIME_STR}";
    }
    Information("Test Results Directory: {0}", TEST_RESULTS);
    CleanDir(TEST_RESULTS);

    if (!string.IsNullOrEmpty(TEST_VERSION) && TEST_VERSION != "latest")
        TEST_DEVICE = $"{TEST_DEVICE}_{TEST_VERSION}";

    Information("Test Device: {0}", TEST_DEVICE);

    // determine the device characteristics
    {
        var working = TEST_DEVICE.Trim().ToLower();
        var api = 36;
        if (working.IndexOf("_") is int idx && idx > 0) {
            api = int.Parse(working.Substring(idx + 1));
            working = working.Substring(0, idx);
        }
        var parts = working.Split('-');
        if (parts[0] != "android")
            throw new Exception("Unexpected platform (expected: android) in device: " + TEST_DEVICE);
        if (parts[1] == "device")
            usingEmulator = false;
        else if (parts[1] != "emulator" && parts[1] != "simulator")
            throw new Exception("Unexpected device type (expected: device|emulator) in device: " + TEST_DEVICE);
        if (parts[2] == "32") {
            DEVICE_ARCH = usingEmulator ? "x86" : "armeabi-v7a";
        } else if (parts[2] == "64") {
            if (RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
                DEVICE_ARCH = "arm64-v8a";
            else
                DEVICE_ARCH = usingEmulator ? "x86_64" : "arm64-v8a";
        }
        DEVICE_ID = $"system-images;android-{api};google_apis;{DEVICE_ARCH}";
    }

    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    if (!SKIP_BUILD) {
        // Pre-build the project before starting the emulator to reduce peak memory usage.
        // Android IL linking is very memory-intensive and running it concurrently with the
        // emulator can cause OOM on CI agents.
        Information("Pre-building Android test project...");
        RunDotNetBuild(csproj, configuration: "Release", properties: new Dictionary<string, string> {
            { "TargetFramework", "net10.0-android" },
        });
        Information("Pre-build complete.");
    }

    if (!usingEmulator) {
        Information("Using a physical device:");
        DotNetTool("android device list");
        return;
    }

    Information("Test Device ID: {0}", DEVICE_ID);

    Information("Creating owned AVD...");
    Information("  SDK: {0}", DEVICE_ID);
    Information("  Device: {0}", DEVICE_NAME);
    SetupAndroidEmulator(ref OWNED_ANDROID_AVD, ref OWNED_ANDROID_SERIAL, DEVICE_ID, DEVICE_NAME);

    TakeSnapshot(TEST_RESULTS, "boot-complete");
});

Teardown(context =>
{
    if (OWNED_ANDROID_AVD == null)
        return;

    try {
        TakeSnapshot(TEST_RESULTS, "teardown");
    } finally {
        TeardownAndroidEmulator(ref OWNED_ANDROID_AVD, ref OWNED_ANDROID_SERIAL, context.ThrownException);
    }
});

Task("Default")
    .Does(() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";

    var properties = new Dictionary<string, string>();
    if (usingEmulator)
        properties["DeviceRunnersDevice"] = OWNED_ANDROID_SERIAL;
    RunDeviceRunnersTest(csproj, (DirectoryPath)TEST_RESULTS, configuration: "Release", framework: "net10.0-android", noBuild: true, properties: properties);
});

RunTarget(TARGET);
