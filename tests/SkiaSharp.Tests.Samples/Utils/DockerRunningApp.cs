using Xunit;
using SkiaSharp.Tests.Goldens;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class DockerRunningApp(string containerName, string diagnostics)
    : RunningApp(diagnostics)
{
    private GoldenEnvironment? environment;

    internal async Task<GoldenRequest> Reference(string caseName, string? variant = null)
    {
        if (environment is null)
        {
            var image = await Docker.ImageId(containerName);
            var metadata = await Docker.ImageEnvironment(image);
            var parts = metadata.Split('|');
            Assert.Equal(3, parts.Length);
            var architecture = parts[1] switch
            {
                "amd64" => "x64",
                "arm64" => "arm64",
                _ => throw new InvalidOperationException($"Unsupported rendering image architecture: {metadata}")
            };
            if (parts[0] == "linux")
            {
                var release = Path.Combine(Diagnostics, "os-release");
                await DownloadFile("/etc/os-release", release);
                environment = GoldenEnvironment.Linux(await File.ReadAllTextAsync(release), architecture);
            }
            else
            {
                Assert.Equal("windows", parts[0]);
                Assert.True(Version.TryParse(parts[2], out var version), $"Image OS version is required: {metadata}");
                environment = new("windows-nanoserver", $"{version!.Build}.{version.Revision}", architecture, metadata);
            }
            TestContext.Current.TestOutputHelper?.WriteLine($"Rendering image: {image}; {environment.Description}");
        }
        return environment.Request(caseName, variant);
    }

    internal override async Task<Uri> GetAddress()
    {
        var binding = await Docker.Port(containerName, "8080/tcp");
        var portText = binding.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        var success = int.TryParse(portText[(portText.LastIndexOf(':') + 1)..], out var port);

        Assert.True(success && port > 0, $"Docker did not publish an HTTP port: {binding}");

        return new Uri($"http://127.0.0.1:{port}");
    }

    internal override async Task<(int ExitCode, string Output, string Error)> WaitForExit()
    {
        var code = await Docker.Wait(containerName);
        Assert.True(int.TryParse(code, out var exitCode), $"Docker returned an invalid container exit code: {code}");

        var result = await ReadOutput();

        return (exitCode, result.Output, result.Error);
    }

    internal Task DownloadFile(string containerPath, string localPath) =>
        Docker.CopyFromContainer(containerName, containerPath, localPath);

    protected override async Task Stop()
    {
        var running = await Docker.InspectRunning(containerName);
        Assert.True(running is "true" or "false", $"Docker returned an invalid running state: {running}");

        if (running == "true")
            await Docker.Stop(containerName);
    }

    protected override Task<(string Output, string Error)> ReadOutput() =>
        Docker.Logs(containerName);
}
