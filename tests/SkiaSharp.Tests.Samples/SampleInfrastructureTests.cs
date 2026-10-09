using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Exercises discovery, isolation, cleanup, and diagnostics without building real samples.
[Trait("Category", "Infrastructure")]
public class SampleInfrastructureTests : IDisposable
{
    private readonly string root = Path.Combine(Repo.ArtifactsDir, $"infrastructure-{Guid.NewGuid():N}");

    private void FileIn(string relative, string text = "<Project />")
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    [Theory]
    [InlineData("Windows", "Gallery.Windows.slnx", "windows.Dockerfile")]
    [InlineData("Mac", "Gallery.Mac.slnx", "linux.Dockerfile")]
    [InlineData("Linux", "Gallery.Linux.slnx", "linux.Dockerfile")]
    public void SelectsHostSolutionsAndDockerfiles(string platform, string solution, string dockerfile)
    {
        FileIn("Gallery/Gallery.slnx");
        foreach (var host in new[] { "Windows", "Mac", "Linux" })
            FileIn($"Gallery/Gallery.{host}.slnx");
        FileIn("Basic/Console/Console.slnx");
        FileIn("Basic/Docker/App.slnx");
        FileIn("Basic/Docker/linux.Dockerfile");
        FileIn("Basic/Docker/windows.Dockerfile");
        FileIn("Basic/Console/obj/Nested.slnx");
        var entries = SampleLookup.Discover(root, platform);
        Assert.Equal(4, entries.Count);
        Assert.Contains(new SampleLookup.Entry("Gallery", solution, SampleLookup.EntryKind.Gallery), entries);
        Assert.Contains(new SampleLookup.Entry(Path.Combine("Basic", "Console"), "Console.slnx",
            SampleLookup.EntryKind.Basic | SampleLookup.EntryKind.Host), entries);
        Assert.Contains(new SampleLookup.Entry(Path.Combine("Basic", "Docker"), "App.slnx", SampleLookup.EntryKind.Basic), entries);
        Assert.Contains(new SampleLookup.Entry(Path.Combine("Basic", "Docker"), dockerfile,
            SampleLookup.EntryKind.Basic | SampleLookup.EntryKind.Docker), entries);
    }

    [Fact]
    public void FiltersAndMissingInputsFailExplicitly()
    {
        FileIn("Console/Console.slnx");
        FileIn("Console/Console.Core.slnx");
        Assert.Equal(2, SampleLookup.Discover(root, "Mac", "Console").Count);
        Assert.Throws<InvalidOperationException>(() => SampleLookup.Discover(root, "Mac", "Missing"));
        Assert.Throws<DirectoryNotFoundException>(() => SampleLookup.Discover(Path.Combine(root, "Missing"), "Mac"));
    }

    [Fact]
    public void ScopedCopyKeepsAncestorsAndGallerySiblings()
    {
        FileIn("inputs/Directory.Build.props");
        FileIn("inputs/NuGet.Config");
        FileIn("inputs/Basic/Directory.Build.targets");
        FileIn("inputs/Basic/Console/App.csproj");
        FileIn("inputs/Basic/Console/obj/assets.json");
        FileIn("inputs/Basic/Web/App.csproj");
        FileIn("inputs/Gallery/App/App.csproj");
        FileIn("inputs/Gallery/Shared/Shared.csproj");
        var source = Path.Combine(root, "inputs");
        var copy = Path.Combine(root, "console-copy");
        SampleWorkspace.CopySample(source, Path.Combine("Basic", "Console"), copy);
        Assert.True(File.Exists(Path.Combine(copy, "Basic", "Console", "App.csproj")));
        Assert.True(File.Exists(Path.Combine(copy, "Directory.Build.props")));
        Assert.True(File.Exists(Path.Combine(copy, "NuGet.Config")));
        Assert.True(File.Exists(Path.Combine(copy, "Basic", "Directory.Build.targets")));
        Assert.False(Directory.Exists(Path.Combine(copy, "Basic", "Console", "obj")));
        Assert.False(Directory.Exists(Path.Combine(copy, "Basic", "Web")));
        Assert.False(Directory.Exists(Path.Combine(copy, "Gallery")));
        copy = Path.Combine(root, "gallery-copy");
        SampleWorkspace.CopySample(source, "Gallery", copy);
        Assert.True(File.Exists(Path.Combine(copy, "Gallery", "Shared", "Shared.csproj")));
        Assert.False(Directory.Exists(Path.Combine(copy, "Basic")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("../outside")]
    public void RejectsEscapingFolders(string folder) =>
        Assert.Throws<ArgumentException>(() => SampleWorkspace.CopySample(root, folder, Path.Combine(root, "copy")));

    [Fact]
    public void RejectsSampleLocalSdkPins()
    {
        FileIn("inputs/Basic/App/global.json", "{}");
        Assert.Throws<InvalidOperationException>(() =>
            SampleWorkspace.CopySample(Path.Combine(root, "inputs"), "Basic", Path.Combine(root, "copy")));
    }

    [Fact]
    public void FailedPreparationCleansOnlyItsOwnedWorkspace()
    {
        FileIn("inputs/App/global.json", "{}");
        FileIn("packages/input.nupkg", "");
        FileIn("workspaces/other-test/keep.txt", "Keep this workspace.");
        Assert.Throws<InvalidOperationException>(() => new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Path.Combine(root, "packages"),
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics")));
        Assert.Equal(Path.Combine(root, "workspaces", "other-test"),
            Assert.Single(Directory.EnumerateDirectories(Path.Combine(root, "workspaces"))));
        Assert.True(File.Exists(Path.Combine(root, "workspaces", "other-test", "keep.txt")));
        Assert.True(File.Exists(Path.Combine(root, "inputs", "App", "global.json")));
    }

    [Fact]
    public async Task ConsumerUsesTheHostSdkWithoutRepositoryPins()
    {
        FileIn("repository/global.json", """
            {"sdk":{"version":"1.2.345","paths":[".dotnet","$host$"],"workloadVersion":"1.2.345"},
             "tools":{"dotnet":"1.2.345"},"msbuild-sdks":{"Example.Sdk":"2.3.456"}}
            """);
        FileIn("repository/output/nugets/input.nupkg");
        FileIn("repository/output/samples/Basic/Console/App.slnx");
        var repository = Path.Combine(root, "repository");
        using var workspace = new SampleWorkspace(
            Path.Combine("Basic", "Console"),
            Path.Combine(repository, "output", "samples"),
            Path.Combine(repository, "output", "nugets"),
            Path.Combine(repository, "output", "samples-test-workspaces"),
            Path.Combine(root, "diagnostics"));
        var working = workspace.Root;
        Assert.StartsWith("basic-console-", Path.GetFileName(working));
        Assert.Equal(Path.GetFileName(working), Path.GetFileName(workspace.DiagnosticsRoot));
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(working, "global.json")));
        Assert.Single(json.RootElement.EnumerateObject());
        var sdk = json.RootElement.GetProperty("sdk");
        Assert.False(sdk.TryGetProperty("version", out _));
        Assert.False(sdk.TryGetProperty("workloadVersion", out _));
        Assert.Equal("$host$", sdk.GetProperty("paths")[0].GetString());
        Assert.True(sdk.GetProperty("allowPrerelease").GetBoolean());
        foreach (var file in new[] { "Directory.Build.props", "Directory.Build.targets" })
            Assert.Empty(XDocument.Load(Path.Combine(working, file)).Root!.Elements());
        var start = new ProcessStartInfo();
        start.Environment["MSBUILD_EXAMPLE"] = "pollution";
        var path = start.Environment["PATH"];
        DotNet.ConfigureProcess(start, workspace);
        Assert.False(start.Environment.ContainsKey("MSBUILD_EXAMPLE"));
        Assert.Equal(path, start.Environment["PATH"]);
        foreach (var key in new[] { "NUGET_PACKAGES", "NUGET_HTTP_CACHE_PATH", "NUGET_SCRATCH", "DOTNET_CLI_HOME" })
            Assert.StartsWith(working + Path.DirectorySeparatorChar, start.Environment[key]);
        var selected = await DotNet.GetVersion(workspace);
        Assert.NotEqual("1.2.345", selected);
        Assert.Matches(@"^\d+\.\d+\.\d+", selected);
        Directory.CreateDirectory(workspace.DiagnosticsRoot);
        workspace.Dispose();
        Assert.False(Directory.Exists(working));
        Assert.True(Directory.Exists(workspace.DiagnosticsRoot));
    }

    [Fact]
    public void ArtifactPackagesCannotFallBackToPublicFeeds()
    {
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "NuGet.Config");
        DotNet.WriteNuGetConfig(file, "packages");
        var mappings = XDocument.Load(file).Root!.Element("packageSourceMapping")!;
        var artifact = Assert.Single(mappings.Elements("packageSource"), element => (string?)element.Attribute("key") == "artifacts");
        Assert.Equal(new[] { "SkiaSharp*", "HarfBuzzSharp*" },
            artifact.Elements("package").Select(element => (string?)element.Attribute("pattern")));
    }

    [Fact]
    public void HttpRequestsUseTheOwnedOriginAndPreserveQueries()
    {
        Assert.Equal("http://127.0.0.1:49152/api/images/SkiaSharp?x=1",
            RunningApp.RequestUri(new Uri("http://127.0.0.1:49152"), "/api/images/SkiaSharp?x=1").AbsoluteUri);
    }

    [Theory]
    [InlineData("Now listening on: http://127.0.0.1:54321", "http://127.0.0.1:54321/")]
    [InlineData("  Now listening on: http://127.0.0.1:54321  ", "http://127.0.0.1:54321/")]
    [InlineData("App url: http://127.0.0.1:60542/?arg=--urls&arg=http%3a%2f%2f127.0.0.1%3a0",
        "http://127.0.0.1:60542/?arg=--urls&arg=http%3a%2f%2f127.0.0.1%3a0")]
    public void DotNetRecognizesOwnedHttpAddress(string output, string expected) =>
        Assert.Equal(new Uri(expected), DotNetRunningApp.ParseAddress(output));

    [Theory]
    [InlineData("App url: https://127.0.0.1:60545/")]
    [InlineData("App url: http://example.com:54321/")]
    [InlineData("App url: http://127.0.0.1:0/")]
    [InlineData("App url: not-an-address")]
    [InlineData("Debug at url: http://127.0.0.1:60542/_framework/debug")]
    public void DotNetRejectsOtherAddresses(string output) =>
        Assert.Null(DotNetRunningApp.ParseAddress(output));

    [Theory]
    [InlineData("https://localhost/health")]
    [InlineData("//example.com/health")]
    [InlineData("/\\example.com/health")]
    [InlineData("/health#fragment")]
    [InlineData("")]
    public void HttpRequestsCannotEscapeTheOwnedOrigin(string path)
    {
        Assert.Throws<ArgumentException>(() => RunningApp.RequestUri(new Uri("http://127.0.0.1:49152"), path));
    }

    [Fact]
    public void SmallAttachmentsArePublishedAndLargeFilesRemainOnDisk()
    {
        FileIn("small.txt", "Sample diagnostic attachment.");
        Assert.NotNull(TestContext.Current.TestOutputHelper);
        TestContext.Current.AddFileAttachment(Path.Combine(root, "small.txt"), "text/plain");
        using (var file = File.Create(Path.Combine(root, "large.binlog")))
            file.SetLength(SampleArtifacts.AttachmentLimit + 1);
        TestContext.Current.AddFileAttachment(Path.Combine(root, "large.binlog"), "application/octet-stream");
        Assert.True(File.Exists(Path.Combine(root, "large.binlog")));
    }

    [Fact]
    public void PngMustDecodeAndHaveTheExpectedSize()
    {
        using var bitmap = new SKBitmap(800, 600);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        SampleImage.Validate(data.ToArray());
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(data.ToArray().AsSpan(0, 12)));
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(data.ToArray(), 400, 300));
    }

    [Theory]
    [InlineData(80, 50, 0, true)]
    [InlineData(80, 50, 3, true)]
    [InlineData(80, 50, 4, false)]
    [InlineData(80, 51, 3, true)]
    [InlineData(80, 51, 4, false)]
    [InlineData(800, 600, 259, true)]
    [InlineData(800, 600, 360, true)]
    [InlineData(800, 600, 361, false)]
    [InlineData(512, 512, 177, true)]
    [InlineData(512, 512, 196, true)]
    [InlineData(512, 512, 197, false)]
    public void GoldenComparisonAllowsAtMostPointZeroSevenFivePercentOfPixels(
        int width, int height, int changedPixels, bool accepted)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "expected.png");
        for (var i = 0; i < changedPixels; i++)
            bitmap.SetPixel(i % width, i / width, SKColors.White);
        var actual = SavePng(bitmap, "actual.png");

        if (accepted)
        {
            SampleImage.CompareGolden(actual, expected);
            Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        }
        else
        {
            var error = Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.CompareGolden(actual, expected));
            Assert.Contains($"{changedPixels}/{width * height} pixels", error.Message);
            Assert.Contains("exceeding", error.Message);
            Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        }
    }

    [Theory]
    [InlineData(1, 0, 0, 255)]
    [InlineData(0, 0, 0, 254)]
    public void GoldenComparisonCountsEvenSingleChannelAndAlphaChanges(byte red, byte green, byte blue, byte alpha)
    {
        using var bitmap = new SKBitmap(80, 50);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "expected.png");
        bitmap.Erase(new SKColor(red, green, blue, alpha));
        var actual = SavePng(bitmap, "actual.png");

        var error = Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.CompareGolden(actual, expected));
        Assert.Contains("4000/4000 pixels", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    private string SavePng(SKBitmap bitmap, string name)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, name);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
        return path;
    }

    [Fact]
    public void GoldenComparisonSupportsPageSizesAndRetainsPixelDifferences()
    {
        Directory.CreateDirectory(root);
        var actual = Path.Combine(root, "page.actual.png");
        var expected = Path.Combine(root, "page.expected.png");
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            File.WriteAllBytes(actual, data.ToArray());
        File.Copy(actual, expected);
        SampleImage.ValidateFile(actual, expected);

        bitmap.Erase(SKColors.Blue);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            File.WriteAllBytes(actual, data.ToArray());
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.CompareGolden(actual, expected));
        Assert.Contains("192/192", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void MissingReviewedGoldenFailsWithoutCreatingAReference()
    {
        Directory.CreateDirectory(root);
        var actual = Path.Combine(root, "missing-reference.actual.png");
        var expected = Path.Combine(root, "missing-reference.png");
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(actual, data.ToArray());
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.ValidateFile(actual, expected));
        Assert.Contains("Missing reviewed sample golden", error.Message);
        Assert.True(File.Exists(actual));
        Assert.False(File.Exists(expected));
    }

    [Fact]
    public void AverageColorErrorAllowsExactlyFivePercentButRejectsOneChannelStepOver()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "average.expected.png");
        bitmap.Erase(new SKColor(238, 17, 17));
        var actual = SavePng(bitmap, "average.actual.png");
        SampleImage.CompareGolden(actual, expected, 0.05);
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));

        bitmap.SetPixel(0, 0, new SKColor(238, 18, 17));
        SavePng(bitmap, "average.actual.png");
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.CompareGolden(actual, expected, 0.05));
        Assert.Contains("average RGBA color error", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void AverageColorErrorDoesNotChangeTheHeadlessPixelBudgetOrExplicitExactComparison()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "channel.expected.png");
        bitmap.Erase(new SKColor(247, 8, 8));
        var actual = SavePng(bitmap, "channel.actual.png");
        SampleImage.CompareGolden(actual, expected, 0.05);
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.CompareGolden(actual, expected));
        Assert.Contains("192/192", error.Message);
        Assert.ThrowsAny<Exception>(() => SampleImage.CompareGolden(actual, expected, 0));
    }

    [Fact]
    public void AverageColorErrorIncludesAlphaInItsFivePercentBoundary()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "alpha.expected.png");
        bitmap.Erase(new SKColor(0, 0, 0, 204));
        var actual = SavePng(bitmap, "alpha.actual.png");
        SampleImage.CompareGolden(actual, expected, 0.05);

        bitmap.SetPixel(0, 0, new SKColor(0, 0, 0, 203));
        SavePng(bitmap, "alpha.actual.png");
        Assert.ThrowsAny<Exception>(() => SampleImage.CompareGolden(actual, expected, 0.05));
    }

    [Fact]
    public void AverageColorErrorAllowsExactlyTheGpuSixPercentBudget()
    {
        using var bitmap = new SKBitmap(5, 1);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "gpu-budget.expected.png");
        bitmap.Erase(new SKColor(234, 20, 20));
        bitmap.SetPixel(0, 0, new SKColor(233, 20, 20));
        var actual = SavePng(bitmap, "gpu-budget.actual.png");
        SampleImage.CompareGolden(actual, expected, 0.06);

        bitmap.SetPixel(1, 0, new SKColor(233, 20, 20));
        SavePng(bitmap, "gpu-budget.actual.png");
        Assert.ThrowsAny<Exception>(() => SampleImage.CompareGolden(actual, expected, 0.06));
    }

    [Fact]
    public void SampleTestBaseOwnsOnlyItsPreparedWorkspace()
    {
        FileIn("inputs/Basic/Console/App.slnx");
        string working;
        using (var test = new LifetimeTest(Path.Combine(root, "inputs")))
        {
            working = test.Prepare();
            Assert.Equal(working, test.Prepare());
            Assert.True(File.Exists(Path.Combine(working, "samples", "Basic", "Console", "App.slnx")));
        }
        Assert.False(Directory.Exists(working));
        Assert.True(Directory.Exists(Path.Combine(root, "inputs")));
    }

    private sealed class LifetimeTest(string samplesDir) : SampleTestBase(samplesDir)
    {
        internal string Prepare() => PrepareSample(Path.Combine("Basic", "Console")).Root;
        internal string Diagnostics => PrepareSample(Path.Combine("Basic", "Console")).DiagnosticsRoot;
        internal Task Build() => BuildSample(Path.Combine("Basic", "Console"), "App.csproj", "Release");
    }

    [Fact]
    public async Task SampleTestBaseBuildsWithRetainedDiagnostics()
    {
        FileIn("inputs/Basic/Console/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);
        using var test = new LifetimeTest(Path.Combine(root, "inputs"));
        await test.Build();
        var working = test.Prepare();
        var diagnostics = Path.Combine(test.Diagnostics, "builds", "Basic", "Console", "App", "Release");
        test.Dispose();
        Assert.False(Directory.Exists(working));
        Assert.Contains("Build succeeded.", File.ReadAllText(Path.Combine(diagnostics, "build.stdout.log")));
        Assert.DoesNotContain("Build succeeded.", File.ReadAllText(Path.Combine(diagnostics, "build.stderr.log")));
        Assert.True(new FileInfo(Path.Combine(diagnostics, "build.binlog")).Length > 0);
    }

    [Fact]
    public async Task DotNetFailedBuildKeepsLogsInTheProvidedDirectory()
    {
        FileIn("inputs/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
              <Target Name="FailBuild" BeforeTargets="CoreCompile">
                <Error Text="Expected sample build failure" />
              </Target>
            </Project>
            """);
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var diagnostics = Path.Combine(root, "provided-build-logs");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DotNet.Build(workspace, Path.Combine(workspace.Root, "samples", "App", "App.csproj"), diagnostics));
        Assert.Contains(diagnostics, error.Message);
        Assert.Contains("Expected sample build failure", File.ReadAllText(Path.Combine(diagnostics, "build.stdout.log")));
        Assert.DoesNotContain("Expected sample build failure", File.ReadAllText(Path.Combine(diagnostics, "build.stderr.log")));
        Assert.DoesNotContain(error.ToString(), File.ReadAllText(Path.Combine(diagnostics, "build.stdout.log")));
        Assert.Contains(error.Message, File.ReadAllText(Path.Combine(diagnostics, "build.failure.log")));
        Assert.True(new FileInfo(Path.Combine(diagnostics, "build.binlog")).Length > 0);
    }

    [Fact]
    public async Task CleanupWaitsForOwnedWindowsFileHandles()
    {
        FileIn("inputs/App/App.csproj");
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        using var file = new FileStream(Path.Combine(workspace.Root, "owned.dll"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        var release = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, TestContext.Current.CancellationToken);
            }
            finally
            {
                file.Dispose();
            }
        }, TestContext.Current.CancellationToken);
        try
        {
            workspace.Dispose();
            Assert.False(Directory.Exists(workspace.Root));
        }
        finally
        {
            await release;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
            SampleWorkspace.DeleteDirectory(root);
    }
}
