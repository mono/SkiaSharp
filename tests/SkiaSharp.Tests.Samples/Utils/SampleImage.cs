using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Checks sample output against a reviewed reference and retains comparison diagnostics.
internal static class SampleImage
{
    internal static (int Width, int Height, byte[] Pixels) ReadPixels(byte[] png)
    {
        using var bitmap = SKBitmap.Decode(png);
        Assert.NotNull(bitmap);
        return (bitmap.Width, bitmap.Height, bitmap.GetPixelSpan().ToArray());
    }

    internal static void AssertRendered(string path)
    {
        using var bitmap = SKBitmap.Decode(path);
        Assert.NotNull(bitmap);
        var colors = new HashSet<SKColor>();
        var colorful = 0;
        var count = 0;
        for (var y = 0; y < bitmap.Height; y += 4)
        {
            for (var x = 0; x < bitmap.Width; x += 4)
            {
                var color = bitmap.GetPixel(x, y);
                colors.Add(color);
                if (Math.Max(color.Red, Math.Max(color.Green, color.Blue)) -
                    Math.Min(color.Red, Math.Min(color.Green, color.Blue)) > 25)
                    colorful++;
                count++;
            }
        }
        Assert.True(colors.Count > 32 && colorful > count * 0.05,
            $"Expected a rendered colored scene, not a blank canvas: {colors.Count} colors, {colorful}/{count} colored pixels; {path}.");
    }

    internal static void AssertSceneChanged(string before, string after)
    {
        using var original = SKBitmap.Decode(before);
        using var changed = SKBitmap.Decode(after);
        Assert.NotNull(original);
        Assert.NotNull(changed);
        Assert.Equal(original.Width, changed.Width);
        Assert.Equal(original.Height, changed.Height);
        for (var y = original.Height / 4; y < original.Height / 2; y++)
            for (var x = original.Width / 4; x < original.Width * 3 / 4; x++)
                if (original.GetPixel(x, y) != changed.GetPixel(x, y))
                    return;
        Assert.Fail($"The rendered scene did not change: {before}; {after}.");
    }

    internal static int CountPixels(string screenshot, SKRectI region, Func<SKColor, bool> matches)
    {
        using var bitmap = SKBitmap.Decode(screenshot);
        Assert.NotNull(bitmap);
        Assert.InRange(region.Left, 0, bitmap.Width - 1);
        Assert.InRange(region.Top, 0, bitmap.Height - 1);
        Assert.InRange(region.Right, region.Left + 1, bitmap.Width);
        Assert.InRange(region.Bottom, region.Top + 1, bitmap.Height);
        var count = 0;
        for (var y = region.Top; y < region.Bottom; y++)
            for (var x = region.Left; x < region.Right; x++)
                if (matches(bitmap.GetPixel(x, y)))
                    count++;
        TestContext.Current.TestOutputHelper?.WriteLine($"Measured {count} matching pixels in {region} from {screenshot}.");
        return count;
    }

    /// <summary>Saves the actual PNG bytes, checks their signature, then validates the saved image against the expected reference.</summary>
    internal static async Task Validate(byte[] bytes, string actual, string expected, SampleImageOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);

        await File.WriteAllBytesAsync(actual, bytes);
        Assert.True(bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "Expected a PNG image.");

        Validate(actual, expected, options);
    }

    /// <summary>Validates image dimensions and pixels against an absolute or golden-directory-relative reference path.</summary>
    internal static void Validate(string actual, string expected, SampleImageOptions? options = null)
    {
        expected = Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected", expected);

        TestContext.Current.AddFileAttachment(actual, "image/png");

        options ??= new();
        var maxErrorPixelFraction = options.MaxErrorPixelFraction;
        var maxAverageColorErrorFraction = options.MaxAverageColorErrorFraction;
        Assert.InRange(maxErrorPixelFraction, 0, 1);
        if (maxAverageColorErrorFraction is { } maximum)
            Assert.InRange(maximum, 0, 1);
        Assert.True(File.Exists(expected), $"Missing reviewed sample golden: {expected}. Actual image retained at {actual}.");

        using var actualImage = SKImage.FromEncodedData(actual);
        using var expectedImage = SKImage.FromEncodedData(expected);
        Assert.NotNull(actualImage);
        Assert.NotNull(expectedImage);
        if (options.ExpectedSize is { } size)
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
            : $"differing pixels {(decimal)result.ErrorPixelCount / result.TotalPixels:P3}, exceeding {maxErrorPixelFraction:P3}";
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"Image comparison: {result.ErrorPixelCount}/{result.TotalPixels} pixels differ; " +
            $"average RGBA color error {averageColorErrorFraction:P3}; " +
            (maxAverageColorErrorFraction is { } allowed
                ? $"allowed average color error {allowed:P3}."
                : $"allowed differing pixels {maxErrorPixelFraction:P3}."));
        if (maxAverageColorErrorFraction is { } averageLimit
            ? averageColorErrorFraction <= averageLimit
            : result.ErrorPixelCount <= result.TotalPixels * maxErrorPixelFraction)
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
