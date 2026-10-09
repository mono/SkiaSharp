using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Shares sample images between tests and cleans up only its own Docker resources.
public sealed class DockerSampleFixture : IAsyncLifetime
{
    private readonly Dictionary<string, string> images = new();
    private readonly HashSet<string> ownedTags = new();
    private readonly HashSet<string> containers = new();
    private bool probed;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async Task<string> Image(string folder, string dockerfile)
    {
        if (images.TryGetValue(folder, out var existing))
            return existing;

        await Probe();

        var identity = $"skiasharp-sample-test-{Guid.NewGuid():N}";
        var tag = $"skiasharp-sample-test:{identity}";
        var context = Path.Combine(Repo.ArtifactsDir, identity, "context");

        try
        {
            SampleWorkspace.CopyTree(Path.Combine(Repo.SamplesDir, folder), context);

            var packages = Path.Combine(context, "packages");
            Directory.CreateDirectory(packages);
            foreach (var package in Directory.EnumerateFiles(Repo.PackagesDir, "*.nupkg", SearchOption.AllDirectories))
            {
                File.Copy(package, Path.Combine(packages, Path.GetFileName(package)));
            }
            Assert.NotEmpty(Directory.EnumerateFiles(packages, "*.nupkg"));

            DotNet.WriteNuGetConfig(Path.Combine(context, "nuget.config"), "packages");

            ownedTags.Add(tag);
            await Run(["build", "--tag", tag, "--file", dockerfile, "."], TimeSpan.FromMinutes(30), context);
            images.Add(folder, tag);

            return tag;
        }
        finally
        {
            if (Directory.Exists(context))
                SampleWorkspace.DeleteDirectory(context);
        }
    }

    public void OwnContainer(string name) =>
        containers.Add(name);

    public async Task RemoveContainer(string name)
    {
        var listed = await Run(["ps", "--all", "--quiet", "--filter", $"name=^/{name}$"]);
        if (listed.Length > 0)
            await Run(["container", "rm", "--force", name]);

        containers.Remove(name);
    }

    public async Task<string> Run(IEnumerable<string> args, TimeSpan? timeout = null, string? workingDir = null)
    {
        var (exitCode, stdout, stderr) = await RunProcess(args, timeout, workingDir);
        Assert.True(exitCode == 0, $"docker {string.Join(" ", args)} failed ({exitCode}):\n{stdout}\n{stderr}");
        return stdout.Trim();
    }

    internal async Task Run(string tag, IEnumerable<string> arguments, Func<DockerRunningApp, Task> test)
    {
        var name = $"skiasharp-sample-test-{Guid.NewGuid():N}";
        var diagnostics = Path.Combine(Repo.ArtifactsDir, name);
        OwnContainer(name);
        try
        {
            var portBinding = OperatingSystem.IsWindows() ? "8080" : "127.0.0.1::8080";

            await Run(["run", "-d", "--name", name, "-p", portBinding, tag, .. arguments], TimeSpan.FromMinutes(2));

            var app = new DockerRunningApp(this, name, diagnostics);

            await app.Run(() => test(app));
        }
        finally
        {
            await RemoveContainer(name);
        }
    }

    internal static Task<(int ExitCode, string Output, string Error)> RunProcess(
        IEnumerable<string> args, TimeSpan? timeout = null, string? workingDir = null) =>
        ProcessRunner.Run(
            "docker",
            args,
            workingDir ?? Directory.GetCurrentDirectory(),
            timeout ?? TimeSpan.FromMinutes(1));

    private async Task Probe()
    {
        if (probed)
            return;

        var osType = await Run(["info", "--format", "{{.OSType}}"]);
        Assert.Equal(OperatingSystem.IsWindows() ? "windows" : "linux", osType);

        probed = true;
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
                var listed = await Run(["image", "ls", "--quiet", "--filter", $"reference={tag}"]);
                if (listed.Length > 0)
                    await Run(["image", "rm", "--force", tag]);
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
