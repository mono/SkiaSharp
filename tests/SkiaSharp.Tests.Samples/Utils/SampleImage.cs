using System.Diagnostics;
using SkiaSharp.Tests.Goldens;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Checks sample output against a reviewed reference and retains comparison diagnostics.
internal static class SampleImage
{
    internal sealed record Comparison(string Actual, string Expected);

    internal static void Validate(string actual, GoldenRequest request, SampleImageOptions? options = null)
    {
        var resolution = SampleReferences.Resolve(request);
        var expected = SampleReferences.Require(resolution, actual);
        try
        {
            Validate(actual, expected, options);
        }
        finally
        {
            SampleReferences.Record(resolution, actual, expected);
        }
    }

    internal static async Task Validate(byte[] bytes, string actual, GoldenRequest request, SampleImageOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);
        await File.WriteAllBytesAsync(actual, bytes);
        Assert.True(bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "Expected a PNG image.");
        Validate(actual, request, options);
    }

    internal static async Task<string> Validate(Func<Task<string>> capture, GoldenRequest request,
        TimeSpan? timeout = null, SampleImageOptions? options = null) =>
        (await Compare(capture, request, timeout, options)).Actual;

    internal static async Task<Comparison> Compare(Func<Task<string>> capture, GoldenRequest request,
        TimeSpan? timeout = null, SampleImageOptions? options = null,
        Func<string, string>? transformReference = null, Func<string, string>? rawActual = null)
    {
        var resolution = SampleReferences.Resolve(request);
        if (resolution.Asset is null)
        {
            var retained = await capture().WaitAsync(timeout ?? TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
            SampleReferences.Require(resolution, retained);
        }
        var expected = resolution.Asset!.Location;
        if (transformReference is not null)
            expected = transformReference(expected);
        string? actual = null;
        try
        {
            actual = await Validate(async () => actual = await capture(), expected, timeout, options);
            return new(actual, expected);
        }
        finally
        {
            if (actual is not null)
                SampleReferences.Record(resolution, actual, expected, rawActual?.Invoke(actual));
        }
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
    internal static void Validate(string actual, string expected, SampleImageOptions? options = null) =>
        Validate(actual, expected, options ?? new(), reportMismatch: true);

    /// <summary>Captures until an image matches the reviewed reference or the timeout expires.</summary>
    internal static async Task<string> Validate(
        Func<Task<string>> capture, string expected, TimeSpan? timeout = null, SampleImageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var limit = timeout ?? TimeSpan.FromSeconds(2);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, TimeSpan.Zero);
        options ??= new();
        var cancellation = TestContext.Current.CancellationToken;
        var clock = Stopwatch.StartNew();
        string? actual = null;

        while (true)
        {
            cancellation.ThrowIfCancellationRequested();
            var remaining = limit - clock.Elapsed;
            if (remaining <= TimeSpan.Zero)
            {
                if (actual is null)
                    throw new TimeoutException("The sample screenshot deadline expired before capture.");
                Validate(actual, expected, options, reportMismatch: true);
                return actual;
            }
            actual = await capture().WaitAsync(remaining, cancellation);
            if (Validate(actual, expected, options, reportMismatch: false))
                return actual;

            remaining = limit - clock.Elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining < TimeSpan.FromMilliseconds(100) ? remaining : TimeSpan.FromMilliseconds(100), cancellation);
        }
    }

    private static bool Validate(string actual, string expected, SampleImageOptions options, bool reportMismatch)
    {
        Assert.True(Path.IsPathFullyQualified(expected), "Reference paths must be absolute; use a typed golden request for stored references.");

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
        if (!reportMismatch && (actualImage is null || expectedImage is null))
            TestContext.Current.AddFileAttachment(actual, "image/png");
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
