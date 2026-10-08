using System.Security.Cryptography;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
public class RunnerPackageTests
{
    [Theory]
    [InlineData("SkiaSharp")]
    [InlineData("HarfBuzzSharp")]
    public void TestBindingsComeFromTheSelectedArtifacts(string packageId)
    {
        var version = DotNet.Setting(packageId + "Version");
        var directory = Path.GetFullPath(DotNet.Setting("PackageDirectory"));
        var artifact = Path.Combine(directory, $"{packageId}.{version}.nupkg");
        Assert.True(File.Exists(artifact), $"Missing artifact package: {artifact}");

        var restored = Path.Combine(DotNet.Setting("RestorePackagesPath"),
            packageId.ToLowerInvariant(), version.ToLowerInvariant());
        using var stream = File.OpenRead(artifact);
        var expectedHash = Convert.ToBase64String(SHA512.HashData(stream));
        var restoredPackage = Path.Combine(restored, $"{packageId.ToLowerInvariant()}.{version.ToLowerInvariant()}.nupkg");
        using var restoredStream = File.OpenRead(restoredPackage);
        Assert.Equal(expectedHash, Convert.ToBase64String(SHA512.HashData(restoredStream)));
        var restoredHash = Path.Combine(restored, $"{packageId.ToLowerInvariant()}.{version.ToLowerInvariant()}.nupkg.sha512");
        Assert.Equal(expectedHash, File.ReadAllText(restoredHash).Trim());
    }
}
