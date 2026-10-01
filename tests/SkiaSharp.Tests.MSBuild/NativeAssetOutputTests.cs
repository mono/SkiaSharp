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
    public async Task DefaultPackageIncludesWindowsMacAndLinux(string family, string command)
    {
        var core = ReadPackage(family);
        var expected = ReadPackage(family + ".NativeAssets.Win32").NativeFiles
            .Concat(ReadPackage(family + ".NativeAssets.macOS").NativeFiles)
            .Concat(ReadPackage(family + ".NativeAssets.Linux").NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/win-", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/osx", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/linux-x64/", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/linux-musl-x64/", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/linux-bionic-x64/", StringComparison.Ordinal));

        var project = dotnet.NewProject($"{family}-default-{command}", ProjectXml(family, core.Version));
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, family, expected);
        Assert.Empty(Directory.EnumerateFiles(Path.Combine(project, "output"), "*.a", SearchOption.AllDirectories));
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp")]
    [InlineData("HarfBuzzSharp")]
    public void WebAssemblyPackageIsDefaultOnlyForNonPlatformFrameworks(string family)
    {
        var core = ReadPackage(family);
        using var zip = ZipFile.OpenRead(Path.Combine(dotnet.PackageDirectory, $"{family}.{core.Version}.nupkg"));
        using var nuspec = zip.Entries.Single(e => e.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
        var dependencies = XDocument.Load(nuspec).Descendants().Where(e => e.Name.LocalName == "group")
            .ToDictionary(group => group.Attribute("targetFramework")!.Value,
                group => group.Elements().Select(dependency => dependency.Attribute("id")!.Value).ToHashSet());

        foreach (var framework in new[] { "net10.0", ".NETFramework4.6.2", ".NETStandard2.0" })
            Assert.Contains(family + ".NativeAssets.WebAssembly", dependencies[framework]);
        var android = dependencies.Single(group => group.Key.StartsWith("net10.0-android", StringComparison.Ordinal));
        Assert.DoesNotContain(family + ".NativeAssets.WebAssembly", android.Value);
    }

    [Theory]
    [MemberData(nameof(RidCases))]
    public async Task DefaultLinuxPackageHonorsRuntimeIdentifiers(string family, string setting, string command, string? commandRid)
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
            ProjectXml(family, core.Version, setting));
        await BuildOrPublish(project, command, commandRid);
        AssertNativeOutput(project, family, expected);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp", "build")]
    [InlineData("HarfBuzzSharp", "publish")]
    public async Task ExplicitLinuxReferenceDoesNotDuplicateNativeOutput(string family, string command)
    {
        var core = ReadPackage(family);
        var expected = Platforms.SelectMany(platform => ReadPackage(family + ".NativeAssets." + platform).NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        var project = dotnet.NewProject($"{family}-explicit-linux-{command}",
            ProjectXml(family, core.Version, includeLinux: true));
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, family, expected);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp", "build", "")]
    [InlineData("SkiaSharp", "build", "   ")]
    [InlineData("HarfBuzzSharp", "publish", "")]
    [InlineData("HarfBuzzSharp", "publish", "   ")]
    public async Task BlankNativeFilterKeepsAllAssetsWithPluralRuntimeIdentifiers(string family, string command, string filter)
    {
        var core = ReadPackage(family);
        var expected = Platforms.SelectMany(platform => ReadPackage(family + ".NativeAssets." + platform).NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        var project = dotnet.NewProject($"{family}-blank-filter-{command}-{(filter.Length == 0 ? "empty" : "whitespace")}",
            ProjectXml(family, core.Version, "multiple", filter: filter));
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, family, expected);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp", "none", "build")]
    [InlineData("SkiaSharp", "multiple", "publish")]
    [InlineData("HarfBuzzSharp", "none", "publish")]
    [InlineData("HarfBuzzSharp", "multiple", "build")]
    public async Task FilterSelectsNativeRidsIndependentlyOfPluralRuntimeIdentifiers(string family, string setting, string command)
    {
        var core = ReadPackage(family);
        var natives = Platforms.Select(platform => ReadPackage(family + ".NativeAssets." + platform)).ToArray();
        var all = natives.SelectMany(package => package.NativeFiles).ToDictionary(p => p.Key, p => p.Value);
        const string filter = "linux-x64;linux-musl-arm64;linux-bionic-x64;osx-x64";
        var expected = all.Where(p => p.Key.StartsWith("runtimes/linux-x64/native/", StringComparison.Ordinal) ||
            p.Key.StartsWith("runtimes/linux-musl-arm64/native/", StringComparison.Ordinal) ||
            p.Key.StartsWith("runtimes/linux-bionic-x64/native/", StringComparison.Ordinal) ||
            p.Key.StartsWith("runtimes/osx/native/", StringComparison.Ordinal))
            .ToDictionary(p => p.Key, p => p.Value);
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/osx/native/", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/linux-bionic-x64/native/", StringComparison.Ordinal));

        var project = dotnet.NewProject($"{family}-filter-{setting}-{command}",
            ProjectXml(family, core.Version, setting, filter: filter));
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, family, expected);
        AssertDependencyNativeAssets(project, family + ".NativeAssets.", expected.Keys);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp", "build")]
    [InlineData("SkiaSharp", "publish")]
    [InlineData("HarfBuzzSharp", "build")]
    [InlineData("HarfBuzzSharp", "publish")]
    public async Task SingularRidIgnoresDifferentNativeFilter(string family, string command)
    {
        var core = ReadPackage(family);
        var expected = ReadPackage(family + ".NativeAssets.Linux").NativeFiles
            .Where(p => p.Key.StartsWith("runtimes/linux-x64/native/", StringComparison.Ordinal))
            .ToDictionary(p => p.Key["runtimes/linux-x64/native/".Length..], p => p.Value);
        var project = dotnet.NewProject($"{family}-singular-filter-{command}",
            ProjectXml(family, core.Version, "multiple", filter: "osx-x64"));
        await BuildOrPublish(project, command, "linux-x64");
        AssertNativeOutput(project, family, expected);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp", "build")]
    [InlineData("HarfBuzzSharp", "publish")]
    public async Task FilterDoesNotRemoveOtherFamilyNativeOrManagedAssets(string family, string command)
    {
        var other = family == "SkiaSharp" ? "HarfBuzzSharp" : "SkiaSharp";
        var version = ReadPackage(family).Version;
        var otherVersion = ReadPackage(other).Version;
        var expected = ReadPackage(family + ".NativeAssets.Linux").NativeFiles
            .Where(p => p.Key.StartsWith("runtimes/linux-x64/native/", StringComparison.Ordinal))
            .ToDictionary(p => p.Key, p => p.Value);
        var otherExpected = Platforms.SelectMany(platform => ReadPackage(other + ".NativeAssets." + platform).NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        var project = dotnet.NewProject($"{family}-isolated-{command}",
            ProjectXml(family, version, filter: "linux-x64", otherFamily: other, otherVersion: otherVersion));
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, family, expected);
        AssertNativeOutput(project, other, otherExpected);
        AssertDependencyNativeAssets(project, family + ".NativeAssets.", expected.Keys);
        AssertDependencyNativeAssets(project, other + ".NativeAssets.", otherExpected.Keys);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("build", false)]
    [InlineData("publish", false)]
    [InlineData("publish", true)]
    public async Task NoDependenciesWinsForSameRidEvenWithDefaultLinux(string command, bool transitive)
    {
        var core = ReadPackage("SkiaSharp");
        var regular = ReadPackage("SkiaSharp.NativeAssets.Linux").NativeFiles;
        var noDeps = ReadPackage("SkiaSharp.NativeAssets.Linux.NoDependencies").NativeFiles;
        Assert.Contains(noDeps.Keys.Intersect(regular.Keys), p => p.StartsWith("runtimes/linux-x64/", StringComparison.Ordinal));
        var selected = regular.Concat(noDeps).GroupBy(p => p.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);
        var all = ReadPackage("SkiaSharp.NativeAssets.Win32").NativeFiles
            .Concat(ReadPackage("SkiaSharp.NativeAssets.macOS").NativeFiles)
            .Concat(selected).ToDictionary(p => p.Key, p => p.Value);

        var project = dotnet.NewProject($"SkiaSharp-nodeps-{command}-{transitive}",
            ProjectXml("SkiaSharp", core.Version, includeNoDeps: !transitive));
        if (transitive)
        {
            var library = dotnet.NewProject($"SkiaSharp-nodeps-lib-{command}",
                ProjectXml("SkiaSharp.NativeAssets.Linux.NoDependencies", core.Version, library: true));
            var libraryProject = Path.Combine(library, "NodepsLibrary.csproj");
            File.Move(Path.Combine(library, "Consumer.csproj"), libraryProject);
            File.WriteAllText(Path.Combine(library, "Program.cs"), "public class ConsumerLibrary { }");
            var xml = XDocument.Load(Path.Combine(project, "Consumer.csproj"));
            xml.Root!.Add(new XElement("ItemGroup", new XElement("ProjectReference",
                new XAttribute("Include", Path.GetRelativePath(project, libraryProject)))));
            xml.Save(Path.Combine(project, "Consumer.csproj"));
        }
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, "SkiaSharp", all);
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.Linux.NoDependencies",
            noDeps.Keys.Where(selected.ContainsKey));
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.Linux",
            regular.Keys.Except(noDeps.Keys));
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("build")]
    [InlineData("publish")]
    public async Task NoDependenciesWinsForSingularRid(string command)
    {
        var core = ReadPackage("SkiaSharp");
        var noDeps = ReadPackage("SkiaSharp.NativeAssets.Linux.NoDependencies").NativeFiles;
        var expected = noDeps.Where(p => p.Key.StartsWith("runtimes/linux-musl-x64/native/", StringComparison.Ordinal))
            .ToDictionary(p => p.Key["runtimes/linux-musl-x64/native/".Length..], p => p.Value);
        Assert.NotEmpty(expected);
        var project = dotnet.NewProject($"SkiaSharp-nodeps-singular-{command}",
            ProjectXml("SkiaSharp", core.Version, includeNoDeps: true, filter: "osx-x64"));
        await BuildOrPublish(project, command, "linux-musl-x64");
        AssertNativeOutput(project, "SkiaSharp", expected);
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.Linux.NoDependencies",
            noDeps.Keys.Where(p => p.StartsWith("runtimes/linux-musl-x64/native/", StringComparison.Ordinal)));
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("build")]
    [InlineData("publish")]
    public async Task NoDependenciesAndNativeFilterAreIndependent(string command)
    {
        var core = ReadPackage("SkiaSharp");
        var noDeps = ReadPackage("SkiaSharp.NativeAssets.Linux.NoDependencies").NativeFiles;
        var expected = noDeps.Where(p => p.Key.StartsWith("runtimes/linux-musl-x64/native/", StringComparison.Ordinal))
            .ToDictionary(p => p.Key, p => p.Value);
        Assert.NotEmpty(expected);
        var project = dotnet.NewProject($"SkiaSharp-nodeps-filter-{command}",
            ProjectXml("SkiaSharp", core.Version, includeNoDeps: true, filter: "linux-musl-x64"));
        await BuildOrPublish(project, command);
        AssertNativeOutput(project, "SkiaSharp", expected);
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.Linux.NoDependencies", expected.Keys);
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.Linux", []);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("build")]
    [InlineData("publish")]
    public async Task ExcludingNoDependenciesNativeAssetsKeepsDefaultLinux(string command)
    {
        var core = ReadPackage("SkiaSharp");
        var expected = Platforms.SelectMany(platform => ReadPackage("SkiaSharp.NativeAssets." + platform).NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        var project = dotnet.NewProject($"SkiaSharp-nodeps-excluded-{command}",
            ProjectXml("SkiaSharp", core.Version, includeNoDeps: true));
        var xml = XDocument.Load(Path.Combine(project, "Consumer.csproj"));
        xml.Descendants("PackageReference")
            .Single(reference => reference.Attribute("Include")?.Value == "SkiaSharp.NativeAssets.Linux.NoDependencies")
            .SetAttributeValue("ExcludeAssets", "native");
        xml.Save(Path.Combine(project, "Consumer.csproj"));

        await BuildOrPublish(project, command);
        AssertNativeOutput(project, "SkiaSharp", expected);
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.Linux", ReadPackage("SkiaSharp.NativeAssets.Linux").NativeFiles.Keys);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("build")]
    [InlineData("publish")]
    public async Task ChangingFilterOnExistingProjectUpdatesNativeSelection(string command)
    {
        var core = ReadPackage("SkiaSharp");
        var project = dotnet.NewProject($"SkiaSharp-incremental-{command}",
            ProjectXml("SkiaSharp", core.Version));
        await BuildOrPublish(project, command, filterProperty: "linux-x64");
        var linux = ReadPackage("SkiaSharp.NativeAssets.Linux").NativeFiles
            .Where(p => p.Key.StartsWith("runtimes/linux-x64/native/", StringComparison.Ordinal))
            .ToDictionary(p => p.Key, p => p.Value);
        AssertNativeOutput(project, "SkiaSharp", linux);
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.", linux.Keys);

        await BuildOrPublish(project, command, filterProperty: "osx-x64");
        var osx = ReadPackage("SkiaSharp.NativeAssets.macOS").NativeFiles;
        AssertNativeOutput(project, "SkiaSharp", osx);
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.", osx.Keys);

        await BuildOrPublish(project, command, filterProperty: "");
        var all = Platforms.SelectMany(platform => ReadPackage("SkiaSharp.NativeAssets." + platform).NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        AssertNativeOutput(project, "SkiaSharp", all);
        AssertDependencyNativeAssets(project, "SkiaSharp.NativeAssets.", all.Keys);
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp")]
    [InlineData("HarfBuzzSharp")]
    public async Task LegacyFrameworkCopiesDefaultLinuxAssets(string family)
    {
        var core = ReadPackage(family);
        var linux = ReadPackage(family + ".NativeAssets.Linux").NativeFiles;
        var project = dotnet.NewProject($"{family}-legacy-default", ProjectXml(family, core.Version, targetFramework: "net462"),
            legacy: true);
        await dotnet.Build(project);
        AssertLegacyNativeOutput(project, family, linux, "linux-x64", "x64");
        AssertLegacyNativeOutput(project, family, linux, "linux-musl-x64", "musl-x64");
        AssertLegacyNativeOutput(project, family, linux, "linux-bionic-x64", "bionic-x64");
    }

    [Fact]
    public async Task LegacyFrameworkPrefersNoDependenciesOverDefaultLinux()
    {
        var core = ReadPackage("SkiaSharp");
        var nodeps = ReadPackage("SkiaSharp.NativeAssets.Linux.NoDependencies").NativeFiles;
        var project = dotnet.NewProject("SkiaSharp-legacy-nodeps",
            ProjectXml("SkiaSharp", core.Version, includeNoDeps: true, targetFramework: "net462"), legacy: true);
        await dotnet.Build(project);
        AssertLegacyNativeOutput(project, "SkiaSharp", nodeps, "linux-x64", "x64");
        AssertLegacyNativeOutput(project, "SkiaSharp", nodeps, "linux-musl-x64", "musl-x64");
        AssertLegacyNativeOutput(project, "SkiaSharp", nodeps, "linux-bionic-x64", "bionic-x64");
    }

    private static void AssertLegacyNativeOutput(string project, string family, Dictionary<string, string> expected,
        string rid, string outputDirectory)
    {
        var asset = $"runtimes/{rid}/native/lib{family}.so";
        Assert.True(expected.TryGetValue(asset, out var hash), $"Missing {asset} in source package");
        var path = Path.Combine(project, "output", outputDirectory, $"lib{family}.so");
        Assert.True(File.Exists(path), $"Missing legacy native output {path}");
        using var stream = File.OpenRead(path);
        Assert.Equal(hash, Convert.ToHexString(SHA256.HashData(stream)));
    }

    private Task BuildOrPublish(string project, string command, string? rid = null, string? filterProperty = null) =>
        command == "build" ? dotnet.Build(project, rid, filterProperty) : dotnet.Publish(project, rid, filterProperty);

    private static string ProjectXml(string family, string version, string setting = "none",
        bool includeLinux = false, bool includeNoDeps = false, string? filter = null,
        string? otherFamily = null, string? otherVersion = null, bool library = false, string targetFramework = "net10.0")
    {
        var properties = new XElement("PropertyGroup",
            new XElement("TargetFramework", targetFramework),
            new XElement("OutputType", library ? "Library" : "Exe"),
            new XElement("SelfContained", "false"),
            new XElement("UseAppHost", library ? "false" : "true"));
        if (setting == "single")
            properties.Add(new XElement("RuntimeIdentifier", "linux-arm64"));
        if (setting == "multiple")
            properties.Add(new XElement("RuntimeIdentifiers", "linux-x64;linux-arm64"));
        if (filter is not null)
            properties.Add(new XElement(family + "FilterRuntimeIdentifiers", filter));
        var packages = new XElement("ItemGroup",
            new XElement("PackageReference", new XAttribute("Include", family), new XAttribute("Version", $"[{version}]")));
        if (includeLinux)
            packages.Add(new XElement("PackageReference", new XAttribute("Include", family + ".NativeAssets.Linux"),
                new XAttribute("Version", $"[{version}]")));
        if (includeNoDeps)
            packages.Add(new XElement("PackageReference", new XAttribute("Include", family + ".NativeAssets.Linux.NoDependencies"),
                new XAttribute("Version", $"[{version}]")));
        if (otherFamily is not null)
            packages.Add(new XElement("PackageReference", new XAttribute("Include", otherFamily),
                new XAttribute("Version", $"[{otherVersion}]")));
        return new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"), properties, packages).ToString();
    }

    private static void AssertDependencyNativeAssets(string project, string packagePrefix, IEnumerable<string> expected)
    {
        using var deps = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "output", "Consumer.deps.json")));
        var actual = deps.RootElement.GetProperty("targets").EnumerateObject()
            .SelectMany(target => target.Value.EnumerateObject())
            .Where(package => package.Name.StartsWith(
                packagePrefix.EndsWith('.') ? packagePrefix : packagePrefix + "/",
                StringComparison.OrdinalIgnoreCase))
            .SelectMany(package =>
            {
                var runtimeTargets = package.Value.TryGetProperty("runtimeTargets", out var assets)
                    ? assets.EnumerateObject().Where(asset => asset.Value.GetProperty("assetType").GetString() == "native")
                        .Select(asset => asset.Name).Where(IsNativeBinary)
                    : Enumerable.Empty<string>();
                var native = package.Value.TryGetProperty("native", out var nativeAssets)
                    ? nativeAssets.EnumerateObject().Select(asset => asset.Name).Where(IsNativeBinary)
                    : Enumerable.Empty<string>();
                return runtimeTargets.Concat(native);
            })
            .Distinct().Order().ToArray();
        Assert.Equal(expected.Distinct().Order(), actual);
    }

    private static bool IsNativeBinary(string path) =>
        path.EndsWith(".dll", StringComparison.Ordinal) ||
        path.EndsWith(".so", StringComparison.Ordinal) ||
        path.EndsWith(".dylib", StringComparison.Ordinal);

    private (string Version, Dictionary<string, string> NativeFiles) ReadPackage(string id)
    {
        var matches = new List<(string Version, Dictionary<string, string> NativeFiles)>();
        foreach (var file in Directory.EnumerateFiles(dotnet.PackageDirectory, id + ".*.nupkg", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase)))
        {
            using var zip = ZipFile.OpenRead(file);
            using var nuspec = zip.Entries.Single(e => e.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
            var metadata = XDocument.Load(nuspec).Root!.Elements().Single(e => e.Name.LocalName == "metadata");
            if (metadata.Elements().Single(e => e.Name.LocalName == "id").Value != id)
                continue;
            var version = metadata.Elements().Single(e => e.Name.LocalName == "version").Value;
            var native = new Dictionary<string, string>();
            foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith("runtimes/", StringComparison.Ordinal) &&
                e.FullName.Contains("/native/", StringComparison.Ordinal) &&
                (e.FullName.EndsWith(".dll", StringComparison.Ordinal) ||
                 e.FullName.EndsWith(".so", StringComparison.Ordinal) ||
                 e.FullName.EndsWith(".dylib", StringComparison.Ordinal))))
            {
                using var stream = entry.Open();
                native.Add(entry.FullName, Convert.ToHexString(SHA256.HashData(stream)));
            }
            matches.Add((version, native));
        }
        Assert.True(matches.Count == 1, $"Expected one real {id} package in {dotnet.PackageDirectory}, found {matches.Count}");
        return matches[0];
    }

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
