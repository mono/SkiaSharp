using System.Globalization;
using System.Text.Json;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class Apple
{
    internal static async Task<ToolCommandResult> CreateSimulator(IosDevice device)
    {
        device.Validate();
        var result = await ProcessRunner.Run("xcrun", ["simctl", "list", "runtimes", "--json"],
            Repo.RootDir, TimeSpan.FromSeconds(30));
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"Could not inspect installed simulator runtimes: {result.Output}\n{result.Error}");
        using var runtimes = JsonDocument.Parse(result.Output);
        if (!runtimes.RootElement.GetProperty("runtimes").EnumerateArray().Any(runtime =>
            runtime.GetProperty("name").GetString() == $"iOS {device.Version}" &&
            runtime.GetProperty("version").GetString() == device.Version &&
            runtime.GetProperty("isAvailable").GetBoolean()))
            throw new InvalidOperationException($"Required iOS {device.Version} runtime is not installed. Tests never download runtimes.");
        return await Execute(["simulator", "create", device.Name, "--device-type", "iPhone 16 Pro",
            "--runtime", $"iOS {device.Version}", "--format", "json"]);
    }

    internal static Task<ToolCommandResult> DeleteSimulator(string udid) =>
        Execute(["simulator", "delete", SimulatorId(udid), "--force"]);

    internal static Task<ToolCommandResult> ListSimulators(IosDevice device)
    {
        device.Validate();
        return Execute(["simulator", "list", "--available", "--name", device.Name,
            "--runtime", $"iOS {device.Version}", "--format", "json"]);
    }

    internal static Task<ToolCommandResult> BootSimulator(string udid) =>
        Execute(["simulator", "boot", SimulatorId(udid), "--wait", "--timeout", "300"], TimeSpan.FromMinutes(6));

    internal static Task<ToolCommandResult> ShutdownSimulator(string udid) =>
        Execute(["simulator", "shutdown", SimulatorId(udid)]);

    internal static Task<ToolCommandResult> RuntimeLogs(string udid, DateTime start, string executable) =>
        Execute(["simulator", "logs", SimulatorId(udid),
            "--start", start.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            "--predicate", $"process == {JsonSerializer.Serialize(executable)}"]);

    internal static Task<ToolCommandResult> BundleIdentifier(string artifact) => BundleProperty(artifact, "CFBundleIdentifier");
    internal static Task<ToolCommandResult> BundleExecutable(string artifact) => BundleProperty(artifact, "CFBundleExecutable");

    private static string SimulatorId(string udid)
    {
        if (!Guid.TryParse(udid, out _))
            throw new ArgumentException("Pass the exact selected simulator UDID, not a name or 'booted' alias.", nameof(udid));
        return udid;
    }

    private static async Task<ToolCommandResult> BundleProperty(string artifact, string key)
    {
        string[] arguments = ["-extract", key, "raw", "-o", "-", Path.Combine(artifact, "Info.plist")];
        var result = await ProcessRunner.Run("/usr/bin/plutil", arguments, Repo.RootDir, TimeSpan.FromSeconds(30));
        return new($"/usr/bin/plutil {string.Join(" ", arguments)}", result.ExitCode, result.Output, result.Error);
    }

    private static async Task<ToolCommandResult> Execute(string[] arguments, TimeSpan? timeout = null)
    {
        var result = await DotNet.ExecuteTool("apple", arguments,
            new DotNetOptions { Timeout = timeout ?? TimeSpan.FromSeconds(30) });
        return new($"dotnet tool run apple -- {string.Join(" ", arguments)}", result.ExitCode, result.Output, result.Error);
    }
}
