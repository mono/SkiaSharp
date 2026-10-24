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
    if (device == "physical") {
        ListAndroidDevices();
        return;
    }
    if (device != "emulator")
        throw new ArgumentException($"Invalid Android device type '{TEST_ANDROID_DEVICE}'. Expected emulator or physical.");
    if (!System.Text.RegularExpressions.Regex.IsMatch(TEST_ANDROID_IMAGE ?? "",
        @"^android-[0-9]+(?:\.[0-9]+)?;[a-zA-Z0-9_]+\z"))
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
    var logs = System.IO.Path.Combine(ROOT_OUTPUT_PATH.FullPath, "logs", "android", ownedAvd);
    System.IO.Directory.CreateDirectory(logs);

    try {
        AndroidCommand("avd", "create", "--name", ownedAvd, "--sdk",
            $"system-images;{TEST_ANDROID_IMAGE};{abi}", "--device", TEST_ANDROID_MODEL, "--force");
        var arguments = new List<string> {
            "avd", "start", "--name", ownedAvd, "--port", port.ToString(),
            "--no-window", "--gpu", IsRunningOnLinux() ? "swiftshader_indirect" : "guest",
            "--no-snapshot", "--no-audio", "--no-boot-anim", "--wait", "--timeout", "300",
            "--no-animations", "--cpu-threshold", "3", "--response-threshold", "5",
            "--partition-size", "4096", "--camera-back", "none", "--camera-front", "none",
        };
        var settings = AndroidCommandSettings(arguments.ToArray());
        settings.EnvironmentVariables = new Dictionary<string, string> {
            ["ANDROID_TOOL_PROCESS_RUNNER_LOG_PATH"] = System.IO.Path.Combine(logs, "android-tool.log"),
        };
        RunProcess("dotnet", settings);

        ListAndroidDevices();
        if (!AndroidDeviceSerials().Contains(ownedSerial, StringComparer.Ordinal))
            throw new Exception($"Android startup did not list the expected owned emulator: {ownedSerial} ({ownedAvd}).");
    } catch (Exception error) {
        TeardownAndroidEmulator(ref ownedAvd, ref ownedSerial, error);
        throw;
    }
}

ProcessSettings AndroidCommandSettings(params string[] arguments)
{
    var builder = new ProcessArgumentBuilder().Append("android");
    foreach (var argument in arguments)
        builder.AppendQuoted(argument);
    var sdk = EnvironmentVariable("ANDROID_HOME");
    if (!string.IsNullOrWhiteSpace(sdk))
        builder.Append("--home").AppendQuoted(sdk);
    return new ProcessSettings { Arguments = builder, WorkingDirectory = ROOT_PATH };
}

void AndroidCommand(params string[] arguments) =>
    RunProcess("dotnet", AndroidCommandSettings(arguments));

string[] AndroidDeviceSerials()
{
    RunProcess("dotnet", AndroidCommandSettings("device", "list", "--format", "json"), out var output);
    using var json = System.Text.Json.JsonDocument.Parse(string.Join("\n", output));
    return json.RootElement.EnumerateArray().Select(device => device.GetProperty("serial").GetString()).ToArray();
}

void ListAndroidDevices()
{
    Information("Available Android devices:");
    AndroidCommand("device", "list");
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
    if (ownedAvd == null)
        return;

    try {
        // The released CLI has no strict stop operation. Keep direct ADB cleanup-only.
        if (ownedSerial != null) {
            var serial = ownedSerial;
            var sdk = EnvironmentVariable("ANDROID_HOME");
            if (string.IsNullOrWhiteSpace(sdk))
                throw new Exception("ANDROID_HOME must select the SDK for cleanup.");
            var adb = System.IO.Path.Combine(sdk, "platform-tools", IsRunningOnWindows() ? "adb.exe" : "adb");
            var port = int.Parse(ownedSerial.Substring("emulator-".Length));
            var clock = System.Diagnostics.Stopwatch.StartNew();
            var nextStopAttempt = TimeSpan.Zero;
            while (true) {
                var state = DeviceState();
                if (state == null && ConsolePortAvailable())
                    break;
                if (clock.Elapsed > TimeSpan.FromSeconds(30))
                    throw new TimeoutException($"Owned Android emulator did not stop: {ownedSerial} (transport: {state ?? "missing"}).");
                if (state == "device" && clock.Elapsed >= nextStopAttempt) {
                    var identity = Property("ro.boot.qemu.avd_name");
                    if (identity.Length == 0)
                        identity = Property("ro.kernel.qemu.avd_name");
                    if (identity != ownedAvd)
                        throw new Exception($"Cleanup refused: {ownedSerial} does not identify owned AVD {ownedAvd}.");
                    Information("Stopping owned Android emulator {0} ({1}).", serial, ownedAvd);
                    RunProcess(adb, new ProcessSettings {
                        Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(serial).Append("emu kill"),
                        Timeout = 30000,
                    });
                    nextStopAttempt = clock.Elapsed + TimeSpan.FromSeconds(5);
                }
                System.Threading.Thread.Sleep(500);
            }

            string DeviceState()
            {
                RunProcess(adb, new ProcessSettings { Arguments = "devices", Timeout = 30000 }, out var devices);
                return devices.Select(line => line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
                    .Where(fields => fields.Length >= 2 && fields[0] == serial)
                    .Select(fields => fields[1]).SingleOrDefault();
            }

            string Property(string property)
            {
                RunProcess(adb, new ProcessSettings {
                    Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(serial)
                        .Append("shell getprop").Append(property),
                    Timeout = 30000,
                }, out var output);
                return string.Join("\n", output).Trim();
            }

            bool ConsolePortAvailable()
            {
                var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
                try {
                    listener.Start();
                    return true;
                } catch (System.Net.Sockets.SocketException) {
                    return false;
                } finally {
                    listener.Stop();
                }
            }
        }
        AndroidCommand("avd", "delete", "--name", ownedAvd);
        ownedAvd = null;
        ownedSerial = null;
    } catch (Exception error) {
        if (originalError != null)
            throw new AggregateException("Android operation and cleanup failed.", originalError, error);
        throw;
    }
}
