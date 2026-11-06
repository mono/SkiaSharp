using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class Docker
{
    internal static Task<string> Ps(string name) =>
        RunCommand(["ps", "--all", "--quiet", "--filter", $"name=^/{name}$"]);

    internal static Task<string> Images(string tag) =>
        RunCommand(["image", "ls", "--quiet", "--filter", $"reference={tag}"]);

    internal static Task RemoveContainer(string name) =>
        RunCommand(["container", "rm", "--force", name]);

    internal static Task RemoveImage(string tag) =>
        RunCommand(["image", "rm", "--force", tag]);

    internal static Task Build(string tag, string dockerfile, string context) =>
        RunCommand(["build", "--tag", tag, "--file", dockerfile, "."], TimeSpan.FromMinutes(30), context);

    internal static Task Run(string tag, string name, string portBinding, IEnumerable<string> arguments) =>
        RunCommand(["run", "-d", "--name", name, "-p", portBinding, tag, .. arguments], TimeSpan.FromMinutes(2));

    internal static Task<(int ExitCode, string Output, string Error)> Info() =>
        Execute(["info", "--format", "{{.OSType}}"], TimeSpan.FromSeconds(30));

    internal static Task<string> Port(string name, string port) =>
        RunCommand(["port", name, port]);

    internal static Task<string> Wait(string name) =>
        RunCommand(["wait", name], TimeSpan.FromMinutes(3));

    internal static Task CopyFromContainer(string name, string containerPath, string localPath) =>
        RunCommand(["cp", "--follow-link", $"{name}:{containerPath}", localPath]);

    internal static Task<string> InspectRunning(string name) =>
        RunCommand(["inspect", "--format", "{{.State.Running}}", name]);

    internal static Task<string> ImageId(string name) =>
        RunCommand(["inspect", "--format", "{{.Image}}", name]);

    internal static Task<string> ImageEnvironment(string image) =>
        RunCommand(["image", "inspect", "--format", "{{.Os}}|{{.Architecture}}|{{.OsVersion}}", image]);

    internal static Task Stop(string name) =>
        RunCommand(["container", "stop", "--time", "5", name]);

    internal static async Task<(string Output, string Error)> Logs(string name)
    {
        var result = await Execute(["logs", name]);
        Assert.True(result.ExitCode == 0, $"Failed to capture Docker sample logs: {result.Error}");
        return (result.Output, result.Error);
    }

    private static async Task<string> RunCommand(IEnumerable<string> arguments, TimeSpan? timeout = null, string? workingDirectory = null)
    {
        var (exitCode, output, error) = await Execute(arguments, timeout, workingDirectory);
        Assert.True(exitCode == 0, $"docker {string.Join(" ", arguments)} failed ({exitCode}):\n{output}\n{error}");
        return output.Trim();
    }

    private static Task<(int ExitCode, string Output, string Error)> Execute(
        IEnumerable<string> arguments, TimeSpan? timeout = null, string? workingDirectory = null) =>
        ProcessRunner.Run(
            "docker",
            arguments,
            workingDirectory ?? Directory.GetCurrentDirectory(),
            timeout ?? TimeSpan.FromMinutes(1));
}
