DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// ANDROID TESTS — build, emulator management, and xharness execution
////////////////////////////////////////////////////////////////////////////////////////////////////

var TEST_APP = Argument("app", EnvironmentVariable("ANDROID_TEST_APP") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("ANDROID_TEST_RESULTS") ?? "");
var TEST_DEVICE = Argument("device", EnvironmentVariable("ANDROID_TEST_DEVICE") ?? "android-emulator-64");
var TEST_VERSION = Argument("deviceVersion", EnvironmentVariable("ANDROID_TEST_DEVICE_VERSION") ?? "36");
var TEST_APP_PACKAGE_NAME = Argument("package", EnvironmentVariable("ANDROID_TEST_APP_PACKAGE_NAME") ?? "");
var TEST_APP_INSTRUMENTATION = Argument("instrumentation", EnvironmentVariable("ANDROID_TEST_APP_INSTRUMENTATION") ?? "devicerunners.xharness.maui.XHarnessInstrumentation");

var ANDROID_AVD = "DEVICE_TESTS_EMULATOR";
var DEVICE_NAME = Argument("skin", EnvironmentVariable("ANDROID_TEST_SKIN") ?? "Nexus 5X");
var DEVICE_ID = "";
var DEVICE_ARCH = "";
var usingEmulator = true;

Setup(context =>
{
    // if app wasn't passed as argument, build it
    if (string.IsNullOrEmpty(TEST_APP)) {
        FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
        var configuration = "Release";
        var tfm = "net10.0-android";
        var rid = "android-" + RuntimeInformation.ProcessArchitecture.ToString().ToLower();
        TEST_APP = ROOT_PATH + $"/tests/SkiaSharp.Tests.Devices/bin/{configuration}/{tfm}/{rid}/com.companyname.SkiaSharpTests-Signed.apk";

        Information("=== Android Test Build Configuration ===");
        Information("  Project:       {0}", csproj);
        Information("  Configuration: {0}", configuration);
        Information("  TFM:           {0}", tfm);
        Information("  RID:           {0}", rid);
        Information("  App Path:      {0}", TEST_APP);
        Information("  OS:            {0}", RuntimeInformation.OSDescription);
        Information("  Arch:          {0}", RuntimeInformation.ProcessArchitecture);
        Information("========================================");

        CleanDirectories($"{PACKAGE_CACHE_PATH}/skiasharp*");
        CleanDirectories($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

        if (!SKIP_BUILD) {
            RunDotNetBuild(csproj,
                configuration: configuration,
                properties: new Dictionary<string, string> {
                    { "TargetFramework", tfm },
                    { "RuntimeIdentifier", rid },
                });
        }
    }

    if (string.IsNullOrEmpty(TEST_RESULTS)) {
        TEST_RESULTS = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.Android/{DATE_TIME_STR}";
    }
    Information("Test Results Directory: {0}", TEST_RESULTS);
    CleanDir(TEST_RESULTS);

    if (!string.IsNullOrEmpty(TEST_VERSION) && TEST_VERSION != "latest")
        TEST_DEVICE = $"{TEST_DEVICE}_{TEST_VERSION}";

    Information("Test App: {0}", TEST_APP);
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
    if (string.IsNullOrEmpty(TEST_APP_PACKAGE_NAME)) {
        var appFile = (FilePath)TEST_APP;
        appFile = appFile.GetFilenameWithoutExtension();
        TEST_APP_PACKAGE_NAME = appFile.FullPath.Replace("-Signed", "");
    }
    if (string.IsNullOrEmpty(TEST_APP_INSTRUMENTATION)) {
        TEST_APP_INSTRUMENTATION = TEST_APP_PACKAGE_NAME + ".TestInstrumentation";
    }

    Information("Test App: {0}", TEST_APP);
    Information("Test App Package Name: {0}", TEST_APP_PACKAGE_NAME);
    Information("Test App Instrumentation: {0}", TEST_APP_INSTRUMENTATION);
    Information("Test Results Directory: {0}", TEST_RESULTS);

    TakeSnapshot(TEST_RESULTS, "starting-tests");

    var complete = false;
    System.Threading.Tasks.Task.Run(() => {
        while (!complete) {
            TakeSnapshot(TEST_RESULTS, "running-tests");
            System.Threading.Thread.Sleep(5000);
        }
    });

    DotNetTool("xharness android test " +
        $"--app=\"{TEST_APP}\" " +
        $"--package-name=\"{TEST_APP_PACKAGE_NAME}\" " +
        $"--instrumentation=\"{TEST_APP_INSTRUMENTATION}\" " +
        $"--output-directory=\"{TEST_RESULTS}\" " +
        $"--timeout=00:15:00 " +
        $"--launch-timeout=00:05:00 " +
        $"--verbosity=\"Debug\" ");

    complete = true;

    TakeSnapshot(TEST_RESULTS, "finished-tests");

    var failed = XmlPeek($"{TEST_RESULTS}/TestResults.xml", "/assemblies/assembly[@failed > 0 or @errors > 0]/@failed");
    if (!string.IsNullOrEmpty(failed)) {
        throw new Exception($"At least {failed} test(s) failed.");
    }
});

RunTarget(TARGET);
