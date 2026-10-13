namespace SkiaSharp.Tests.Samples.Utils;

public abstract class BasicSampleTestBase : SampleTestBase
{
    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Where(sample => sample.Kind.HasFlag(SampleLookup.EntryKind.Basic) && !sample.Kind.HasFlag(SampleLookup.EntryKind.Docker))
            .Select(sample => new object[] { sample.Folder, sample.FileName, "Release" });

    private protected async Task RunSample(string name, IEnumerable<string> arguments, Func<DotNetRunningApp, string, Task> test)
    {
        var folder = Path.Combine("Basic", name);
        var sample = Cases().Single(sample => (string)sample[0] == folder);
        var configuration = (string)sample[2];
        var options = new DotNetOptions { Configuration = configuration };
        var workspace = await BuildSample(folder, (string)sample[1], options);
        var project = Path.Combine(workspace.Root, "samples", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");
        await DotNet.Run(workspace, project, arguments, app => test(app, project), options);
    }
}
