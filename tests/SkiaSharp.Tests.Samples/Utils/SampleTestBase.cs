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

    protected Task<SampleWorkspace> BuildSample(string folder, string project, DotNetBuildOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return BuildSampleImplementation(folder, project, options.Configuration, options.Framework, options.Properties);
    }

    protected Task<SampleWorkspace> BuildSample(string folder, string solution, string configuration) =>
        BuildSampleImplementation(folder, solution, configuration, null);

    protected Task<SampleWorkspace> BuildSample(string folder, string solution, string configuration, string framework)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(framework);
        return BuildSampleImplementation(folder, solution, configuration, framework);
    }

    protected Task<SampleWorkspace> BuildSample(
        string folder, string solution, string configuration, string framework,
        IReadOnlyDictionary<string, string> properties)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(framework);
        ArgumentNullException.ThrowIfNull(properties);
        return BuildSampleImplementation(folder, solution, configuration, framework, properties);
    }

    private async Task<SampleWorkspace> BuildSampleImplementation(
        string folder, string solution, string configuration, string? framework,
        IReadOnlyDictionary<string, string>? properties = null)
    {
        var prepared = PrepareSample(folder);

        var project = Path.Combine(prepared.Root, "samples", folder, solution);
        if (!File.Exists(project))
            throw new FileNotFoundException("Selected sample solution is missing", project);

        var diagnostics = Path.Combine(prepared.DiagnosticsRoot, "builds", folder, Path.GetFileNameWithoutExtension(solution), configuration);
        if (framework is not null)
            diagnostics = Path.Combine(diagnostics, framework);

        await DotNet.GetVersion(prepared);

        if (framework is null)
            await DotNet.Build(prepared, project, diagnostics, configuration);
        else if (properties is not null)
            await DotNet.Build(prepared, project, diagnostics, configuration, framework, properties);
        else
            await DotNet.Build(prepared, project, diagnostics, configuration, framework);
        return prepared;
    }

    public void Dispose() =>
        workspace?.Dispose();
}
