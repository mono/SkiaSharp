using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

public class WasmNativeReferenceTests
{
    [Theory]
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
            // Retired versions are decoys: supported frameworks must never select them.
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
                if (family == "SkiaSharp")
                {
                    using var link = JsonDocument.Parse(await profile.EvaluateWasmLinkInputs(directory));
                    var items = link.RootElement.GetProperty("Items");
                    Assert.Equal("WebGPU", Assert.Single(items.GetProperty("EmccExportedRuntimeMethod")
                        .EnumerateArray()).GetProperty("Identity").GetString());
                    Assert.Equal(new[] { "_wgpuTextureRelease", "_wgpuCreateInstance" },
                        items.GetProperty("EmccExportedFunction").EnumerateArray()
                            .Select(item => item.GetProperty("Identity").GetString()));
                    var libraries = items.GetProperty("Content").EnumerateArray().ToArray();
                    Assert.Equal(new[] { "library_webgpu_enum_tables.js", "library_webgpu_generated_struct_info.js",
                        "library_webgpu_generated_sig_info.js", "library_webgpu.js" },
                        libraries.Select(item => Path.GetFileName(item.GetProperty("Identity").GetString())));
                    Assert.All(libraries, item => Assert.Equal("js-library", item.GetProperty("Kind").GetString()));
                    var flags = link.RootElement.GetProperty("Properties").GetProperty("EmccExtraLDFlags").GetString();
                    Assert.Contains("USE_WEBGL2=1", flags);
                    Assert.DoesNotContain("USE_WEBGPU", flags);
                }
            }
        }
    }

    [Theory]
    [InlineData("SkiaSharp", false)]
    [InlineData("SkiaSharp", true)]
    [InlineData("HarfBuzzSharp", false)]
    [InlineData("HarfBuzzSharp", true)]
    public async Task WasmTargetsRejectUnsupportedFrameworks(string family, bool packaged)
    {
        using var profile = new DotNet();
        var relative = packaged
            ? $"binding/{family}.NativeAssets.WebAssembly/buildTransitive/{family}.targets"
            : $"binding/IncludeNativeAssets.{family}.targets";
        var targets = Path.Combine(profile.RepositoryDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
        foreach (var framework in new[] { "6.0", "7.0", "8.0" })
        foreach (var sdk in new[] { "UsingMicrosoftNETSdkWebAssembly", "UsingMicrosoftNETSdkBlazorWebAssembly" })
        {
            var project = new XElement("Project",
                new XElement("PropertyGroup",
                    new XElement("TargetFramework", $"net{framework}"),
                    new XElement("TargetFrameworkVersion", framework),
                    new XElement(sdk, "true")),
                new XElement("Import", new XAttribute("Project", targets)),
                new XElement("Target", new XAttribute("Name", "PrepareForBuild")));
            var directory = profile.NewProject($"{family}-{packaged}-{framework}-{sdk}", project.ToString());
            using var references = JsonDocument.Parse(await profile.EvaluateNativeFileReferences(directory));
            Assert.Empty(references.RootElement.GetProperty("Items").GetProperty("NativeFileReference").EnumerateArray());
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => profile.PrepareForBuild(directory));
            Assert.Contains($"{family} WebAssembly native assets require .NET 9.0 or later.", error.Message);

            project.Descendants(sdk).Single().Value = "false";
            var nonWasm = profile.NewProject($"{family}-{packaged}-{framework}-{sdk}-non-wasm", project.ToString());
            await profile.PrepareForBuild(nonWasm);
        }
    }
}
