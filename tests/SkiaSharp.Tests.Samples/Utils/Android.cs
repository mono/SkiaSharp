using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Samples.Utils;

// The caller selects the installed SDK; repository tool resolution remains owned by DotNet.
internal sealed class Android
{
    private readonly string adb;
    internal string SdkDirectory { get; }

    internal Android(string sdkDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sdkDirectory);
        SdkDirectory = Path.GetFullPath(sdkDirectory);
        adb = Path.Combine(SdkDirectory, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");
        if (!File.Exists(adb))
            throw new FileNotFoundException("Install Android platform-tools before selecting Android tests.", adb);
    }

    internal Task<ToolCommandResult> Manifest(string artifact) =>
        Execute(["apk", "manifest", "info", "--pkg", artifact, "--format", "xml"]);

    internal Task<ToolCommandResult> ListAvds() => ExecuteSdk(["avd", "list", "--format", "json"]);
    internal Task<ToolCommandResult> ListDevices() => ExecuteSdk(["device", "list", "--format", "json"]);

    internal Task<ToolCommandResult> CreateAvd(AndroidDevice device)
    {
        device.Validate();
        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => "arm64-v8a",
            Architecture.X64 => "x86_64",
            _ => throw new InvalidOperationException("The declared emulator profiles require an arm64 or x64 host.")
        };
        var image = device.Api switch
        {
            24 => $"system-images;android-24;google_apis;{architecture}",
            37 => $"system-images;android-37.1;google_apis_ps16k;{architecture}",
            _ => throw new ArgumentOutOfRangeException(nameof(device), "Declare an installed image for the selected profile.")
        };
        var imageDirectory = Path.Combine(SdkDirectory, image.Replace(';', Path.DirectorySeparatorChar));
        if (!File.Exists(Path.Combine(imageDirectory, "package.xml")) ||
            !File.Exists(Path.Combine(imageDirectory, "system.img")))
            throw new DirectoryNotFoundException($"Required emulator image is not installed: {imageDirectory}. Tests never download images.");
        return ExecuteSdk(["avd", "create", "--name", device.Name, "--sdk", image, "--device", "pixel_9"]);
    }

    internal Task<ToolCommandResult> DeleteAvd(AndroidDevice device)
    {
        device.Validate();
        return ExecuteSdk(["avd", "delete", "--name", device.Name]);
    }

    internal Task<ToolCommandResult> ListPackages(AndroidDevice device) =>
        ExecuteSdk(["device", "packages", "--id", DeviceSelector(device), "--format", "json"]);

    internal Task<ToolCommandResult> Install(AndroidDevice device, string artifact) =>
        ExecuteSdk(["device", "install", "--id", DeviceSelector(device), "--pkg", artifact], TimeSpan.FromMinutes(2));

    internal Task<ToolCommandResult> Uninstall(AndroidDevice device, string applicationId) =>
        ExecuteSdk(["device", "uninstall", "--id", DeviceSelector(device), "--pkg", applicationId]);

    internal Task<ToolCommandResult> Launch(AndroidDevice device, string applicationId, string activity) =>
        ExecuteSdk(["device", "launch", "--id", DeviceSelector(device), "--pkg", applicationId, "--activity", activity],
            TimeSpan.FromMinutes(2));

    internal Process StartAvd(AndroidDevice device)
    {
        var port = device.Validate();
        return DotNet.StartTool("android",
            ["avd", "start", "--name", device.Name, "--port", port.ToString(CultureInfo.InvariantCulture),
                "--home", SdkDirectory, "--wait", "--wait-exit", "--timeout", "300", "--no-snapshot", "--no-boot-anim"]);
    }

    internal Task<ToolCommandResult> AvdName(AndroidDevice device) => Adb(device, ["emu", "avd", "name"]);
    internal Task<ToolCommandResult> Property(AndroidDevice device, string name) => Adb(device, ["shell", "getprop", name]);
    internal Task<ToolCommandResult> PageSize(AndroidDevice device) => Adb(device, ["shell", "getconf", "PAGE_SIZE"]);
    internal Task<ToolCommandResult> LauncherActivity(AndroidDevice device, string applicationId) =>
        Adb(device, ["shell", "cmd", "package", "resolve-activity", "--brief", applicationId]);
    internal Task<ToolCommandResult> ForceStop(AndroidDevice device, string applicationId) =>
        Adb(device, ["shell", "am", "force-stop", applicationId]);
    internal Task<ToolCommandResult> RuntimeLogs(AndroidDevice device) =>
        Adb(device, ["logcat", "-d", "-t", "5000", "-s", "AndroidRuntime", "DOTNET", "mono-stdout", "mono-stderr"]);
    internal Task<ToolCommandResult> ListForwards() => RunAdb(["forward", "--list"]);
    internal Task<ToolCommandResult> RemoveForward(AndroidDevice device, int port)
    {
        if (port is <= 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port));
        return Adb(device, ["forward", "--remove", $"tcp:{port.ToString(CultureInfo.InvariantCulture)}"]);
    }

    private static string DeviceSelector(AndroidDevice device)
    {
        device.Validate();
        return $"^{Regex.Escape(device.Serial)}$";
    }

    private Task<ToolCommandResult> Adb(AndroidDevice device, string[] arguments)
    {
        device.Validate();
        return RunAdb(["-s", device.Serial, .. arguments]);
    }

    private async Task<ToolCommandResult> RunAdb(string[] arguments)
    {
        var result = await ProcessRunner.Run(adb, arguments, Repo.RootDir, TimeSpan.FromSeconds(30));
        return new($"{adb} {string.Join(" ", arguments)}", result.ExitCode, result.Output, result.Error);
    }

    private Task<ToolCommandResult> ExecuteSdk(string[] arguments, TimeSpan? timeout = null) =>
        Execute([.. arguments, "--home", SdkDirectory], timeout);

    private static async Task<ToolCommandResult> Execute(string[] arguments, TimeSpan? timeout = null)
    {
        var result = await DotNet.ExecuteTool("android", arguments,
            new DotNetOptions { Timeout = timeout ?? TimeSpan.FromSeconds(30) });
        return new($"dotnet tool run android -- {string.Join(" ", arguments)}", result.ExitCode, result.Output, result.Error);
    }
}
