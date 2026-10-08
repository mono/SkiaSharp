namespace SkiaSharp.Tests.Samples.Utils;

// Gives each test case one lazily prepared sample workspace.
public abstract class SampleTestBase : IDisposable
{
    private SampleWorkspace? workspace;
    private string? preparedFolder;
    private readonly string samplesDir;

    protected SampleTestBase() : this(Repo.SamplesDir)
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

    public void Dispose() => workspace?.Dispose();
}
