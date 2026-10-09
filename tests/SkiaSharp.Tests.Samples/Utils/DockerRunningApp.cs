using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class DockerRunningApp(DockerSampleFixture docker, string containerName, string diagnostics)
    : RunningApp(diagnostics)
{
    internal override async Task<Uri> GetAddress()
    {
        var binding = await docker.Run(["port", containerName, "8080/tcp"]);
        var portText = binding.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        var success = int.TryParse(portText[(portText.LastIndexOf(':') + 1)..], out var port);

        Assert.True(success && port > 0, $"Docker did not publish an HTTP port: {binding}");

        return new Uri($"http://127.0.0.1:{port}");
    }

    internal override async Task<(int ExitCode, string Output, string Error)> WaitForExit()
    {
        var code = await docker.Run(["wait", containerName], TimeSpan.FromMinutes(3));
        Assert.True(int.TryParse(code, out var exitCode), $"Docker returned an invalid container exit code: {code}");

        var result = await ReadOutput();

        return (exitCode, result.Output, result.Error);
    }

    internal Task DownloadFile(string containerPath, string localPath) =>
        docker.Run(["cp", $"{containerName}:{containerPath}", localPath]);

    protected override async Task Stop()
    {
        var running = await docker.Run(["inspect", "--format", "{{.State.Running}}", containerName]);
        Assert.True(running is "true" or "false", $"Docker returned an invalid running state: {running}");

        if (running == "true")
            await docker.Run(["container", "stop", "--time", "5", containerName]);
    }

    protected override async Task<(string Output, string Error)> ReadOutput()
    {
        var result = await DockerSampleFixture.RunProcess(["logs", containerName]);
        Assert.True(result.ExitCode == 0, $"Failed to capture Docker sample logs: {result.Error}");

        return (result.Output, result.Error);
    }
}
