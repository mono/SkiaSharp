using System.IO.Compression;
using System.Runtime.InteropServices;
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
        var expected = family == "SkiaSharp" ? [] : ReadPackage(family + ".NativeAssets.Win32").NativeFiles
            .Concat(ReadPackage(family + ".NativeAssets.macOS").NativeFiles)
            .ToDictionary(p => p.Key, p => p.Value);
        if (family == "HarfBuzzSharp")
        {
            Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/win-", StringComparison.Ordinal));
            Assert.Contains(expected.Keys, path => path.StartsWith("runtimes/osx", StringComparison.Ordinal));
        }
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
        var platforms = family == "SkiaSharp" ? ["Linux"] : Platforms;
        var natives = platforms.Select(platform => ReadPackage(family + ".NativeAssets." + platform)).ToArray();
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
        var platforms = family == "SkiaSharp" ? ["Linux"] : Platforms;
        var natives = platforms.Select(platform => ReadPackage(family + ".NativeAssets." + platform)).ToArray();
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

    [Theory]
    [InlineData("win-x64")]
    [InlineData("win-x86")]
    [InlineData("win-arm64")]
    [InlineData("osx-x64")]
    [InlineData("osx-arm64")]
    [InlineData("linux-x64")]
    [InlineData("linux-arm64")]
    [InlineData("linux-musl-x64")]
    [InlineData("linux-musl-arm64")]
    public async Task RuntimeGraphSelectsOnlyTheRequestedNativePackage(string rid)
    {
        var core = ReadPackage("SkiaSharp");
        var native = ReadPackage("SkiaSharp.runtime." + rid);
        Assert.Equal(core.Version, native.Version);
        var prefix = $"runtimes/{rid}/native/";
        Assert.NotEmpty(native.NativeFiles);
        Assert.All(native.NativeFiles.Keys, path => Assert.StartsWith(prefix, path));
        var expected = native.NativeFiles.ToDictionary(p => p.Key[prefix.Length..], p => p.Value);
        var project = dotnet.NewProject($"SkiaSharp-automatic-{rid}",
            ProjectXml("SkiaSharp", core.Version, "net10.0"));

        await dotnet.Publish(project, rid);

        AssertNativeOutput(project, "SkiaSharp", expected, "net10.0");
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal(["SkiaSharp.runtime." + rid],
            assets.RootElement.GetProperty("libraries").EnumerateObject()
                .Select(p => p.Name.Split('/')[0])
                .Where(id => id.StartsWith("SkiaSharp.runtime.", StringComparison.Ordinal))
                .Order());
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("android-arm")]
    [InlineData("android-arm64")]
    [InlineData("android-x86")]
    [InlineData("android-x64")]
    [InlineData("linux-musl-x86")]
    public async Task RuntimeGraphDoesNotFallBackToLinuxForExcludedRids(string rid)
    {
        var core = ReadPackage("SkiaSharp");
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, "net10.0"));
        xml.Element("PropertyGroup")!.Element("OutputType")!.Value = "Library";
        xml.Element("PropertyGroup")!.Element("UseAppHost")!.Value = "false";
        xml.Element("PropertyGroup")!.Add(new XElement("EnableRuntimePackDownload", "false"));
        var project = dotnet.NewProject($"SkiaSharp-excluded-rid-graph-{rid}", xml.ToString());

        await dotnet.Restore(project, rid);

        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.DoesNotContain(assets.RootElement.GetProperty("libraries").EnumerateObject(),
            p => p.Name.StartsWith("SkiaSharp.runtime.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("win-x64")]
    [InlineData("linux-x64")]
    public async Task Win32CompatibilityPackagePreservesOrdinaryDependencies(string? rid)
    {
        var core = ReadPackage("SkiaSharp");
        var windows = ReadPackage("SkiaSharp.NativeAssets.Win32");
        Assert.Equal(3, windows.NativeFiles.Count);
        var expected = rid is null ? windows.NativeFiles :
            ReadPackage("SkiaSharp.runtime." + rid).NativeFiles
                .ToDictionary(p => p.Key[$"runtimes/{rid}/native/".Length..], p => p.Value);
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, "net10.0"));
        xml.Element("ItemGroup")!.Add(new XElement("PackageReference",
            new XAttribute("Include", "SkiaSharp.NativeAssets.Win32"),
            new XAttribute("Version", $"[{core.Version}]")));
        var project = dotnet.NewProject($"SkiaSharp-win32-compat-{rid ?? "portable"}", xml.ToString());

        await dotnet.Publish(project, rid);

        AssertNativeOutput(project, "SkiaSharp", expected, "net10.0");
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal(["SkiaSharp.runtime.win-arm64", "SkiaSharp.runtime.win-x64", "SkiaSharp.runtime.win-x86"],
            assets.RootElement.GetProperty("libraries").EnumerateObject()
                .Select(p => p.Name.Split('/')[0])
                .Where(id => id.StartsWith("SkiaSharp.runtime.win-", StringComparison.Ordinal))
                .Order());
        dotnet.CleanBuildOutput(project);
    }

    [Fact]
    public async Task CurrentRidSelectedByTheApplicationWorksOnFirstBuild()
    {
        var core = ReadPackage("SkiaSharp");
        var rid = RuntimeInformation.RuntimeIdentifier;
        var native = ReadPackage("SkiaSharp.runtime." + rid);
        var prefix = $"runtimes/{rid}/native/";
        var expected = native.NativeFiles.ToDictionary(p => p.Key[prefix.Length..], p => p.Value);
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, "net10.0"));
        xml.Element("PropertyGroup")!.Add(new XElement("UseCurrentRuntimeIdentifier", "true"));
        var project = dotnet.NewProject("SkiaSharp-current-rid-first-build", xml.ToString());
        File.WriteAllText(Path.Combine(project, "Program.cs"), """
            using SkiaSharp;
            using var bitmap = new SKBitmap(8, 8);
            bitmap.Erase(SKColors.Magenta);
            if (bitmap.GetPixel(3, 3) != SKColors.Magenta)
                throw new System.Exception("Native pixel round-trip failed.");
            System.Console.WriteLine("Native pixel round-trip passed.");
            """);

        await dotnet.Build(project);

        AssertNativeOutput(project, "SkiaSharp", expected, "net10.0");
        Assert.Contains("Native pixel round-trip passed.",
            await dotnet.Execute(project, Path.Combine("output", "Consumer.dll")));
        dotnet.CleanBuildOutput(project);
    }

    [Theory]
    [InlineData("net462", false)]
    [InlineData("net462", true)]
    [InlineData("net48", false)]
    [InlineData("net48", true)]
    public async Task FrameworkSdkPreservesArchitectureFolders(string framework, bool optOut)
    {
        var core = ReadPackage("SkiaSharp");
        var windows = ReadPackage("SkiaSharp.NativeAssets.Win32");
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, framework));
        xml.Element("PropertyGroup")!.Add(new XElement("ShouldIncludeNativeSkiaSharp", !optOut));
        var project = dotnet.NewProject($"SkiaSharp-framework-{framework}-{optOut}", xml.ToString());

        await dotnet.Build(project);

        foreach (var arch in new[] { "x86", "x64", "arm64" })
        {
            var file = Path.Combine(project, "output", arch, "libSkiaSharp.dll");
            if (optOut)
                Assert.False(File.Exists(file));
            else
            {
                Assert.True(File.Exists(file), $"Missing legacy architecture folder: {file}");
                using var stream = File.OpenRead(file);
                Assert.Equal(windows.NativeFiles[$"runtimes/win-{arch}/native/libSkiaSharp.dll"],
                    Convert.ToHexString(SHA256.HashData(stream)));
            }
        }
    }

    [Theory]
    [InlineData("net462", "v4.6.2", false)]
    [InlineData("net462", "v4.6.2", true)]
    [InlineData("net48", "v4.8", false)]
    [InlineData("net48", "v4.8", true)]
    public async Task LegacyPackagesDirectoryImportsCopyArchitectureFolders(string framework, string frameworkVersion,
        bool optOut)
    {
        var windows = ReadPackage("SkiaSharp.NativeAssets.Win32");
        var project = CreateLegacyNativeProject("SkiaSharp.NativeAssets.Win32", windows.Version,
            framework, frameworkVersion, optOut);

        await dotnet.CopyLegacyNativeAssets(project);

        foreach (var arch in new[] { "x86", "x64", "arm64" })
        {
            var file = Path.Combine(project, "output", arch, "libSkiaSharp.dll");
            if (optOut)
                Assert.False(File.Exists(file));
            else
            {
                Assert.True(File.Exists(file), $"Legacy import did not supply {arch} assets");
                using var stream = File.OpenRead(file);
                Assert.Equal(windows.NativeFiles[$"runtimes/win-{arch}/native/libSkiaSharp.dll"],
                    Convert.ToHexString(SHA256.HashData(stream)));
            }
        }
    }

    [Theory]
    [InlineData("net462", "v4.6.2", false)]
    [InlineData("net462", "v4.6.2", true)]
    [InlineData("net48", "v4.8", false)]
    [InlineData("net48", "v4.8", true)]
    public async Task LegacyMacOSImportsPreserveUniversalRootCopy(string framework, string frameworkVersion,
        bool optOut)
    {
        var mac = ReadPackage("SkiaSharp.NativeAssets.macOS");
        var project = CreateLegacyNativeProject("SkiaSharp.NativeAssets.macOS", mac.Version,
            framework, frameworkVersion, optOut);

        await dotnet.CopyLegacyNativeAssets(project);

        foreach (var folder in new[] { "", "x64", "arm64" })
        {
            var file = Path.Combine(project, "output", folder, "libSkiaSharp.dylib");
            if (optOut)
                Assert.False(File.Exists(file));
            else
            {
                Assert.True(File.Exists(file), $"Missing macOS compatibility copy: {file}");
                using var stream = File.OpenRead(file);
                Assert.Equal(mac.NativeFiles["runtimes/osx/native/libSkiaSharp.dylib"],
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
        var regular = ReadPackage("SkiaSharp.runtime." + rid);
        var nodeps = ReadPackage("SkiaSharp.runtime." + rid + ".NoDependencies");
        var prefix = $"runtimes/{rid}/native/";
        var expected = nodeps.NativeFiles.ToDictionary(p => p.Key[prefix.Length..], p => p.Value);
        Assert.NotEmpty(expected);
        Assert.NotEqual(regular.NativeFiles[prefix + "libSkiaSharp.so"], expected["libSkiaSharp.so"]);
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, "net10.0"));
        var references = xml.Element("ItemGroup")!;
        var reference = new XElement("PackageReference",
            new XAttribute("Include", direct ? "SkiaSharp.runtime." + rid + ".NoDependencies" :
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
            if (target.Value.TryGetProperty($"SkiaSharp.runtime.{rid}/{core.Version}", out var library))
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
        var expected = ReadPackage("SkiaSharp.NativeAssets.Linux.NoDependencies").NativeFiles;
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
                p.Name.StartsWith("SkiaSharp.runtime.linux", StringComparison.Ordinal) &&
                !p.Name.Contains(".NoDependencies/", StringComparison.Ordinal)))
                Assert.False(library.Value.TryGetProperty("runtimeTargets", out var runtimeTargets) &&
                    runtimeTargets.EnumerateObject().Any());
        dotnet.CleanBuildOutput(project);
    }

    [Fact]
    public async Task SingleRidNoDependenciesReferencePreservesOtherDefaultRids()
    {
        var core = ReadPackage("SkiaSharp");
        var regular = ReadPackage("SkiaSharp.runtime.linux-musl-arm64");
        var prefix = "runtimes/linux-musl-arm64/native/";
        var expected = regular.NativeFiles.ToDictionary(p => p.Key[prefix.Length..], p => p.Value);
        var xml = XElement.Parse(ProjectXml("SkiaSharp", core.Version, "net10.0"));
        xml.Element("ItemGroup")!.Add(new XElement("PackageReference",
            new XAttribute("Include", "SkiaSharp.runtime.linux-musl-x64.NoDependencies"),
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

    private Task BuildOrPublish(string project, string command, string? rid = null) =>
        command == "build" ? dotnet.Build(project) : dotnet.Publish(project, rid);

    private string CreateLegacyNativeProject(string packageId, string version, string framework,
        string frameworkVersion, bool optOut)
    {
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
                new XElement("ShouldIncludeNativeSkiaSharp", !optOut),
                new XElement("OutputPath", Path.Combine(project, "output") + Path.DirectorySeparatorChar)),
            new XElement("Import", new XAttribute("Project", import)),
            new XElement("Target", new XAttribute("Name", "CopyNativeFiles"),
                new XElement("ItemGroup",
                    new XElement("_NativeCopy", new XAttribute("Include", "@(None);@(Content)"),
                        new XAttribute("Condition", "'%(Filename)' == 'libSkiaSharp'"))),
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
                if (dependencyId.StartsWith("SkiaSharp.runtime.", StringComparison.Ordinal))
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
