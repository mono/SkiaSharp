using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

public class NativeAssetOutputTests(DotNet dotnet) : IClassFixture<DotNet>
{
    private static readonly string[] Families = ["SkiaSharp", "HarfBuzzSharp"];
    private static readonly string[] Platforms = ["Win32", "macOS", "Linux"];
    private static readonly string[] TargetFrameworks = ["net10.0", "net11.0"];
    private static readonly string[] DefaultRuntimeIds =
    [
        "win-x86", "win-x64", "win-arm64", "osx-x64", "osx-arm64",
        "linux-x86", "linux-x64", "linux-arm", "linux-arm64", "linux-riscv64", "linux-loongarch64",
        "linux-musl-x64", "linux-musl-arm", "linux-musl-arm64", "linux-musl-riscv64", "linux-musl-loongarch64",
        "linux-bionic-x64", "linux-bionic-arm64"
    ];
    private readonly Dictionary<string, string> packagePaths = new(StringComparer.OrdinalIgnoreCase);

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
    public async Task PortablePackagePreservesItsNativeDependencyPolicy(string family, string command, string targetFramework)
    {
        var core = ReadPackage(family);
        var expected = ReadDefaultNativeFiles(family);
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/win-", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/osx", StringComparison.Ordinal));
        Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/linux-x64/", StringComparison.Ordinal));
        Assert.DoesNotContain(expected.Keys, path => path.StartsWith("runtimes/linux-musl-x86/", StringComparison.Ordinal));

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

    public static IEnumerable<object[]> RidPublishCases() =>
        Families.SelectMany(family =>
            new[] { "win-x64", "win-x86", "win-arm64", "osx-x64", "osx-arm64", "linux-x64", "linux-arm64", "linux-musl-x64", "linux-musl-arm64" }
                .Select(rid => new object[] { family, rid }));

    [Theory]
    [InlineData("SkiaSharp")]
    [InlineData("HarfBuzzSharp")]
    public void RuntimeGraphUsesItsOwnExactVersionPackages(string family)
    {
        var core = ReadPackage(family);
        using var zip = ZipFile.OpenRead(packagePaths[family]);
        using var stream = zip.GetEntry("runtime.json")!.Open();
        using var graph = JsonDocument.Parse(stream);
        var runtimes = graph.RootElement.GetProperty("runtimes");
        foreach (var rid in DefaultRuntimeIds)
        {
            var dependency = Assert.Single(runtimes.GetProperty(rid).GetProperty(family).EnumerateObject());
            Assert.Equal(RuntimePackageId(rid, family: family), dependency.Name);
            Assert.Equal($"[{core.Version}]", dependency.Value.GetString());
        }
        foreach (var rid in new[] { "android", "android-arm", "android-arm64", "android-x86", "android-x64",
            "tizen", "tizen-armel", "tizen-x86", "linux-musl-x86" })
            Assert.Empty(runtimes.GetProperty(rid).GetProperty(family).EnumerateObject());
    }

    [Theory]
    [MemberData(nameof(RidPublishCases))]
    public async Task RidPublishDeploysOnlyTheRequestedNativeAssets(string family, string rid)
    {
        var core = ReadPackage(family);
        var native = ReadPackage(RuntimePackageId(rid, family: family));
        Assert.Equal(core.Version, native.Version);
        var prefix = $"runtimes/{rid}/native/";
        Assert.NotEmpty(native.NativeFiles);
        Assert.All(native.NativeFiles.Keys, path => Assert.StartsWith(prefix, path));
        var expected = native.NativeFiles.ToDictionary(p => p.Key[prefix.Length..], p => p.Value);
        var project = dotnet.NewProject($"{family}-automatic-{rid}",
            ProjectXml(family, core.Version, "net10.0"));

        await dotnet.Publish(project, rid);

        AssertNativeOutput(project, family, expected, "net10.0");
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal(DefaultRuntimeIds.Select(id => RuntimePackageId(id, family: family)).Order(),
            assets.RootElement.GetProperty("libraries").EnumerateObject()
                .Select(p => p.Name.Split('/')[0])
                .Where(IsRuntimePackageId)
                .Order());
        using var deps = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "output", "Consumer.deps.json")));
        var deployedNativeFiles = new List<string>();
        foreach (var target in deps.RootElement.GetProperty("targets").EnumerateObject())
            foreach (var library in target.Value.EnumerateObject())
                if (library.Value.TryGetProperty("native", out var files))
                    deployedNativeFiles.AddRange(files.EnumerateObject()
                        .Select(file => Path.GetFileName(file.Name))
                        .Where(file => file.StartsWith($"lib{family}.", StringComparison.Ordinal)));
        Assert.Equal(expected.Keys.Order(), deployedNativeFiles.Order());
        dotnet.CleanBuildOutput(project);
    }

    public static IEnumerable<object[]> ExcludedRidCases() =>
        Families.SelectMany(family =>
            new[] { "android-arm", "android-arm64", "android-x86", "android-x64", "linux-musl-x86" }
                .Select(rid => new object[] { family, rid }));

    [Theory]
    [MemberData(nameof(ExcludedRidCases))]
    public async Task RuntimeGraphDoesNotAddPackagesForExcludedRids(string family, string rid)
    {
        var core = ReadPackage(family);
        var xml = XElement.Parse(ProjectXml(family, core.Version, "net10.0"));
        xml.Element("PropertyGroup")!.Element("OutputType")!.Value = "Library";
        xml.Element("PropertyGroup")!.Element("UseAppHost")!.Value = "false";
        xml.Element("PropertyGroup")!.Add(new XElement("EnableRuntimePackDownload", "false"));
        var project = dotnet.NewProject($"{family}-excluded-rid-graph-{rid}", xml.ToString());

        await dotnet.Restore(project, rid);

        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal(DefaultRuntimeIds.Select(id => RuntimePackageId(id, family: family)).Order(),
            assets.RootElement.GetProperty("libraries").EnumerateObject()
                .Select(p => p.Name.Split('/')[0])
                .Where(IsRuntimePackageId)
                .Order());
    }

    [Theory]
    [InlineData("SkiaSharp", null)]
    [InlineData("SkiaSharp", "win-x64")]
    [InlineData("SkiaSharp", "linux-x64")]
    [InlineData("HarfBuzzSharp", null)]
    [InlineData("HarfBuzzSharp", "win-x64")]
    [InlineData("HarfBuzzSharp", "linux-x64")]
    public async Task Win32CompatibilityPackagePreservesOrdinaryDependencies(string family, string? rid)
    {
        var core = ReadPackage(family);
        var windows = ReadPackage(family + ".NativeAssets.Win32");
        Assert.Equal(3, windows.NativeFiles.Count);
        var expected = rid is null ? ReadDefaultNativeFiles(family) :
            ReadPackage(RuntimePackageId(rid, family: family)).NativeFiles
                .ToDictionary(p => p.Key[$"runtimes/{rid}/native/".Length..], p => p.Value);
        var xml = XElement.Parse(ProjectXml(family, core.Version, "net10.0"));
        xml.Element("ItemGroup")!.Add(new XElement("PackageReference",
            new XAttribute("Include", family + ".NativeAssets.Win32"),
            new XAttribute("Version", $"[{core.Version}]")));
        var project = dotnet.NewProject($"{family}-win32-compat-{rid ?? "portable"}", xml.ToString());

        await dotnet.Publish(project, rid);

        AssertNativeOutput(project, family, expected, "net10.0");
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal(new[] { "win-arm64", "win-x64", "win-x86" }.Select(id => RuntimePackageId(id, family: family)),
            assets.RootElement.GetProperty("libraries").EnumerateObject()
                .Select(p => p.Name.Split('/')[0])
                .Where(id => id.StartsWith(family + ".NativeAssets.Win32.win-", StringComparison.Ordinal))
                .Order());
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp")]
    [InlineData("HarfBuzzSharp")]
    public async Task CommandLineRidWorksOnFirstBuild(string family)
    {
        var core = ReadPackage(family);
        var rid = RuntimeInformation.RuntimeIdentifier;
        var native = ReadPackage(RuntimePackageId(rid, family: family));
        var prefix = $"runtimes/{rid}/native/";
        var expected = native.NativeFiles.ToDictionary(p => p.Key[prefix.Length..], p => p.Value);
        var project = dotnet.NewProject($"{family}-command-line-rid-first-build",
            ProjectXml(family, core.Version, "net10.0"));
        WriteNativeProgram(project, family);

        await dotnet.Build(project, rid: rid);

        AssertNativeOutput(project, family, expected, "net10.0");
        Assert.Contains("Native round-trip passed.",
            await dotnet.Execute(project, Path.Combine("output", "Consumer.dll")));
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp", "net10.0")]
    [InlineData("SkiaSharp", "net11.0")]
    [InlineData("HarfBuzzSharp", "net10.0")]
    [InlineData("HarfBuzzSharp", "net11.0")]
    public async Task PortableApplicationLoadsNativeAssetsOnFirstBuild(string family, string targetFramework)
    {
        var core = ReadPackage(family);
        var project = dotnet.NewProject($"{family}-portable-first-build-{targetFramework}",
            ProjectXml(family, core.Version, targetFramework));
        WriteNativeProgram(project, family);

        await dotnet.Build(project);

        AssertNativeOutput(project, family, ReadDefaultNativeFiles(family), targetFramework);
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal([targetFramework], assets.RootElement.GetProperty("targets").EnumerateObject().Select(p => p.Name));
        Assert.Contains("Native round-trip passed.",
            await dotnet.Execute(project, Path.Combine("output", "Consumer.dll")));
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("SkiaSharp", "net462", false)]
    [InlineData("SkiaSharp", "net462", true)]
    [InlineData("SkiaSharp", "net48", false)]
    [InlineData("SkiaSharp", "net48", true)]
    [InlineData("HarfBuzzSharp", "net462", false)]
    [InlineData("HarfBuzzSharp", "net462", true)]
    [InlineData("HarfBuzzSharp", "net48", false)]
    [InlineData("HarfBuzzSharp", "net48", true)]
    public async Task FrameworkSdkPreservesArchitectureFolders(string family, string framework, bool optOut)
    {
        var core = ReadPackage(family);
        var windows = ReadPackage(family + ".NativeAssets.Win32");
        var xml = XElement.Parse(ProjectXml(family, core.Version, framework));
        xml.Element("PropertyGroup")!.Add(new XElement("ShouldIncludeNative" + family, !optOut));
        var project = dotnet.NewProject($"{family}-framework-{framework}-{optOut}", xml.ToString());

        await dotnet.Build(project);

        foreach (var arch in new[] { "x86", "x64", "arm64" })
        {
            var file = Path.Combine(project, "output", arch, $"lib{family}.dll");
            if (optOut)
                Assert.False(File.Exists(file));
            else
            {
                Assert.True(File.Exists(file), $"Missing legacy architecture folder: {file}");
                using var stream = File.OpenRead(file);
                Assert.Equal(windows.NativeFiles[$"runtimes/win-{arch}/native/lib{family}.dll"],
                    Convert.ToHexString(SHA256.HashData(stream)));
            }
        }
    }

    [Theory]
    [InlineData("SkiaSharp", "net462", "v4.6.2", false)]
    [InlineData("SkiaSharp", "net462", "v4.6.2", true)]
    [InlineData("SkiaSharp", "net48", "v4.8", false)]
    [InlineData("SkiaSharp", "net48", "v4.8", true)]
    [InlineData("HarfBuzzSharp", "net462", "v4.6.2", false)]
    [InlineData("HarfBuzzSharp", "net462", "v4.6.2", true)]
    [InlineData("HarfBuzzSharp", "net48", "v4.8", false)]
    [InlineData("HarfBuzzSharp", "net48", "v4.8", true)]
    public async Task LegacyPackagesDirectoryImportsCopyArchitectureFolders(string family, string framework, string frameworkVersion,
        bool optOut)
    {
        var windows = ReadPackage(family + ".NativeAssets.Win32");
        var project = CreateLegacyNativeProject(family + ".NativeAssets.Win32", windows.Version,
            framework, frameworkVersion, optOut);

        await dotnet.CopyLegacyNativeAssets(project);

        foreach (var arch in new[] { "x86", "x64", "arm64" })
        {
            var file = Path.Combine(project, "output", arch, $"lib{family}.dll");
            if (optOut)
                Assert.False(File.Exists(file));
            else
            {
                Assert.True(File.Exists(file), $"Legacy import did not supply {arch} assets");
                using var stream = File.OpenRead(file);
                Assert.Equal(windows.NativeFiles[$"runtimes/win-{arch}/native/lib{family}.dll"],
                    Convert.ToHexString(SHA256.HashData(stream)));
            }
        }
    }

    [Theory]
    [InlineData("SkiaSharp", "net462", "v4.6.2", false)]
    [InlineData("SkiaSharp", "net462", "v4.6.2", true)]
    [InlineData("SkiaSharp", "net48", "v4.8", false)]
    [InlineData("SkiaSharp", "net48", "v4.8", true)]
    [InlineData("HarfBuzzSharp", "net462", "v4.6.2", false)]
    [InlineData("HarfBuzzSharp", "net462", "v4.6.2", true)]
    [InlineData("HarfBuzzSharp", "net48", "v4.8", false)]
    [InlineData("HarfBuzzSharp", "net48", "v4.8", true)]
    public async Task LegacyMacOSImportsPreserveUniversalRootCopy(string family, string framework, string frameworkVersion,
        bool optOut)
    {
        var mac = ReadPackage(family + ".NativeAssets.macOS");
        var project = CreateLegacyNativeProject(family + ".NativeAssets.macOS", mac.Version,
            framework, frameworkVersion, optOut);

        await dotnet.CopyLegacyNativeAssets(project);

        foreach (var folder in new[] { "", "x64", "arm64" })
        {
            var file = Path.Combine(project, "output", folder, $"lib{family}.dylib");
            if (optOut)
                Assert.False(File.Exists(file));
            else
            {
                Assert.True(File.Exists(file), $"Missing macOS compatibility copy: {file}");
                using var stream = File.OpenRead(file);
                Assert.Equal(mac.NativeFiles[$"runtimes/osx/native/lib{family}.dylib"],
                    Convert.ToHexString(SHA256.HashData(stream)));
            }
        }
    }

    [Theory]
    [InlineData("net462", false)]
    [InlineData("net48", true)]
    public async Task FrameworkNoDependenciesReplacesRegularArchitectureCopies(string framework, bool reverseOrder)
    {
        var core = ReadPackage("SkiaSharp");
        var nodeps = ReadPackage("SkiaSharp.NativeAssets.Linux.NoDependencies");
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, framework, includeLinux: true));
        var reference = new XElement("PackageReference",
            new XAttribute("Include", "SkiaSharp.NativeAssets.Linux.NoDependencies"),
            new XAttribute("Version", $"[{core.Version}]"));
        if (reverseOrder)
            xml.Element("ItemGroup")!.AddFirst(reference);
        else
            xml.Element("ItemGroup")!.Add(reference);
        var project = dotnet.NewProject($"SkiaSharp-framework-nodeps-{framework}-{reverseOrder}", xml.ToString());

        await dotnet.Build(project);

        foreach (var (path, hash) in nodeps.NativeFiles)
        {
            var folder = path.Split('/')[1]["linux-".Length..];
            var file = Path.Combine(project, "output", folder, "libSkiaSharp.so");
            Assert.True(File.Exists(file), $"Missing Framework NoDependencies copy: {file}");
            using var stream = File.OpenRead(file);
            Assert.Equal(hash, Convert.ToHexString(SHA256.HashData(stream)));
        }
    }

    [Theory]
    [InlineData("linux-x64", false, false)]
    [InlineData("linux-x64", true, false)]
    [InlineData("linux-musl-x64", false, false)]
    [InlineData("linux-musl-x64", true, false)]
    [InlineData("linux-x64", false, true)]
    [InlineData("linux-x64", true, true)]
    [InlineData("linux-musl-x64", false, true)]
    [InlineData("linux-musl-x64", true, true)]
    public async Task NoDependenciesWinsRegardlessOfReferenceOrder(string rid, bool reverseOrder, bool direct)
    {
        var core = ReadPackage("SkiaSharp");
        var regular = ReadPackage(RuntimePackageId(rid));
        var nodeps = ReadPackage(RuntimePackageId(rid, noDependencies: true));
        var prefix = $"runtimes/{rid}/native/";
        var expected = nodeps.NativeFiles.ToDictionary(p => p.Key[prefix.Length..], p => p.Value);
        Assert.NotEmpty(expected);
        Assert.NotEqual(regular.NativeFiles[prefix + "libSkiaSharp.so"], expected["libSkiaSharp.so"]);
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, "net10.0"));
        var references = xml.Element("ItemGroup")!;
        var reference = new XElement("PackageReference",
            new XAttribute("Include", direct ? RuntimePackageId(rid, noDependencies: true) :
                "SkiaSharp.NativeAssets.Linux.NoDependencies"),
            new XAttribute("Version", $"[{core.Version}]"));
        if (reverseOrder)
            references.AddFirst(reference);
        else
            references.Add(reference);
        var project = dotnet.NewProject($"SkiaSharp-nodeps-{rid}-{reverseOrder}-{direct}", xml.ToString());

        await dotnet.Publish(project, rid);

        AssertNativeOutput(project, "SkiaSharp", expected, "net10.0");
        using var deps = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "output", "Consumer.deps.json")));
        foreach (var target in deps.RootElement.GetProperty("targets").EnumerateObject())
        {
            if (target.Value.TryGetProperty($"{RuntimePackageId(rid)}/{core.Version}", out var library))
            {
                Assert.False(library.TryGetProperty("native", out var native) &&
                    native.EnumerateObject().Any());
            }
        }
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PortableNoDependenciesReplacesExplicitRegularLinuxAssets(bool reverseOrder)
    {
        var core = ReadPackage("SkiaSharp");
        var expected = ReadPackage("SkiaSharp.NativeAssets.Linux.NoDependencies").NativeFiles
            .Concat(ReadDefaultNativeFiles("SkiaSharp").Where(p => !p.Key.StartsWith("runtimes/linux-", StringComparison.Ordinal)))
            .ToDictionary(p => p.Key, p => p.Value);
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, "net10.0", includeLinux: true));
        var references = xml.Element("ItemGroup")!;
        var reference = new XElement("PackageReference",
            new XAttribute("Include", "SkiaSharp.NativeAssets.Linux.NoDependencies"),
            new XAttribute("Version", $"[{core.Version}]"));
        if (reverseOrder)
            references.AddFirst(reference);
        else
            references.Add(reference);
        var project = dotnet.NewProject($"SkiaSharp-portable-nodeps-{reverseOrder}", xml.ToString());

        await dotnet.Publish(project);

        AssertNativeOutput(project, "SkiaSharp", expected, "net10.0");
        using var deps = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "output", "Consumer.deps.json")));
        foreach (var target in deps.RootElement.GetProperty("targets").EnumerateObject())
            foreach (var library in target.Value.EnumerateObject().Where(p =>
                p.Name.StartsWith("SkiaSharp.NativeAssets.Linux.linux-", StringComparison.Ordinal)))
                Assert.False(library.Value.TryGetProperty("runtimeTargets", out var runtimeTargets) &&
                    runtimeTargets.EnumerateObject().Any());
        dotnet.CleanBuildOutput(project);
    }

    [Fact]
    public async Task SingleRidNoDependenciesReferencePreservesOtherDefaultRids()
    {
        var core = ReadPackage("SkiaSharp");
        var regular = ReadPackage("SkiaSharp.NativeAssets.Linux.linux-musl-arm64");
        var prefix = "runtimes/linux-musl-arm64/native/";
        var expected = regular.NativeFiles.ToDictionary(p => p.Key[prefix.Length..], p => p.Value);
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, "net10.0"));
        xml.Element("ItemGroup")!.Add(new XElement("PackageReference",
            new XAttribute("Include", "SkiaSharp.NativeAssets.Linux.NoDependencies.linux-musl-x64"),
            new XAttribute("Version", $"[{core.Version}]")));
        var project = dotnet.NewProject("SkiaSharp-nodeps-preserves-other-rid", xml.ToString());
        File.WriteAllText(Path.Combine(project, "readme.txt"), "Unrelated application content.");
        xml.Add(new XElement("ItemGroup", new XElement("Content", new XAttribute("Include", "readme.txt"),
            new XAttribute("CopyToOutputDirectory", "PreserveNewest"))));
        File.WriteAllText(Path.Combine(project, "Consumer.csproj"), xml.ToString());

        await dotnet.Publish(project, "linux-musl-arm64");

        AssertNativeOutput(project, "SkiaSharp", expected, "net10.0");
        Assert.True(File.Exists(Path.Combine(project, "output", "readme.txt")));
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("linux-x64")]
    [InlineData("linux-musl-x64")]
    public async Task NoDependenciesPreservesHarfBuzzNativeAssets(string rid)
    {
        var skia = ReadPackage("SkiaSharp");
        var harfbuzz = ReadPackage("HarfBuzzSharp");
        var expectedSkia = ReadPackage(RuntimePackageId(rid, noDependencies: true)).NativeFiles;
        var expectedHarfBuzz = ReadPackage(RuntimePackageId(rid, family: "HarfBuzzSharp")).NativeFiles;
        var xml = XElement.Parse(ProjectXml("SkiaSharp", skia.Version, "net10.0"));
        xml.Element("ItemGroup")!.Add(
            new XElement("PackageReference", new XAttribute("Include", "HarfBuzzSharp"),
                new XAttribute("Version", $"[{harfbuzz.Version}]")),
            new XElement("PackageReference", new XAttribute("Include", RuntimePackageId(rid, noDependencies: true)),
                new XAttribute("Version", $"[{skia.Version}]")));
        var project = dotnet.NewProject($"both-libraries-nodeps-{rid}", xml.ToString());

        await dotnet.Publish(project, rid);

        var prefix = $"runtimes/{rid}/native/";
        AssertNativeOutput(project, "SkiaSharp", expectedSkia.ToDictionary(p => p.Key[prefix.Length..], p => p.Value), "net10.0");
        AssertNativeOutput(project, "HarfBuzzSharp", expectedHarfBuzz.ToDictionary(p => p.Key[prefix.Length..], p => p.Value), "net10.0");
        dotnet.CleanBuildOutput(project);
    }

    private Task BuildOrPublish(string project, string command, string? rid = null) =>
        command == "build" ? dotnet.Build(project) : dotnet.Publish(project, rid);

    private static string RuntimePackageId(string rid, bool noDependencies = false, string family = "SkiaSharp")
    {
        var platform = rid.Split('-')[0] switch
        {
            "win" => "Win32",
            "osx" => "macOS",
            "linux" => "Linux",
            _ => throw new ArgumentException($"Unsupported test RID: {rid}", nameof(rid))
        };
        return $"{family}.NativeAssets.{platform}{(noDependencies ? ".NoDependencies" : "")}.{rid}";
    }

    private static bool IsRuntimePackageId(string id) =>
        Regex.IsMatch(id, "^(SkiaSharp|HarfBuzzSharp)[.]NativeAssets[.](Win32[.]win-|macOS[.]osx-|Linux[.](NoDependencies[.])?linux-)");

    private Dictionary<string, string> ReadDefaultNativeFiles(string family) =>
        Platforms.SelectMany(platform => ReadPackage(family + ".NativeAssets." + platform).NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);

    private static void WriteNativeProgram(string project, string family) =>
        File.WriteAllText(Path.Combine(project, "Program.cs"), family == "SkiaSharp" ? """
            using SkiaSharp;
            using var bitmap = new SKBitmap(8, 8);
            bitmap.Erase(SKColors.Magenta);
            if (bitmap.GetPixel(3, 3) != SKColors.Magenta)
                throw new System.Exception("Native pixel round-trip failed.");
            System.Console.WriteLine("Native round-trip passed.");
            """ : """
            using var buffer = new HarfBuzzSharp.Buffer();
            buffer.AddUtf8("abc");
            if (buffer.Length != 3)
                throw new System.Exception("Native text round-trip failed.");
            System.Console.WriteLine("Native round-trip passed.");
            """);

    private string CreateLegacyNativeProject(string packageId, string version, string framework,
        string frameworkVersion, bool optOut)
    {
        var family = packageId.Split('.')[0];
        var project = dotnet.NewProject($"{packageId}-packages-config-imports-{framework}-{optOut}", "<Project />");
        var packages = Path.Combine(project, "packages");
        foreach (var (id, file) in packagePaths)
            ZipFile.ExtractToDirectory(file, Path.Combine(packages, id + "." + version));
        var import = Path.Combine(packages, $"{packageId}.{version}", "build", framework, packageId + ".targets");
        Assert.True(File.Exists(import), $"Missing packages.config build import: {import}");
        var xml = new XElement("Project",
            new XElement("PropertyGroup",
                new XElement("TargetFramework", framework),
                new XElement("TargetFrameworkIdentifier", ".NETFramework"),
                new XElement("TargetFrameworkVersion", frameworkVersion),
                new XElement("ShouldIncludeNative" + family, !optOut),
                new XElement("OutputPath", Path.Combine(project, "output") + Path.DirectorySeparatorChar)),
            new XElement("Import", new XAttribute("Project", import)),
            new XElement("Target", new XAttribute("Name", "CopyNativeFiles"),
                new XElement("ItemGroup",
                    new XElement("_NativeCopy", new XAttribute("Include", "@(None);@(Content)"),
                        new XAttribute("Condition", $"'%(Filename)' == 'lib{family}'"))),
                new XElement("Copy", new XAttribute("SourceFiles", "@(_NativeCopy)"),
                    new XAttribute("DestinationFiles", "@(_NativeCopy->'$(OutputPath)%(Link)')"))));
        File.WriteAllText(Path.Combine(project, "Consumer.csproj"), xml.ToString());
        return project;
    }

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
        var matches = new List<(string Version, Dictionary<string, string> NativeFiles)>();
        var dependencyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.EnumerateFiles(dotnet.PackageDirectory, id + ".*.nupkg", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase)))
        {
            using var zip = ZipFile.OpenRead(file);
            using var nuspec = zip.Entries.Single(e => e.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
            var metadata = XDocument.Load(nuspec).Root!.Elements().Single(e => e.Name.LocalName == "metadata");
            if (metadata.Elements().Single(e => e.Name.LocalName == "id").Value != id)
                continue;
            packagePaths[id] = file;
            var version = metadata.Elements().Single(e => e.Name.LocalName == "version").Value;
            foreach (var dependency in metadata.Descendants().Where(e => e.Name.LocalName == "dependency"))
            {
                var dependencyId = dependency.Attribute("id")!.Value;
                if (IsRuntimePackageId(dependencyId))
                    dependencyIds.Add(dependencyId);
            }
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
        var package = matches[0];
        foreach (var dependencyId in dependencyIds)
        {
            var dependency = ReadPackage(dependencyId);
            Assert.Equal(package.Version, dependency.Version);
            foreach (var (path, hash) in dependency.NativeFiles)
                package.NativeFiles.Add(path, hash);
        }
        return package;
    }

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
