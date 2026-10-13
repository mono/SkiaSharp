using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed record SampleImageOptions
{
    public SKSizeI? ExpectedSize { get; init; }
    public double? MaxAverageColorErrorFraction { get; init; }
}

// Checks that sample output is a decodable image with the expected dimensions.
internal static class SampleImage
{
    private const decimal MaximumErrorPixelFraction = 0.00075m;

    public static void Validate(ReadOnlySpan<byte> png, int width = 800, int height = 600)
    {
        Assert.True(png.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "Expected a PNG file.");

        using var bitmap = SKBitmap.Decode(png);
        Assert.NotNull(bitmap);
        Assert.Equal(width, bitmap.Width);
        Assert.Equal(height, bitmap.Height);
    }

    internal static void ValidateFile(string actual, string golden, SampleImageOptions? options = null)
    {
        if (options?.ExpectedSize is { } size)
            Validate(File.ReadAllBytes(actual), size.Width, size.Height);
        TestContext.Current.AddFileAttachment(actual, "image/png");

        CompareGolden(actual, Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected", golden),
            options);
    }

    internal static async Task SaveAndValidate(byte[] png, string actual, string golden, SampleImageOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);

        await File.WriteAllBytesAsync(actual, png);

        ValidateFile(actual, golden, options);
    }

    internal static void CompareGolden(string actual, string expected, SampleImageOptions? options = null)
    {
        var maxAverageColorErrorFraction = options?.MaxAverageColorErrorFraction;
        if (maxAverageColorErrorFraction is { } maximum)
            Assert.InRange(maximum, 0, 1);
        Assert.True(File.Exists(expected), $"Missing reviewed sample golden: {expected}. Actual image retained at {actual}.");

        using var actualImage = SKImage.FromEncodedData(actual);
        using var expectedImage = SKImage.FromEncodedData(expected);
        Assert.NotNull(actualImage);
        Assert.NotNull(expectedImage);
        if (options?.ExpectedSize is { } size)
        {
            Assert.Equal(size.Width, actualImage.Width);
            Assert.Equal(size.Height, actualImage.Height);
        }
        Assert.Equal(expectedImage.Width, actualImage.Width);
        Assert.Equal(expectedImage.Height, actualImage.Height);

        var result = SkiaSharp.Extended.SKPixelComparer.Compare(expectedImage, actualImage, 0);
        var averageColorErrorFraction = result.AbsoluteError / (result.TotalPixels * 4.0 * 255);
        var comparison = maxAverageColorErrorFraction is { } limit
            ? $"average RGBA color error {averageColorErrorFraction:P3}, exceeding {limit:P3}"
            : $"differing pixels {(decimal)result.ErrorPixelCount / result.TotalPixels:P3}, exceeding {MaximumErrorPixelFraction:P3}";
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"Image comparison: {result.ErrorPixelCount}/{result.TotalPixels} pixels differ; " +
            $"average RGBA color error {averageColorErrorFraction:P3}; " +
            (maxAverageColorErrorFraction is { } allowed
                ? $"allowed average color error {allowed:P3}."
                : $"allowed differing pixels {MaximumErrorPixelFraction:P3}."));
        if (maxAverageColorErrorFraction is { } averageLimit
            ? averageColorErrorFraction <= averageLimit
            : result.ErrorPixelCount <= result.TotalPixels * MaximumErrorPixelFraction)
            return;

        using var diff = SkiaSharp.Extended.SKPixelComparer.GenerateDifferenceImage(expectedImage, actualImage, 0);
        using var data = diff.Encode(SKEncodedImageFormat.Png, 100);
        var diffPath = Path.ChangeExtension(actual, ".diff.png");
        File.WriteAllBytes(diffPath, data.ToArray());

        TestContext.Current.AddFileAttachment(expected, "image/png", $"expected-{Path.GetFileName(expected)}");
        TestContext.Current.AddFileAttachment(diffPath, "image/png");

        Assert.Fail($"Sample pixels differ from {expected}: {result.ErrorPixelCount}/{result.TotalPixels} pixels; " +
            $"{comparison}; actual {actual}; diff {diffPath}.");
    }
}
