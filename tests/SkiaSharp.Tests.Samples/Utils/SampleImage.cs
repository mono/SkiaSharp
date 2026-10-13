using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

/// <summary>Specifies dimensions and error limits for sample image comparisons.</summary>
internal sealed record SampleImageOptions
{
    /// <summary>The default allowed fraction of differing pixels: 0.075%.</summary>
    internal const decimal DefaultMaxErrorPixelFraction = 0.00075m;

    /// <summary>Gets the optional required output size. Actual and expected dimensions must always match.</summary>
    internal SKSizeI? ExpectedSize { get; init; }

    /// <summary>
    /// Gets the fraction of pixels allowed to differ by any RGBA channel value.
    /// The default is 0.00075 (0.075%). This limit applies when <see cref="MaxAverageColorErrorFraction"/> is not set.
    /// </summary>
    internal decimal MaxErrorPixelFraction { get; init; } = DefaultMaxErrorPixelFraction;

    /// <summary>
    /// Gets the optional mean absolute RGBA channel error limit, normalized by 255.
    /// When set, replaces <see cref="MaxErrorPixelFraction"/> rather than adding a second check.
    /// Small changes across many pixels can pass; use only for animated GPU smoke checks.
    /// </summary>
    internal double? MaxAverageColorErrorFraction { get; init; }
}

// Checks sample output against a reviewed reference and retains comparison diagnostics.
internal static class SampleImage
{
    /// <summary>Saves the actual image bytes, then validates the saved image against the expected reference.</summary>
    internal static async Task Validate(byte[] bytes, string actual, string expected, SampleImageOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);

        await File.WriteAllBytesAsync(actual, bytes);

        Validate(actual, expected, options);
    }

    /// <summary>Validates image dimensions and pixels against an absolute or golden-directory-relative reference path.</summary>
    internal static void Validate(string actual, string expected, SampleImageOptions? options = null)
    {
        expected = Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected", expected);

        AttachImage(actual);

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

        AttachImage(expected, $"expected-{Path.GetFileName(expected)}");
        AttachImage(diffPath);

        Assert.Fail($"Sample pixels differ from {expected}: {result.ErrorPixelCount}/{result.TotalPixels} pixels; " +
            $"{comparison}; actual {actual}; diff {diffPath}.");
    }

    private static void AttachImage(string file, string? name = null)
    {
        name ??= Path.GetFileName(file);
        var attachmentName = name;
        for (var suffix = 2; TestContext.Current.Attachments?.ContainsKey(attachmentName) == true; suffix++)
            attachmentName = $"{name}-{suffix}";
        TestContext.Current.AddFileAttachment(file, "image/png", attachmentName);
    }
}
