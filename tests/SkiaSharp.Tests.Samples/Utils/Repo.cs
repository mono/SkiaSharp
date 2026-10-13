using Xunit;
using System.Text.Json;

namespace SkiaSharp.Tests.Samples.Utils;

// Conventional repository paths shared by the sample tests.
internal static class Repo
{
    public static string RootDir =>
        AppContext.GetData("SampleTest.RepositoryDirectory") as string is { Length: > 0 } root
            ? Path.GetFullPath(root)
            : throw new InvalidOperationException("SampleTest.RepositoryDirectory runtime configuration is missing");

    public static string PackagesDir => Path.Combine(RootDir, "output", "nugets");
    public static string ArtifactsDir => Path.Combine(RootDir, "output", "logs", "testlogs", "samples");
    public static string WorkspacesDir => Path.Combine(RootDir, "output", "samples-test-workspaces");
    public static string SamplesDir => Path.Combine(RootDir, "output", GetVersion().Contains('-') ? "samples-preview" : "samples");
    internal static string DefaultSdkVersion
    {
        get
        {
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(RootDir, "global.json")));
            return json.RootElement.GetProperty("sdk").GetProperty("version").GetString()
                ?? throw new InvalidOperationException("Repository SDK version is missing.");
        }
    }

    private static string GetVersion()
    {
        const string packageId = "SkiaSharp";
        var file = Assert.Single(
            Directory.EnumerateFiles(PackagesDir, packageId + ".*.nupkg"),
            path => char.IsDigit(Path.GetFileName(path)[packageId.Length + 1]) &&
                !path.EndsWith(".symbols.nupkg", StringComparison.Ordinal));
        return Path.GetFileName(file)[(packageId.Length + 1)..^6];
    }
}
