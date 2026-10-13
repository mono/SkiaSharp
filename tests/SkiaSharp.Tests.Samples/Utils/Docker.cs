using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class Docker
{
    internal static async Task<string> Run(IEnumerable<string> arguments, TimeSpan? timeout = null, string? workingDirectory = null)
    {
        var (exitCode, output, error) = await Execute(arguments, timeout, workingDirectory);
        Assert.True(exitCode == 0, $"docker {string.Join(" ", arguments)} failed ({exitCode}):\n{output}\n{error}");
        return output.Trim();
    }

    internal static Task<(int ExitCode, string Output, string Error)> Execute(
        IEnumerable<string> arguments, TimeSpan? timeout = null, string? workingDirectory = null) =>
        ProcessRunner.Run(
            "docker",
            arguments,
            workingDirectory ?? Directory.GetCurrentDirectory(),
            timeout ?? TimeSpan.FromMinutes(1));
}
