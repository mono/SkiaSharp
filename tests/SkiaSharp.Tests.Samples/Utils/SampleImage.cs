using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Checks sample output against a reviewed reference and retains comparison diagnostics.
internal static class SampleImage
{
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
    internal static void Validate(string actual, string expected, SampleImageOptions? options = null) =>
        Validate(actual, expected, options ?? new(), reportMismatch: true);

    internal static Task Validate(
        Func<string> capture, string expected, TimeSpan? timeout = null, SampleImageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        return Validate(() => Task.FromResult(capture()), expected, timeout, options);
    }

    internal static async Task Validate(
        Func<Task<string>> capture, string expected, TimeSpan? timeout = null, SampleImageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var limit = timeout ?? TimeSpan.FromSeconds(2);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, TimeSpan.Zero);
        options ??= new();
        if (AppContext.TryGetSwitch("SampleTest.CaptureReferences", out var captureReferences) && captureReferences)
        {
            await Task.Delay(limit, TestContext.Current.CancellationToken);
            var actual = await capture();
            using var image = SKImage.FromEncodedData(actual);
            Assert.NotNull(image);
            if (options.ExpectedSize is { } size)
            {
                Assert.Equal(size.Width, image.Width);
                Assert.Equal(size.Height, image.Height);
            }
            expected = Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected", expected);
            var record = Path.ChangeExtension(actual, ".reference.json");
            await File.WriteAllTextAsync(record, JsonSerializer.Serialize(new { actual, expected }),
                TestContext.Current.CancellationToken);
            TestContext.Current.AddFileAttachment(actual, "image/png");
            TestContext.Current.AddFileAttachment(record, "application/json");
            TestContext.Current.TestOutputHelper?.WriteLine(
                $"REFERENCE CAPTURE ONLY (not a golden validation): {actual} => {expected}");
            return;
        }
        var clock = Stopwatch.StartNew();
        while (true)
        {
            TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
            var actual = await capture();
            if (Validate(actual, expected, options, reportMismatch: clock.Elapsed >= limit))
                return;

            var remaining = limit - clock.Elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining < TimeSpan.FromMilliseconds(500) ? remaining : TimeSpan.FromMilliseconds(500),
                    TestContext.Current.CancellationToken);
        }
    }

    private static bool Validate(string actual, string expected, SampleImageOptions options, bool reportMismatch)
    {
        expected = Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected", expected);

        if (reportMismatch || !File.Exists(expected))
            TestContext.Current.AddFileAttachment(actual, "image/png");

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
        var dimensionsMatch = actualImage.Width == expectedImage.Width && actualImage.Height == expectedImage.Height &&
            (options.ExpectedSize is not { } required || actualImage.Width == required.Width && actualImage.Height == required.Height);
        if (!dimensionsMatch && !reportMismatch)
            return false;
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
        {
            if (!reportMismatch)
                TestContext.Current.AddFileAttachment(actual, "image/png");
            return true;
        }

        if (!reportMismatch)
            return false;

        using var diff = SkiaSharp.Extended.SKPixelComparer.GenerateDifferenceImage(expectedImage, actualImage, 0);
        using var data = diff.Encode(SKEncodedImageFormat.Png, 100);
        var diffPath = Path.ChangeExtension(actual, ".diff.png");
        File.WriteAllBytes(diffPath, data.ToArray());

        TestContext.Current.AddFileAttachment(expected, "image/png", $"expected-{Path.GetFileName(expected)}");
        TestContext.Current.AddFileAttachment(diffPath, "image/png");

        Assert.Fail($"Sample pixels differ from {expected}: {result.ErrorPixelCount}/{result.TotalPixels} pixels; " +
            $"{comparison}; actual {actual}; diff {diffPath}.");
        return false;
    }
}
