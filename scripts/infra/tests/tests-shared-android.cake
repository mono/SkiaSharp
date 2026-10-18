////////////////////////////////////////////////////////////////////////////////////////////////////
// ANDROID EMULATOR — one named AVD and explicit serial owned by this Cake run
////////////////////////////////////////////////////////////////////////////////////////////////////

string ANDROID_AVD_MANAGER = null;
string ANDROID_ADB = null;
System.Diagnostics.Process ANDROID_EMULATOR_PROCESS = null;
System.Threading.Tasks.Task<string> ANDROID_EMULATOR_STDOUT = null;
System.Threading.Tasks.Task<string> ANDROID_EMULATOR_STDERR = null;

void SetupAndroidEmulator(ref string ownedAvd, ref string ownedSerial, string systemImage, string deviceModel)
{
    var listingOptions = new[] { "--showdescription", "--dryrun", "--noop", "--whatif" };
    if (System.Environment.GetCommandLineArgs().Any(arg =>
        listingOptions.Contains(arg.Split('=')[0], StringComparer.OrdinalIgnoreCase)))
        return;

    if (string.IsNullOrWhiteSpace(systemImage) ||
        !System.Text.RegularExpressions.Regex.IsMatch(systemImage,
            @"^system-images;android-[0-9]+(?:\.[0-9]+)?;[a-zA-Z0-9_]+;(?:x86|x86_64|armeabi-v7a|arm64-v8a)$") ||
        string.IsNullOrWhiteSpace(deviceModel) || deviceModel.Any(char.IsControl))
        throw new ArgumentException("Declare a full Android system image (including ABI) and a device model.");
    if (ownedAvd != null || ownedSerial != null || ANDROID_EMULATOR_PROCESS != null)
        throw new InvalidOperationException("This Cake run already owns an Android emulator.");

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
    try {
        AndroidAvdCommand("create", "avd", "--name", ownedAvd, "--package", systemImage, "--device", deviceModel);
        ownedSerial = $"emulator-{port}";
        var gpu = IsRunningOnLinux() ? "swiftshader_indirect" : "guest";
        var start = new System.Diagnostics.ProcessStartInfo(
            System.IO.Path.Combine(sdk, "emulator", IsRunningOnWindows() ? "emulator.exe" : "emulator")) {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in new[] {
            "-avd", ownedAvd, "-port", port.ToString(), "-gpu", gpu,
            "-no-window", "-no-snapshot", "-no-audio", "-no-boot-anim", "-show-kernel",
            "-camera-back", "none", "-camera-front", "none",
        })
            start.ArgumentList.Add(argument);

        // Retain the process before readiness: adb may never become available on a failed boot.
        ANDROID_EMULATOR_PROCESS = System.Diagnostics.Process.Start(start);
        ANDROID_EMULATOR_STDOUT = ANDROID_EMULATOR_PROCESS.StandardOutput.ReadToEndAsync();
        ANDROID_EMULATOR_STDERR = ANDROID_EMULATOR_PROCESS.StandardError.ReadToEndAsync();
        WaitForAndroidBoot(ownedSerial);
        AssertOwnedAndroidEmulator(ownedAvd, ownedSerial);
        foreach (var setting in new[] { "window_animation_scale", "transition_animation_scale", "animator_duration_scale" }) {
            if (StartProcess(ANDROID_ADB, new ProcessSettings {
                Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial)
                    .Append("shell settings put global").Append(setting).Append("0"),
                Timeout = 10000,
            }) != 0)
                throw new Exception($"Failed to disable {setting} on owned Android emulator {ownedSerial}.");
        }
    } catch (Exception error) {
        TeardownAndroidEmulator(ref ownedAvd, ref ownedSerial, error);
        throw;
    }
}

void WaitForAndroidBoot(string ownedSerial)
{
    // Match the repository device suite's five-minute boot budget, not a consumer session timeout.
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
    var settings = new ProcessSettings {
        Arguments = new ProcessArgumentBuilder().Append("-s").AppendQuoted(ownedSerial).Append("emu avd name"),
        RedirectStandardOutput = true,
        Timeout = 30000,
    };
    var exit = StartProcess(ANDROID_ADB, settings, out var stdout);
    if (exit != 0 || !stdout.Any(line => line.Trim().Equals(ownedAvd, StringComparison.Ordinal)))
        throw new Exception($"The exact serial {ownedSerial} did not identify owned AVD {ownedAvd}.");
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

void AndroidAvdCommand(params string[] arguments)
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
    Information("{0}", stdout.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult());
    Information("{0}", stderr.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult());
    if (process.ExitCode != 0)
        throw new Exception($"Selected avdmanager failed with exit code {process.ExitCode}.");
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
            System.IO.File.WriteAllText(System.IO.Path.Combine(logs, "emulator.stdout.log"),
                ANDROID_EMULATOR_STDOUT.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult());
            System.IO.File.WriteAllText(System.IO.Path.Combine(logs, "emulator.stderr.log"),
                ANDROID_EMULATOR_STDERR.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult());
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
