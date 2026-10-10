using System.Diagnostics;
using System.Text;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class DotNetRunningApp : RunningApp
{
    private readonly Process process;
    private readonly StringBuilder output = new();
    private readonly TaskCompletionSource<Uri> address = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Task stdout;
    private readonly Task<string> stderr;
    private readonly Task exit;

    internal DotNetRunningApp(Process process, string diagnostics)
        : base(diagnostics)
    {
        this.process = process;
        stdout = CaptureOutput();
        stderr = process.StandardError.ReadToEndAsync();
        exit = process.WaitForExitAsync();
    }

    internal override async Task<(int ExitCode, string Output, string Error)> WaitForExit()
    {
        await exit.WaitAsync(TimeSpan.FromMinutes(3), TestContext.Current.CancellationToken);

        var result = await ReadOutput();
        return (process.ExitCode, result.Output, result.Error);
    }

    protected override async Task<(string Output, string Error)> ReadOutput()
    {
        await stdout.WaitAsync(TimeSpan.FromMinutes(1));
        var errors = await stderr.WaitAsync(TimeSpan.FromMinutes(1));
        return (output.ToString(), errors);
    }

    internal override async Task<Uri> GetAddress()
    {
        var completed = await Task.WhenAny(address.Task, exit).WaitAsync(TimeSpan.FromMinutes(2), TestContext.Current.CancellationToken);
        if (completed == address.Task)
            return await address.Task;

        var result = await WaitForExit();

        throw new InvalidOperationException($"App exited ({result.ExitCode}) before listening:\n{result.Output}\n{result.Error}");
    }

    protected override async Task Stop()
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);

        await exit.WaitAsync(TimeSpan.FromMinutes(1));
    }

    private async Task CaptureOutput()
    {
        while (await process.StandardOutput.ReadLineAsync() is { } line)
        {
            output.AppendLine(line);
            if (ParseAddress(line) is { } uri)
                address.TrySetResult(uri);
        }
    }

    internal static Uri? ParseAddress(string line)
    {
        var text = line.Trim();
        var prefix = text.StartsWith("Now listening on: ", StringComparison.Ordinal)
            ? "Now listening on: "
            : "App url: ";
        return text.StartsWith(prefix, StringComparison.Ordinal) &&
            Uri.TryCreate(text[prefix.Length..], UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttp && uri.Host == "127.0.0.1" && uri.Port > 0
            ? uri
            : null;
    }
}
