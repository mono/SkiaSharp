using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

[Collection("Package consumers")]
public class NativeAssetOutputTests(DotNet dotnet)
{
    private static readonly string[] Families = ["SkiaSharp", "HarfBuzzSharp"];
    private static readonly string[] Platforms = ["Win32", "macOS", "Linux"];
    private static readonly string[] TargetFrameworks = ["net10.0", "net11.0"];

    public static TheoryData<string, string> DefaultCases
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var family in Families)
                foreach (var targetFramework in TargetFrameworks)
                    data.Add(family, targetFramework);
            return data;
        }
    }

    public static TheoryData<string, string, string> BuildRidCases
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            foreach (var family in Families)
                foreach (var setting in new[] { "none", "single", "multiple" })
                    foreach (var targetFramework in TargetFrameworks)
                        data.Add(family, setting, targetFramework);
            return data;
        }
    }

    public static TheoryData<string, string, string?, string> PublishRidCases
    {
        get
        {
            var data = new TheoryData<string, string, string?, string>();
            foreach (var family in Families)
                foreach (var setting in new[] { "none", "single", "multiple" })
                    foreach (var framework in TargetFrameworks)
                    {
                        data.Add(family, setting, null, framework);
                        data.Add(family, setting, "linux-x64", framework);
                    }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(DefaultCases))]
    public async Task BuildDefaultPackageIncludesWindowsAndMacButNotLinux(string family, string targetFramework)
    {
        var consumer = await CreateDefaultConsumer(family, targetFramework, "build");
        await dotnet.Build(consumer.Project);
        AssertNativeOutput(consumer.Project, family, consumer.Expected, targetFramework);
        dotnet.CleanBuildOutput(consumer.Project);
    }

    [Theory]
    [MemberData(nameof(DefaultCases))]
    public async Task PublishDefaultPackageIncludesWindowsAndMacButNotLinux(string family, string targetFramework)
    {
        var consumer = await CreateDefaultConsumer(family, targetFramework, "publish");
        await dotnet.Publish(consumer.Project);
        AssertNativeOutput(consumer.Project, family, consumer.Expected, targetFramework);
        dotnet.CleanBuildOutput(consumer.Project);
    }

    private async Task<(string Project, Dictionary<string, string> Expected)> CreateDefaultConsumer(
        string family, string targetFramework, string name)
    {
        var core = dotnet.Packages.Get(family);
        var expected = NativeFiles(dotnet.Packages.Get(family + ".NativeAssets.Win32"))
            .Concat(NativeFiles(dotnet.Packages.Get(family + ".NativeAssets.macOS")))
            .ToDictionary(p => p.Key, p => p.Value);
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/win-", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/osx", StringComparison.Ordinal));
        Assert.DoesNotContain(expected.Keys, path => path.StartsWith("runtimes/linux", StringComparison.Ordinal));

        var project = await CreateProject($"{family}-default-{name}-{targetFramework}", family, core.Version, targetFramework);
        return (project, expected);
    }

    [Theory]
    [MemberData(nameof(BuildRidCases))]
    public async Task BuildExplicitLinuxPackageHonorsRuntimeIdentifiers(string family, string setting, string targetFramework)
    {
        var consumer = await CreateRidConsumer(family, setting, null, targetFramework, "build");
        await dotnet.Build(consumer.Project);
        AssertNativeOutput(consumer.Project, family, consumer.Expected, targetFramework);
        dotnet.CleanBuildOutput(consumer.Project);
    }

    [Theory]
    [MemberData(nameof(PublishRidCases))]
    public async Task PublishExplicitLinuxPackageHonorsRuntimeIdentifiers(string family, string setting,
        string? commandRid, string targetFramework)
    {
        var consumer = await CreateRidConsumer(family, setting, commandRid, targetFramework, "publish");
        await dotnet.Publish(consumer.Project, commandRid);
        AssertNativeOutput(consumer.Project, family, consumer.Expected, targetFramework);
        dotnet.CleanBuildOutput(consumer.Project);
    }

    private async Task<(string Project, Dictionary<string, string> Expected)> CreateRidConsumer(string family,
        string setting, string? commandRid, string targetFramework, string name)
    {
        var core = dotnet.Packages.Get(family);
        var natives = Platforms.Select(platform => dotnet.Packages.Get(family + ".NativeAssets." + platform)).ToArray();
        var all = natives.SelectMany(NativeFiles).ToDictionary(p => p.Key, p => p.Value);
        Assert.Contains(all.Keys, path => path.StartsWith("runtimes/linux-x64/native/", StringComparison.Ordinal));
        Assert.Contains(all.Keys, path => path.StartsWith("runtimes/linux-arm64/native/", StringComparison.Ordinal));
        var selectedRid = commandRid ?? (setting == "single" ? "linux-arm64" : null);
        var expected = selectedRid is null ? all : all
            .Where(p => p.Key.StartsWith($"runtimes/{selectedRid}/native/", StringComparison.Ordinal))
            .ToDictionary(p => p.Key[$"runtimes/{selectedRid}/native/".Length..], p => p.Value);
        Assert.NotEmpty(expected);

        var project = await CreateProject($"{family}-{setting}-{name}-{commandRid ?? "default"}-{targetFramework}",
            family, core.Version, targetFramework, setting, includeLinux: true);
        return (project, expected);
    }

    [Theory]
    [InlineData("SkiaSharp")]
    [InlineData("HarfBuzzSharp")]
    public async Task BuildMultiTargetConsolePreservesEachFrameworkOutput(string family)
    {
        var consumer = await CreateMultiTargetConsumer(family, "build");
        await dotnet.Build(consumer.Project, outputDirectory: null);
        var outputs = TargetFrameworks.Select(framework =>
            (Framework: framework, OutputDirectory: Path.Combine("bin", "Release", framework))).ToArray();
        foreach (var (framework, outputDirectory) in outputs)
            AssertNativeOutput(consumer.Project, family, consumer.Expected, framework, outputDirectory, TargetFrameworks);
        dotnet.CleanBuildOutput(consumer.Project, outputs);
    }

    [Theory]
    [InlineData("SkiaSharp")]
    [InlineData("HarfBuzzSharp")]
    public async Task PublishMultiTargetConsolePreservesEachFrameworkOutput(string family)
    {
        var consumer = await CreateMultiTargetConsumer(family, "publish");
        var outputs = TargetFrameworks.Select(framework =>
            (Framework: framework, OutputDirectory: Path.Combine("output", framework))).ToArray();
        foreach (var (framework, outputDirectory) in outputs)
        {
            await dotnet.Publish(consumer.Project, framework: framework, outputDirectory: outputDirectory,
                diagnosticLabel: $"publish-{framework}");
            AssertNativeOutput(consumer.Project, family, consumer.Expected, framework, outputDirectory, TargetFrameworks);
        }
        foreach (var (framework, outputDirectory) in outputs)
            AssertNativeOutput(consumer.Project, family, consumer.Expected, framework, outputDirectory, TargetFrameworks);
        dotnet.CleanBuildOutput(consumer.Project, outputs);
    }

    private async Task<(string Project, Dictionary<string, string> Expected)> CreateMultiTargetConsumer(string family, string name)
    {
        var core = dotnet.Packages.Get(family);
        var natives = Platforms.Select(platform => dotnet.Packages.Get(family + ".NativeAssets." + platform)).ToArray();
        var expected = natives.SelectMany(NativeFiles).ToDictionary(p => p.Key, p => p.Value);
        var project = await CreateProject($"{family}-multi-target-{name}", family, core.Version,
            string.Join(";", TargetFrameworks), includeLinux: true, multiTarget: true);
        return (project, expected);
    }

    private Task<string> CreateProject(string name, string family, string version, string targetFramework, string setting = "none",
        bool includeLinux = false, bool multiTarget = false)
        => dotnet.NewTemplateProject(name, "console", multiTarget ? TargetFrameworks[0] : targetFramework, project =>
    {
        if (multiTarget)
            project.Descendants("TargetFramework").Remove();
        var properties = new XElement("PropertyGroup",
            new XElement(multiTarget ? "TargetFrameworks" : "TargetFramework", targetFramework),
            new XElement("OutputType", "Exe"),
            new XElement("SelfContained", "false"),
            new XElement("UseAppHost", "true"));
        if (setting == "single")
            properties.Add(new XElement("RuntimeIdentifier", "linux-arm64"));
        if (setting == "multiple")
            properties.Add(new XElement("RuntimeIdentifiers", "linux-x64;linux-arm64"));
        var packages = new XElement("ItemGroup",
            new XElement("PackageReference", new XAttribute("Include", family), new XAttribute("Version", $"[{version}]")));
        if (includeLinux)
            packages.Add(new XElement("PackageReference", new XAttribute("Include", family + ".NativeAssets.Linux"),
                new XAttribute("Version", $"[{version}]")));
        if (!multiTarget)
            properties.Element("TargetFramework")!.Remove();
        project.Add(properties, packages);
    });

    private static IEnumerable<KeyValuePair<string, string>> NativeFiles(ArtifactPackage package) =>
        package.Files.Where(entry => entry.Key.StartsWith("runtimes/", StringComparison.Ordinal) &&
            entry.Key.Contains("/native/", StringComparison.Ordinal) &&
            (entry.Key.EndsWith(".dll", StringComparison.Ordinal) || entry.Key.EndsWith(".so", StringComparison.Ordinal) ||
             entry.Key.EndsWith(".dylib", StringComparison.Ordinal)));

    private static void AssertNativeOutput(string project, string family, Dictionary<string, string> expected,
        string targetFramework, string? outputDirectory = null, string[]? restoredFrameworks = null)
    {
        var output = Path.Combine(project, outputDirectory ?? "output");
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal((restoredFrameworks ?? [targetFramework]).Order(),
            assets.RootElement.GetProperty("project").GetProperty("frameworks").EnumerateObject().Select(p => p.Name).Order());
        using var runtimeConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "Consumer.runtimeconfig.json")));
        Assert.Equal(targetFramework, runtimeConfig.RootElement.GetProperty("runtimeOptions").GetProperty("tfm").GetString());
        Assert.True(File.Exists(Path.Combine(output, family + ".dll")), $"Missing managed {family} assembly");
        var actual = Directory.EnumerateFiles(output, $"lib{family}.*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".dll", StringComparison.Ordinal) ||
                file.EndsWith(".so", StringComparison.Ordinal) || file.EndsWith(".dylib", StringComparison.Ordinal))
            .ToDictionary(file => Path.GetRelativePath(output, file).Replace('\\', '/'), file =>
            {
                using var stream = File.OpenRead(file);
                return Convert.ToHexString(SHA256.HashData(stream));
            });
        Assert.Equal(expected.Keys.Order(), actual.Keys.Order());
        foreach (var (path, hash) in expected)
            Assert.Equal(hash, actual[path]);
    }
}
