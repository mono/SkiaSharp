using Xunit;

namespace SkiaSharp.Native.Tests;

public class NativeTests
{
    private static string CasesDirectory => Path.Combine(AppContext.BaseDirectory, "Cases");

    public static IEnumerable<object[]> TestFolders()
    {
        var directories = Directory.GetDirectories(CasesDirectory);
        if (directories.Length == 0)
            throw new InvalidOperationException($"No native test folders found in {CasesDirectory}.");

        foreach (var directory in directories.OrderBy(Path.GetFileName, StringComparer.Ordinal))
            yield return [Path.GetFileName(directory)];
    }

    [Theory]
    [MemberData(nameof(TestFolders))]
    public async Task Run(string name)
    {
        var directory = Path.Combine(CasesDirectory, name);
        var script = Path.Combine(directory, "test.sh");
        Assert.True(File.Exists(script), $"Missing native test entry point: {script}");

        var repository = new DirectoryInfo(AppContext.BaseDirectory);
        while (repository != null && !File.Exists(Path.Combine(repository.FullName, "build.cake")))
            repository = repository.Parent;
        Assert.NotNull(repository);

        await NativeProcess.RunAsync("bash",
            [script, Path.Combine(repository.FullName, "output", "native")], directory);
    }
}
