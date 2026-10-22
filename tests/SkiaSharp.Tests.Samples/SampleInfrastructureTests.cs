using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.Goldens;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Exercises discovery, isolation, cleanup, and diagnostics without building real samples.
[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
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
    public void DockerContextContainsSelectedSampleAndLocalPackagesOnly()
    {
        FileIn("inputs/Basic/DockerConsole/linux.Dockerfile", "FROM scratch");
        FileIn("inputs/Basic/DockerConsole/App.csproj");
        FileIn("inputs/Basic/DockerConsole/bin/App.dll");
        FileIn("inputs/Basic/DockerConsole/obj/assets.json");
        FileIn("inputs/Basic/Web/App.csproj");
        FileIn("packages/nested/SkiaSharp.1.2.3.nupkg", "package sentinel");
        FileIn("packages/not-a-package.txt");
        var context = Path.Combine(root, "context");

        DockerSampleTestBase.PrepareContext(Path.Combine(root, "inputs", "Basic", "DockerConsole"),
            Path.Combine(root, "packages"), context);

        Assert.True(File.Exists(Path.Combine(context, "linux.Dockerfile")));
        Assert.True(File.Exists(Path.Combine(context, "App.csproj")));
        Assert.False(Directory.Exists(Path.Combine(context, "bin")));
        Assert.False(Directory.Exists(Path.Combine(context, "obj")));
        Assert.False(Directory.Exists(Path.Combine(context, "Basic")));
        Assert.Equal("package sentinel", File.ReadAllText(Path.Combine(context, "packages", "SkiaSharp.1.2.3.nupkg")));
        Assert.Single(Directory.EnumerateFiles(Path.Combine(context, "packages")));
        var config = XDocument.Load(Path.Combine(context, "nuget.config"));
        var artifacts = Assert.Single(config.Descendants("packageSources").Elements("add"),
            source => (string?)source.Attribute("key") == "artifacts");
        Assert.Equal("packages", (string?)artifacts.Attribute("value"));
        Assert.False(File.Exists(Path.Combine(context, "global.json")));
    }

    [Fact]
    public void DockerContextRejectsAnEmptyPackageCohort()
    {
        FileIn("inputs/App.csproj");
        Directory.CreateDirectory(Path.Combine(root, "packages"));
        Assert.Throws<Xunit.Sdk.NotEmptyException>(() =>
            DockerSampleTestBase.PrepareContext(Path.Combine(root, "inputs"),
                Path.Combine(root, "packages"), Path.Combine(root, "context")));
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
    public void SamplePackagesUseOnlyStagedArtifactsAndRepositoryMirrors()
    {
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "NuGet.Config");
        DotNet.WriteNuGetConfig(file, "packages");
        var config = XDocument.Load(file).Root!;
        Assert.Equal(new[] { "artifacts", "dotnet-public", "dotnet-eng" },
            config.Element("packageSources")!.Elements("add").Select(element => (string?)element.Attribute("key")));
        Assert.Equal(new[] { "artifacts", "dotnet-public", "dotnet-eng" },
            config.Element("packageSourceMapping")!.Elements("packageSource").Select(element => (string?)element.Attribute("key")));
    }

    [Fact]
    public async Task HttpRequestsUseTheOwnedOriginAndPreserveQueries()
    {
        var app = new AddressRunningApp(Path.Combine(root, "http"));
        await app.Run(async () =>
        {
            Assert.Equal("http://127.0.0.1:49152/api/images/SkiaSharp?x=1",
                (await app.RequestUri("/api/images/SkiaSharp?x=1")).AbsoluteUri);
            Assert.Equal("http://127.0.0.1:49152/health",
                (await app.RequestUri("/health")).AbsoluteUri);
            Assert.Equal(1, app.AddressRequests);
        });
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
    public async Task HttpRequestsCannotEscapeTheOwnedOrigin(string path)
    {
        var app = new AddressRunningApp(Path.Combine(root, "http"));
        await app.Run(() => Assert.ThrowsAsync<ArgumentException>(() => app.RequestUri(path)));
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
    public void RepeatedAttachmentsPreserveEachDiagnosticSnapshot()
    {
        FileIn("snapshot.txt", "First diagnostic.");
        var path = Path.Combine(root, "snapshot.txt");
        TestContext.Current.AddFileAttachment(path, "text/plain");
        File.WriteAllText(path, "Second diagnostic.");
        TestContext.Current.AddFileAttachment(path, "text/plain");
        TestContext.Current.AddFileAttachment(path, "text/plain", "snapshot.txt");
        Assert.Contains("snapshot.txt", TestContext.Current.Attachments!.Keys);
        Assert.Contains("snapshot.txt-2", TestContext.Current.Attachments.Keys);
        Assert.Contains("snapshot.txt-3", TestContext.Current.Attachments.Keys);
    }

    [Fact]
    public async Task TypedGoldenComparisonUsesPackagedBytesAndRetainsStrictPolicy()
    {
        var request = SampleReferences.Host("console");
        var reference = SampleReferences.Resolve(request);
        Assert.NotNull(reference.Path);
        Assert.StartsWith(AppContext.BaseDirectory, reference.Path);
        var actual = Path.Combine(root, "typed.actual.png");
        await SampleImage.Validate(File.ReadAllBytes(reference.Path), actual, request,
            new() { ExpectedSize = new(800, 600), MaxErrorPixelFraction = 0 });
        using var bitmap = SKBitmap.Decode(reference.Path);
        Assert.NotNull(bitmap);
        bitmap.Erase(SKColors.Red);
        actual = SavePng(bitmap, "typed-mismatch.actual.png");
        var error = Assert.Throws<Xunit.Sdk.FailException>(() =>
            SampleImage.Validate(actual, request, new() { MaxErrorPixelFraction = 0 }));
        Assert.Contains("Sample pixels differ", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void MissingTypedGoldenReportsOrderedAttemptsAndRetainsActual()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var actual = SavePng(bitmap, "typed-missing.actual.png");
        var request = new GoldenRequest("missing-profile", "12.6.2", "missing-case", "arm64", "engine");
        var resolution = SampleReferences.Resolve(request);
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, request));
        Assert.Null(resolution.Key);
        Assert.Contains(resolution.Describe(), error.Message);
        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        Assert.False(File.Exists(Path.Combine(SampleReferences.Root, resolution.Candidates[0])));
    }

    [Fact]
    public void GoldenMismatchKeepsActualAndExpectedAttachmentsDistinct()
    {
        var actual = Path.Combine(root, "actual", "canvas.png");
        var expected = Path.Combine(root, "expected", "canvas.png");
        Save(actual, SKColors.Red);
        Save(expected, SKColors.Blue);
        TestContext.Current.AddFileAttachment(actual, "image/png");
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
        Assert.Contains("Sample pixels differ", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        Assert.Contains("canvas.png-2", TestContext.Current.Attachments!.Keys);
        Assert.Contains("expected-canvas.png", TestContext.Current.Attachments.Keys);

        static void Save(string path, SKColor color)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var bitmap = new SKBitmap(4, 4);
            bitmap.Erase(color);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(path, data.ToArray());
        }
    }

    [Fact]
    public async Task ScreenshotComparisonRetriesUntilItMatches()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "retry.expected.png");
        bitmap.Erase(SKColors.Red);
        var actual = SavePng(bitmap, "retry.actual.png");
        var attempts = 0;

        var matched = await SampleImage.Validate(() =>
        {
            if (++attempts == 2)
                File.Copy(expected, actual, overwrite: true);
            return Task.FromResult(actual);
        }, expected);

        Assert.Equal(2, attempts);
        Assert.Equal(actual, matched);
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        Assert.DoesNotContain("expected-" + Path.GetFileName(expected), TestContext.Current.Attachments.Keys);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScreenshotComparisonReportsPersistentMismatch(bool wrongSize)
    {
        using var expectedBitmap = new SKBitmap(4, 4);
        expectedBitmap.Erase(SKColors.Blue);
        var expected = SavePng(expectedBitmap, "persistent.expected.png");
        using var actualBitmap = new SKBitmap(wrongSize ? 2 : 4, 4);
        actualBitmap.Erase(SKColors.Red);
        var actual = SavePng(actualBitmap, "persistent.actual.png");

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(() => Task.FromResult(actual), expected, TimeSpan.FromMilliseconds(150)));

        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        if (!wrongSize)
        {
            Assert.Contains("Sample pixels differ", error.Message);
            Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
            Assert.Contains("expected-" + Path.GetFileName(expected), TestContext.Current.Attachments.Keys);
        }
    }

    [Fact]
    public async Task ScreenshotComparisonRetriesTransientDimensions()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "dimensions.expected.png");
        using var small = new SKBitmap(2, 2);
        small.Erase(SKColors.Blue);
        var actual = SavePng(small, "dimensions.actual.png");
        var attempts = 0;
        await SampleImage.Validate(() =>
        {
            if (++attempts == 2)
                File.Copy(expected, actual, overwrite: true);
            return Task.FromResult(actual);
        }, expected, options: new() { ExpectedSize = new(4, 4) });
        Assert.Equal(2, attempts);
    }

    [Theory]
    [InlineData("missing-reference")]
    [InlineData("corrupt-reference")]
    [InlineData("corrupt-capture")]
    [InlineData("capture-error")]
    [InlineData("canceled-capture")]
    public async Task ScreenshotComparisonDoesNotRetryErrors(string failure)
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "error.expected.png");
        var actual = SavePng(bitmap, "error.actual.png");
        if (failure == "missing-reference")
            File.Delete(expected);
        if (failure == "corrupt-reference")
            File.WriteAllText(expected, "not a PNG");
        if (failure == "corrupt-capture")
            File.WriteAllText(actual, "not a PNG");
        var attempts = 0;
        var sentinel = new InvalidOperationException("Browser capture failed.");

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(() =>
            {
                attempts++;
                return failure switch
                {
                    "capture-error" => Task.FromException<string>(sentinel),
                    "canceled-capture" => Task.FromCanceled<string>(new CancellationToken(canceled: true)),
                    _ => Task.FromResult(actual)
                };
            }, expected));

        Assert.Equal(1, attempts);
        if (failure == "capture-error")
            Assert.Same(sentinel, error);
        if (failure == "canceled-capture")
            Assert.IsAssignableFrom<OperationCanceledException>(error);
        if (failure == "missing-reference")
            Assert.False(File.Exists(expected));
    }

    [Fact]
    public async Task ScreenshotComparisonBoundsCaptureTime()
    {
        var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            await Assert.ThrowsAsync<TimeoutException>(() =>
                SampleImage.Validate(() => pending.Task, "unused.png", TimeSpan.FromMilliseconds(100)));
        }
        finally
        {
            pending.TrySetCanceled(TestContext.Current.CancellationToken);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ScreenshotComparisonRejectsInvalidTimeoutBeforeCapture(int milliseconds)
    {
        var attempts = 0;
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            SampleImage.Validate(() =>
            {
                attempts++;
                return Task.FromResult("unused.png");
            }, "unused.png", TimeSpan.FromMilliseconds(milliseconds)));
        Assert.Equal(0, attempts);
    }

    [Fact]
    public async Task ImageMustDecodeAndHaveTheExpectedSize()
    {
        using var bitmap = new SKBitmap(800, 600);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var expected = SavePng(bitmap, "decode.expected.png");
        var options = new SampleImageOptions { ExpectedSize = new(800, 600) };
        await SampleImage.Validate(data.ToArray(), Path.Combine(root, "decode.actual.png"), expected, options);
        await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(data.ToArray().AsSpan(0, 12).ToArray(),
                Path.Combine(root, "invalid.actual.png"), expected, options));
        var invalidSignature = Path.Combine(root, "invalid-signature.actual.png");
        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(new byte[] { 1, 2, 3 }, invalidSignature, expected, options));
        Assert.Contains("Expected a PNG image", error.Message);
        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(invalidSignature));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(data.ToArray(), Path.Combine(root, "wrong-size.actual.png"), expected,
                options with { ExpectedSize = new(400, 300) }));
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
            SampleImage.Validate(actual, expected);
            Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        }
        else
        {
            var error = Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Validate(actual, expected));
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

        var error = Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Validate(actual, expected));
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
    public async Task ImageOptionsApplyToSavedFilesAndDirectComparisonsWithoutMutation()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "options.expected.png");
        var actual = Path.Combine(root, "options.actual.png");
        var options = new SampleImageOptions { ExpectedSize = new(16, 12) };

        await SampleImage.Validate(File.ReadAllBytes(expected), actual, expected, options);
        SampleImage.Validate(actual, expected, options);
        Assert.Contains("options.actual.png-2", TestContext.Current.Attachments!.Keys);
        Assert.Equal(new SKSizeI(16, 12), options.ExpectedSize);
        Assert.Equal(0.00075m, options.MaxErrorPixelFraction);
        Assert.Null(options.MaxAverageColorErrorFraction);

        var wrongSize = options with { ExpectedSize = new(15, 12) };
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected, wrongSize));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(File.ReadAllBytes(expected),
                Path.Combine(root, "wrong-size.actual.png"), expected, wrongSize));
        Assert.Equal(new SKSizeI(16, 12), options.ExpectedSize);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void ImageOptionsRejectInvalidAverageColorErrorLimits(double limit)
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "invalid-options.expected.png");
        var actual = SavePng(bitmap, "invalid-options.actual.png");
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected,
            new() { MaxAverageColorErrorFraction = limit }));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void ImageOptionsApplyAnExplicitPixelBudget(int changedPixels, bool accepted)
    {
        using var bitmap = new SKBitmap(10, 10);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "pixel-options.expected.png");
        for (var i = 0; i < changedPixels; i++)
            bitmap.SetPixel(i, 0, SKColors.White);
        var actual = SavePng(bitmap, "pixel-options.actual.png");
        var options = new SampleImageOptions { MaxErrorPixelFraction = 0.01m };
        if (accepted)
            SampleImage.Validate(actual, expected, options);
        else
            Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Validate(actual, expected, options));
        Assert.Equal(0.00075m, new SampleImageOptions().MaxErrorPixelFraction);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void ImageOptionsRejectInvalidPixelBudgets(int limit)
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "invalid-pixel-options.expected.png");
        var actual = SavePng(bitmap, "invalid-pixel-options.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxErrorPixelFraction = limit }));
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
        SampleImage.Validate(actual, expected);

        bitmap.Erase(SKColors.Blue);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            File.WriteAllBytes(actual, data.ToArray());
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
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
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
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
        SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));

        bitmap.SetPixel(0, 0, new SKColor(238, 18, 17));
        SavePng(bitmap, "average.actual.png");
        var error = Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 }));
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
        SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
        Assert.Contains("192/192", error.Message);
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0 }));
    }

    [Fact]
    public void AverageColorErrorIncludesAlphaInItsFivePercentBoundary()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "alpha.expected.png");
        bitmap.Erase(new SKColor(0, 0, 0, 204));
        var actual = SavePng(bitmap, "alpha.actual.png");
        SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });

        bitmap.SetPixel(0, 0, new SKColor(0, 0, 0, 203));
        SavePng(bitmap, "alpha.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 }));
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
        SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.06 });

        bitmap.SetPixel(1, 0, new SKColor(233, 20, 20));
        SavePng(bitmap, "gpu-budget.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.06 }));
    }

    [Fact]
    public void DotNetSampleTestBaseOwnsOnlyItsPreparedWorkspace()
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

    private sealed class LifetimeTest(string samplesDir) : DotNetSampleTestBase(samplesDir)
    {
        internal string Prepare() => PrepareSample(Path.Combine("Basic", "Console")).Root;
        internal string Diagnostics => PrepareSample(Path.Combine("Basic", "Console")).DiagnosticsRoot;
        internal SampleWorkspace Workspace => PrepareSample(Path.Combine("Basic", "Console"));
        internal Task Build(DotNetOptions? options = null) =>
            BuildSample(Path.Combine("Basic", "Console"), "App.csproj", options);
    }

    [Fact]
    public async Task DotNetSampleTestBaseBuildsWithRetainedDiagnostics()
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
    public async Task ExplicitFrameworkBuildKeepsDeclaredSharedReferencesAndFrameworkDiagnostics()
    {
        FileIn("inputs/Basic/Console/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net10.0;net10.0-windows</TargetFrameworks>
                <EnableWindowsTargeting>true</EnableWindowsTargeting>
                <OutputType>Exe</OutputType>
              </PropertyGroup>
              <Target Name="VerifySelectedFramework" BeforeTargets="CoreCompile">
                <Error Condition="'$(TargetFramework)' != 'net10.0-windows' or '$(TargetFrameworks)' != 'net10.0;net10.0-windows'"
                       Text="Build must select net10.0-windows without overriding the declared framework list." />
                <Error Condition="'$(ContractSentinel)' != 'forwarded-by-buildsample'"
                       Text="BuildSample must forward the test-owned property dictionary." />
                <Message Importance="high" Text="Selected $(TargetFramework) from $(TargetFrameworks)" />
                <Message Importance="high" Text="Contract sentinel: $(ContractSentinel)" />
                <Message Importance="high" Text="Build working directory: $(MSBuildStartupDirectory)" />
              </Target>
              <ItemGroup>
                <ProjectReference Include="Library/Library.csproj" />
                <ProjectReference Include="Shared/Shared.csproj" />
                <Compile Remove="Library/**/*.cs" />
                <Compile Remove="Shared/**/*.cs" />
              </ItemGroup>
            </Project>
            """);
        FileIn("inputs/Basic/Console/Library/Library.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>netstandard2.1</TargetFramework></PropertyGroup>
              <Target Name="VerifyLibraryFramework" BeforeTargets="CoreCompile">
                <Error Condition="'$(TargetFramework)' != 'netstandard2.1'"
                       Text="The library's declared framework must not be retargeted." />
              </Target>
            </Project>
            """);
        FileIn("inputs/Basic/Console/Shared/Shared.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
              </PropertyGroup>
              <Target Name="VerifySharedFramework" BeforeTargets="CoreCompile">
                <Error Condition="'$(TargetFramework)' != 'net10.0' or '$(TargetFrameworks)' != ''"
                       Text="The ordinary shared project's declared framework must not be retargeted." />
              </Target>
            </Project>
            """);
        FileIn("inputs/Basic/Console/Program.cs", """
            System.Console.WriteLine(System.AppContext.TargetFrameworkName);
            System.Console.WriteLine(string.Join("|", args));
            System.Console.WriteLine("Run working directory: " + System.Environment.CurrentDirectory);
            if (System.Array.IndexOf(args, "--wait") >= 0)
                System.Threading.Thread.Sleep(60000);
            """);
        using var test = new LifetimeTest(Path.Combine(root, "inputs"));
        var properties = new Dictionary<string, string> { ["ContractSentinel"] = "forwarded-by-buildsample" };
        var workingDirectory = Path.Combine(test.Workspace.Root, "custom working directory");
        Directory.CreateDirectory(workingDirectory);
        var options = new DotNetOptions
        {
            Framework = "net10.0-windows", Properties = properties,
            Timeout = TimeSpan.FromMinutes(1), WorkingDirectory = Path.Combine(workingDirectory, ".")
        };
        await test.Build(options);
        var working = test.Prepare();
        var diagnostics = Path.Combine(test.Diagnostics, "builds", "Basic", "Console", "App", "Release", "net10.0-windows");
        var output = File.ReadAllText(Path.Combine(diagnostics, "build.stdout.log"));
        Assert.Contains("Selected net10.0-windows from net10.0;net10.0-windows", output);
        Assert.Contains("Contract sentinel: forwarded-by-buildsample", output);
        Assert.Contains($"Build working directory: {workingDirectory}", output);
        Assert.Equal(new KeyValuePair<string, string>("ContractSentinel", "forwarded-by-buildsample"), Assert.Single(properties));
        Assert.True(new FileInfo(Path.Combine(diagnostics, "build.binlog")).Length > 0);
        Assert.False(Directory.Exists(Path.Combine(working, "samples", "Basic", "Console", "bin", "Release", "net10.0")));
        Assert.True(File.Exists(Path.Combine(working, "samples", "Basic", "Console", "Library", "bin", "Release", "netstandard2.1", "Library.dll")));
        Assert.True(File.Exists(Path.Combine(working, "samples", "Basic", "Console", "Shared", "bin", "Release", "net10.0", "Shared.dll")));

        var workspace = test.Workspace;
        var project = Path.Combine(working, "samples", "Basic", "Console", "App.csproj");
        var called = false;
        await DotNet.Run(workspace, project, ["argument with spaces", "--sentinel"], async app =>
        {
            called = true;
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains(".NETCoreApp,Version=v10.0", result.Output);
            Assert.Contains("argument with spaces|--sentinel", result.Output);
            Assert.Contains($"Run working directory: {workingDirectory}", result.Output);
        }, options);
        Assert.True(called);
        Assert.Contains("argument with spaces|--sentinel",
            File.ReadAllText(Path.Combine(workspace.DiagnosticsRoot, "run", "run.stdout.log")));
        foreach (var waitForAddress in new[] { false, true })
        {
            await Assert.ThrowsAsync<TimeoutException>(() =>
                DotNet.Run(workspace, project, ["--wait"], async app =>
                {
                    if (waitForAddress)
                        await app.GetAddress();
                    else
                        await app.WaitForExit();
                }, options with { Timeout = TimeSpan.FromMilliseconds(100) }));
            Assert.True(File.Exists(Path.Combine(workspace.DiagnosticsRoot, "run", "run.stderr.log")));
        }
        test.Dispose();
        Assert.False(Directory.Exists(working));
        Assert.True(Directory.Exists(diagnostics));
    }

    [Fact]
    public async Task DefaultBuildAndCallbackRunKeepTheirOriginalContract()
    {
        FileIn("inputs/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType></PropertyGroup>
            </Project>
            """);
        FileIn("inputs/App/Program.cs", """
            System.Console.WriteLine(string.Join("|", args));
            System.Console.WriteLine("Sample host URLs: " + System.Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
            """);
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var project = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
        await DotNet.Build(workspace, project, Path.Combine(workspace.DiagnosticsRoot, "build"));
        var called = false;
        await DotNet.Run(workspace, project, ["old argument", "--sentinel"], async app =>
        {
            called = true;
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("old argument|--sentinel", result.Output);
            Assert.Contains("Sample host URLs: http://127.0.0.1:0", result.Output);
        });
        Assert.True(called);
    }

    [Fact]
    public void ExplicitBuildPropertiesKeepDefaultFlagsAndDoNotMutateTheCaller()
    {
        FileIn("inputs/App/App.csproj");
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var project = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
        var diagnostics = Path.Combine(workspace.DiagnosticsRoot, "build");
        var properties = new Dictionary<string, string> { ["RuntimeIdentifier"] = "iossimulator-arm64" };
        var options = new DotNetOptions { Framework = "net10.0-ios", Properties = properties };
        var defaults = DotNet.BuildArguments(workspace, project, diagnostics);
        var arguments = DotNet.BuildArguments(workspace, project, diagnostics, options);
        Assert.Equal(defaults, arguments.Take(defaults.Length));
        Assert.Equal(new[]
        {
            "--framework", "net10.0-ios", "-p:RuntimeIdentifier=iossimulator-arm64"
        }, arguments.Skip(defaults.Length));
        Assert.Equal(Path.GetFullPath(project), arguments[1]);
        Assert.Single(arguments, argument => argument.StartsWith("-bl:", StringComparison.Ordinal));
        Assert.Single(arguments, argument => argument.StartsWith("-p:RestoreConfigFile=", StringComparison.Ordinal));
        Assert.DoesNotContain(arguments, argument => argument.StartsWith("-p:TargetFramework", StringComparison.Ordinal));
        Assert.Equal(new KeyValuePair<string, string>("RuntimeIdentifier", "iossimulator-arm64"), Assert.Single(properties));
        Assert.Equal(arguments, DotNet.BuildArguments(workspace, project, diagnostics, options));
        var run = DotNet.RunArguments(project, ["argument with spaces", "--sentinel"], options);
        Assert.Equal(new[]
        {
            "run", "--project", Path.GetFullPath(project), "-c", "Release", "--no-build", "--no-launch-profile",
            "--framework", "net10.0-ios", "-p:RuntimeIdentifier=iossimulator-arm64", "--", "argument with spaces", "--sentinel"
        }, run);
        foreach (var name in new[] { "TargetFramework", "TargetFrameworks", "configuration", "RestoreConfigFile", "RestoreNoCache", "UseSharedCompilation", " " })
        {
            var conflicting = new Dictionary<string, string> { [name] = "override" };
            Assert.Throws<ArgumentException>(() =>
                DotNet.BuildArguments(workspace, project, diagnostics, options with { Properties = conflicting }));
            Assert.Throws<ArgumentException>(() =>
                DotNet.RunArguments(project, [], options with { Properties = conflicting }));
        }
    }

    [Fact]
    public void DotNetOptionsDefaultsAndCopiesKeepArgumentsIndependent()
    {
        var defaults = new DotNetOptions();
        Assert.Equal("Release", defaults.Configuration);
        Assert.Null(defaults.Framework);
        Assert.Empty(defaults.Properties);
        Assert.Null(defaults.Timeout);
        Assert.Null(defaults.WorkingDirectory);
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, string>)defaults.Properties).Add("Sentinel", "value"));
        var options = defaults with { Configuration = "Debug", Framework = "net10.0", WorkingDirectory = "." };
        Assert.Equal("Release", defaults.Configuration);
        Assert.Null(defaults.Framework);
        Assert.Null(defaults.WorkingDirectory);
        Assert.Equal(Path.GetFullPath("."), options.GetWorkingDirectory(root));
        Assert.Equal(Path.GetFullPath(root), defaults.GetWorkingDirectory(root));
        var project = Path.Combine(root, "project with spaces.csproj");
        Assert.Equal(new[]
        {
            "run", "--project", project, "-c", "Release", "--no-build", "--no-launch-profile", "--", "argument with spaces"
        }, DotNet.RunArguments(project, ["argument with spaces"]));
        var run = DotNet.RunArguments(project, [], options);
        Assert.Equal("Debug", run[4]);
        Assert.Equal(new[] { "--framework", "net10.0", "--" }, run.Skip(7));
    }

    [Fact]
    public async Task InvalidDotNetOptionsFailBeforeStartingProcesses()
    {
        FileIn("inputs/App/App.csproj");
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var project = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
        var diagnostics = Path.Combine(workspace.DiagnosticsRoot, "invalid-build");
        var invalid = new DotNetOptions[]
        {
            new() { Configuration = "" }, new() { Configuration = " " }, new() { Configuration = null! },
            new() { Framework = "" }, new() { Framework = " " },
            new() { WorkingDirectory = "" }, new() { WorkingDirectory = " " },
            new() { Properties = null! }, new() { Timeout = TimeSpan.Zero },
            new() { Timeout = Timeout.InfiniteTimeSpan }, new() { Timeout = TimeSpan.MaxValue }
        };
        foreach (var options in invalid)
        {
            Assert.ThrowsAny<ArgumentException>(() => DotNet.BuildArguments(workspace, project, diagnostics, options));
            Assert.ThrowsAny<ArgumentException>(() => DotNet.RunArguments(project, [], options));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => DotNet.Build(workspace, project, diagnostics, options));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => DotNet.Run(workspace, project, [], _ => Task.CompletedTask, options));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => DotNet.Execute(workspace, ["--version"], options));
        }
        Assert.False(Directory.Exists(diagnostics));
        Assert.False(Directory.Exists(Path.Combine(workspace.DiagnosticsRoot, "run")));
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CleanupRecoversOnlyMacMetadataCreatedDuringDeletion(bool unrelatedFile)
    {
        FileIn("owned/obj/initial.bin", "Build output.");
        if (unrelatedFile)
            FileIn("owned/obj/late.bin", "Unrelated residue.");
        FileIn("other-workspace/.DS_Store", "Keep this metadata.");
        FileIn("diagnostics/build.log", "Keep these diagnostics.");
        var owned = Path.Combine(root, "owned");
        var obj = Path.Combine(owned, "obj");
        var calls = 0;
        void Delete(string path)
        {
            calls++;
            if (calls == 1)
            {
                File.Delete(Path.Combine(obj, "initial.bin"));
                File.WriteAllText(Path.Combine(path, ".DS_Store"), "Late parent metadata.");
                File.WriteAllText(Path.Combine(obj, ".DS_Store"), "Late metadata.");
                Directory.Delete(obj);
            }
            Directory.Delete(path, recursive: true);
        }

        if (OperatingSystem.IsMacOS() && !unrelatedFile)
        {
            SampleWorkspace.DeleteDirectory(owned, Delete);
            Assert.Equal(2, calls);
            Assert.False(Directory.Exists(owned));
        }
        else
        {
            Assert.Throws<IOException>(() => SampleWorkspace.DeleteDirectory(owned, Delete));
            Assert.Equal(1, calls);
            Assert.True(Directory.Exists(owned));
        }
        Assert.True(File.Exists(Path.Combine(root, "other-workspace", ".DS_Store")));
        Assert.True(File.Exists(Path.Combine(root, "diagnostics", "build.log")));
    }

    [Fact]
    public void CleanupDoesNotRetryUnrelatedErrorsWithMetadataPresent()
    {
        FileIn("owned/obj/.DS_Store", "Metadata.");
        var owned = Path.Combine(root, "owned");
        var expected = new IOException("Expected unrelated I/O failure.", 1234);
        var calls = 0;
        var actual = Assert.Throws<IOException>(() => SampleWorkspace.DeleteDirectory(owned, _ =>
        {
            calls++;
            throw expected;
        }));
        Assert.Same(expected, actual);
        Assert.Equal(1, calls);
        Assert.True(File.Exists(Path.Combine(owned, "obj", ".DS_Store")));
    }

    [Theory]
    [InlineData("late.bin", 1)]
    [InlineData(".DS_Store", 2)]
    public void CleanupDoesNotHidePersistentOrNonMetadataWriters(string name, int macCalls)
    {
        FileIn($"owned/obj/{name}", "Late file.");
        var owned = Path.Combine(root, "owned");
        var obj = Path.Combine(owned, "obj");
        var calls = 0;
        void Delete(string path)
        {
            calls++;
            File.WriteAllText(Path.Combine(obj, name), "Late file.");
            Directory.Delete(obj);
        }

        Assert.Throws<IOException>(() => SampleWorkspace.DeleteDirectory(owned, Delete));
        Assert.Equal(OperatingSystem.IsMacOS() ? macCalls : 1, calls);
        Assert.True(File.Exists(Path.Combine(obj, name)));
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

    private sealed class AddressRunningApp(string diagnostics) : RunningApp(diagnostics)
    {
        internal int AddressRequests { get; private set; }

        internal override Task<Uri> GetAddress()
        {
            AddressRequests++;
            return Task.FromResult(new Uri("http://127.0.0.1:49152"));
        }

        internal override Task<(int ExitCode, string Output, string Error)> WaitForExit() =>
            Task.FromResult((0, "", ""));

        protected override Task Stop() => Task.CompletedTask;

        protected override Task<(string Output, string Error)> ReadOutput() =>
            Task.FromResult(("", ""));
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
            SampleWorkspace.DeleteDirectory(root);
    }
}
