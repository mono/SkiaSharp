using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

public class WasmNativeReferenceTests
{
    [Theory]
    [InlineData("8.0", "3.1.34")]
    [InlineData("9.0", "3.1.56")]
    [InlineData("10.0", "3.1.56")]
    [InlineData("11.0", "6.0.2")]
    [InlineData("12.0", "6.0.2")]
    public async Task WasmTargetsSelectTheDeclaredFrameworkToolchain(string framework, string emscripten)
    {
        using var profile = new DotNet();
        foreach (var family in new[] { "SkiaSharp", "HarfBuzzSharp" })
        foreach (var packaged in new[] { false, true })
        {
            var relative = packaged
                ? $"binding/{family}.NativeAssets.WebAssembly/buildTransitive/{family}.targets"
                : $"binding/IncludeNativeAssets.{family}.targets";
            var targets = Path.Combine(profile.Root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(targets)!);
            File.Copy(Path.Combine(profile.RepositoryDirectory, relative.Replace('/', Path.DirectorySeparatorChar)), targets);
            var archives = Path.Combine(profile.Root, "output", "native", "wasm", $"lib{family}.a");
            foreach (var version in new[] { "3.1.34", "3.1.56", "5.0.6", "6.0.2" })
            foreach (var feature in new[] { "st", "mt", "st,simd", "mt,simd" })
            {
                var file = Path.Combine(archives, version, feature, $"lib{family}.a");
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                File.WriteAllText(file, "Native reference selection fixture");
            }
            foreach (var (threads, simd, feature) in new[]
            {
                (false, false, "st"), (true, false, "mt"),
                (false, true, "st,simd"), (true, true, "mt,simd")
            })
            {
                var project = new XElement("Project",
                    new XElement("PropertyGroup",
                        new XElement("TargetFramework", $"net{framework}"),
                        new XElement("TargetFrameworkVersion", framework),
                        new XElement("UsingMicrosoftNETSdkWebAssembly", "true"),
                        new XElement("WasmEnableThreads", threads),
                        new XElement("WasmEnableSIMD", simd),
                        new XElement($"{family}StaticLibraryPath", archives)),
                    new XElement("Import", new XAttribute("Project", targets)));
                var directory = profile.NewProject($"{family}-{packaged}-{threads}-{simd}", project.ToString());
                var json = await profile.EvaluateNativeFileReferences(directory);
                using var result = JsonDocument.Parse(json);
                var reference = Assert.Single(result.RootElement.GetProperty("Items")
                    .GetProperty("NativeFileReference").EnumerateArray());
                Assert.Equal(Path.GetFullPath(Path.Combine(archives, emscripten, feature, $"lib{family}.a")),
                    reference.GetProperty("FullPath").GetString());
            }
        }
    }
}
