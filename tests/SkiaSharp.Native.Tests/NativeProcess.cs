using System.Diagnostics;
using Xunit;

namespace SkiaSharp.Native.Tests;

internal static class NativeProcess
{
    public static async Task RunAsync(string executable, string[] arguments, string workingDirectory)
    {
        var start = new ProcessStartInfo(executable) {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start)
            ?? throw new InvalidOperationException($"Could not start {executable}.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        var timedOut = false;
        try {
            await process.WaitForExitAsync(timeout.Token);
        } catch (OperationCanceledException) when (timeout.IsCancellationRequested) {
            timedOut = true;
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }

        var output = $"Command: {executable} {string.Join(" ", arguments)}\n" +
            $"Working directory: {workingDirectory}\n" +
            $"stdout:\n{await stdout}\nstderr:\n{await stderr}";
        Assert.False(timedOut, $"Native test exceeded five minutes.\n{output}");
        Assert.True(process.ExitCode == 0, $"Native test exited {process.ExitCode}.\n{output}");
    }
}
