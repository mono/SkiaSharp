////////////////////////////////////////////////////////////////////////////////////////////////////
// ANDROID EMULATOR — lifecycle and readiness belong to the released dotnet android tool
////////////////////////////////////////////////////////////////////////////////////////////////////

var TEST_ANDROID_DEVICE = Argument("androidDevice", EnvironmentVariable("TEST_ANDROID_DEVICE") ?? "emulator");
var TEST_ANDROID_IMAGE = Argument("androidImage", EnvironmentVariable("TEST_ANDROID_IMAGE") ?? "android-36;google_apis");
var TEST_ANDROID_MODEL = Argument("androidModel", EnvironmentVariable("TEST_ANDROID_MODEL") ?? "Nexus 5X");

void SetupAndroidEmulator(ref string ownedAvd, ref string ownedSerial)
{
    if (IsCakeListingCommand())
        return;

    if (ownedAvd != null || ownedSerial != null)
        throw new InvalidOperationException("This Cake run already owns an Android emulator.");

    var device = TEST_ANDROID_DEVICE.Trim().ToLowerInvariant();
    if (device == "physical")
        return;
    if (device != "emulator")
        throw new ArgumentException($"Invalid Android device type '{TEST_ANDROID_DEVICE}'. Expected emulator or physical.");

    if (!System.Text.RegularExpressions.Regex.IsMatch(TEST_ANDROID_IMAGE ?? "", @"^android-[0-9]+(?:\.[0-9]+)?;[a-zA-Z0-9_]+\z"))
        throw new ArgumentException($"Invalid Android emulator image: {TEST_ANDROID_IMAGE}");
    if (string.IsNullOrWhiteSpace(TEST_ANDROID_MODEL) || TEST_ANDROID_MODEL.Any(char.IsControl))
        throw new ArgumentException("Declare an Android emulator model.");

    var abi = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture switch {
        System.Runtime.InteropServices.Architecture.X64 => "x86_64",
        System.Runtime.InteropServices.Architecture.Arm64 => "arm64-v8a",
        _ => throw new ArgumentException("Android emulators require an arm64 or x64 host."),
    };

    var port = AndroidEmulatorPort();
    ownedAvd = $"SkiaSharp-Tests-{Guid.NewGuid():N}";
    ownedSerial = $"emulator-{port}";
    var logs = ROOT_OUTPUT_PATH.Combine($"logs/testlogs/android/{ownedAvd}");

    try {
        RunAndroidCli(logs.CombineWithFilePath("android-create.log"), "avd", "create",
            "--name", ownedAvd,
            "--sdk", $"system-images;{TEST_ANDROID_IMAGE};{abi}",
            "--device", TEST_ANDROID_MODEL,
            "--force");
        RunAndroidCli(logs.CombineWithFilePath("android-start.log"), "avd", "start",
            "--name", ownedAvd,
            "--port", port.ToString(),
            "--gpu", IsRunningOnLinux() ? "swiftshader_indirect" : "guest",
            "--no-window", "--no-snapshot", "--no-audio", "--no-boot-anim", "--no-animations",
            "--wait", "--timeout", "300",
            "--cpu-threshold", "3", "--response-threshold", "5",
            "--partition-size", "4096",
            "--camera-back", "none", "--camera-front", "none");
    } catch (Exception error) when (BuildSystem.IsLocalBuild) {
        TeardownAndroidEmulator(ref ownedAvd, ref ownedSerial, error);
        throw;
    }
}

void RunAndroidCli(FilePath logFile, params string[] arguments)
{
    var builder = new ProcessArgumentBuilder().Append("android");
    foreach (var argument in arguments)
        builder.AppendQuoted(argument);
    var sdk = EnvironmentVariable("ANDROID_HOME");
    if (!string.IsNullOrWhiteSpace(sdk))
        builder.Append("--home").AppendQuoted(sdk);
    EnsureDirectoryExists(logFile.GetDirectory());
    RunProcess("dotnet", new ProcessSettings {
        Arguments = builder,
        WorkingDirectory = ROOT_PATH,
        EnvironmentVariables = new Dictionary<string, string> {
            ["ANDROID_TOOL_PROCESS_RUNNER_LOG_PATH"] = logFile.FullPath,
        },
    });
}

void StopCIAndroidEmulators()
{
    if (BuildSystem.IsLocalBuild || IsCakeListingCommand())
        return;

    // CI agents are disposable; stop emulator processes even when ADB is unavailable.
    foreach (var process in System.Diagnostics.Process.GetProcesses()) {
        using (process) {
            if ((process.ProcessName != "emulator" && !process.ProcessName.StartsWith("qemu-system-", StringComparison.Ordinal)) || process.HasExited)
                continue;
            Information("Stopping CI Android emulator process {0} ({1}).", process.Id, process.ProcessName);
            process.Kill(entireProcessTree: true);
            if (!process.WaitForExit(30000))
                Warning("CI Android emulator process {0} has not exited after 30 seconds; continuing cleanup.", process.Id);
        }
    }
}

int AndroidEmulatorPort()
{
    for (var port = 5554; port <= 5682; port += 2) {
        var console = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
        var transport = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port + 1);
        try {
            console.Start();
            transport.Start();
            return port;
        } catch (System.Net.Sockets.SocketException) {
            // Leave other devices alone and choose an unused pair.
        } finally {
            console.Stop();
            transport.Stop();
        }
    }
    throw new Exception("No unused Android emulator port pair is available.");
}

void TeardownAndroidEmulator(ref string ownedAvd, ref string ownedSerial, Exception originalError)
{
    try {
        if (!BuildSystem.IsLocalBuild) {
            try {
                if (ownedAvd != null && ownedSerial != null) {
                    Information("Android cleanup: AVD {0}, serial {1}.", ownedAvd, ownedSerial);
                    var logFile = ROOT_OUTPUT_PATH.Combine($"logs/testlogs/android-cleanup/{ownedAvd}")
                        .CombineWithFilePath("android-cleanup.log");
                    RunAndroidCli(logFile, "device", "info", "--id", "^" + ownedSerial + "$", "--format", "json");
                }
            } finally {
                StopCIAndroidEmulators();
            }
        }
        if (ownedAvd == null)
            return;

        if (BuildSystem.IsLocalBuild && ownedSerial != null)
            StopLocalAndroidEmulator(ownedSerial);
        var deleteLog = ROOT_OUTPUT_PATH.Combine($"logs/testlogs/android/{ownedAvd}").CombineWithFilePath("android-delete.log");
        RunAndroidCli(deleteLog, "avd", "delete", "--name", ownedAvd);
        ownedAvd = null;
        ownedSerial = null;
    } catch (Exception error) {
        if (originalError != null)
            throw new AggregateException("Android operation and cleanup failed.", originalError, error);
        throw;
    }
}

// Replace with dotnet android avd stop when available in a released tool.
void StopLocalAndroidEmulator(string serial)
{
    DirectoryPath sdk = EnvironmentVariable("ANDROID_HOME");
    var adb = sdk.Combine("platform-tools").CombineWithFilePath(IsRunningOnWindows() ? "adb.exe" : "adb");
    Information("Stopping owned Android emulator {0}.", serial);
    RunProcess(adb, $"-s {serial} emu kill");
}
