using System.IO.Compression;
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
        var sdkMajor = int.Parse(dotnet.SdkVersion.Split('.')[0]);
        Assert.True(sdkMajor is 10 or 11, $"Unsupported WASM consumer SDK: {dotnet.SdkVersion}");
        var toolchain = sdkMajor >= 11 ? "6.0.2" : "3.1.56";
        var packages = Families.SelectMany(family => new[] { family, family + ".NativeAssets.WebAssembly" })
            .ToDictionary(id => id, ReadPackage);
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

        var project = dotnet.NewProject($"wasm-net{sdkMajor}-mono", ProjectXml(sdkMajor, packages),
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

    private (string Version, Dictionary<string, string> Archives) ReadPackage(string id)
    {
        var matches = new List<(string Version, Dictionary<string, string> Archives)>();
        foreach (var file in Directory.EnumerateFiles(dotnet.PackageDirectory, id + ".*.nupkg", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase)))
        {
            using var zip = ZipFile.OpenRead(file);
            using var nuspec = zip.Entries.Single(entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
            var metadata = XDocument.Load(nuspec).Root!.Elements().Single(element => element.Name.LocalName == "metadata");
            if (metadata.Elements().Single(element => element.Name.LocalName == "id").Value != id)
                continue;
            var version = metadata.Elements().Single(element => element.Name.LocalName == "version").Value;
            var archives = new Dictionary<string, string>();
            foreach (var entry in zip.Entries.Where(entry => entry.FullName.StartsWith("buildTransitive/", StringComparison.Ordinal)
                && entry.FullName.EndsWith(".a", StringComparison.Ordinal)))
            {
                using var stream = entry.Open();
                archives.Add(entry.FullName, Convert.ToHexString(SHA256.HashData(stream)));
            }
            matches.Add((version, archives));
        }
        Assert.True(matches.Count == 1, $"Expected one real {id} package in {dotnet.PackageDirectory}, found {matches.Count}");
        return matches[0];
    }

    private static string ProjectXml(int sdkMajor,
        Dictionary<string, (string Version, Dictionary<string, string> Archives)> packages)
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
                new XElement("TargetFramework", $"net{sdkMajor}.0"),
                new XElement("UseMonoRuntime", "true"),
                new XElement("WasmMainJSPath", "main.js"),
                new XElement("ImplicitUsings", "enable")),
            new XElement("ItemGroup", packages.Select(package =>
                new XElement("PackageReference", new XAttribute("Include", package.Key),
                    new XAttribute("Version", $"[{package.Value.Version}]")))),
            snapshot).ToString();
    }
}
