using System.Security.Cryptography;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

public class NativeAssetOutputTests(DotNet dotnet) : IClassFixture<DotNet>
{
    private static readonly string[] Families = ["SkiaSharp", "HarfBuzzSharp"];
    private static readonly string[] Platforms = ["Win32", "macOS", "Linux"];

    public static TheoryData<string, string, string, string?> RidCases
    {
        get
        {
            var data = new TheoryData<string, string, string, string?>();
            foreach (var family in Families)
            {
                foreach (var setting in new[] { "none", "single", "multiple" })
                {
                    data.Add(family, setting, "build", null);
                    data.Add(family, setting, "publish", null);
                    data.Add(family, setting, "publish", "linux-x64");
                }
            }
            return data;
        }
    }

    [Theory]
    [InlineData("SkiaSharp", "build")]
    [InlineData("SkiaSharp", "publish")]
    [InlineData("HarfBuzzSharp", "build")]
    [InlineData("HarfBuzzSharp", "publish")]
    public async Task DefaultPackageIncludesWindowsAndMacButNotLinux(string family, string command)
    {
        var core = ReadPackage(family);
        var expected = ReadPackage(family + ".NativeAssets.Win32").NativeFiles
            .Concat(ReadPackage(family + ".NativeAssets.macOS").NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/win-", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/osx", StringComparison.Ordinal));
        Assert.DoesNotContain(expected.Keys, path => path.StartsWith("runtimes/linux", StringComparison.Ordinal));

        var project = dotnet.NewProject($"{family}-default-{command}", ProjectXml(family, core.Version));
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, family, expected);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [MemberData(nameof(RidCases))]
    public async Task ExplicitLinuxPackageHonorsRuntimeIdentifiers(string family, string setting, string command, string? commandRid)
    {
        var core = ReadPackage(family);
        var natives = Platforms.Select(platform => ReadPackage(family + ".NativeAssets." + platform)).ToArray();
        Assert.All(natives, package => Assert.Equal(core.Version, package.Version));
        var all = natives.SelectMany(package => package.NativeFiles).ToDictionary(p => p.Key, p => p.Value);
        Assert.Contains(all.Keys, path => path.StartsWith("runtimes/linux-x64/native/", StringComparison.Ordinal));
        Assert.Contains(all.Keys, path => path.StartsWith("runtimes/linux-arm64/native/", StringComparison.Ordinal));
        var selectedRid = commandRid ?? (setting == "single" ? "linux-arm64" : null);
        var expected = selectedRid is null ? all : all
            .Where(p => p.Key.StartsWith($"runtimes/{selectedRid}/native/", StringComparison.Ordinal))
            .ToDictionary(p => p.Key[$"runtimes/{selectedRid}/native/".Length..], p => p.Value);
        Assert.NotEmpty(expected);

        var project = dotnet.NewProject($"{family}-{setting}-{command}-{commandRid ?? "default"}",
            ProjectXml(family, core.Version, setting, includeLinux: true));
        await BuildOrPublish(project, command, commandRid);
        AssertNativeOutput(project, family, expected);
        dotnet.CleanBuildOutput(project);
    }

    private Task BuildOrPublish(string project, string command, string? rid = null) =>
        command == "build" ? dotnet.Build(project) : dotnet.Publish(project, rid);

    private static string ProjectXml(string family, string version, string setting = "none", bool includeLinux = false)
    {
        var properties = new XElement("PropertyGroup",
            new XElement("TargetFramework", "net10.0"),
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
        return new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"), properties, packages).ToString();
    }

    private ArtifactPackage ReadPackage(string id) => ArtifactPackage.Read(dotnet.PackageDirectory, id);

    private static void AssertNativeOutput(string project, string family, Dictionary<string, string> expected)
    {
        var output = Path.Combine(project, "output");
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
