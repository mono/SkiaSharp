using System.Diagnostics;
using System.Runtime.InteropServices;
using SkiaSharp.Tests.Shared;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Checks sample output against a reviewed reference and retains comparison diagnostics.
internal static class SampleImage
{
    internal static string Root => Path.Combine(AppContext.BaseDirectory, "Goldens");
    internal static GoldenStore Store => new(new[] { Root });

    internal static GoldenRequest Host(string casePath) =>
        new(SampleLookup.HostPlatform.ToLowerInvariant(), new Version(0, 0, 0), casePath,
            RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant());

    internal static GoldenRequest Container(string casePath) =>
        new(OperatingSystem.IsWindows() ? "docker-windows" : "docker-linux", new Version(0, 0, 0), casePath);

    internal static async Task Validate(byte[] bytes, string actual, GoldenRequest request, SampleImageOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);
        await File.WriteAllBytesAsync(actual, bytes);
        Assert.True(bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "Expected a PNG image.");
        Validate(actual, request, options);
    }

    internal static void Validate(string actual, GoldenRequest request, SampleImageOptions? options = null)
    {
        var resolution = Store.Resolve(request);
        TestContext.Current.TestOutputHelper?.WriteLine(resolution.Describe());
        try
        {
            Require(resolution, actual);
            Validate(actual, resolution.Location!, resolution.ReadBytes(), options ?? new(), reportMismatch: true);
        }
        finally
        {
            Emit(resolution, actual);
        }
    }

    internal static async Task<string> Validate(
        Func<Task<string>> capture, GoldenRequest request, TimeSpan? timeout = null, SampleImageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var limit = timeout ?? TimeSpan.FromSeconds(2);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, TimeSpan.Zero);
        var resolution = Store.Resolve(request);
        TestContext.Current.TestOutputHelper?.WriteLine(resolution.Describe());
        if (resolution.Key is null)
        {
            var missing = await capture().WaitAsync(limit, TestContext.Current.CancellationToken);
            Emit(resolution, missing);
            Require(resolution, missing);
        }
        string? actual = null;
        try
        {
            return await Retry(async () => actual = await capture(), limit,
                (path, report) => Validate(path, resolution.Location!, resolution.ReadBytes(), options ?? new(), report));
        }
        finally
        {
            if (actual is not null)
                Emit(resolution, actual);
        }
    }

    private static void Require(GoldenResolution resolution, string actual)
    {
        if (resolution.Key is null)
            TestContext.Current.AddFileAttachment(actual, "image/png");
        Assert.True(resolution.Key is not null,
            $"Missing reviewed sample golden. {resolution.Describe()} Actual retained at {actual}.");
    }

    private static void Emit(GoldenResolution resolution, string actual)
    {
        Write("actual", File.ReadAllBytes(actual));
        if (resolution.Key is not null)
            Write("reference", resolution.ReadBytes());
        var diff = Path.ChangeExtension(actual, ".diff.png");
        if (File.Exists(diff))
            Write("diff", File.ReadAllBytes(diff));

        void Write(string role, byte[] bytes)
        {
            using var image = SKImage.FromEncodedData(bytes)
                ?? throw new InvalidOperationException("Cannot emit an invalid PNG diagnostic.");
            TestContext.Current.TestOutputHelper?.WriteLine(
                GoldenDiagnostics.Marker("Samples", role, resolution.Request, image.Width, image.Height, bytes));
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

    /// <summary>Validates image dimensions and pixels against an absolute or canonical golden-relative reference path.</summary>
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
        return await Retry(capture, limit, (actual, report) => Validate(actual, expected, options, report));
    }

    private static async Task<string> Retry(Func<Task<string>> capture, TimeSpan limit, Func<string, bool, bool> compare)
    {
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
                compare(actual, true);
                return actual;
            }
            actual = await capture().WaitAsync(remaining, cancellation);
            if (compare(actual, false))
                return actual;

            remaining = limit - clock.Elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining < TimeSpan.FromMilliseconds(100) ? remaining : TimeSpan.FromMilliseconds(100), cancellation);
        }
    }

    private static bool Validate(string actual, string expected, SampleImageOptions options, bool reportMismatch)
    {
        expected = Path.Combine(Root, expected);

        if (reportMismatch || !File.Exists(expected))
            TestContext.Current.AddFileAttachment(actual, "image/png");

        Assert.True(File.Exists(expected), $"Missing reviewed sample golden: {expected}. Actual image retained at {actual}.");
        return Validate(actual, expected, File.ReadAllBytes(expected), options, reportMismatch, attachActual: false);
    }

    private static bool Validate(string actual, string expected, byte[] expectedBytes, SampleImageOptions options,
        bool reportMismatch, bool attachActual = true)
    {
        if (reportMismatch && attachActual)
            TestContext.Current.AddFileAttachment(actual, "image/png");
        var maxErrorPixelFraction = options.MaxErrorPixelFraction;
        var maxAverageColorErrorFraction = options.MaxAverageColorErrorFraction;
        Assert.InRange(maxErrorPixelFraction, 0, 1);
        if (maxAverageColorErrorFraction is { } maximum)
            Assert.InRange(maximum, 0, 1);

        using var actualImage = SKImage.FromEncodedData(actual);
        using var expectedImage = SKImage.FromEncodedData(expectedBytes);
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

        var result = SKPixelComparer.Compare(expectedImage, actualImage, 0);
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

        using var diff = SKPixelComparer.GenerateDifferenceImage(expectedImage, actualImage, 0);
        using var data = diff.Encode(SKEncodedImageFormat.Png, 100);
        var diffPath = Path.ChangeExtension(actual, ".diff.png");
        File.WriteAllBytes(diffPath, data.ToArray());

        if (File.Exists(expected))
            TestContext.Current.AddFileAttachment(expected, "image/png", $"expected-{Path.GetFileName(expected)}");
        else
        {
            var name = $"expected-{Path.GetFileName(expected)}";
            var unique = name;
            for (var suffix = 2; TestContext.Current.Attachments?.ContainsKey(unique) == true; suffix++)
                unique = $"{name}-{suffix}";
            TestContext.Current.AddAttachment(unique, expectedBytes, "image/png");
        }
        TestContext.Current.AddFileAttachment(diffPath, "image/png");

        Assert.Fail($"Sample pixels differ from {expected}: {result.ErrorPixelCount}/{result.TotalPixels} pixels; " +
            $"{comparison}; actual {actual}; diff {diffPath}.");
        return false;
    }

}
