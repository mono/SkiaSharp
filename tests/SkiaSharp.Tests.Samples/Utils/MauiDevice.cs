using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class MauiDevice : IAsyncDisposable
{
    internal const string AndroidName = "Pixel_API_37";
    internal const string AndroidVersion = "17";
    internal const int AndroidApi = 37;
    internal const string AndroidSerial = "emulator-5556";
    internal const string IosName = "iPhone 16 Pro";
    internal const string IosVersion = "26.2";
    internal const string WdaBundleId = "com.skiasharp.samples.tests.WebDriverAgentRunner";

    private readonly SampleWorkspace workspace;
    private readonly bool ios;
    private string? adb;
    private bool bootedSimulator;
    private bool ownsApp;
    private string? applicationId;
    private bool ownsWda;
    private Process? emulator;
    private Task<string>? emulatorOutput;
    private Task<string>? emulatorError;
    private readonly DateTime started = DateTime.Now;

    internal string Id { get; private set; } = "";

    internal MauiDevice(SampleWorkspace workspace, bool ios)
    {
        this.workspace = workspace;
        this.ios = ios;
    }

    internal async Task Prepare(string name, string version, string appId)
    {
        applicationId = appId;
        if (ios)
        {
            var json = await Tool("apple", ["simulator", "list", "--available", "--name", name,
                "--runtime", $"iOS {version}", "--format", "json"]);
            var selected = RequireIosSimulator(json, name, version);
            Id = selected.Id;
            if (selected.State != "Shutdown" &&
                Environment.GetEnvironmentVariable("SKIASHARP_MAUI_USE_BOOTED_DEVICE") != "1")
                throw new InvalidOperationException(
                    $"Selected simulator {Id} is already {selected.State}. Shut it down or explicitly dedicate it using SKIASHARP_MAUI_USE_BOOTED_DEVICE=1.");
            if (selected.State == "Shutdown")
            {
                bootedSimulator = true;
            }
            await Tool("apple", ["simulator", "boot", Id, "--wait", "--timeout", "300"], TimeSpan.FromMinutes(6));
            if (await HasApp(appId))
                throw new InvalidOperationException($"Selected simulator already contains {appId}; refusing to overwrite its app/data.");
            if (await HasApp(WdaBundleId + ".xctrunner"))
                throw new InvalidOperationException("Selected simulator already contains the test WDA runner; refusing to reuse a foreign driver.");
        }
        else
        {
            var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME")
                ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
                ?? throw new InvalidOperationException("Set ANDROID_HOME or ANDROID_SDK_ROOT to the explicitly installed Android SDK.");
            adb = Path.Combine(sdk, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");
            if (!File.Exists(adb))
                throw new FileNotFoundException("Install Android platform-tools before running MAUI device tests.", adb);
            Id = AndroidSerial;
            using var connected = JsonDocument.Parse(await Tool("android", ["device", "list", "--format", "json"]));
            var alreadyRunning = connected.RootElement.EnumerateArray()
                .Any(device => device.GetProperty("serial").GetString() == Id);
            if (!alreadyRunning)
            {
                using var avds = JsonDocument.Parse(await Tool("android", ["avd", "list", "--format", "json"]));
                if (!avds.RootElement.EnumerateArray().Any(avd => avd.GetProperty("name").GetString() == name))
                    throw new InvalidOperationException($"Create the explicitly selected {name} AVD before running the MAUI tests.");
                AppiumFixture.RequireFreePort(5556);
                AppiumFixture.RequireFreePort(5557);
                emulator = ProcessRunner.Start("dotnet",
                    ["tool", "run", "android", "--", "avd", "start", "--name", name, "--port", "5556",
                        "--home", sdk, "--wait", "--wait-exit", "--timeout", "300", "--no-snapshot", "--no-boot-anim"],
                    Repo.RootDir);
                var booted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                emulatorOutput = ReadBootOutput(emulator, booted);
                emulatorError = emulator.StandardError.ReadToEndAsync();
                await Task.WhenAny(booted.Task, emulator.WaitForExitAsync()).WaitAsync(TimeSpan.FromMinutes(6));
                if (!booted.Task.IsCompletedSuccessfully || emulator.HasExited)
                    throw new InvalidOperationException($"Owned {name} emulator did not boot; inspect the dotnet android log.");
                Assert.Equal("1", (await Adb("shell", "getprop", "sys.boot_completed")).Trim());
            }
            else if (Environment.GetEnvironmentVariable("SKIASHARP_MAUI_USE_BOOTED_DEVICE") != "1")
                throw new InvalidOperationException(
                    $"Explicitly dedicate {name} at {Id} with SKIASHARP_MAUI_USE_BOOTED_DEVICE=1; tests will take foreground focus.");
            var avd = await Adb("emu", "avd", "name");
            Assert.Equal(name, avd.Split('\n')[0].Trim());
            Assert.Equal(version, (await Adb("shell", "getprop", "ro.build.version.release")).Trim());
            Assert.Equal(AndroidApi.ToString(), (await Adb("shell", "getprop", "ro.build.version.sdk")).Trim());
            Assert.Equal("16384", (await Adb("shell", "getconf", "PAGE_SIZE")).Trim());
            if (await HasApp(appId))
                throw new InvalidOperationException($"Selected emulator already contains {appId}; refusing to overwrite its app/data.");
        }
    }

    internal async Task Install(string artifact)
    {
        // Mark ownership before installation so partial session/launch failures still clean up.
        ownsApp = true;
        if (ios)
            await Tool("apple", ["simulator", "app", "install", Id, artifact], TimeSpan.FromMinutes(2));
        else
            await Tool("android", ["device", "install", "--id", $"^{Id}$", "--pkg", artifact], TimeSpan.FromMinutes(2));
        Assert.True(await HasApp(applicationId!), "The selected tool did not install the actual generated sample.");
    }

    internal async Task<string> AndroidActivity() =>
        (await Adb("shell", "cmd", "package", "resolve-activity", "--brief", applicationId!))
            .Split('\n', StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split('/').Last();

    internal void OwnWda() => ownsWda = ios;

    internal async Task CaptureLogs()
    {
        if (ios && ownsApp)
            await Tool("apple", ["simulator", "logs", Id, "--start", started.ToString("yyyy-MM-dd HH:mm:ss"),
                "--predicate", "processImagePath CONTAINS 'SkiaSharpSample'"]);
        else if (ownsApp)
        {
            var pid = (await Adb("shell", "pidof", applicationId!)).Trim();
            await Adb("logcat", "-d", "-t", "500", $"--pid={pid}");
        }
    }

    internal static (string Id, string State) RequireIosSimulator(string json, string name, string version)
    {
        using var devices = JsonDocument.Parse(json);
        var matches = new List<(string Id, string State)>();
        foreach (var device in devices.RootElement.EnumerateArray())
            if (device.GetProperty("name").GetString() == name && device.GetProperty("isAvailable").GetBoolean() &&
                device.GetProperty("runtime").GetProperty("name").GetString() == $"iOS {version}" &&
                device.GetProperty("runtime").GetProperty("version").GetString() == version)
                matches.Add((device.GetProperty("udid").GetString()
                    ?? throw new InvalidOperationException("Selected simulator has no UDID."),
                    device.GetProperty("state").GetString() ?? ""));
        if (matches.Count != 1)
            throw new InvalidOperationException(
                $"Required exactly one available '{name}' simulator on iOS {version}, found {matches.Count}. Install the exact runtime/device; no fallback is selected.");
        return matches[0];
    }

    private Task<string> Adb(params string[] arguments) =>
        Command(adb ?? throw new InvalidOperationException("Android preflight has not completed."),
            ["-s", Id, .. arguments]);

    private Task<string> Tool(string tool, string[] arguments, TimeSpan? timeout = null) =>
        Command("dotnet", ["tool", "run", tool, "--", .. arguments], timeout, Repo.RootDir);

    private async Task<bool> HasApp(string appId)
    {
        using var apps = JsonDocument.Parse(ios
            ? await Tool("apple", ["simulator", "app", "list", Id, "--format", "json"])
            : await Tool("android", ["device", "packages", "--id", $"^{Id}$", "--format", "json"]));
        return apps.RootElement.EnumerateArray().Any(app =>
            app.GetProperty(ios ? "CFBundleIdentifier" : "packageName").GetString() == appId);
    }

    private static async Task<string> ReadBootOutput(Process process, TaskCompletionSource booted)
    {
        var text = new StringBuilder();
        while (await process.StandardOutput.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            text.AppendLine(line);
            if (line.Contains("Boot wait completed in", StringComparison.Ordinal))
                booted.TrySetResult();
        }
        return text.ToString();
    }

    private async Task<string> Command(string tool, string[] arguments, TimeSpan? timeout = null, string? directory = null)
    {
        var result = await ProcessRunner.Run(tool, arguments, directory ?? workspace.Root, timeout ?? TimeSpan.FromSeconds(30));
        var diagnostics = Path.Combine(workspace.DiagnosticsRoot, "device");
        Directory.CreateDirectory(diagnostics);
        var path = Path.Combine(diagnostics, $"{Guid.NewGuid():N}.log");
        await File.WriteAllTextAsync(path,
            $"{tool} {string.Join(" ", arguments)}\nExit: {result.ExitCode}\n{result.Output}\n{result.Error}");
        TestContext.Current.AddFileAttachment(path, "text/plain");
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"{tool} failed ({result.ExitCode}); inspect {path}.");
        return result.Output;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (ownsApp)
            {
                if (await HasApp(applicationId!))
                {
                    if (ios)
                        await Tool("apple", ["simulator", "app", "uninstall", Id, applicationId!]);
                    else
                        await Tool("android", ["device", "uninstall", "--id", $"^{Id}$", "--pkg", applicationId!]);
                    Assert.False(await HasApp(applicationId!), "The selected tool did not remove the owned sample.");
                }
            }
        }
        finally
        {
            try
            {
                try
                {
                    if (ownsWda && await HasApp(WdaBundleId + ".xctrunner"))
                        await Tool("apple", ["simulator", "app", "uninstall", Id, WdaBundleId + ".xctrunner"]);
                }
                finally
                {
                    if (bootedSimulator)
                        await Tool("apple", ["simulator", "shutdown", Id]);
                }
            }
            finally
            {
                if (emulator is not null)
                {
                    try
                    {
                        if (!emulator.HasExited)
                            emulator.Kill(entireProcessTree: true);
                        await emulator.WaitForExitAsync();
                        var log = Path.Combine(workspace.DiagnosticsRoot, "emulator.log");
                        await File.WriteAllTextAsync(log, $"{await emulatorOutput!}\n{await emulatorError!}");
                        TestContext.Current.AddFileAttachment(log, "text/plain");
                    }
                    finally
                    {
                        emulator.Dispose();
                    }
                }
            }
        }
    }
}
