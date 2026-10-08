namespace SkiaSharp.Tests.Samples.Utils;

// Prepares and builds each sample in one owned workspace.
public abstract class SampleTestBase : IDisposable
{
    private SampleWorkspace? workspace;
    private string? preparedFolder;
    private readonly string samplesDir;

    protected SampleTestBase()
        : this(Repo.SamplesDir)
    {
    }

    protected SampleTestBase(string samplesDir)
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

    protected async Task BuildSample(string folder, string solution, string configuration)
    {
        var prepared = PrepareSample(folder);
        var project = Path.Combine(prepared.Root, "samples", folder, solution);
        if (!File.Exists(project))
            throw new FileNotFoundException("Selected sample solution is missing", project);

        var diagnostics = Path.Combine(prepared.DiagnosticsRoot, "builds", folder,
            Path.GetFileNameWithoutExtension(solution), configuration);
        await DotNet.GetVersion(prepared);
        await DotNet.Build(prepared, project, diagnostics, configuration);
    }

    public void Dispose() =>
        workspace?.Dispose();
}
