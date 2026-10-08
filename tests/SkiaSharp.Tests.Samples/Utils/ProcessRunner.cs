using System.Diagnostics;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Runs any tool with captured output and a bounded process-tree lifetime.
internal static class ProcessRunner
{
    public static async Task<(int ExitCode, string Output, string Error)> Run(
        string executable,
        IEnumerable<string> arguments,
        string directory,
        TimeSpan timeout,
        Action<ProcessStartInfo>? configure = null)
    {
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }
        configure?.Invoke(start);

        WriteOutput($"{executable} {string.Join(" ", start.ArgumentList.Select(arg => $"\"{arg}\""))}");

        using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {executable}");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var cancellation = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException($"{executable} timed out after {timeout}: {directory}\n{await stdout}\n{await stderr}");
        }
        var text = await stdout;
        var errors = await stderr;

        WriteOutput(text);
        if (errors.Length > 0)
            WriteOutput(errors);

        return (process.ExitCode, text, errors);
    }

    private static void WriteOutput(string message)
    {
        if (TestContext.Current.TestOutputHelper is { } output)
            output.WriteLine(message);
        else
            TestContext.Current.SendDiagnosticMessage(message);
    }
}
