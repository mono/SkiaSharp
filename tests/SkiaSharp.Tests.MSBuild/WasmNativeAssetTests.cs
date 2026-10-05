using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

[Collection("Package consumers")]
public class WasmNativeAssetTests(DotNet dotnet)
{
    private static readonly string[] Families = ["SkiaSharp", "HarfBuzzSharp"];

    public static TheoryData<string, bool, bool> Cases
    {
        get
        {
            var data = new TheoryData<string, bool, bool>();
            foreach (var framework in new[] { "net10.0", "net11.0" })
                foreach (var threads in new[] { false, true })
                    foreach (var simd in new[] { false, true })
                        data.Add(framework, threads, simd);
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [Trait("Category", "Wasm")]
    public async Task BuildBlazorTemplateLinksBothPackageFamilies(string framework, bool threads, bool simd)
    {
        var consumer = await CreateConsumer("build", framework, threads, simd);
        await dotnet.Build(consumer.Project, outputDirectory: null);
        AssertConsumer(consumer.Project, framework, threads, simd, consumer.Packages,
            Path.Combine("bin", "Release", framework, "wwwroot"));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [Trait("Category", "Wasm")]
    public async Task PublishBlazorTemplateLinksBothPackageFamilies(string framework, bool threads, bool simd)
    {
        var consumer = await CreateConsumer("publish", framework, threads, simd);
        await dotnet.Publish(consumer.Project, outputDirectory: null);
        AssertConsumer(consumer.Project, framework, threads, simd, consumer.Packages,
            Path.Combine("bin", "Release", framework, "publish", "wwwroot"));
    }

    private async Task<(string Project, Dictionary<string, ArtifactPackage> Packages)> CreateConsumer(
        string name, string framework, bool threads, bool simd)
    {
        var packages = Families.SelectMany(family => new[] { family, family + ".NativeAssets.WebAssembly" })
            .ToDictionary(id => id, dotnet.Packages.Get);
        var variant = (threads ? "mt" : "st") + (simd ? ",simd" : "");
        var project = await dotnet.NewTemplateProject($"wasm-{framework}-{name}-{variant}", "blazorwasm", framework,
            xml => ConfigureProject(xml, threads, simd, packages));
        File.WriteAllText(Path.Combine(project, "Pages", "Home.razor"),
            """
            @page "/"
            @using SkiaSharp
            <h1>Package consumer</h1>
            <p>@result</p>
            @code {
                private string result = "";
                protected override void OnInitialized()
                {
                    using var bitmap = new SKBitmap(1, 1);
                    bitmap.SetPixel(0, 0, SKColors.Red);
                    using var buffer = new HarfBuzzSharp.Buffer();
                    buffer.AddUtf8("WASM");
                    buffer.GuessSegmentProperties();
                    result = $"{bitmap.GetPixel(0, 0)}: {buffer.Length}";
                }
            }
            """);
        return (project, packages);
    }

    private void AssertConsumer(string project, string framework, bool threads, bool simd,
        Dictionary<string, ArtifactPackage> packages, string outputDirectory)
    {
        var variant = (threads ? "mt" : "st") + (simd ? ",simd" : "");
        ArtifactPackage.AssertRestore(dotnet, project, framework, packages.Values);

        var evidence = File.ReadAllLines(Path.Combine(project, "wasm-native-assets.txt"));
        Assert.Contains("TargetFramework=" + framework, evidence);
        Assert.Contains("UseMonoRuntime=true", evidence, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("WasmBuildNative=true", evidence, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("WasmEnableThreads=" + threads, evidence, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("WasmEnableSIMD=" + simd, evidence, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("WasmEnableExceptionHandling=true", evidence, StringComparer.OrdinalIgnoreCase);
        var expected = Families.SelectMany(family =>
        {
            var package = packages[family + ".NativeAssets.WebAssembly"];
            // These are the current shipping selectors, deliberately independent of the consumer SDK.
            var toolchain = family == "HarfBuzzSharp" && framework == "net11.0" ? "5.0.6" : "3.1.56";
            var prefix = $"buildTransitive/netstandard1.0/lib{family}.a/{toolchain}/{variant}/";
            var archives = package.Files.Where(entry => entry.Key.StartsWith(prefix, StringComparison.Ordinal) &&
                entry.Key.EndsWith(".a", StringComparison.Ordinal)).ToArray();
            Assert.NotEmpty(archives);
            return archives.Select(entry => new KeyValuePair<string, string>(package.CachePath(dotnet, entry.Key), entry.Value));
        }).ToDictionary(entry => entry.Key, entry => entry.Value, ArtifactPackage.PathComparer);
        var selected = evidence.Where(line => line.EndsWith(".a", StringComparison.Ordinal)).Select(Path.GetFullPath);
        Assert.Equal(expected.Keys.Order(ArtifactPackage.PathComparer), selected.Order(ArtifactPackage.PathComparer),
            ArtifactPackage.PathComparer);
        foreach (var (path, hash) in expected)
            ArtifactPackage.AssertFileHash(path, hash);

        var responses = Directory.GetFiles(Path.Combine(project, "obj"), "emcc-link.rsp", SearchOption.AllDirectories);
        Assert.NotEmpty(responses);
        var link = string.Join("\n", responses.Select(File.ReadAllText)).Replace('\\', '/');
        foreach (var path in expected.Keys)
            Assert.Contains(path.Replace('\\', '/'), link, StringComparison.Ordinal);
        Assert.Contains("-fwasm-exceptions", link, StringComparison.Ordinal);
        var bundle = Path.Combine(project, outputDirectory);
        Assert.True(File.Exists(Path.Combine(bundle, "index.html")), $"Missing Blazor entry point in {bundle}");
        var output = Path.Combine(bundle, "_framework");
        Assert.NotEmpty(Directory.GetFiles(output, "blazor.webassembly*.js"));
        Assert.NotEmpty(Directory.GetFiles(output, "dotnet*.js"));
        var wasm = Directory.GetFiles(output, "dotnet.native*.wasm");
        Assert.NotEmpty(wasm);
        foreach (var file in wasm)
        {
            using var stream = File.OpenRead(file);
            var header = new byte[4];
            Assert.Equal(4, stream.Read(header));
            Assert.Equal(new byte[] { 0, 0x61, 0x73, 0x6d }, header);
        }
    }

    private static void ConfigureProject(XElement project, bool threads, bool simd, Dictionary<string, ArtifactPackage> packages) =>
        project.Add(
            new XElement("PropertyGroup",
                new XElement("UseMonoRuntime", "true"),
                new XElement("WasmEnableThreads", threads.ToString().ToLowerInvariant()),
                new XElement("WasmEnableSIMD", simd.ToString().ToLowerInvariant())),
            new XElement("ItemGroup", packages.Select(package => new XElement("PackageReference",
                new XAttribute("Include", package.Key), new XAttribute("Version", $"[{package.Value.Version}]")))),
            new XElement("Target", new XAttribute("Name", "RecordWasmNativeAssets"),
                new XAttribute("AfterTargets", "ResolveReferences"),
                new XElement("WriteLinesToFile", new XAttribute("File", "wasm-native-assets.txt"),
                    new XAttribute("Overwrite", "true"),
                    new XAttribute("Lines", "TargetFramework=$(TargetFramework);UseMonoRuntime=$(UseMonoRuntime);" +
                        "WasmBuildNative=$(WasmBuildNative);WasmEnableExceptionHandling=$(WasmEnableExceptionHandling);" +
                        "WasmEnableSIMD=$(WasmEnableSIMD);WasmEnableThreads=$(WasmEnableThreads)")),
                new XElement("WriteLinesToFile", new XAttribute("File", "wasm-native-assets.txt"),
                    new XAttribute("Lines", "@(NativeFileReference->'%(FullPath)')"))));
}
