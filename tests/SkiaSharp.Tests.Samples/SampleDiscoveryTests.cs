using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Checks the actual generated case sets independently of builds and Docker availability.
[Trait("Category", "Infrastructure")]
public class SampleDiscoveryTests
{
    private static readonly string[] CommonSamples =
        ["BlazorWebAssembly", "BrowserWebAssembly", "Console", "DockerConsole", "DockerWebApi", "Gtk3", "Gtk4", "Tizen", "Web"];

    private static readonly Dictionary<string, string[]> HostSamples = new()
    {
        ["Windows"] = ["Android", "MacCatalyst", "Maui", "macOS", "tvOS", "WindowsForms", "WinUI", "WPF"],
        ["Mac"] = ["Android", "iOS", "MacCatalyst", "Maui", "macOS", "tvOS", "UnoPlatform"],
        ["Linux"] = ["UnoPlatform"]
    };

    private static readonly Dictionary<string, string> Dockerfiles = new()
    {
        ["Windows"] = "windows.Dockerfile",
        ["Mac"] = "linux.Dockerfile",
        ["Linux"] = "linux.Dockerfile"
    };

    [Theory]
    [InlineData(nameof(DockerSampleTests.DockerImageBuilds))]
    [InlineData(nameof(DockerSampleTests.ConsoleSampleRuns))]
    [InlineData(nameof(DockerSampleTests.WebApiSampleReturnsImage))]
    public void DockerTestsUseAvailabilityRatherThanPlatformExclusions(string method)
    {
        var test = typeof(DockerSampleTests).GetMethod(method);
        Assert.NotNull(test);
        var attribute = Assert.IsAssignableFrom<FactAttribute>(
            Assert.Single(test.GetCustomAttributes(typeof(FactAttribute), inherit: true)));
        Assert.Null(attribute.SkipType);
        Assert.Null(attribute.SkipWhen);
        Assert.Null(attribute.SkipUnless);
        Assert.Null(attribute.Skip);
    }

    [Fact]
    public void DockerCasesCoverBothSamples()
    {
        var cases = DockerSampleTests.Cases().ToArray();
        Assert.All(cases, row => Assert.Equal(2, row.Length));
        Assert.Equal(
            new[] { Path.Combine("Basic", "DockerConsole"), Path.Combine("Basic", "DockerWebApi") },
            cases.Select(row => Assert.IsType<string>(row[0])).Order(StringComparer.Ordinal));
        Assert.All(cases, row =>
        {
            Assert.Equal(Dockerfiles[SampleLookup.HostPlatform], Assert.IsType<string>(row[1]));
            Assert.True(File.Exists(Path.Combine(Repo.SamplesDir,
                Assert.IsType<string>(row[0]), Assert.IsType<string>(row[1]))));
        });
    }

    [Fact]
    public void SampleKindsDescribeHostDockerHttpAndDeviceCapabilities()
    {
        var entries = SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform);
        Assert.Equal(SampleLookup.EntryKind.Basic | SampleLookup.EntryKind.Host,
            Assert.Single(entries, entry => entry.Folder == Path.Combine("Basic", "Console")).Kind);
        Assert.Equal(SampleLookup.EntryKind.Basic | SampleLookup.EntryKind.Host | SampleLookup.EntryKind.Http,
            Assert.Single(entries, entry => entry.Folder == Path.Combine("Basic", "Web")).Kind);
        Assert.Equal(SampleLookup.EntryKind.Basic | SampleLookup.EntryKind.Docker | SampleLookup.EntryKind.Http,
            Assert.Single(entries, entry => entry.Folder == Path.Combine("Basic", "DockerWebApi") &&
                entry.Kind.HasFlag(SampleLookup.EntryKind.Docker)).Kind);
        Assert.All(entries.Where(entry => entry.Folder == Path.Combine("Basic", "Android") ||
            entry.Folder == Path.Combine("Basic", "iOS")), entry =>
                Assert.True(entry.Kind.HasFlag(SampleLookup.EntryKind.Device)));
    }

    [Fact]
    public void SampleCasesCoverEligibleBasicSamples()
    {
        var cases = BasicSampleTests.Cases().ToArray();
        Assert.All(cases, row => Assert.Equal(3, row.Length));
        var expected = CommonSamples.Concat(HostSamples[SampleLookup.HostPlatform])
            .Select(name => Path.Combine("Basic", name)).Order(StringComparer.Ordinal);
        Assert.Equal(expected,
            cases.Select(row => Assert.IsType<string>(row[0])).Order(StringComparer.Ordinal));
        Assert.All(cases, row =>
        {
            Assert.Equal("Release", Assert.IsType<string>(row[2]));
            Assert.True(File.Exists(Path.Combine(Repo.SamplesDir,
                Assert.IsType<string>(row[0]), Assert.IsType<string>(row[1]))));
        });
    }

    [Fact]
    public void GalleryCasesSelectTheHostSolution()
    {
        var row = Assert.Single(GallerySampleTests.Cases());
        Assert.Equal(3, row.Length);
        Assert.Equal("Gallery", Assert.IsType<string>(row[0]));
        Assert.Equal($"SkiaSharpSample.{SampleLookup.HostPlatform}.slnx", Assert.IsType<string>(row[1]));
        Assert.Equal("Release", Assert.IsType<string>(row[2]));
        Assert.True(File.Exists(Path.Combine(Repo.SamplesDir,
            Assert.IsType<string>(row[0]), Assert.IsType<string>(row[1]))));
    }
}
