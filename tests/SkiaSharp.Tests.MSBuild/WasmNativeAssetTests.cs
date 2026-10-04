using System.Security.Cryptography;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

public class WasmNativeAssetTests(DotNet dotnet) : IClassFixture<DotNet>
{
    private static readonly string[] Families = ["SkiaSharp", "HarfBuzzSharp"];

    [Fact]
    [Trait("Category", "Wasm")]
    public async Task DefaultMonoBuildLinksBothRealPackageFamilies()
    {
        var sdkMajor = DotNet.ConsumerSdkMajor;
        var toolchain = sdkMajor >= 11 ? "6.0.2" : "3.1.56";
        var packages = Families.SelectMany(family => new[] { family, family + ".NativeAssets.WebAssembly" })
            .ToDictionary(id => id, id => ArtifactPackage.Read(dotnet.PackageDirectory, id));
        foreach (var family in Families)
            Assert.Equal(packages[family].Version, packages[family + ".NativeAssets.WebAssembly"].Version);

        var expected = Families.SelectMany(family =>
        {
            var id = family + ".NativeAssets.WebAssembly";
            var package = packages[id];
            var prefix = $"buildTransitive/netstandard1.0/lib{family}.a/{toolchain}/st,simd/";
            var archives = package.Archives.Where(entry => entry.Key.StartsWith(prefix, StringComparison.Ordinal))
                .ToDictionary(entry => Path.GetFullPath(Path.Combine(dotnet.PackagesCacheDirectory,
                    id.ToLowerInvariant(), package.Version.ToLowerInvariant(),
                    entry.Key.Replace('/', Path.DirectorySeparatorChar))), entry => entry.Value,
                    StringComparer.OrdinalIgnoreCase);
            Assert.NotEmpty(archives);
            return archives;
        }).ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);

        var project = dotnet.NewProject($"wasm-net{sdkMajor}-mono", ProjectXml(packages),
            """
            using SkiaSharp;

            using var bitmap = new SKBitmap(1, 1);
            bitmap.SetPixel(0, 0, SKColors.Red);
            using var buffer = new HarfBuzzSharp.Buffer();
            buffer.AddUtf8("WASM");
            buffer.GuessSegmentProperties();
            Console.WriteLine($"{bitmap.GetPixel(0, 0)}: {buffer.Length}");
            """,
            """
            import { dotnet } from './_framework/dotnet.js';
            await dotnet.create();
            await dotnet.run();
            """);

        // Native linking invokes the workload's compiler, not just managed compilation.
        await dotnet.Build(project, configuration: "Debug", timeoutMinutes: 15);

        var evidence = File.ReadAllLines(Path.Combine(project, "wasm-native-assets.txt"));
        Assert.Contains("WasmBuildNative=true", evidence, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("WasmEnableExceptionHandling=true", evidence, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("WasmEnableSIMD=true", evidence, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("WasmEnableThreads=true", evidence, StringComparer.OrdinalIgnoreCase);
        var selected = evidence.Where(line => line.EndsWith(".a", StringComparison.Ordinal))
            .Select(Path.GetFullPath).ToArray();
        Assert.Equal(expected.Keys.Order(StringComparer.OrdinalIgnoreCase),
            selected.Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        foreach (var (path, hash) in expected)
        {
            Assert.True(File.Exists(path), $"Missing selected archive: {path}");
            using var stream = File.OpenRead(path);
            Assert.Equal(hash, Convert.ToHexString(SHA256.HashData(stream)));
        }

        var responses = Directory.GetFiles(Path.Combine(project, "obj"), "emcc-link.rsp", SearchOption.AllDirectories);
        Assert.NotEmpty(responses);
        var link = string.Join("\n", responses.Select(File.ReadAllText)).Replace('\\', '/');
        foreach (var path in expected.Keys)
            Assert.Contains(path.Replace('\\', '/'), link, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("-fwasm-exceptions", link, StringComparison.Ordinal);

        var nativeWasm = Directory.GetFiles(Path.Combine(project, "obj"), "dotnet.native*.wasm", SearchOption.AllDirectories);
        Assert.NotEmpty(nativeWasm);
        var header = new byte[4];
        foreach (var file in nativeWasm)
        {
            using var stream = File.OpenRead(file);
            Assert.Equal(4, stream.Read(header));
            Assert.Equal(new byte[] { 0, 0x61, 0x73, 0x6d }, header);
        }
    }

    private static string ProjectXml(Dictionary<string, ArtifactPackage> packages)
    {
        var evidence = "wasm-native-assets.txt";
        var snapshot = new XElement("Target",
            new XAttribute("Name", "RecordWasmNativeAssets"),
            new XAttribute("AfterTargets", "ResolveReferences"),
            new XElement("WriteLinesToFile", new XAttribute("File", evidence),
                new XAttribute("Lines", "WasmBuildNative=$(WasmBuildNative)"), new XAttribute("Overwrite", "true")),
            new XElement("WriteLinesToFile", new XAttribute("File", evidence),
                new XAttribute("Lines", "WasmEnableExceptionHandling=$(WasmEnableExceptionHandling)")),
            new XElement("WriteLinesToFile", new XAttribute("File", evidence),
                new XAttribute("Lines", "WasmEnableSIMD=$(WasmEnableSIMD)")),
            new XElement("WriteLinesToFile", new XAttribute("File", evidence),
                new XAttribute("Lines", "WasmEnableThreads=$(WasmEnableThreads)")),
            new XElement("WriteLinesToFile", new XAttribute("File", evidence),
                new XAttribute("Lines", "@(NativeFileReference->'%(FullPath)')")));
        return new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk.WebAssembly"),
            new XElement("PropertyGroup",
                new XElement("OutputType", "Exe"),
                new XElement("TargetFramework", DotNet.ConsumerTargetFramework),
                new XElement("UseMonoRuntime", "true"),
                new XElement("WasmMainJSPath", "main.js"),
                new XElement("ImplicitUsings", "enable")),
            new XElement("ItemGroup", packages.Select(package =>
                new XElement("PackageReference", new XAttribute("Include", package.Key),
                    new XAttribute("Version", $"[{package.Value.Version}]")))),
            snapshot).ToString();
    }
}
