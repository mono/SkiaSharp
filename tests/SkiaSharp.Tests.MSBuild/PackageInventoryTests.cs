using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

[Collection("Package consumers")]
public class PackageInventoryTests(DotNet dotnet)
{
    [Fact]
    public void ProducerPackagesHaveConsistentIdentityAndRequiredNativeAssets()
    {
        var platforms = new List<string> { "Win32", "macOS", "Linux", "WebAssembly", "Android" };
        if (OperatingSystem.IsMacOS())
            platforms.AddRange(["iOS", "MacCatalyst"]);
        var packages = new List<ArtifactPackage>();
        foreach (var family in new[] { "SkiaSharp", "HarfBuzzSharp" })
        {
            var core = dotnet.Packages.Get(family);
            packages.Add(core);
            foreach (var platform in platforms)
            {
                var package = dotnet.Packages.Get(family + ".NativeAssets." + platform);
                Assert.Equal(core.Version, package.Version);
                Assert.Contains(package.Files.Keys, path => path.EndsWith(".a", StringComparison.Ordinal) ||
                    path.StartsWith("runtimes/", StringComparison.Ordinal));
                if (platform == "WebAssembly")
                {
                    foreach (var toolchain in family == "SkiaSharp" ? new[] { "3.1.56" } : new[] { "3.1.56", "5.0.6" })
                        foreach (var variant in new[] { "st", "st,simd", "mt", "mt,simd" })
                            Assert.Contains(package.Files.Keys, path => path.StartsWith(
                                $"buildTransitive/netstandard1.0/lib{family}.a/{toolchain}/{variant}/",
                                StringComparison.Ordinal) && path.EndsWith(".a", StringComparison.Ordinal));
                }
                packages.Add(package);
            }
        }
        foreach (var id in new[] { "SkiaSharp.HarfBuzz", "SkiaSharp.Views.Maui.Controls", "SkiaSharp.Views.Maui.Core",
            "SkiaSharp.NativeAssets.WinUI" })
        {
            var package = dotnet.Packages.Get(id);
            Assert.Equal(dotnet.Packages.Get("SkiaSharp").Version, package.Version);
            packages.Add(package);
        }
        Assert.Single(packages.Select(package => package.RepositoryCommit).Distinct());
    }
}
