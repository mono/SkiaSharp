using System.Runtime.InteropServices;
using SkiaSharp.Tests.Shared;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Checks sample output against a reviewed reference and retains comparison diagnostics.
internal static class SampleImage
{
    internal static string Root => Path.Combine(AppContext.BaseDirectory, "Goldens");
    internal static GoldenStore Store => new(new[] { Root });
    private static GoldenVerifier Verifier => new(Store);

    internal static GoldenRequest Host(string casePath) =>
        new(SampleLookup.HostPlatform.ToLowerInvariant(), casePath, architecture:
            RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant());

    internal static GoldenRequest Container(string casePath) =>
        new(OperatingSystem.IsWindows() ? "docker-windows" : "docker-linux", casePath);

    internal static async Task Validate(byte[] bytes, string actual, GoldenRequest request, ImageComparisonOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);
        await File.WriteAllBytesAsync(actual, bytes);
        Assert.True(GoldenImage.HasPngSignature(bytes), "Expected a PNG image.");
        Validate(actual, request, options);
    }

    internal static void Validate(string actual, GoldenRequest request, ImageComparisonOptions? options = null)
    {
        var verification = Verifier.Verify(request, ImageSource.FromFile(actual), options);
        Complete(verification);
    }

    internal static async Task<string> Validate(
        Func<Task<string>> capture, GoldenRequest request, TimeSpan? timeout = null, ImageComparisonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var limit = timeout ?? TimeSpan.FromSeconds(2);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, TimeSpan.Zero);
        var verification = await Verifier.VerifyAsync(
            request,
            async () => ImageSource.FromFile(await capture()),
            options,
            limit,
            TestContext.Current.CancellationToken);
        return Complete(verification);
    }

    private static string Complete(GoldenVerification verification)
    {
        TestContext.Current.TestOutputHelper?.WriteLine(verification.Resolution.Describe());
        foreach (var marker in verification.Markers())
            TestContext.Current.TestOutputHelper?.WriteLine(marker);

        if (File.Exists(verification.Actual.Location))
            TestContext.Current.AddFileAttachment(verification.Actual.Location, "image/png");
        if (verification.IsMatch)
            return verification.Actual.Location;
        if (verification.IsMissing)
            Assert.Fail(verification.Failure + $" Actual retained at {verification.Actual.Location}.");

        var image = verification.Image!;
        AttachExpected(image);
        string? diffPath = null;
        if (image.DifferencePng is { } difference)
        {
            diffPath = Path.ChangeExtension(image.Actual.Location, ".diff.png");
            GoldenImage.Write(diffPath, difference);
            TestContext.Current.AddFileAttachment(diffPath, "image/png");
        }

        Assert.Fail(
            $"Sample pixels differ from {image.Expected.Location}: {image.Failure} " +
            $"Actual {image.Actual.Location}" + (diffPath == null ? "." : $"; diff {diffPath}."));
        return verification.Actual.Location;
    }

    /// <summary>Saves the actual PNG bytes, checks their signature, then compares it with another image file.</summary>
    internal static async Task Validate(byte[] bytes, string actual, string expected, ImageComparisonOptions? options = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(actual)!);

        await File.WriteAllBytesAsync(actual, bytes);
        Assert.True(GoldenImage.HasPngSignature(bytes), "Expected a PNG image.");

        Validate(actual, expected, options);
    }

    /// <summary>Compares two image files by dimensions and pixels.</summary>
    internal static void Validate(string actual, string expected, ImageComparisonOptions? options = null) =>
        Complete(Verify(actual, expected, options));

    /// <summary>Captures until an image matches another image file or the timeout expires.</summary>
    internal static async Task<string> Validate(
        Func<Task<string>> capture, string expected, TimeSpan? timeout = null, ImageComparisonOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var limit = timeout ?? TimeSpan.FromSeconds(2);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, TimeSpan.Zero);
        var verification = await ImageVerifier.VerifyAsync(
            async () => ImageSource.FromFile(await capture()),
            ImageSource.FromFile(expected),
            options,
            limit,
            TestContext.Current.CancellationToken);
        return Complete(verification);
    }

    private static ImageVerification Verify(string actual, string expected, ImageComparisonOptions? options)
    {
        Assert.True(File.Exists(expected), $"Missing comparison image: {expected}. Actual image retained at {actual}.");
        return ImageVerifier.Verify(ImageSource.FromFile(actual), ImageSource.FromFile(expected), options);
    }

    private static string Complete(ImageVerification verification)
    {
        TestContext.Current.AddFileAttachment(verification.Actual.Location, "image/png");
        if (verification.Comparison is { } comparison)
        {
            TestContext.Current.TestOutputHelper?.WriteLine(
                $"Image comparison: {comparison.Pixels.ErrorPixelCount}/{comparison.Pixels.TotalPixels} pixels differ; " +
                $"average RGBA color error {comparison.AverageColorErrorFraction:P3}.");
        }
        if (verification.IsMatch)
            return verification.Actual.Location;

        AttachExpected(verification);
        string? diffPath = null;
        if (verification.DifferencePng is { } difference)
        {
            diffPath = Path.ChangeExtension(verification.Actual.Location, ".diff.png");
            GoldenImage.Write(diffPath, difference);
            TestContext.Current.AddFileAttachment(diffPath, "image/png");
        }

        Assert.Fail(
            $"Sample pixels differ from {verification.Expected.Location}: {verification.Failure} " +
            $"Actual {verification.Actual.Location}" + (diffPath == null ? "." : $"; diff {diffPath}."));
        return verification.Actual.Location;
    }

    private static void AttachExpected(ImageVerification verification)
    {
        if (File.Exists(verification.Expected.Location))
        {
            TestContext.Current.AddFileAttachment(
                verification.Expected.Location, "image/png", $"expected-{Path.GetFileName(verification.Expected.Location)}");
            return;
        }

        var name = $"expected-{Path.GetFileName(verification.Expected.Location)}";
        var unique = name;
        for (var suffix = 2; TestContext.Current.Attachments?.ContainsKey(unique) == true; suffix++)
            unique = $"{name}-{suffix}";
        TestContext.Current.AddAttachment(unique, verification.ExpectedPng, "image/png");
    }
}
