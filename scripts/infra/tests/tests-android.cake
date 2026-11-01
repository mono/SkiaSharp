DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "tests-shared.cake"
#load "tests-shared-android.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// ANDROID TESTS — build, emulator management, and dotnet test execution
////////////////////////////////////////////////////////////////////////////////////////////////////

var TEST_RESULTS = Argument("results", EnvironmentVariable("ANDROID_TEST_RESULTS") ?? "");

string OWNED_ANDROID_AVD = null;
string OWNED_ANDROID_SERIAL = null;

Setup(context =>
{
    if (!TARGET.Equals("Default", StringComparison.OrdinalIgnoreCase) || IsCakeListingCommand())
        return;

    StopCIAndroidEmulators();

    if (string.IsNullOrEmpty(TEST_RESULTS)) {
        TEST_RESULTS = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.Android/{DATE_TIME_STR}";
    }
    Information("Test Results Directory: {0}", TEST_RESULTS);
    CleanDir(TEST_RESULTS);

    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    if (!SKIP_BUILD) {
        // Pre-build the project before starting the emulator to reduce peak memory usage.
        // Android IL linking is very memory-intensive and running it concurrently with the
        // emulator can cause OOM on CI agents.
        Information("Pre-building Android test project...");
        var properties = new Dictionary<string, string> {
            { "TargetFramework", "net10.0-android" },
        };
        AddGpuOptOut(properties);
        RunDotNetBuild(csproj, configuration: "Release", properties: properties);
        Information("Pre-build complete.");
    }

    SetupAndroidEmulator(ref OWNED_ANDROID_AVD, ref OWNED_ANDROID_SERIAL);

    if (OWNED_ANDROID_SERIAL != null)
        TakeSnapshot(TEST_RESULTS, "boot-complete");
});

Teardown(context =>
{
    if (!TARGET.Equals("Default", StringComparison.OrdinalIgnoreCase) || IsCakeListingCommand())
        return;

    try {
        if (OWNED_ANDROID_AVD != null)
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
    if (OWNED_ANDROID_SERIAL != null) {
        properties["DeviceRunnersDevice"] = OWNED_ANDROID_SERIAL;
        properties["AdbTarget"] = "-s " + OWNED_ANDROID_SERIAL;
    }
    RunDeviceRunnersTest(csproj, (DirectoryPath)TEST_RESULTS, configuration: "Release", framework: "net10.0-android", noBuild: true, properties: properties);
});

RunTarget(TARGET);
