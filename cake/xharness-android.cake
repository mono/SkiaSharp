#addin nuget:?package=Cake.Android.Adb&version=3.2.0
#addin nuget:?package=Cake.Android.AvdManager&version=2.2.0

DirectoryPath ROOT_PATH = MakeAbsolute(Directory(".."));

#load "shared.cake"

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("ANDROID_TEST_PROJECT") ?? "");
string TEST_DEVICE = Argument("device", EnvironmentVariable("ANDROID_TEST_DEVICE") ?? "android-emulator-32_30");

// optional
var TEST_APP = Argument("app", EnvironmentVariable("ANDROID_TEST_APP") ?? "");
var TEST_APP_PACKAGE_NAME = Argument("package", EnvironmentVariable("ANDROID_TEST_APP_PACKAGE_NAME") ?? "");
var TEST_APP_INSTRUMENTATION = Argument("instrumentation", EnvironmentVariable("ANDROID_TEST_APP_INSTRUMENTATION") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("ANDROID_TEST_RESULTS") ?? "");

// other
string ANDROID_AVD = "DEVICE_TESTS_EMULATOR";
string DEVICE_NAME = "Nexus 5X";
string DEVICE_ID = "system-images;android-30;google_apis_playstore;x86";
string DEVICE_ARCH = "x86";

// set up env
var ANDROID_SDK_ROOT = Argument("android", EnvironmentVariable("ANDROID_SDK_ROOT") ?? EnvironmentVariable("ANDROID_SDK_ROOT"));
if (string.IsNullOrEmpty(ANDROID_SDK_ROOT)) {
    throw new Exception("Environment variable 'ANDROID_SDK_ROOT' must be set to the Android SDK root.");
}
System.Environment.SetEnvironmentVariable("PATH",
    $"{ANDROID_SDK_ROOT}/tools/bin" + System.IO.Path.PathSeparator +
    $"{ANDROID_SDK_ROOT}/platform-tools" + System.IO.Path.PathSeparator +
    $"{ANDROID_SDK_ROOT}/emulator" + System.IO.Path.PathSeparator +
    EnvironmentVariable("PATH"));

Information("Android SDK Root: {0}", ANDROID_SDK_ROOT);
Information("Project File: {0}", PROJECT);
Information("Build Configuration: {0}", CONFIGURATION);

var avdSettings = new AndroidAvdManagerToolSettings { SdkRoot = ANDROID_SDK_ROOT };
var adbSettings = new AdbToolSettings { SdkRoot = ANDROID_SDK_ROOT };
var emuSettings = new AndroidEmulatorToolSettings { SdkRoot = ANDROID_SDK_ROOT, ArgumentCustomization = args => args.Append("-no-window") };

AndroidEmulatorProcess emulatorProcess = null;

Setup(context =>
{
    Information("Test Device: {0}", TEST_DEVICE);

    // determine the device characteristics
    {
        var working = TEST_DEVICE.Trim().ToLower();
        var emulator = true;
        var api = 30;
        // version
        if (working.IndexOf("_") is int idx && idx > 0) {
            api = int.Parse(working.Substring(idx + 1));
            working = working.Substring(0, idx);
        }
        var parts = working.Split('-');
        // os
        if (parts[0] != "android")
            throw new Exception("Unexpected platform (expected: android) in device: " + TEST_DEVICE);
        // device/emulator
        if (parts[1] == "device")
            emulator = false;
        else if (parts[1] != "emulator" && parts[1] != "simulator")
            throw new Exception("Unexpected device type (expected: device|emulator) in device: " + TEST_DEVICE);
        // arch/bits
        if (parts[2] == "32") {
            if (emulator)
                DEVICE_ARCH = "x86";
            else
                DEVICE_ARCH = "armeabi-v7a";
        } else if (parts[2] == "64") {
            if (emulator)
                DEVICE_ARCH = "x86_64";
            else
                DEVICE_ARCH = "arm64-v8a";
        }
        DEVICE_ID = $"system-images;android-{api};google_apis_playstore;{DEVICE_ARCH}";

        // we are not using a virtual device, so quit
        if (!emulator)
            return;
    }

    Information("Test Device ID: {0}", DEVICE_ID);

    // delete the AVD first, if it exists
    Information("Deleting AVD if exists: {0}...", ANDROID_AVD);
    try { AndroidAvdDelete(ANDROID_AVD, avdSettings); }
    catch { }

    // create the new AVD
    Information("Creating AVD: {0}...", ANDROID_AVD);
    AndroidAvdCreate(ANDROID_AVD, DEVICE_ID, DEVICE_NAME, force: true, settings: avdSettings);

    // start the emulator
    Information("Starting Emulator: {0}...", ANDROID_AVD);
    emulatorProcess = AndroidEmulatorStart(ANDROID_AVD, emuSettings);
});

Teardown(context =>
{
    // no virtual device was used
    if (emulatorProcess == null)
        return;

    // stop and cleanup the emulator
    AdbEmuKill(adbSettings);

    System.Threading.Thread.Sleep(5000);

    // kill the process if it has not already exited
    try { emulatorProcess.Kill(); }
    catch { }

    // delete the AVD
    try { AndroidAvdDelete(ANDROID_AVD, avdSettings); }
    catch { }
});

Task("Default")
    .Does(() =>
{
    if (string.IsNullOrEmpty(TEST_APP)) {
        if (string.IsNullOrEmpty(PROJECT.FullPath))
            throw new Exception("If no app was specified, an app must be provided.");
        var binDir = PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION).FullPath;
        var apps = GetFiles(binDir + "/*-Signed.apk");
        if (apps.Any()) {
            TEST_APP = apps.FirstOrDefault().FullPath;
        } else {
            apps = GetFiles(binDir + "/*.apk");
            TEST_APP = apps.First().FullPath;
        }
    }
    if (string.IsNullOrEmpty(TEST_APP_PACKAGE_NAME)) {
        var appFile = (FilePath)TEST_APP;
        appFile = appFile.GetFilenameWithoutExtension();
        TEST_APP_PACKAGE_NAME = appFile.FullPath.Replace("-Signed", "");
    }
    if (string.IsNullOrEmpty(TEST_APP_INSTRUMENTATION)) {
        TEST_APP_INSTRUMENTATION = TEST_APP_PACKAGE_NAME + ".TestInstrumentation";
    }
    if (string.IsNullOrEmpty(TEST_RESULTS)) {
        TEST_RESULTS = TEST_APP + "-results";
    }

    Information("Test App: {0}", TEST_APP);
    Information("Test App Package Name: {0}", TEST_APP_PACKAGE_NAME);
    Information("Test App Instrumentation: {0}", TEST_APP_INSTRUMENTATION);
    Information("Test Results Directory: {0}", TEST_RESULTS);

    CleanDirectories(TEST_RESULTS);

    RunProcess("xharness", "android test " +
        $"--app=\"{TEST_APP}\" " +
        $"--package-name=\"{TEST_APP_PACKAGE_NAME}\" " +
        $"--instrumentation=\"{TEST_APP_INSTRUMENTATION}\" " +
        $"--device-arch=\"{DEVICE_ARCH}\" " +
        $"--output-directory=\"{TEST_RESULTS}\" " +
        $"--verbosity=\"Debug\" ");
});

RunTarget(TARGET);
