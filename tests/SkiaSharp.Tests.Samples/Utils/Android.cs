using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Samples.Utils;

// Explicit sample-compatibility candidates, not claims about installed runtimes or driver readiness.
internal sealed record AndroidDevice(string Name, string Version, int Api, string Serial, int PageSize)
{
    internal static AndroidDevice PixelApi26 { get; } =
        new("Pixel_API_26", "8.0", 26, "emulator-5558", 4096);

    internal static AndroidDevice PixelApi37 { get; } =
        new("Pixel_API_37", "17", 37, "emulator-5560", 16384);

    internal int Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || !Regex.IsMatch(Name, @"^[A-Za-z0-9_.-]+$"))
            throw new ArgumentException("Declare the exact AVD name, not a path or wildcard.", nameof(Name));
        if (string.IsNullOrWhiteSpace(Version) || !Regex.IsMatch(Version, @"^[1-9][0-9]*(\.[0-9]+)*$") || Api <= 0)
            throw new ArgumentException("Declare the exact Android release and a positive API level.", nameof(Version));
        if (PageSize is < 4096 or > 65536 || (PageSize & (PageSize - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(PageSize), "Declare the expected power-of-two page size in bytes.");
        var selected = Regex.Match(Serial ?? "", @"^emulator-([0-9]+)$");
        var port = 0;
        if (!selected.Success || !int.TryParse(selected.Groups[1].Value, out port) ||
            port is < 5554 or > 5682 || port % 2 != 0 || Serial != $"emulator-{port}")
            throw new ArgumentException("Declare an exact emulator serial with an even console port from 5554 through 5682.", nameof(Serial));
        return port;
    }
}

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

    internal Task<ToolCommandResult> ListAvds() =>
        ExecuteSdk(["avd", "list", "--format", "json"]);

    internal Task<ToolCommandResult> ListDevices() =>
        ExecuteSdk(["device", "list", "--format", "json"]);

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
            26 => $"system-images;android-26;google_apis;{architecture}",
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
        return ExecuteSdk(["avd", "delete", "--name", device.Name, "--force"]);
    }

    internal Task<ToolCommandResult> AvdName(AndroidDevice device) =>
        Adb(device, ["emu", "avd", "name"]);

    internal Task<ToolCommandResult> PageSize(AndroidDevice device) =>
        Adb(device, ["shell", "getconf", "PAGE_SIZE"]);

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
