////////////////////////////////////////////////////////////////////////////////////////////////////
// ANDROID EMULATOR — one named AVD and explicit serial owned by this Cake run
////////////////////////////////////////////////////////////////////////////////////////////////////

var TEST_ANDROID_DEVICE = Argument("androidDevice", EnvironmentVariable("TEST_ANDROID_DEVICE") ?? "emulator");
var TEST_ANDROID_IMAGE = Argument("androidImage", EnvironmentVariable("TEST_ANDROID_IMAGE") ?? "android-36;google_apis");
var TEST_ANDROID_MODEL = Argument("androidModel", EnvironmentVariable("TEST_ANDROID_MODEL") ?? "Nexus 5X");

string ANDROID_AVD_MANAGER = null;
string ANDROID_ADB = null;
System.Diagnostics.Process ANDROID_EMULATOR_PROCESS = null;
System.Threading.Tasks.Task<string> ANDROID_EMULATOR_STDOUT = null;
System.Threading.Tasks.Task<string> ANDROID_EMULATOR_STDERR = null;

void SetupAndroidEmulator(ref string ownedAvd, ref string ownedSerial)
{
    if (IsCakeListingCommand())
        return;

    if (ownedAvd != null || ownedSerial != null || ANDROID_EMULATOR_PROCESS != null)
        throw new InvalidOperationException("This Cake run already owns an Android emulator.");

    var device = TEST_ANDROID_DEVICE.Trim().ToLowerInvariant();
    if (device == "physical") {
        Information("Using a physical device:");
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
    var systemImage = $"system-images;{TEST_ANDROID_IMAGE};{abi}";

    Information("Creating owned AVD...");
    Information("  SDK: {0}", systemImage);
    Information("  Device: {0}", TEST_ANDROID_MODEL);

    var sdk = EnvironmentVariable("ANDROID_HOME");
    var sdkManager = EnvironmentVariable("ANDROID_SDK_MANAGER_PATH");
    if (string.IsNullOrWhiteSpace(sdk) || !System.IO.Directory.Exists(sdk) ||
        string.IsNullOrWhiteSpace(sdkManager) || !System.IO.File.Exists(sdkManager))
        throw new Exception("ANDROID_HOME and the selected ANDROID_SDK_MANAGER_PATH must exist.");

    var imagePath = System.IO.Path.Combine(sdk, systemImage.Replace(';', System.IO.Path.DirectorySeparatorChar));
    if (!System.IO.File.Exists(System.IO.Path.Combine(imagePath, "system.img")))
        throw new Exception($"The exact selected emulator image must already be installed: {imagePath}");

    // Follow SDK provisioning's selected command-line tools, including latest-2 layouts.
    ANDROID_AVD_MANAGER = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(sdkManager),
        IsRunningOnWindows() ? "avdmanager.bat" : "avdmanager");
    ANDROID_ADB = System.IO.Path.Combine(sdk, "platform-tools", IsRunningOnWindows() ? "adb.exe" : "adb");
    if (!System.IO.File.Exists(ANDROID_AVD_MANAGER) || !System.IO.File.Exists(ANDROID_ADB) ||
        !System.IO.File.Exists(System.IO.Path.Combine(sdk, "emulator", IsRunningOnWindows() ? "emulator.exe" : "emulator")))
        throw new Exception("The selected SDK must provide avdmanager, adb and emulator.");

    var port = AndroidEmulatorPort();
    ownedAvd = $"SkiaSharp-Tests-{Guid.NewGuid():N}";
    var appium = EnvironmentVariable("TEST_ANDROID_PREINSTALL_APPIUM_SERVER") == "1";
    try {
        AndroidAvdCommand("create", "avd", "--name", ownedAvd, "--package", systemImage, "--device", TEST_ANDROID_MODEL);
        ownedSerial = $"emulator-{port}";
        var gpu = IsRunningOnLinux() ? "swiftshader_indirect" : "guest";
        var start = new System.Diagnostics.ProcessStartInfo(
            System.IO.Path.Combine(sdk, "emulator", IsRunningOnWindows() ? "emulator.exe" : "emulator")) {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        var arguments = new List<string> {
            "-avd", ownedAvd, "-port", port.ToString(), "-gpu", gpu,
        };
        if (appium)
            arguments.AddRange(new[] { "-cores", "2", "-memory", "4096" });
        arguments.AddRange(new[] {
            "-partition-size", "4096",
            "-no-window", "-no-snapshot", "-no-audio", "-no-boot-anim", "-show-kernel",
            "-camera-back", "none", "-camera-front", "none",
        });
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        // Retain the process before readiness: adb may never become available on a failed boot.
        ANDROID_EMULATOR_PROCESS = System.Diagnostics.Process.Start(start);
        ANDROID_EMULATOR_STDOUT = ANDROID_EMULATOR_PROCESS.StandardOutput.ReadToEndAsync();
        ANDROID_EMULATOR_STDERR = ANDROID_EMULATOR_PROCESS.StandardError.ReadToEndAsync();
        if (appium) {
            WaitForAndroidAppiumBoot(ownedAvd, ownedSerial);
            InstallAndroidAppiumArtifacts(ownedSerial);
        } else {
            WaitForAndroidBoot(ownedSerial);
            AssertOwnedAndroidEmulator(ownedAvd, ownedSerial);
        }
        foreach (var setting in new[] { "window_animation_scale", "transition_animation_scale", "animator_duration_scale" }) {
            if (StartProcess(ANDROID_ADB, new ProcessSettings {
                Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial)
                    .Append("shell settings put global").Append(setting).Append("0"),
                Timeout = 10000,
            }) != 0)
                throw new Exception($"Failed to disable {setting} on owned Android emulator {ownedSerial}.");
        }
        ListAndroidDevices();
    } catch (Exception error) {
        var failure = error;
        if (appium && ANDROID_EMULATOR_PROCESS != null && !ANDROID_EMULATOR_PROCESS.HasExited && ownedSerial != null) {
            try {
                CaptureOwnedAndroidBootDiagnostics(ownedAvd, ownedSerial);
            } catch (Exception diagnosticsError) {
                failure = new AggregateException("Owned Android setup and boot diagnostics failed.", error, diagnosticsError);
            }
        }
        TeardownAndroidEmulator(ref ownedAvd, ref ownedSerial, failure);
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
}

void InstallAndroidAppiumArtifacts(string ownedSerial)
{
    var home = EnvironmentVariable("APPIUM_HOME");
    if (string.IsNullOrWhiteSpace(home))
        home = System.IO.Path.Combine(EnvironmentVariable("HOME"), ".appium");
    if (!System.IO.Directory.Exists(home))
        throw new Exception($"The installed Appium home does not exist: {home}");

    var server = System.IO.Directory.GetFiles(home, "appium-uiautomator2-server-v*.apk",
        System.IO.SearchOption.AllDirectories).Single();
    var testServer = System.IO.Directory.GetFiles(home, "appium-uiautomator2-server-debug-androidTest.apk",
        System.IO.SearchOption.AllDirectories).Single();
    foreach (var apk in new[] { server, testServer }) {
        Install(apk, "UiAutomator2 server");
    }

    var appsRoot = System.IO.Path.Combine(ROOT_OUTPUT_PATH.FullPath, "appium-apps", "android");
    var apps = System.IO.Directory.Exists(appsRoot)
        ? System.IO.Directory.GetFiles(appsRoot, "app.apk", System.IO.SearchOption.AllDirectories)
        : Array.Empty<string>();
    if (apps.Length == 0)
        throw new Exception($"Acquire prepared Android Appium applications before booting the emulator: {appsRoot}");
    foreach (var app in apps)
        Install(app, "prepared Appium application");

    Environment.SetEnvironmentVariable("TEST_ANDROID_APPIUM_SERVER_PREINSTALLED", "1");

    void Install(string apk, string description)
    {
        var exit = StartProcess(ANDROID_ADB, new ProcessSettings {
            Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial)
                .Append("install -r --no-incremental").AppendQuoted(apk),
            Timeout = 120000,
        });
        if (exit != 0)
            throw new Exception($"Failed to preinstall the exact {description} on {ownedSerial}: {apk}");
    }
}

void CaptureOwnedAndroidBootDiagnostics(string ownedAvd, string ownedSerial)
{
    AssertOwnedAndroidEmulator(ownedAvd, ownedSerial);
    var logs = System.IO.Path.Combine(ROOT_PATH.FullPath, "output", "logs", "android", ownedAvd);
    System.IO.Directory.CreateDirectory(logs);
    // These shell diagnostics are not exposed by the pinned dotnet android CLI.
    foreach (var diagnostic in new[] {
        (Name: "last-anr", Command: "dumpsys activity lastanr"),
        (Name: "resources", Command: "dumpsys cpuinfo && dumpsys meminfo"),
    }) {
        var exit = StartProcess(ANDROID_ADB, new ProcessSettings {
            Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial)
                .Append("shell").AppendQuoted(diagnostic.Command),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Timeout = 30000,
        }, out var stdout, out var stderr);
        var lines = stdout.Concat(stderr).ToArray();
        System.IO.File.WriteAllLines(System.IO.Path.Combine(logs, diagnostic.Name + ".log"), lines);
        Information("Owned Android {0} (first 120 lines):\n{1}", diagnostic.Name, string.Join("\n", lines.Take(120)));
        if (diagnostic.Name == "resources")
            Information("Owned Android memory summary (last 40 lines):\n{0}", string.Join("\n", lines.TakeLast(40)));
        if (exit != 0)
            throw new Exception($"Owned Android {diagnostic.Name} diagnostics failed with exit code {exit}.");
    }
}

void ListAndroidDevices()
{
    Information("Available Android devices:");
    DotNetTool("android device list");
}

void WaitForAndroidBoot(string ownedSerial)
{
    var clock = System.Diagnostics.Stopwatch.StartNew();
    while (clock.Elapsed < TimeSpan.FromSeconds(300)) {
        if (ANDROID_EMULATOR_PROCESS.HasExited)
            throw new Exception($"Owned Android emulator exited during boot: {ANDROID_EMULATOR_PROCESS.ExitCode}.");
        var exit = StartProcess(ANDROID_ADB, new ProcessSettings {
            Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial)
                .Append("shell getprop sys.boot_completed"),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Timeout = 10000,
        }, out var stdout, out var stderr);
        if (exit == 0 && stdout.Any(line => line.Trim() == "1"))
            return;
        System.Threading.Thread.Sleep(500);
    }
    throw new TimeoutException($"Owned Android emulator {ownedSerial} did not complete boot within 300 seconds.");
}

void WaitForAndroidAppiumBoot(string ownedAvd, string ownedSerial)
{
    // Package Manager readiness is observable directly; aggregate Linux load is not a service readiness signal.
    var clock = System.Diagnostics.Stopwatch.StartNew();
    var stable = 0;
    var last = "boot properties unavailable";
    while (clock.Elapsed < TimeSpan.FromSeconds(300)) {
        if (ANDROID_EMULATOR_PROCESS.HasExited)
            throw new Exception($"Owned Android emulator exited during boot: {ANDROID_EMULATOR_PROCESS.ExitCode}.");
        var response = System.Diagnostics.Stopwatch.StartNew();
        var exit = StartProcess(ANDROID_ADB, new ProcessSettings {
            Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial)
                .Append("shell").AppendQuoted("getprop sys.boot_completed; pm path android; cat /proc/loadavg"),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Timeout = 10000,
        }, out var stdout, out var stderr);
        var lines = stdout.Select(line => line.Trim()).Where(line => line.Length > 0).ToArray();
        var ready = exit == 0 && lines.Length == 3 && lines[0] == "1" &&
            lines[1].StartsWith("package:/", StringComparison.Ordinal) &&
            response.Elapsed < TimeSpan.FromSeconds(5);
        var identityReady = ready && HasOwnedAndroidEmulatorIdentity(ownedAvd, ownedSerial);
        ready = identityReady && response.Elapsed < TimeSpan.FromSeconds(5);
        last = $"exit={exit}, response={response.Elapsed.TotalSeconds:F1}s, identity={(identityReady ? "exact" : "not confirmed")}, output={string.Join(" | ", lines)}, error={string.Join(" | ", stderr)}";
        stable = ready ? stable + 1 : 0;
        if (stable == 3) {
            Information("Owned Android emulator ready: {0}", last);
            return;
        }
        System.Threading.Thread.Sleep(1000);
    }
    throw new TimeoutException($"Owned Android emulator {ownedSerial} did not reach boot/Package Manager/identity/response readiness within 300 seconds: {last}.");
}

int AndroidEmulatorPort()
{
    for (var port = 5554; port <= 5682; port += 2) {
        var console = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
        var adb = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port + 1);
        try {
            console.Start();
            adb.Start();
            return port;
        } catch (System.Net.Sockets.SocketException) {
            // A busy pair belongs to another process; never stop it or reuse its device.
        } finally {
            console.Stop();
            adb.Stop();
        }
    }
    throw new Exception("No unused Android emulator console/adb port pair is available.");
}

void AssertOwnedAndroidEmulator(string ownedAvd, string ownedSerial)
{
    if (!HasOwnedAndroidEmulatorIdentity(ownedAvd, ownedSerial))
        throw new Exception($"The exact serial {ownedSerial} has not reported owned AVD {ownedAvd}.");
}

bool HasOwnedAndroidEmulatorIdentity(string ownedAvd, string ownedSerial)
{
    var settings = new ProcessSettings {
        Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial).Append("emu avd name"),
        RedirectStandardOutput = true,
        Timeout = 30000,
    };
    var exit = StartProcess(ANDROID_ADB, settings, out var stdout);
    if (exit != 0)
        throw new Exception($"The exact serial {ownedSerial} did not identify owned AVD {ownedAvd}.");

    var identity = AndroidEmulatorIdentityLines(stdout);
    if (identity.Length == 0) {
        settings.Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial)
            .Append("shell getprop ro.boot.qemu.avd_name");
        exit = StartProcess(ANDROID_ADB, settings, out stdout);
        identity = stdout.Select(line => line.Trim()).Where(line => line.Length > 0).ToArray();
        if (exit == 0 && identity.Length == 0) {
            // Older images publish the same exact AVD name under the legacy kernel prefix.
            settings.Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial)
                .Append("shell getprop ro.kernel.qemu.avd_name");
            exit = StartProcess(ANDROID_ADB, settings, out stdout);
            identity = stdout.Select(line => line.Trim()).Where(line => line.Length > 0).ToArray();
        }
    }

    if (exit != 0 || identity.Length > 1 ||
        (identity.Length == 1 && !identity[0].Equals(ownedAvd, StringComparison.Ordinal)))
        throw new Exception($"The exact serial {ownedSerial} did not identify owned AVD {ownedAvd}. " +
            $"Exit: {exit}; reported identity: {string.Join(" | ", identity)}");
    return identity.Length == 1;
}

string[] AndroidEmulatorIdentityLines(IEnumerable<string> output)
{
    var lines = output.Select(line => line.Trim()).Where(line => line.Length > 0).ToList();
    if (lines.Count > 0 && lines[lines.Count - 1].Equals("OK", StringComparison.Ordinal))
        lines.RemoveAt(lines.Count - 1);
    return lines.ToArray();
}

string AndroidAvdWindowsArguments(string manager, params string[] arguments)
{
    // cmd expands percent variables even inside quotes; reject inputs it cannot pass literally.
    if (new[] { manager }.Concat(arguments).Any(value => value.Any(c => c == '"' || c == '%' || char.IsControl(c))))
        throw new ArgumentException("Windows avdmanager paths and arguments cannot contain quotes, percent signs or control characters.");

    var command = new ProcessArgumentBuilder().AppendQuoted(manager);
    foreach (var argument in arguments)
        command.AppendQuoted(argument);
    return $"/d /v:off /s /c \"{command.Render()}\"";
}

string AndroidAvdCommand(params string[] arguments)
{
    var start = new System.Diagnostics.ProcessStartInfo(IsRunningOnWindows() ? "cmd.exe" : ANDROID_AVD_MANAGER) {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    if (IsRunningOnWindows()) {
        start.Arguments = AndroidAvdWindowsArguments(ANDROID_AVD_MANAGER, arguments);
    } else {
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
    }
    using var process = System.Diagnostics.Process.Start(start);
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    if (arguments[0] == "create")
        process.StandardInput.WriteLine("no");
    process.StandardInput.Close();
    if (!process.WaitForExit(30000)) {
        process.Kill(entireProcessTree: true);
        if (!process.WaitForExit(30000))
            throw new TimeoutException("Owned avdmanager process did not stop within 30 seconds.");
        throw new TimeoutException("Selected avdmanager did not finish within 30 seconds.");
    }
    var output = stdout.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
    Information("{0}", output);
    Information("{0}", stderr.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult());
    if (process.ExitCode != 0)
        throw new Exception($"Selected avdmanager failed with exit code {process.ExitCode}.");
    return output;
}

void TeardownAndroidEmulator(ref string ownedAvd, ref string ownedSerial, Exception originalError)
{
    if (ownedAvd == null)
        return;

    var failures = new List<Exception>();
    if (ANDROID_EMULATOR_PROCESS != null) {
        try {
            if (!ANDROID_EMULATOR_PROCESS.HasExited)
                ANDROID_EMULATOR_PROCESS.Kill(entireProcessTree: true);
            if (!ANDROID_EMULATOR_PROCESS.WaitForExit(30000))
                throw new TimeoutException($"Owned Android emulator {ownedSerial} did not stop within 30 seconds.");
        } catch (Exception error) {
            failures.Add(error);
        }

        try {
            var logs = System.IO.Path.Combine(ROOT_PATH.FullPath, "output", "logs", "android", ownedAvd);
            System.IO.Directory.CreateDirectory(logs);
            var stdout = ANDROID_EMULATOR_STDOUT.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
            var stderr = ANDROID_EMULATOR_STDERR.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
            System.IO.File.WriteAllText(System.IO.Path.Combine(logs, "emulator.stdout.log"),
                stdout);
            System.IO.File.WriteAllText(System.IO.Path.Combine(logs, "emulator.stderr.log"),
                stderr);
            if (originalError != null) {
                Information("Owned Android emulator stdout (last 100 lines):\n{0}",
                    string.Join("\n", stdout.Split('\n').TakeLast(100)));
                Information("Owned Android emulator stderr (last 100 lines):\n{0}",
                    string.Join("\n", stderr.Split('\n').TakeLast(100)));
            }
        } catch (Exception error) {
            failures.Add(error);
        }
    }

    if (ANDROID_EMULATOR_PROCESS == null || ANDROID_EMULATOR_PROCESS.HasExited) {
        ANDROID_EMULATOR_PROCESS?.Dispose();
        ANDROID_EMULATOR_PROCESS = null;
        ANDROID_EMULATOR_STDOUT = null;
        ANDROID_EMULATOR_STDERR = null;
        // Never remove an AVD's files while its owned emulator is still running.
        try {
            // Creation can fail before registering any AVD. Non-compact listing also includes
            // invalid/partially created AVDs, which must still be removed by their exact name.
            var namePattern = @"^\s*Name:\s*" + System.Text.RegularExpressions.Regex.Escape(ownedAvd) + @"\s*$";
            var exists = ownedSerial != null || AndroidAvdCommand("list", "avd").Split('\n').Any(line =>
                System.Text.RegularExpressions.Regex.IsMatch(line, namePattern));
            if (exists)
                AndroidAvdCommand("delete", "avd", "--name", ownedAvd);
            ownedAvd = null;
            ownedSerial = null;
        } catch (Exception error) {
            failures.Add(error);
        }
    }
    if (failures.Count != 0) {
        if (originalError != null)
            failures.Insert(0, originalError);
        throw new AggregateException("Owned Android emulator cleanup failed.", failures);
    }
}
