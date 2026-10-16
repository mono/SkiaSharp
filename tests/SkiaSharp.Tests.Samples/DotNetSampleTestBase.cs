using SkiaSharp.Tests.Samples.Utils;

namespace SkiaSharp.Tests.Samples;

// Prepares and builds each sample in one owned workspace.
public abstract class DotNetSampleTestBase : IDisposable
{
    private SampleWorkspace? workspace;
    private string? preparedFolder;
    private readonly string samplesDir;

    protected DotNetSampleTestBase()
        : this(Repo.SamplesDir)
    {
    }

    protected DotNetSampleTestBase(string samplesDir)
    {
        this.samplesDir = samplesDir;
    }

    protected SampleWorkspace PrepareSample(string folder)
    {
        if (workspace is not null && preparedFolder != folder)
            throw new InvalidOperationException("A test case may only prepare one sample workspace.");

        preparedFolder = folder;
        workspace ??= new SampleWorkspace(folder, samplesDir, Repo.PackagesDir, Repo.WorkspacesDir, Repo.ArtifactsDir);
        return workspace;
    }

    protected async Task<SampleWorkspace> BuildSample(string folder, string solution, DotNetOptions? options = null)
    {
        options ??= new();
        options.Validate();
        var prepared = PrepareSample(folder);

        var project = Path.Combine(prepared.Root, "samples", folder, solution);
        if (!File.Exists(project))
            throw new FileNotFoundException("Selected sample solution is missing", project);

        var diagnostics = Path.Combine(prepared.DiagnosticsRoot, "builds", folder, Path.GetFileNameWithoutExtension(solution), options.Configuration);
        if (options.Framework is not null)
            diagnostics = Path.Combine(diagnostics, options.Framework);

        await DotNet.GetVersion(prepared);

        await DotNet.Build(prepared, project, diagnostics, options);
        return prepared;
    }

    private protected async Task RunSample(string folder, IEnumerable<string> arguments, Func<DotNetRunningApp, string, Task> test)
    {
        folder = folder.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
        var sample = SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Single(sample => sample.Folder == folder && !sample.Kind.HasFlag(SampleLookup.EntryKind.Docker));
        var options = new DotNetOptions();
        var workspace = await BuildSample(folder, sample.FileName, options);
        var project = Path.Combine(workspace.Root, "samples", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");
        await DotNet.Run(workspace, project, arguments, app => test(app, project), options);
    }

    private protected Task RunBrowserSample(string folder, IEnumerable<string> arguments, Func<BrowserRunningApp, Task> test) =>
        RunSample(folder, arguments, (app, _) => BrowserRunningApp.Run(app, test));

    public void Dispose() =>
        workspace?.Dispose();
}
