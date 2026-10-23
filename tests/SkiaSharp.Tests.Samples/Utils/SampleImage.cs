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
        new(SampleLookup.HostPlatform.ToLowerInvariant(), casePath, architecture:
            RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant());

    internal static GoldenRequest Container(string casePath) =>
        new(OperatingSystem.IsWindows() ? "docker-windows" : "docker-linux", casePath);

    internal static async Task Validate(byte[] bytes, string actual, GoldenRequest request, GoldenComparisonOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);
        await File.WriteAllBytesAsync(actual, bytes);
        Assert.True(GoldenImage.HasPngSignature(bytes), "Expected a PNG image.");
        Validate(actual, request, options);
    }

    internal static void Validate(string actual, GoldenRequest request, GoldenComparisonOptions? options = null)
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
        Func<Task<string>> capture, GoldenRequest request, TimeSpan? timeout = null, GoldenComparisonOptions? options = null)
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
            return await ImageCapture.RetryAsync(async () => actual = await capture(), limit,
                (path, report) => Validate(path, resolution.Location!, resolution.ReadBytes(), options ?? new(), report),
                TestContext.Current.CancellationToken);
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
        var diff = Path.ChangeExtension(actual, ".diff.png");
        var actualPng = File.Exists(actual) ? File.ReadAllBytes(actual) : null;
        var differencePng = File.Exists(diff) ? File.ReadAllBytes(diff) : null;
        foreach (var marker in GoldenImage.Markers(resolution, actualPng, differencePng))
            TestContext.Current.TestOutputHelper?.WriteLine(marker);
    }

    /// <summary>Saves the actual PNG bytes, checks their signature, then compares it with another image file.</summary>
    internal static async Task Validate(byte[] bytes, string actual, string expected, GoldenComparisonOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);

        await File.WriteAllBytesAsync(actual, bytes);
        Assert.True(GoldenImage.HasPngSignature(bytes), "Expected a PNG image.");

        Validate(actual, expected, options);
    }

    /// <summary>Compares two image files by dimensions and pixels.</summary>
    internal static void Validate(string actual, string expected, GoldenComparisonOptions? options = null) =>
        Validate(actual, expected, options ?? new(), reportMismatch: true);

    /// <summary>Captures until an image matches another image file or the timeout expires.</summary>
    internal static async Task<string> Validate(
        Func<Task<string>> capture, string expected, TimeSpan? timeout = null, GoldenComparisonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var limit = timeout ?? TimeSpan.FromSeconds(2);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, TimeSpan.Zero);
        options ??= new();
        return await ImageCapture.RetryAsync(
            capture, limit, (actual, report) => Validate(actual, expected, options, report),
            TestContext.Current.CancellationToken);
    }

    private static bool Validate(string actual, string expected, GoldenComparisonOptions options, bool reportMismatch)
    {
        if (reportMismatch || !File.Exists(expected))
            TestContext.Current.AddFileAttachment(actual, "image/png");

        Assert.True(File.Exists(expected), $"Missing comparison image: {expected}. Actual image retained at {actual}.");
        return Validate(actual, expected, File.ReadAllBytes(expected), options, reportMismatch, attachActual: false);
    }

    private static bool Validate(string actual, string expected, byte[] expectedBytes, GoldenComparisonOptions options,
        bool reportMismatch, bool attachActual = true)
    {
        if (reportMismatch && attachActual)
            TestContext.Current.AddFileAttachment(actual, "image/png");
        var actualBytes = File.ReadAllBytes(actual);
        var actualDecodes = GoldenImage.TryGetSize(actualBytes, out var actualSize);
        var expectedDecodes = GoldenImage.TryGetSize(expectedBytes, out var expectedSize);
        if (!reportMismatch && (!actualDecodes || !expectedDecodes))
            TestContext.Current.AddFileAttachment(actual, "image/png");
        Assert.True(actualDecodes, $"Failed to decode image: {actual}.");
        Assert.True(expectedDecodes, $"Failed to decode image: {expected}.");
        var dimensionsMatch = actualSize == expectedSize &&
            (options.ExpectedSize is not { } required || actualSize == required);
        if (!dimensionsMatch && !reportMismatch)
            return false;
        if (options.ExpectedSize is { } size)
            Assert.Equal(size, actualSize);
        Assert.Equal(expectedSize, actualSize);

        var result = GoldenImage.Compare(expectedBytes, actualBytes, options, expected, actual);
        var failure = options.MaxAverageColorErrorFraction is { } limit
            ? $"average RGBA color error {result.AverageColorErrorFraction:P3}, exceeding {limit:P3}"
            : $"differing pixels {(double)result.Pixels.ErrorPixelCount / result.Pixels.TotalPixels:P3}, exceeding {options.MaxErrorPixelFraction:P3}";
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"Image comparison: {result.Pixels.ErrorPixelCount}/{result.Pixels.TotalPixels} pixels differ; " +
            $"average RGBA color error {result.AverageColorErrorFraction:P3}; " +
            (options.MaxAverageColorErrorFraction is { } allowed
                ? $"allowed average color error {allowed:P3}."
                : $"allowed differing pixels {options.MaxErrorPixelFraction:P3}."));
        if (result.IsMatch)
        {
            if (!reportMismatch)
                TestContext.Current.AddFileAttachment(actual, "image/png");
            return true;
        }

        if (!reportMismatch)
            return false;

        var diffPath = Path.ChangeExtension(actual, ".diff.png");
        GoldenImage.Write(diffPath, GoldenImage.Difference(expectedBytes, actualBytes, options, expected, actual));

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

        Assert.Fail($"Sample pixels differ from {expected}: {result.Pixels.ErrorPixelCount}/{result.Pixels.TotalPixels} pixels; " +
            $"{failure}; actual {actual}; diff {diffPath}.");
        return false;
    }

}
