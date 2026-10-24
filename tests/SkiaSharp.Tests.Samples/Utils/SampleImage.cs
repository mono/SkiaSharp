using System.Diagnostics;
using System.Runtime.InteropServices;
using SkiaSharp.Tests.Shared;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class SampleImage
{
    internal sealed record Comparison(string Actual, string Expected);

    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    internal static string Root => Path.Combine(AppContext.BaseDirectory, "Goldens");
    internal static GoldenStore Store { get; } = new(new[] { Root });

    internal static Golden Host(string casePath) =>
        Store.Resolve(new(
            SampleLookup.HostPlatform.ToLowerInvariant(),
            casePath,
            architecture: RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()));

    internal static Golden Browser(string casePath) =>
        Store.Resolve(new(
            SampleLookup.HostPlatform.ToLowerInvariant(),
            casePath,
            architecture: RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()));

    internal static Golden Container(string casePath) =>
        Store.Resolve(new(
            OperatingSystem.IsWindows() ? "docker-windows" : "docker-linux",
            casePath));

    internal static async Task<Comparison> Compare(
        Func<Task<string>> capture,
        GoldenCase goldenCase,
        TimeSpan? timeout = null,
        Func<string, string>? transformReference = null,
        Func<string, string>? rawActual = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(goldenCase);

        var limit = timeout ?? DefaultTimeout;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, TimeSpan.Zero);

        var golden = Store.Resolve(goldenCase);
        var clock = Stopwatch.StartNew();
        var first = true;
        var actual = await capture().WaitAsync(limit, TestContext.Current.CancellationToken);

        if (golden.ReferenceKey is null)
        {
            TestContext.Current.TestOutputHelper?.WriteLine(golden.Describe());
            TestContext.Current.AddFileAttachment(actual, "image/png");
            Emit(golden, "actual", rawActual?.Invoke(actual) ?? actual);
            Assert.Fail($"Missing reviewed golden. {golden.Describe()} Actual retained at {actual}.");
            throw new InvalidOperationException("Missing golden verification unexpectedly returned.");
        }

        var expected = MaterializeReference(golden, actual);
        if (transformReference is not null)
            expected = transformReference(expected);

        TestContext.Current.TestOutputHelper?.WriteLine(golden.Describe());
        try
        {
            var remaining = limit - clock.Elapsed;
            if (remaining <= TimeSpan.Zero)
            {
                Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected));
            }
            else
            {
                actual = await Golden.VerifyAsync(
                    GoldenInput.Capture(async () =>
                    {
                        if (first)
                        {
                            first = false;
                            return actual;
                        }
                        return actual = await capture();
                    }),
                    GoldenInput.Path(expected),
                    remaining,
                    TestContext.Current.CancellationToken);
            }
            return new(actual, expected);
        }
        catch
        {
            Emit(golden, "actual", rawActual?.Invoke(actual) ?? actual);
            Emit(golden, "reference", expected);
            var diff = Path.ChangeExtension(actual, ".diff.png");
            if (File.Exists(diff))
                Emit(golden, "diff", diff);
            throw;
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

        TestContext.Current.TestOutputHelper?.WriteLine(
            $"Measured {count} matching pixels in {region} from {screenshot}.");
        return count;
    }

    private static string MaterializeReference(Golden golden, string actual)
    {
        if (golden.ReferenceLocation is { } location && File.Exists(location))
            return location;

        var key = golden.ReferenceKey ?? throw new InvalidOperationException(golden.Describe());
        var output = Path.Combine(
            Path.GetDirectoryName(actual) ?? throw new InvalidOperationException("Actual image has no directory."),
            "references",
            key.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllBytes(output, golden.ReadBytes());
        return output;
    }

    private static void Emit(Golden golden, string role, string path)
    {
        var bytes = File.ReadAllBytes(path);
        TestContext.Current.TestOutputHelper?.WriteLine(golden.FormatMarker(role, bytes));
    }
}
