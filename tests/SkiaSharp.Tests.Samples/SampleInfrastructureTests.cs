using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
public class SampleInfrastructureTests : IDisposable
{
    private readonly string root = Path.Combine(DotNet.Setting("ArtifactsDirectory"), $"discovery-{Guid.NewGuid():N}");

    private void FileIn(string relative)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "<Project />");
    }

    [Theory]
    [InlineData("Windows")]
    [InlineData("Mac")]
    [InlineData("Linux")]
    public void SelectsSolutionsForTheHostAndIncludesGallery(string platform)
    {
        FileIn("Gallery/Gallery.slnx");
        foreach (var host in new[] { "Windows", "Mac", "Linux" })
            FileIn($"Gallery/Gallery.{host}.slnx");
        FileIn("Console/Console.slnx");
        FileIn("ArbitraryFolder/Arbitrary.slnx");
        FileIn("ArbitraryFolder/linux.Dockerfile");
        FileIn("ArbitraryFolder/windows.Dockerfile");
        FileIn("ArbitraryFolder/run.ps1");
        var entries = SampleLookup.Discover(root, platform);
        Assert.Equal(3, entries.Count);
        Assert.Contains(new SampleEntry("Gallery", $"Gallery.{platform}.slnx", SampleKind.Gallery), entries);
        Assert.Contains(new SampleEntry("Console", "Console.slnx", SampleKind.Sample), entries);
        Assert.Contains(new SampleEntry("ArbitraryFolder",
            platform == "Windows" ? "windows.Dockerfile" : "linux.Dockerfile", SampleKind.Docker), entries);
        Assert.DoesNotContain(entries, entry => entry.FileName == "Gallery.slnx" ||
            entry is { Folder: "ArbitraryFolder", Kind: SampleKind.Sample });
    }

    [Fact]
    public void DiscoversSampleRootsWithoutScanningNestedProjects()
    {
        FileIn("Basic/Console/Console.slnx");
        FileIn("Gallery/Gallery.Mac.slnx");
        FileIn("Basic/AnyName/linux.Dockerfile");
        FileIn("Basic/OtherName/Dockerfile");
        FileIn("Basic/Console/Legacy.sln");
        FileIn("Basic/Console/Legacy.slnf");
        FileIn("Basic/Console/obj/Nested.slnx");
        FileIn("Gallery/Blazor/obj/Nested.slnf");
        FileIn("Nested/Level/TooDeep/Dockerfile");

        var entries = SampleLookup.Discover(root, "Mac");
        Assert.Equal(4, entries.Count);
        Assert.Contains(new SampleEntry(Path.Combine("Basic", "Console"), "Console.slnx", SampleKind.Sample), entries);
        Assert.Contains(new SampleEntry("Gallery", "Gallery.Mac.slnx", SampleKind.Gallery), entries);
        Assert.Contains(new SampleEntry(Path.Combine("Basic", "AnyName"), "linux.Dockerfile", SampleKind.Docker), entries);
        Assert.Contains(new SampleEntry(Path.Combine("Basic", "OtherName"), "Dockerfile", SampleKind.Docker), entries);
    }

    [Fact]
    public void FilterAndMissingInputsFailExplicitly()
    {
        FileIn("Console/Console.slnx");
        FileIn("Web/Web.slnx");
        Assert.Single(SampleLookup.Discover(root, "Mac", "Console"));
        Assert.Throws<InvalidOperationException>(() => SampleLookup.Discover(root, "Mac", "Missing"));
        Assert.Throws<DirectoryNotFoundException>(() => SampleLookup.Discover(Path.Combine(root, "Missing"), "Mac"));
    }

    [Fact]
    public void DottedSolutionNamesAreNotHostVariants()
    {
        FileIn("Console/Console.slnx");
        FileIn("Console/Console.Core.slnx");
        FileIn("Ordinary/Ordinary.slnx");
        FileIn("Ordinary/Ordinary.Core.slnx");
        FileIn("Console/Console.Mac.slnx");
        FileIn("Console/Console.Windows.slnx");
        var entries = SampleLookup.Discover(root, "Mac");
        Assert.Contains(new SampleEntry("Console", "Console.Core.slnx", SampleKind.Sample), entries);
        Assert.Contains(new SampleEntry("Console", "Console.Mac.slnx", SampleKind.Sample), entries);
        Assert.DoesNotContain(entries, entry => entry is { Folder: "Console", FileName: "Console.slnx" });
        Assert.Contains(new SampleEntry("Ordinary", "Ordinary.slnx", SampleKind.Sample), entries);
        Assert.Contains(new SampleEntry("Ordinary", "Ordinary.Core.slnx", SampleKind.Sample), entries);
        Assert.Equal(4, entries.Count);
    }

    [Fact]
    public void SampleAndGalleryRowsAreSeparateFromDockerRows()
    {
        FileIn("Console/Console.slnx");
        FileIn($"Gallery/Gallery.{SampleLookup.HostPlatform}.slnx");
        FileIn("One/Console.slnx");
        FileIn("One/Dockerfile");
        FileIn("Two/Dockerfile");
        FileIn("Two/sample.http");
        var entries = SampleLookup.Discover(root, SampleLookup.HostPlatform);
        Assert.Equal(2, entries.Count(entry => entry.Kind == SampleKind.Docker));
        Assert.Equal(2, entries.Count(entry => entry.Kind != SampleKind.Docker));
        Assert.Equal(SampleKind.Docker, Assert.Single(SampleLookup.Discover(root, SampleLookup.HostPlatform, "One")).Kind);
        var key = "SampleTest.SamplesDirectory";
        var filterKey = "SampleTest.Filter";
        var previous = AppContext.GetData(key);
        var previousFilter = AppContext.GetData(filterKey);
        try
        {
            AppContext.SetData(key, root);
            AppContext.SetData(filterKey, "");
            var buildRows = SampleBuildTests.Cases().ToArray();
            Assert.Equal(2, buildRows.Length);
            Assert.All(buildRows, row => Assert.Equal(2, row.Length));
            var dockerRows = DockerSampleTests.Cases().ToArray();
            Assert.Equal(2, dockerRows.Length);
            Assert.All(dockerRows, row => Assert.Equal(2, row.Length));
            Assert.Equal(2, dockerRows.Select(row => row[0]).Distinct().Count());
            AppContext.SetData(filterKey, "One");
            Assert.Empty(SampleBuildTests.Cases());
            Assert.Single(DockerSampleTests.Cases());
        }
        finally
        {
            AppContext.SetData(key, previous);
            AppContext.SetData(filterKey, previousFilter);
        }
    }

    [Fact]
    public void ConsoleRunsFollowTheSameSampleFilter()
    {
        FileIn("Basic/Console/SkiaSharpSample.slnx");
        FileIn("Basic/Container/Dockerfile");
        var samplesKey = "SampleTest.SamplesDirectory";
        var filterKey = "SampleTest.Filter";
        var originalSamples = AppContext.GetData(samplesKey);
        var originalFilter = AppContext.GetData(filterKey);
        try
        {
            AppContext.SetData(samplesKey, root);
            AppContext.SetData(filterKey, "Console");
            Assert.Single(ConsoleSampleTests.Cases());
            AppContext.SetData(filterKey, "Container");
            Assert.Empty(ConsoleSampleTests.Cases());
            Assert.Single(DockerSampleTests.Cases());
        }
        finally
        {
            AppContext.SetData(samplesKey, originalSamples);
            AppContext.SetData(filterKey, originalFilter);
        }
    }

    [Fact]
    public void PlainHttpRequestsRebaseToTheAllocatedPort()
    {
        var file = Path.Combine(root, "sample.http");
        Directory.CreateDirectory(root);
        File.WriteAllText(file, "GET http://localhost:8080/health\n###\nGET http://localhost:8080/api/images/SkiaSharp\n");
        var requests = DockerSampleTests.ReadHttpRequests(file);
        Assert.Equal(2, requests.Count);
        Assert.Equal("http://127.0.0.1:49152/api/images/SkiaSharp",
            DockerSampleTests.Rebase(requests[1], 49152).AbsoluteUri);
    }

    [Theory]
    [InlineData("POST http://localhost:8080/health")]
    [InlineData("GET https://localhost:8080/health")]
    [InlineData("GET http://example.com:8080/health")]
    [InlineData("GET http://localhost:8080/health\nAccept: image/png")]
    public void UnsupportedHttpSyntaxFails(string content)
    {
        var file = Path.Combine(root, "sample.http");
        Directory.CreateDirectory(root);
        File.WriteAllText(file, content);
        Assert.Throws<InvalidOperationException>(() => DockerSampleTests.ReadHttpRequests(file));
    }

    [Fact]
    public void RealPngDecodesAndMalformedOrIncorrectImagesFail()
    {
        using var bitmap = new SKBitmap(800, 600);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var png = data.ToArray();
        SampleImage.Validate(png);
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(png.AsSpan(0, 12)));
        var malformed = (byte[])png.Clone();
        malformed[0] = 0;
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(malformed));

        using var wrongSize = new SKBitmap(400, 300);
        using var wrongImage = SKImage.FromBitmap(wrongSize);
        using var wrongData = wrongImage.Encode(SKEncodedImageFormat.Png, 100);
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(wrongData.ToArray()));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("3.4.567", null)]
    [InlineData("3.4.567", "7.8.901-preview.2")]
    public void InheritsOrPinsSdkAndOptionalWorkload(string? sdkVersion, string? workloadVersion)
    {
        FileIn("packages/consumer.nupkg");
        FileIn("inputs/Console.slnx");
        FileIn("inputs/Console/Consumer.csproj");
        File.WriteAllText(Path.Combine(root, "inputs", "Console", "Consumer.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
        var repository = Path.Combine(root, "checkout");
        if (OperatingSystem.IsWindows())
            repository = repository.Replace('\\', '/');
        Directory.CreateDirectory(repository);
        var repoGlobal = Path.Combine(repository, "global.json");
        var original = """
            {
              "sdk": {
                "version": "1.2.345",
                "paths": [".dotnet", "$host$"],
                "workloadVersion": "1.2.345",
                "errorMessage": "Use the repo SDK"
              },
              "tools": { "dotnet": "1.2.345" },
              "msbuild-sdks": { "Example.Sdk": "2.3.456" }
            }
            """;
        File.WriteAllText(repoGlobal, original);
        var sdkKey = "SampleTest.SdkVersion";
        var workloadKey = "SampleTest.WorkloadVersion";
        var packagesKey = "SampleTest.PackageDirectory";
        var artifactsKey = "SampleTest.ArtifactsDirectory";
        var samplesKey = "SampleTest.SamplesDirectory";
        var repositoryKey = "SampleTest.RepositoryDirectory";
        var previous = new[] { sdkKey, workloadKey, packagesKey, artifactsKey, samplesKey, repositoryKey }
            .ToDictionary(key => key, AppContext.GetData);
        try
        {
            AppContext.SetData(sdkKey, sdkVersion);
            AppContext.SetData(workloadKey, workloadVersion);
            AppContext.SetData(packagesKey, Path.Combine(root, "packages"));
            AppContext.SetData(artifactsKey, Path.Combine(root, "external-artifacts"));
            AppContext.SetData(samplesKey, Path.Combine(root, "inputs"));
            AppContext.SetData(repositoryKey, repository);
            using var workspace = new SampleWorkspace("Console");
            var profile = workspace.Profile();
            using var packageFixture = new DotNet();
            Assert.Single(typeof(DotNet).GetConstructors());
            Assert.Same(profile, workspace.Profile());
            Assert.Equal(profile.SdkVersion, packageFixture.SdkVersion);
            Assert.Equal(sdkVersion, profile.SdkVersion);
            foreach (var directory in new[] { profile.Root, packageFixture.Root })
            {
                Assert.StartsWith(Path.GetFullPath(Path.Combine(repository, "output", "samples-test-workspaces")), directory);
                foreach (var file in new[] { "Directory.Build.props", "Directory.Build.targets" })
                    Assert.Empty(XDocument.Load(Path.Combine(directory, file)).Root!.Elements());
                Assert.StartsWith(Path.GetFullPath(Path.Combine(root, "external-artifacts")),
                    directory == profile.Root ? profile.DiagnosticsRoot : packageFixture.DiagnosticsRoot);
                var localGlobal = Path.Combine(directory, "global.json");
                using var json = JsonDocument.Parse(File.ReadAllText(localGlobal));
                var sdk = json.RootElement.GetProperty("sdk");
                Assert.Equal(sdkVersion ?? "1.2.345", sdk.GetProperty("version").GetString());
                if (sdkVersion is not null)
                {
                    Assert.Equal("disable", sdk.GetProperty("rollForward").GetString());
                    Assert.True(sdk.GetProperty("allowPrerelease").GetBoolean());
                }
                Assert.Equal(Path.GetFullPath(Path.Combine(repository, ".dotnet")), sdk.GetProperty("paths")[0].GetString());
                Assert.Equal("$host$", sdk.GetProperty("paths")[1].GetString());
                Assert.Equal("Use the repo SDK", sdk.GetProperty("errorMessage").GetString());
                Assert.False(json.RootElement.TryGetProperty("tools", out _));
                Assert.False(json.RootElement.TryGetProperty("msbuild-sdks", out _));
                if (sdkVersion is null)
                    Assert.Equal("1.2.345", sdk.GetProperty("workloadVersion").GetString());
                else if (workloadVersion is null)
                    Assert.False(sdk.TryGetProperty("workloadVersion", out _));
                else
                    Assert.Equal(workloadVersion, sdk.GetProperty("workloadVersion").GetString());
                Assert.False(json.RootElement.TryGetProperty("workload", out _));
            }
            var copiedProject = XDocument.Load(Path.Combine(profile.Root, "samples", "Console", "Consumer.csproj"));
            Assert.Equal("net10.0", Assert.Single(copiedProject.Descendants("TargetFramework")).Value);
            Assert.Equal(original, File.ReadAllText(repoGlobal));
        }
        finally
        {
            foreach (var (key, value) in previous)
                AppContext.SetData(key, value);
        }
    }

    [Theory]
    [InlineData(null, "7.8.901")]
    [InlineData("invalid", null)]
    [InlineData("3.4.567", "invalid")]
    public void InvalidSdkOrWorkloadOverridesFail(string? sdkVersion, string? workloadVersion)
    {
        var sdkKey = "SampleTest.SdkVersion";
        var workloadKey = "SampleTest.WorkloadVersion";
        var previousSdk = AppContext.GetData(sdkKey);
        var previousWorkload = AppContext.GetData(workloadKey);
        try
        {
            AppContext.SetData(sdkKey, sdkVersion);
            AppContext.SetData(workloadKey, workloadVersion);
            Assert.Throws<InvalidOperationException>(() => new DotNet());
        }
        finally
        {
            AppContext.SetData(sdkKey, previousSdk);
            AppContext.SetData(workloadKey, previousWorkload);
        }
    }

    [Fact]
    public async Task ConsumerBuildKeepsItsFrameworkAndDoesNotImportRepositoryTargets()
    {
        var key = "SampleTest.RepositoryDirectory";
        var previous = AppContext.GetData(key);
        var repository = Path.Combine(root, "isolated-checkout");
        Directory.CreateDirectory(repository);
        File.Copy(Path.Combine(DotNet.Setting("RepositoryDirectory"), "global.json"),
            Path.Combine(repository, "global.json"));
        File.WriteAllText(Path.Combine(repository, "Directory.Build.props"),
            "<Project><PropertyGroup><RepositorySettingsLeaked>true</RepositorySettingsLeaked></PropertyGroup></Project>");
        File.WriteAllText(Path.Combine(repository, "Directory.Build.targets"),
            "<Project><Target Name=\"RejectRepositoryImports\" BeforeTargets=\"Build\"><Error Text=\"Repository targets reached the consumer.\" /></Target></Project>");
        try
        {
            AppContext.SetData(key, repository);
            using var profile = new DotNet();
            var project = profile.NewProject("isolated-net10", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <OutputType>Exe</OutputType>
                  </PropertyGroup>
                  <Target Name="RejectRepositoryProperties" BeforeTargets="Build">
                    <Error Condition="'$(RepositorySettingsLeaked)' != ''" Text="Repository properties reached the consumer." />
                  </Target>
                </Project>
                """);
            await profile.Build(project);
            using var runtime = JsonDocument.Parse(File.ReadAllText(
                Path.Combine(project, "output", "Consumer.runtimeconfig.json")));
            Assert.Equal("net10.0", runtime.RootElement.GetProperty("runtimeOptions").GetProperty("tfm").GetString());
            profile.CleanBuildOutput(project);
        }
        finally
        {
            AppContext.SetData(key, previous);
        }
    }

    [Theory]
    [InlineData("net10.0", "net9.0")]
    [InlineData("net11.0", "net10.0")]
    public void PreviousAndCurrentConsumerFrameworks(string current, string previousFramework)
    {
        var key = "SampleTest.ConsumerTargetFramework";
        var original = AppContext.GetData(key);
        try
        {
            AppContext.SetData(key, current);
            Assert.Equal((previousFramework, current), DotNet.ConsumerFrameworks());
            Assert.Equal(8, NativeAssetOutputTests.DefaultCases.Count);
            Assert.Equal(36, NativeAssetOutputTests.RidCases.Count);
        }
        finally
        {
            AppContext.SetData(key, original);
        }
    }

    [Theory]
    [InlineData("net10.0")]
    [InlineData("net11.0")]
    public void PlatformProbeFrameworkIsIndependentOfThePackageMatrix(string framework)
    {
        var key = "SampleTest.ConsumerTargetFramework";
        var original = AppContext.GetData(key);
        try
        {
            AppContext.SetData(key, framework);
            Assert.Equal("net10.0", GeneratedAppTestBase.BaseFramework);
            Assert.Equal(framework, DotNet.ConsumerFrameworks().Current);
        }
        finally
        {
            AppContext.SetData(key, original);
        }
    }

    [Theory]
    [InlineData("net4.8")]
    [InlineData("net10.0-windows")]
    public void UnsupportedConsumerFrameworkFails(string framework)
    {
        var key = "SampleTest.ConsumerTargetFramework";
        var original = AppContext.GetData(key);
        try
        {
            AppContext.SetData(key, framework);
            Assert.Throws<InvalidOperationException>(() => DotNet.ConsumerFrameworks());
        }
        finally
        {
            AppContext.SetData(key, original);
        }
    }

    [Fact]
    public void PackageCategoriesAreIndependent()
    {
        var type = typeof(NativeAssetOutputTests);
        Assert.DoesNotContain(type.CustomAttributes, attribute => attribute.AttributeType == typeof(TraitAttribute));
        foreach (var (method, category) in new[]
        {
            ("DefaultPackageIncludesWindowsAndMacButNotLinux", "PackageOutput"),
            ("ExplicitLinuxPackageHonorsRuntimeIdentifiers", "PackageOutput"),
            ("MultiTargetConsolePreservesEachFrameworkOutput", "PackageMultiTarget")
        })
        {
            var attributes = type.GetMethod(method)!.CustomAttributes;
            Assert.Contains(attributes, attribute =>
                attribute.AttributeType == typeof(TraitAttribute) &&
                attribute.ConstructorArguments[0].Value?.ToString() == "Category" &&
                attribute.ConstructorArguments[1].Value?.ToString() == category);
        }
    }

    [Fact]
    public void DockerPortBindingMatchesTheHostNetwork()
    {
        Assert.Equal("8080", DockerSampleTests.HttpPortBinding(true));
        Assert.Equal("127.0.0.1::8080", DockerSampleTests.HttpPortBinding(false));
    }

    [Fact]
    public void DockerBuildAndRunCategoriesAreIndependent()
    {
        var type = typeof(DockerSampleTests);
        Assert.Contains(type.CustomAttributes, attribute =>
            attribute.AttributeType == typeof(TraitAttribute) &&
            attribute.ConstructorArguments[0].Value?.ToString() == "Category" &&
            attribute.ConstructorArguments[1].Value?.ToString() == "Docker");
        foreach (var (method, category) in new[]
        {
            ("DockerImageBuilds", "DockerBuild"),
            ("DockerSampleRuns", "SampleRun")
        })
        {
            Assert.Contains(type.GetMethod(method)!.CustomAttributes, attribute =>
                attribute.AttributeType == typeof(TraitAttribute) &&
                attribute.ConstructorArguments[1].Value?.ToString() == category);
        }
        Assert.Contains(typeof(ConsoleSampleTests).CustomAttributes, attribute =>
            attribute.AttributeType == typeof(TraitAttribute) &&
            attribute.ConstructorArguments[1].Value?.ToString() == "SampleRun");
    }

    [Fact]
    public void CopiedSampleTreesKeepSiblingProjectsAndDiscardBuildProducts()
    {
        FileIn("inputs/App/App.slnx");
        FileIn("inputs/Sibling/Sibling.csproj");
        FileIn("inputs/App/obj/project.assets.json");
        var copy = Path.Combine(root, "copy");
        SampleWorkspace.CopyTree(Path.Combine(root, "inputs"), copy);
        Assert.True(File.Exists(Path.Combine(copy, "Sibling", "Sibling.csproj")));
        Assert.False(Directory.Exists(Path.Combine(copy, "App", "obj")));
        Assert.True(File.Exists(Path.Combine(copy, "App", "App.slnx")));
    }

    [Fact]
    public void SampleWorkspaceCopiesOnlyItsSampleAndAncestorConfiguration()
    {
        FileIn("inputs/Directory.Build.props");
        FileIn("inputs/NuGet.Config");
        FileIn("inputs/Basic/Directory.Build.targets");
        FileIn("inputs/Basic/Console/Console.slnx");
        FileIn("inputs/Basic/Console/App/App.csproj");
        FileIn("inputs/Basic/Console/App/obj/project.assets.json");
        FileIn("inputs/Basic/Web/Web.slnx");
        FileIn("inputs/Gallery/Gallery.slnx");

        var destination = Path.Combine(root, "scoped-copy");
        SampleWorkspace.CopySample(Path.Combine(root, "inputs"), Path.Combine("Basic", "Console"), destination);

        Assert.True(File.Exists(Path.Combine(destination, "Basic", "Console", "Console.slnx")));
        Assert.True(File.Exists(Path.Combine(destination, "Basic", "Console", "App", "App.csproj")));
        Assert.True(File.Exists(Path.Combine(destination, "Directory.Build.props")));
        Assert.True(File.Exists(Path.Combine(destination, "NuGet.Config")));
        Assert.True(File.Exists(Path.Combine(destination, "Basic", "Directory.Build.targets")));
        Assert.False(Directory.Exists(Path.Combine(destination, "Basic", "Console", "App", "obj")));
        Assert.False(Directory.Exists(Path.Combine(destination, "Basic", "Web")));
        Assert.False(Directory.Exists(Path.Combine(destination, "Gallery")));
    }

    [Fact]
    public void GalleryWorkspacePreservesItsSharedProjectsWithoutCopyingBasicSamples()
    {
        FileIn("inputs/Gallery/Gallery.slnx");
        FileIn("inputs/Gallery/Blazor/App.csproj");
        FileIn("inputs/Gallery/Shared/Shared.csproj");
        FileIn("inputs/Basic/Console/Console.slnx");
        var destination = Path.Combine(root, "gallery-copy");

        SampleWorkspace.CopySample(Path.Combine(root, "inputs"), "Gallery", destination);

        Assert.True(File.Exists(Path.Combine(destination, "Gallery", "Blazor", "App.csproj")));
        Assert.True(File.Exists(Path.Combine(destination, "Gallery", "Shared", "Shared.csproj")));
        Assert.False(Directory.Exists(Path.Combine(destination, "Basic")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("../outside")]
    public void SampleWorkspaceRejectsInvalidFolders(string folder)
    {
        Assert.Throws<ArgumentException>(() =>
            SampleWorkspace.CopySample(root, folder, Path.Combine(root, "invalid-copy")));
    }

    [Fact]
    public void SampleTestBaseDisposesItsOwnedWorkingDirectory()
    {
        var source = Path.Combine(root, "lifecycle-inputs");
        FileIn("lifecycle-inputs/Basic/Console/Console.slnx");
        var key = "SampleTest.SamplesDirectory";
        var previous = AppContext.GetData(key);
        string working;
        try
        {
            AppContext.SetData(key, source);
            using (var test = new WorkspaceLifetimeTest(Assert.IsAssignableFrom<ITestOutputHelper>(TestContext.Current.TestOutputHelper)))
            {
                working = test.Prepare();
                Assert.True(Directory.Exists(working));
                Assert.True(File.Exists(Path.Combine(working, "samples", "Basic", "Console", "Console.slnx")));
            }
            Assert.False(Directory.Exists(working));
            Assert.True(Directory.Exists(source));
        }
        finally
        {
            AppContext.SetData(key, previous);
        }
    }

    private sealed class WorkspaceLifetimeTest(ITestOutputHelper output) : SampleTestBase(output)
    {
        protected override string SampleFolder => Path.Combine("Basic", "Console");
        internal string Prepare() => PrepareSample().Root;
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
