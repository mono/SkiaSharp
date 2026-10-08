using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

public abstract class SampleTestBase(ITestOutputHelper output) : IDisposable
{
    private SampleWorkspace? workspace;
    private string? preparedFolder;
    private DotNet? generatedWorkspace;

    protected ITestOutputHelper Output { get; } = output;
    protected virtual string? SampleFolder => null;

    protected DotNet PrepareSample(string? folder = null)
    {
        folder ??= SampleFolder;
        if (string.IsNullOrWhiteSpace(folder))
            throw new InvalidOperationException("The test must declare its sample folder.");
        if (generatedWorkspace is not null)
            throw new InvalidOperationException("A generated-app test cannot also copy a sample.");
        if (workspace is not null && preparedFolder != folder)
            throw new InvalidOperationException("A test case may only prepare one sample workspace.");
        preparedFolder = folder;
        workspace ??= new SampleWorkspace(folder);
        return workspace.Profile();
    }

    protected DotNet PrepareGeneratedProject()
    {
        if (workspace is not null)
            throw new InvalidOperationException("A sample test cannot also prepare a generated application.");
        return generatedWorkspace ??= DotNet.ForSamples();
    }

    public void Dispose()
    {
        try
        {
            workspace?.Dispose();
        }
        finally
        {
            generatedWorkspace?.Dispose();
        }
    }
}
