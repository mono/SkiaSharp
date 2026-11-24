using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public abstract class DockerSampleTestBase(DockerSampleFixture docker)
{
    protected Task<string> BuildSample(string folder, string dockerfile)
    {
        folder = folder.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
        return docker.Image(folder, async tag =>
        {
            var context = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity(folder), "context");
            try
            {
                PrepareContext(Path.Combine(Repo.SamplesDir, folder), Repo.PackagesDir, context);
                await Docker.Build(tag, dockerfile, context);
            }
            finally
            {
                if (Directory.Exists(context))
                    SampleWorkspace.DeleteDirectory(context);
            }
        });
    }

    internal static void PrepareContext(string sampleDir, string packagesDir, string context)
    {
        SampleWorkspace.CopyTree(sampleDir, context);
        var packages = Path.Combine(context, "packages");
        Directory.CreateDirectory(packages);
        foreach (var package in Directory.EnumerateFiles(packagesDir, "*.nupkg", SearchOption.AllDirectories))
            File.Copy(package, Path.Combine(packages, Path.GetFileName(package)));
        Assert.NotEmpty(Directory.EnumerateFiles(packages, "*.nupkg"));
        DotNet.WriteNuGetConfig(Path.Combine(context, "nuget.config"), "packages");
    }

    private protected async Task RunSample(string folder, IEnumerable<string> arguments, Func<DockerRunningApp, Task> test)
    {
        folder = folder.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
        var sample = SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Single(sample => sample.Folder == folder && sample.Kind.HasFlag(SampleLookup.EntryKind.Docker));
        var tag = await BuildSample(folder, sample.FileName);
        await docker.Run(tag, arguments, test);
    }
}
