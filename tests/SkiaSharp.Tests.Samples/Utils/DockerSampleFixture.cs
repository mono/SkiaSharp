using System.ComponentModel;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Shares sample images between tests and cleans up only its own Docker resources.
public sealed class DockerSampleFixture : IAsyncLifetime
{
    private readonly Dictionary<string, string> images = new();
    private readonly HashSet<string> ownedTags = new();
    private readonly HashSet<string> containers = new();
    private bool probed;
    private string? unavailableReason;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    internal async Task<string> Image(string folder, Func<string, Task> build)
    {
        if (images.TryGetValue(folder, out var existing))
            return existing;

        await Probe();

        var identity = SampleWorkspace.CreateIdentity(folder);
        var tag = $"skiasharp-sample-test:{identity}";
        ownedTags.Add(tag);
        await build(tag);
        images.Add(folder, tag);
        return tag;
    }

    public void OwnContainer(string name) =>
        containers.Add(name);

    public async Task RemoveContainer(string name)
    {
        var listed = await Docker.Ps(name);
        if (listed.Length > 0)
            await Docker.RemoveContainer(name);

        containers.Remove(name);
    }

    internal async Task Run(string tag, IEnumerable<string> arguments, Func<DockerRunningApp, Task> test)
    {
        var folder = images.Single(image => image.Value == tag).Key;
        var name = SampleWorkspace.CreateIdentity(folder);
        var diagnostics = Path.Combine(Repo.ArtifactsDir, name);
        OwnContainer(name);
        try
        {
            var portBinding = OperatingSystem.IsWindows() ? "8080" : "127.0.0.1::8080";

            await Docker.Run(tag, name, portBinding, arguments);

            var app = new DockerRunningApp(name, diagnostics);

            await app.Run(() => test(app));
        }
        finally
        {
            await RemoveContainer(name);
        }
    }

    private async Task Probe()
    {
        if (!probed)
        {
            try
            {
                var result = await Docker.Info();
                if (result.ExitCode != 0)
                    unavailableReason = $"Docker is unavailable: docker info exited {result.ExitCode}.\n{result.Output}\n{result.Error}";
                else
                    Assert.Equal(OperatingSystem.IsWindows() ? "windows" : "linux", result.Output.Trim());
            }
            catch (Win32Exception error) when (error.NativeErrorCode is 2 or 3)
            {
                unavailableReason = $"Docker CLI is unavailable: {error.Message}. PATH={Environment.GetEnvironmentVariable("PATH")}";
            }
            catch (TimeoutException error)
            {
                unavailableReason = $"Docker info did not respond within 30 seconds: {error.Message}";
            }
            probed = true;
        }

        if (unavailableReason is not null)
            Assert.Skip(unavailableReason);
    }

    public async ValueTask DisposeAsync()
    {
        // Attempt all owned-resource cleanup before reporting any failures.
        var errors = new List<Exception>();
        foreach (var name in containers.ToArray())
        {
            try
            {
                await RemoveContainer(name);
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }
        foreach (var tag in ownedTags)
        {
            try
            {
                var listed = await Docker.Images(tag);
                if (listed.Length > 0)
                    await Docker.RemoveImage(tag);
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }
        if (errors.Count > 0)
            throw new AggregateException("Owned Docker resource cleanup failed", errors);
    }
}
