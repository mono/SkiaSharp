using System.ComponentModel;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

public sealed class DockerSampleFixture : IAsyncLifetime
{
    private readonly Dictionary<string, string> images = new();
    private readonly HashSet<string> ownedTags = new();
    private readonly HashSet<string> containers = new();
    private bool probed;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async Task<string> Image(string folder, string dockerfile, ITestOutputHelper output)
    {
        if (images.TryGetValue(folder, out var existing))
            return existing;
        await Probe(output);

        var identity = $"skiasharp-sample-test-{Guid.NewGuid():N}";
        var tag = $"skiasharp-sample-test:{identity}";
        var context = Path.Combine(DotNet.Setting("ArtifactsDirectory"), identity, "context");
        try
        {
            SampleWorkspace.CopyTree(Path.Combine(DotNet.Setting("SamplesDirectory"), folder), context);
            var packages = Path.Combine(context, "packages");
            Directory.CreateDirectory(packages);
            foreach (var package in Directory.EnumerateFiles(DotNet.Setting("PackageDirectory"), "*.nupkg", SearchOption.AllDirectories))
                File.Copy(package, Path.Combine(packages, Path.GetFileName(package)));
            Assert.NotEmpty(Directory.EnumerateFiles(packages, "*.nupkg"));
            DotNet.WriteNuGetConfig(Path.Combine(context, "nuget.config"), "packages");
            ownedTags.Add(tag);
            await Run(["build", "--tag", tag, "--file", dockerfile, "."], context, TimeSpan.FromMinutes(30), output);
            images.Add(folder, tag);
            return tag;
        }
        finally
        {
            if (Directory.Exists(context))
                Directory.Delete(context, recursive: true);
        }
    }

    public void OwnContainer(string name) => containers.Add(name);

    public async Task RemoveContainer(string name, ITestOutputHelper? output)
    {
        var listed = await Run(["ps", "--all", "--quiet", "--filter", $"name=^/{name}$"],
            Directory.GetCurrentDirectory(), TimeSpan.FromMinutes(1), output);
        if (listed.Length > 0)
            await Run(["container", "rm", "--force", name], Directory.GetCurrentDirectory(), TimeSpan.FromMinutes(1), output);
        containers.Remove(name);
    }

    public async Task<string> Run(IEnumerable<string> args, string directory, TimeSpan timeout, ITestOutputHelper? output)
    {
        var (exitCode, stdout, stderr) = await DotNet.RunProcess("docker", args, directory, timeout, output);
        Assert.True(exitCode == 0, $"docker {string.Join(" ", args)} failed ({exitCode}):\n{stdout}\n{stderr}");
        return stdout.Trim();
    }

    private async Task Probe(ITestOutputHelper output)
    {
        if (probed)
            return;
        (int ExitCode, string Output, string Error) result;
        try
        {
            result = await DotNet.RunProcess("docker", ["info", "--format", "{{.OSType}}"],
                Directory.GetCurrentDirectory(), TimeSpan.FromMinutes(1), output);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
        {
            Assert.Skip($"Docker executable is unavailable: {ex.Message}");
            throw;
        }
        if (result.ExitCode != 0)
            Assert.Skip($"Docker daemon is unavailable: {result.Error}");
        Assert.Equal(OperatingSystem.IsWindows() ? "windows" : "linux", result.Output.Trim());
        probed = true;
    }

    public async ValueTask DisposeAsync()
    {
        var errors = new List<Exception>();
        foreach (var name in containers.ToArray())
            try
            {
                await RemoveContainer(name, null);
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        foreach (var tag in ownedTags)
        {
            try
            {
                var listed = await Run(["image", "ls", "--quiet", "--filter", $"reference={tag}"],
                    Directory.GetCurrentDirectory(), TimeSpan.FromMinutes(1), null);
                if (listed.Length > 0)
                    await Run(["image", "rm", "--force", tag], Directory.GetCurrentDirectory(), TimeSpan.FromMinutes(1), null);
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
