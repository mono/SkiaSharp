using System.Security.Cryptography;
using System.Text.Json;
using SkiaSharp.Tests.Goldens;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class SampleReferences
{
    internal static string Root => Path.Combine(Repo.RootDir, "tests", "Goldens", "v1");

    private static readonly Lazy<GoldenStoreIndex> Store = new(() => GoldenStoreIndex.FromDirectory(Root));
    private static readonly Lazy<GoldenEnvironment> HostEnvironment = new(GoldenEnvironment.Current);

    internal static GoldenRequest Host(string caseName, string? variant = null)
    {
        TestContext.Current.TestOutputHelper?.WriteLine(HostEnvironment.Value.Description);
        return HostEnvironment.Value.Request(caseName, variant);
    }

    internal static GoldenResolution Resolve(GoldenRequest request)
    {
        var resolution = Store.Value.Resolve(request);
        TestContext.Current.TestOutputHelper?.WriteLine(resolution.Describe());
        return resolution;
    }

    internal static string Require(GoldenResolution resolution, string actual, string? rawActual = null)
    {
        if (resolution.Asset is null)
        {
            TestContext.Current.AddFileAttachment(actual, "image/png");
            Emit("actual", resolution, rawActual ?? actual);
        }
        Assert.True(resolution.Asset is not null,
            $"Missing reviewed sample golden. {resolution.Describe()} Actual retained at {actual}.");
        return resolution.Asset!.Location;
    }

    internal static void Record(GoldenResolution resolution, string actual, string comparedReference, string? rawActual = null)
    {
        var asset = resolution.Asset!;
        var record = Path.ChangeExtension(actual, ".reference.json");
        var dimensions = Dimensions(actual);
        var referenceDimensions = Dimensions(asset.Location);
        File.WriteAllText(record, JsonSerializer.Serialize(new
        {
            schema = 2,
            request = resolution.Request,
            selected = asset.Key.ToString(),
            source = asset.Location,
            sha256 = Convert.ToHexString(SHA256.HashData(asset.ReadBytes())).ToLowerInvariant(),
            candidates = resolution.Candidates,
            actual,
            rawActual,
            comparedReference,
            dimensions,
            referenceDimensions,
            captureContract = comparedReference == asset.Location ? "raw-v1" : "iphone-16-pro-home39-v1"
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.Current.AddFileAttachment(record, "application/json");
        Emit("actual", resolution, rawActual ?? actual);
        Emit("reference", resolution, asset.Location);
        var diff = Path.ChangeExtension(actual, ".diff.png");
        if (File.Exists(diff))
            Emit("diff", resolution, diff);
    }

    private static object Dimensions(string path)
    {
        using var image = SKImage.FromEncodedData(path);
        return image is null
            ? new { width = 0, height = 0, decodable = false }
            : new { width = image.Width, height = image.Height, decodable = true };
    }

    private static void Emit(string role, GoldenResolution resolution, string path)
    {
        var request = resolution.Request;
        var capture = $"{request.Profile}/{request.Version}/{request.CaseName}" +
            (request.Variant is { Length: > 0 } variant ? "." + variant : "") +
            "." + request.Architecture + ".png";
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 24 || !bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        {
            TestContext.Current.TestOutputHelper?.WriteLine($"Cannot emit PNG diagnostic; invalid image retained at {path}.");
            return;
        }
        var width = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16, 4));
        var height = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20, 4));
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"##SKIA-GOLDEN-V2## role={role} key={capture} selected={resolution.Asset?.Key.ToString() ?? "-"} " +
            $"size={width}x{height} base64={Convert.ToBase64String(bytes)}");
    }
}
