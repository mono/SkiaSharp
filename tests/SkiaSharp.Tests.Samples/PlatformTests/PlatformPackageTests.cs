using System.Security.Cryptography;
using System.Text.Json;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples.PlatformTests;

[Trait("Category", "Infrastructure")]
public class PlatformPackageTests
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
        var metadataPath = Path.Combine(restored, ".nupkg.metadata");
        Assert.True(File.Exists(metadataPath), $"Missing restored package metadata: {metadataPath}");
        using var metadata = JsonDocument.Parse(File.ReadAllText(metadataPath));
        Assert.Equal(directory, Path.GetFullPath(metadata.RootElement.GetProperty("source").GetString()!));

        using var stream = File.OpenRead(artifact);
        var expectedHash = Convert.ToBase64String(SHA512.HashData(stream));
        var restoredHash = Path.Combine(restored, $"{packageId.ToLowerInvariant()}.{version.ToLowerInvariant()}.nupkg.sha512");
        Assert.Equal(expectedHash, File.ReadAllText(restoredHash).Trim());
    }
}
