using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

public class NativeAssetOutputTests(DotNet dotnet) : IClassFixture<DotNet>
{
    private static readonly string[] Families = ["SkiaSharp", "HarfBuzzSharp"];
    private static readonly string[] Platforms = ["Win32", "macOS", "Linux"];
    private static readonly string[] TargetFrameworks = ["net10.0", "net11.0"];

    public static TheoryData<string, string, string> DefaultCases
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            foreach (var family in Families)
                foreach (var command in new[] { "build", "publish" })
                    foreach (var targetFramework in TargetFrameworks)
                        data.Add(family, command, targetFramework);
            return data;
        }
    }

    public static TheoryData<string, string, string, string?, string> RidCases
    {
        get
        {
            var data = new TheoryData<string, string, string, string?, string>();
            foreach (var family in Families)
            {
                foreach (var setting in new[] { "none", "single", "multiple" })
                {
                    foreach (var targetFramework in TargetFrameworks)
                    {
                        data.Add(family, setting, "build", null, targetFramework);
                        data.Add(family, setting, "publish", null, targetFramework);
                        data.Add(family, setting, "publish", "linux-x64", targetFramework);
                    }
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(DefaultCases))]
    public async Task DefaultPackageIncludesWindowsAndMacButNotLinux(string family, string command, string targetFramework)
    {
        var core = ReadPackage(family);
        var expected = ReadPackage(family + ".NativeAssets.Win32").NativeFiles
            .Concat(ReadPackage(family + ".NativeAssets.macOS").NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/win-", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/osx", StringComparison.Ordinal));
        Assert.DoesNotContain(expected.Keys, path => path.StartsWith("runtimes/linux", StringComparison.Ordinal));

        var project = dotnet.NewProject($"{family}-default-{command}-{targetFramework}",
            ProjectXml(family, core.Version, targetFramework));
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, family, expected, targetFramework);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [MemberData(nameof(RidCases))]
    public async Task ExplicitLinuxPackageHonorsRuntimeIdentifiers(string family, string setting, string command,
        string? commandRid, string targetFramework)
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

        var project = dotnet.NewProject($"{family}-{setting}-{command}-{commandRid ?? "default"}-{targetFramework}",
            ProjectXml(family, core.Version, targetFramework, setting, includeLinux: true));
        await BuildOrPublish(project, command, commandRid);
        AssertNativeOutput(project, family, expected, targetFramework);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp", "build")]
    [InlineData("SkiaSharp", "publish")]
    [InlineData("HarfBuzzSharp", "build")]
    [InlineData("HarfBuzzSharp", "publish")]
    public async Task MultiTargetConsolePreservesEachFrameworkOutput(string family, string command)
    {
        var core = ReadPackage(family);
        var natives = Platforms.Select(platform => ReadPackage(family + ".NativeAssets." + platform)).ToArray();
        Assert.All(natives, package => Assert.Equal(core.Version, package.Version));
        var expected = natives.SelectMany(package => package.NativeFiles).ToDictionary(p => p.Key, p => p.Value);
        var project = dotnet.NewProject($"{family}-multi-target-{string.Join("-", TargetFrameworks)}-{command}",
            ProjectXml(family, core.Version, string.Join(";", TargetFrameworks), includeLinux: true, multiTarget: true));
        var outputs = TargetFrameworks.Select(framework => (Framework: framework,
            OutputDirectory: command == "build" ? Path.Combine("bin", "Release", framework) : Path.Combine("output", framework)))
            .ToArray();

        if (command == "build")
            await dotnet.Build(project, outputDirectory: null);
        else
        {
            foreach (var (framework, outputDirectory) in outputs)
            {
                await dotnet.Publish(project, framework: framework, outputDirectory: outputDirectory,
                    diagnosticLabel: $"publish-{framework}");
                AssertNativeOutput(project, family, expected, framework, outputDirectory, TargetFrameworks);
            }
        }
        foreach (var (framework, outputDirectory) in outputs)
            AssertNativeOutput(project, family, expected, framework, outputDirectory, TargetFrameworks);
        dotnet.CleanBuildOutput(project, outputs);
    }

    [Fact]
    public void Issue_4259_WindowsNativeAssetPackagesKeepSymbolsSeparate()
    {
        var packageIds = new List<string>
        {
            "SkiaSharp.NativeAssets.Win32",
            "SkiaSharp.NativeAssets.NanoServer",
            "HarfBuzzSharp.NativeAssets.Win32",
        };
        if (PackageExists("SkiaSharp.NativeAssets.WinUI", symbols: false))
            packageIds.Add("SkiaSharp.NativeAssets.WinUI");
        else if (OperatingSystem.IsWindows())
            Assert.Fail($"Missing SkiaSharp.NativeAssets.WinUI package in {dotnet.PackageDirectory}");

        foreach (var id in packageIds)
        {
            var product = ReadPackageEntries(id, symbols: false);
            var symbols = ReadPackageEntries(id, symbols: true);

            Assert.Equal(product.Version, symbols.Version);
            Assert.DoesNotContain(product.Entries, IsRuntimeSymbol);

            var productBinaries = product.Entries
                .Where(entry => entry.StartsWith("runtimes/", StringComparison.Ordinal) &&
                    entry.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var symbolPdbs = symbols.Entries
                .Where(entry => entry.StartsWith("runtimes/", StringComparison.Ordinal) &&
                    entry.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.NotEmpty(productBinaries);
            Assert.NotEmpty(symbolPdbs);
            Assert.All(productBinaries, binary => Assert.Contains(binary, symbols.Entries));
            Assert.All(symbolPdbs, pdb =>
                Assert.Contains(Path.ChangeExtension(pdb, ".dll").Replace('\\', '/'), symbols.Entries));
        }
    }

    private Task BuildOrPublish(string project, string command, string? rid = null) =>
        command == "build" ? dotnet.Build(project) : dotnet.Publish(project, rid);

    private static string ProjectXml(string family, string version, string targetFramework, string setting = "none",
        bool includeLinux = false, bool multiTarget = false)
    {
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
        return new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"), properties, packages).ToString();
    }

    private (string Version, Dictionary<string, string> NativeFiles) ReadPackage(string id)
    {
        var package = FindPackage(id, symbols: false);
        var native = new Dictionary<string, string>();
        using var zip = ZipFile.OpenRead(package.Path);
        foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith("runtimes/", StringComparison.Ordinal) &&
            e.FullName.Contains("/native/", StringComparison.Ordinal) &&
            (e.FullName.EndsWith(".dll", StringComparison.Ordinal) ||
             e.FullName.EndsWith(".so", StringComparison.Ordinal) ||
             e.FullName.EndsWith(".dylib", StringComparison.Ordinal))))
        {
            using var stream = entry.Open();
            native.Add(entry.FullName, Convert.ToHexString(SHA256.HashData(stream)));
        }
        return (package.Version, native);
    }

    private (string Version, HashSet<string> Entries) ReadPackageEntries(string id, bool symbols)
    {
        var package = FindPackage(id, symbols);
        using var zip = ZipFile.OpenRead(package.Path);
        return (package.Version, zip.Entries
            .Select(entry => entry.FullName.Replace('\\', '/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private (string Version, string Path) FindPackage(string id, bool symbols)
    {
        var matches = new List<(string Version, string Path)>();
        foreach (var file in Directory.EnumerateFiles(dotnet.PackageDirectory, id + ".*.nupkg", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase) == symbols))
        {
            using var zip = ZipFile.OpenRead(file);
            using var nuspec = zip.Entries.Single(e => e.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
            var metadata = XDocument.Load(nuspec).Root!.Elements().Single(e => e.Name.LocalName == "metadata");
            if (metadata.Elements().Single(e => e.Name.LocalName == "id").Value != id)
                continue;
            var version = metadata.Elements().Single(e => e.Name.LocalName == "version").Value;
            matches.Add((version, file));
        }
        var kind = symbols ? "symbol" : "product";
        Assert.True(matches.Count == 1,
            $"Expected one {kind} {id} package in {dotnet.PackageDirectory}, found {matches.Count}");
        return matches[0];
    }

    private bool PackageExists(string id, bool symbols) =>
        Directory.EnumerateFiles(dotnet.PackageDirectory, id + ".*.nupkg", SearchOption.AllDirectories)
            .Any(file => file.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase) == symbols);

    private static bool IsRuntimeSymbol(string entry) =>
        entry.StartsWith("runtimes/", StringComparison.Ordinal) &&
        (entry.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) ||
         entry.EndsWith(".dbg", StringComparison.OrdinalIgnoreCase));

    private static void AssertNativeOutput(string project, string family, Dictionary<string, string> expected,
        string targetFramework, string? outputDirectory = null, string[]? restoredFrameworks = null)
    {
        var output = Path.Combine(project, outputDirectory ?? "output");
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal((restoredFrameworks ?? [targetFramework]).Order(),
            assets.RootElement.GetProperty("project").GetProperty("frameworks").EnumerateObject().Select(p => p.Name).Order());
        var depsPath = Path.Combine(output, "Consumer.deps.json");
        Assert.DoesNotContain(".pdb", File.ReadAllText(depsPath), StringComparison.OrdinalIgnoreCase);
        using var runtimeConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "Consumer.runtimeconfig.json")));
        Assert.Equal(targetFramework, runtimeConfig.RootElement.GetProperty("runtimeOptions").GetProperty("tfm").GetString());
        Assert.True(File.Exists(Path.Combine(output, family + ".dll")), $"Missing managed {family} assembly");
        Assert.DoesNotContain(Directory.EnumerateFiles(output, "*.pdb", SearchOption.AllDirectories),
            file => Path.GetFileName(file).Equals($"lib{family}.pdb", StringComparison.OrdinalIgnoreCase));
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
