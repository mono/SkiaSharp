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
                packages.Add(package);
            }
        }
        foreach (var id in new[] { "SkiaSharp.HarfBuzz", "SkiaSharp.Views.Maui.Controls", "SkiaSharp.Views.Maui.Core" })
        {
            var package = dotnet.Packages.Get(id);
            Assert.Equal(dotnet.Packages.Get("SkiaSharp").Version, package.Version);
            packages.Add(package);
        }
        Assert.Single(packages.Select(package => package.RepositoryCommit).Distinct());
    }
}
