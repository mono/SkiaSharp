DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// ANDROID TESTS — build, emulator management, and dotnet test execution
////////////////////////////////////////////////////////////////////////////////////////////////////

var TEST_RESULTS = Argument("results", EnvironmentVariable("ANDROID_TEST_RESULTS") ?? "");
var TEST_DEVICE = Argument("device", EnvironmentVariable("ANDROID_TEST_DEVICE") ?? "android-emulator-64");
var TEST_VERSION = Argument("deviceVersion", EnvironmentVariable("ANDROID_TEST_DEVICE_VERSION") ?? "36");

var ANDROID_AVD = "DEVICE_TESTS_EMULATOR";
var DEVICE_NAME = Argument("skin", EnvironmentVariable("ANDROID_TEST_SKIN") ?? "Nexus 5X");
var DEVICE_ID = "";
var DEVICE_ARCH = "";
var usingEmulator = true;

Setup(context =>
{
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

    // Pre-build the project before starting the emulator to reduce peak memory usage.
    // Android IL linking is very memory-intensive and running it concurrently with the
    // emulator can cause OOM on CI agents.
    Information("Pre-building Android test project...");
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DotNetBuild(MakeAbsolute(csproj).FullPath, new DotNetBuildSettings {
        Configuration = "Release",
        Framework = "net10.0-android",
    });
    Information("Pre-build complete.");

    if (!usingEmulator) {
        Information("Using a physical device:");
        DotNetTool("android device list");
        return;
    }

    Information("Test Device ID: {0}", DEVICE_ID);

    Information("Creating AVD: {0}...", ANDROID_AVD);
    Information("  SDK: {0}", DEVICE_ID);
    Information("  Device: {0}", DEVICE_NAME);
    DotNetTool($"android avd create --name \"{ANDROID_AVD}\" --sdk \"{DEVICE_ID}\" --device \"{DEVICE_NAME}\" --force");

    Information("Listing AVDs after creation:");
    DotNetTool("android avd list");

    var gpuMode = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? "swiftshader_indirect"
        : "guest";
    Information("Starting Emulator: {0}...", ANDROID_AVD);
    Information("  GPU: {0}", gpuMode);
    DotNetTool($"android avd start --name \"{ANDROID_AVD}\" --gpu {gpuMode} --wait --no-window --no-snapshot --no-audio --no-boot-anim --no-animations --cpu-threshold 3 --response-threshold 5 --camera-back none --camera-front none --timeout 300");

    Information("Emulator started:");
    DotNetTool("android device list");

    // Set up adb reverse so the app can connect to the host's TCP listener
    // via localhost (the CLI's env injects DEVICE_RUNNERS_HOST_NAMES=localhost;10.0.2.2)
    Information("Setting up adb reverse for TCP port forwarding...");
    var androidHome = EnvironmentVariable("ANDROID_HOME") ?? EnvironmentVariable("ANDROID_SDK_ROOT");
    var adb = androidHome != null
        ? $"{androidHome}/platform-tools/adb"
        : "adb";
    StartProcess(adb, "reverse tcp:16384 tcp:16384");

    TakeSnapshot(TEST_RESULTS, "boot-complete");
});

Teardown(context =>
{
    if (!usingEmulator)
        return;

    TakeSnapshot(TEST_RESULTS, "teardown");
    DotNetTool($"android avd delete --name \"{ANDROID_AVD}\"");
});

Task("Default")
    .Does(() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";

    RunDeviceRunnersTest(csproj, (DirectoryPath)TEST_RESULTS, configuration: "Release", framework: "net10.0-android", noBuild: true);
});

RunTarget(TARGET);
