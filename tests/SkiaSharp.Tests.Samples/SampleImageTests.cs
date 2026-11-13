using System.Diagnostics;
using System.Runtime.InteropServices;
using SkiaSharp.Tests.Shared;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
public class SampleImageTests : IDisposable
{
    private readonly string root = Path.Combine(Repo.ArtifactsDir, $"sample-images-{Guid.NewGuid():N}");

    [Fact]
    public void BrowserGoldensUseTheHostOperatingSystemArchitecture()
    {
        Assert.Equal(
            RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(),
            SampleImage.Browser("Blazor/cpu").Case.Architecture);
        Assert.Equal(
            RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
            SampleImage.Host("Console/output").Case.Architecture);
        Assert.Equal(
            "mac/Blazor/cpu.x64.png",
            SampleImage.Store.Resolve(new("mac", "Blazor/cpu", architecture: "x64")).ReferenceKey);
    }

    [Fact]
    public void TypedGoldenComparisonUsesPackagedBytesAndRetainsStrictPolicy()
    {
        var golden = SampleImage.Host("Console/output");
        Assert.NotNull(golden.ReferenceLocation);
        Assert.StartsWith(AppContext.BaseDirectory, golden.ReferenceLocation);
        var actual = Path.Combine(root, "typed.actual.png");
        golden.Verify(
            GoldenInput.Path(WriteFile(actual, golden.ReadBytes())),
            new(expectedSize: new(800, 600), maxErrorPixelFraction: 0));
        using var bitmap = SKBitmap.Decode(golden.ReadBytes());
        Assert.NotNull(bitmap);
        bitmap.Erase(SKColors.Red);
        actual = SavePng(bitmap, "typed-mismatch.actual.png");
        var error = Assert.Throws<Xunit.Sdk.FailException>(() =>
            golden.Verify(
                GoldenInput.Path(actual),
                new(maxErrorPixelFraction: 0)));
        Assert.Contains("Golden images differ", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void MissingTypedGoldenReportsOrderedAttemptsAndRetainsActual()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var actual = SavePng(bitmap, "typed-missing.actual.png");
        var golden = SampleImage.Store.Resolve(new(
            "missing-profile", "Missing/case", new Version(12, 6, 2), "arm64", "engine"));
        var error = Assert.ThrowsAny<Exception>(() => golden.Verify(GoldenInput.Path(actual)));
        Assert.Null(golden.ReferenceKey);
        Assert.Contains(golden.Describe(), error.Message);
        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        Assert.False(File.Exists(Path.Combine(SampleImage.Root, golden.CandidateKeys[0])));
    }

    [Fact]
    public void MissingTypedGoldenStillValidatesTheCapture()
    {
        var golden = SampleImage.Store.Resolve(new("missing-profile", "Missing/invalid"));
        var invalid = new byte[] { 1, 2, 3 };
        var decodeError = Assert.Throws<InvalidOperationException>(() =>
            golden.Verify(GoldenInput.Bytes(invalid)));
        Assert.Contains("Failed to decode image", decodeError.Message);

        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var actual = EncodePng(bitmap);
        var sizeError = Assert.Throws<Xunit.Sdk.FailException>(() =>
            golden.Verify(
                GoldenInput.Bytes(actual),
                new(expectedSize: new(8, 8))));
        Assert.Contains("Image is 4x4, expected 8x8", sizeError.Message);
    }

    [Fact]
    public void InvalidTypedCaptureReportsTheComparisonFailureWithoutDiagnosticMasking()
    {
        var actual = Path.Combine(root, "invalid-typed.actual.png");
        Directory.CreateDirectory(root);
        File.WriteAllBytes(actual, new byte[] { 1, 2, 3 });

        var error = Assert.ThrowsAny<Exception>(() =>
            SampleImage.Host("Console/output").Verify(GoldenInput.Path(actual)));

        Assert.Contains("Failed to decode image", error.Message);
    }

    [Fact]
    public void GoldenMismatchKeepsActualAndExpectedAttachmentsDistinct()
    {
        var actual = Path.Combine(root, "actual", "canvas.png");
        var expected = Path.Combine(root, "expected", "canvas.png");
        Save(actual, SKColors.Red);
        Save(expected, SKColors.Blue);
        TestContext.Current.AddFileAttachment(actual, "image/png");
        var error = Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected)));
        Assert.Contains("Golden images differ", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        Assert.Contains("canvas.png-2", TestContext.Current.Attachments!.Keys);
        Assert.Contains("expected-canvas.png", TestContext.Current.Attachments.Keys);

        static void Save(string path, SKColor color)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var bitmap = new SKBitmap(4, 4);
            bitmap.Erase(color);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(path, data.ToArray());
        }
    }

    [Fact]
    public void InMemoryMismatchAttachesTheDifference()
    {
        using var actualBitmap = new SKBitmap(4, 4);
        actualBitmap.Erase(SKColors.Red);
        using var expectedBitmap = new SKBitmap(4, 4);
        expectedBitmap.Erase(SKColors.Blue);

        Assert.Throws<Xunit.Sdk.FailException>(() =>
            Golden.Verify(
                GoldenInput.Bytes(EncodePng(actualBitmap)),
                GoldenInput.Bytes(EncodePng(expectedBitmap))));
        Assert.Contains("difference.png", TestContext.Current.Attachments!.Keys);
    }

    [Fact]
    public void EncodedInputsAreNormalizedToPng()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var jpeg = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        var jpegBytes = jpeg.ToArray();
        using var decoded = SKImage.FromEncodedData(jpegBytes);
        Assert.NotNull(decoded);
        using var png = decoded.Encode(SKEncodedImageFormat.Png, 100);
        var expected = WriteFile(
            Path.Combine(root, "encoded.expected.png"),
            png.ToArray());
        var actual = Path.Combine(root, "encoded.actual.jpg");
        Directory.CreateDirectory(root);
        File.WriteAllBytes(actual, jpegBytes);

        Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected));
        Golden.Verify(
            GoldenInput.Bytes(jpegBytes.AsSpan(), "memory.jpg"),
            GoldenInput.Path(expected));

        Assert.Contains("encoded.actual.png", TestContext.Current.Attachments!.Keys);
        Assert.Contains("memory.png", TestContext.Current.Attachments.Keys);
    }

    [Fact]
    public void ComparisonTransformCropsOnlyComparedPixelsAndRetainsOriginalFiles()
    {
        using var bitmap = new SKBitmap(6, 7);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "crop.expected.png");
        for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
                if (x < 1 || x >= 4 || y < 1 || y >= 5)
                    bitmap.SetPixel(x, y, SKColors.White);
        var actual = SavePng(bitmap, "crop.actual.png");
        var originalActual = File.ReadAllBytes(actual);
        var originalExpected = File.ReadAllBytes(expected);
        Assert.Throws<Xunit.Sdk.FailException>(() =>
            Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected), new(maxErrorPixelFraction: 0)));

        var options = new GoldenOptions(comparisonTransform: image =>
            image.Subset(new(1, 1, image.Width - 2, image.Height - 2))
                ?? throw new InvalidOperationException("Could not crop the test image."),
            maxErrorPixelFraction: 0);
        Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected), options);
        Golden.Verify(GoldenInput.Bytes(originalActual), GoldenInput.Bytes(originalExpected), options);
        Assert.Equal(originalActual, File.ReadAllBytes(actual));
        Assert.Equal(originalExpected, File.ReadAllBytes(expected));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ComparisonTransformStillRejectsInteriorChangesAndKeepsRawDiagnostics(bool resolved)
    {
        using var bitmap = new SKBitmap(4, 5);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "references/fixture/crop.png");
        bitmap.SetPixel(1, 1, SKColors.Red);
        var actual = SavePng(bitmap, "crop-failure.actual.png");
        var actualBytes = File.ReadAllBytes(actual);
        var expectedBytes = File.ReadAllBytes(expected);
        var options = new GoldenOptions(comparisonTransform: CropForComparison, maxErrorPixelFraction: 0);
        var golden = new GoldenStore(new[] { Path.Combine(root, "references") }).Resolve(new("fixture", "crop"));
        var failure = Assert.Throws<Xunit.Sdk.FailException>(() =>
        {
            if (resolved)
                golden.Verify(GoldenInput.Path(actual), options);
            else
                Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected), options);
        });

        Assert.Contains("1/16 pixels differ", failure.Message);
        using var difference = SKBitmap.Decode(Path.ChangeExtension(actual, ".diff.png"));
        Assert.Equal(new SKSizeI(4, 4), difference.Info.Size);
        Assert.Equal(actualBytes, File.ReadAllBytes(actual));
        Assert.Equal(expectedBytes, File.ReadAllBytes(expected));
        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        Assert.Contains("expected-crop.png", TestContext.Current.Attachments.Keys);
    }

    [Fact]
    public void ComparisonTransformCanReturnBorrowedImagesUnchanged()
    {
        using var bitmap = new SKBitmap(4, 5);
        var png = EncodePng(bitmap);
        var images = new List<SKImage>();
        Golden.Verify(GoldenInput.Bytes(png), GoldenInput.Bytes(png), new(comparisonTransform: image =>
        {
            Assert.All(images, input => Assert.NotEqual(IntPtr.Zero, input.Handle));
            images.Add(image);
            return image;
        }));
        Assert.Equal(2, images.Count);
        Assert.All(images, image => Assert.Equal(IntPtr.Zero, image.Handle));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ComparisonTransformDisposesOwnedImagesAfterSuccessOrMismatch(bool mismatch)
    {
        using var bitmap = new SKBitmap(4, 5);
        bitmap.Erase(SKColors.Black);
        var expected = GoldenInput.Bytes(EncodePng(bitmap));
        if (mismatch)
            bitmap.Erase(SKColors.Red);
        var actual = GoldenInput.Bytes(EncodePng(bitmap));
        var transformed = new List<SKImage>();
        var options = new GoldenOptions(comparisonTransform: image =>
        {
            var cropped = CropForComparison(image);
            transformed.Add(cropped);
            return cropped;
        });
        if (mismatch)
            Assert.Throws<Xunit.Sdk.FailException>(() => Golden.Verify(actual, expected, options));
        else
            Golden.Verify(actual, expected, options);
        Assert.Equal(2, transformed.Count);
        Assert.All(transformed, image => Assert.Equal(IntPtr.Zero, image.Handle));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ComparisonTransformCleansEarlierResultsWhenALaterTransformFails(bool returnsNull)
    {
        using var bitmap = new SKBitmap(4, 5);
        var input = GoldenInput.Bytes(EncodePng(bitmap));
        SKImage? transformed = null;
        var failure = new NotSupportedException("Transform failed.");
        var options = new GoldenOptions(comparisonTransform: image =>
        {
            if (transformed is null)
                return transformed = CropForComparison(image);
            if (returnsNull)
                return null!;
            throw failure;
        });
        if (returnsNull)
            Assert.Contains("returned no image", Assert.Throws<InvalidOperationException>(() =>
                Golden.Verify(input, input, options)).Message);
        else
            Assert.Same(failure, Assert.Throws<NotSupportedException>(() => Golden.Verify(input, input, options)));
        Assert.NotNull(transformed);
        Assert.Equal(IntPtr.Zero, transformed.Handle);
    }

    [Fact]
    public void ComparisonTransformRejectsDisposedResultsBeforeReadingPixels()
    {
        using var bitmap = new SKBitmap(4, 5);
        var input = GoldenInput.Bytes(EncodePng(bitmap));
        var failure = Assert.Throws<InvalidOperationException>(() => Golden.Verify(input, input,
            new(comparisonTransform: image =>
            {
                var cropped = CropForComparison(image);
                cropped.Dispose();
                return cropped;
            })));
        Assert.Contains("returned a disposed image", failure.Message);
    }

    [Fact]
    public void ComparisonTransformCanShareOneOwnedResultAcrossBothInputs()
    {
        using var bitmap = new SKBitmap(4, 5);
        var input = GoldenInput.Bytes(EncodePng(bitmap));
        var transformed = SKImage.FromBitmap(bitmap);
        try
        {
            Golden.Verify(input, input, new(comparisonTransform: _ => transformed));
            Assert.Equal(IntPtr.Zero, transformed.Handle);
        }
        finally
        {
            if (transformed.Handle != IntPtr.Zero)
                transformed.Dispose();
        }
    }

    [Fact]
    public void ComparisonTransformsMustProduceMatchingDimensions()
    {
        using var bitmap = new SKBitmap(4, 5);
        var input = GoldenInput.Bytes(EncodePng(bitmap));
        var count = 0;
        var failure = Assert.Throws<InvalidOperationException>(() => Golden.Verify(input, input,
            new(comparisonTransform: image =>
                image.Subset(new(0, 0, image.Width, image.Height - ++count))
                    ?? throw new InvalidOperationException("Could not crop the test image."))));
        Assert.Contains("Unable to compare images of different sizes: 4x3 vs 4x4", failure.Message);
    }

    [Fact]
    public void ComparisonTransformDoesNotRelaxOriginalImageDimensions()
    {
        using var expected = new SKBitmap(4, 5);
        expected.Erase(SKColors.Black);
        var reference = GoldenInput.Bytes(EncodePng(expected));
        var options = new GoldenOptions(comparisonTransform: CropForComparison, expectedSize: new(4, 5));
        Golden.Verify(reference, reference, options);
        var sizeFailure = Assert.Throws<Xunit.Sdk.FailException>(() =>
            Golden.Verify(reference, reference, new(comparisonTransform: CropForComparison, expectedSize: new(4, 4))));
        Assert.Contains("Image is 4x5, expected 4x4", sizeFailure.Message);

        using var actual = new SKBitmap(4, 4);
        actual.Erase(SKColors.Black);
        var failure = Assert.Throws<Xunit.Sdk.FailException>(() =>
            Golden.Verify(GoldenInput.Bytes(EncodePng(actual)), reference, options));
        Assert.Contains("Image dimensions differ: actual 4x4, expected 4x5", failure.Message);
    }

    [Fact]
    public async Task ResolvedComparisonTransformUsesSharedRetriesAndRawCaptures()
    {
        using var bitmap = new SKBitmap(4, 5);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "references/fixture/crop-retry.png");
        var golden = new GoldenStore(new[] { Path.Combine(root, "references") }).Resolve(new("fixture", "crop-retry"));
        var attempts = 0;
        var actual = await golden.VerifyAsync(GoldenInput.Capture(() =>
        {
            bitmap.Erase(++attempts == 1 ? SKColors.Red : SKColors.Black);
            for (var x = 0; x < bitmap.Width; x++)
                bitmap.SetPixel(x, 4, SKColors.Blue);
            return Task.FromResult(SavePng(bitmap, $"crop-retry-{attempts}.actual.png"));
        }), new(comparisonTransform: CropForComparison, maxErrorPixelFraction: 0),
            timeout: TimeSpan.FromSeconds(2), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, attempts);
        using var capture = SKBitmap.Decode(actual);
        Assert.Equal(new SKSizeI(4, 5), capture.Info.Size);
        Assert.Equal(SKColors.Blue, capture.GetPixel(0, 4));
        Assert.Equal(File.ReadAllBytes(expected), golden.ReadBytes());
    }

    [Fact]
    public async Task MissingGoldenStillFailsWithoutTransformingTheRawActual()
    {
        using var bitmap = new SKBitmap(4, 5);
        bitmap.Erase(SKColors.Blue);
        var actual = SavePng(bitmap, "crop-missing.actual.png");
        var original = File.ReadAllBytes(actual);
        var golden = new GoldenStore(new[] { Path.Combine(root, "missing") }).Resolve(new("fixture", "missing"));
        var failure = await Assert.ThrowsAsync<Xunit.Sdk.FailException>(() =>
            golden.VerifyAsync(GoldenInput.Capture(() => Task.FromResult(actual)),
                new(comparisonTransform: _ => throw new InvalidOperationException("Missing references must fail before comparison.")),
                timeout: TimeSpan.FromSeconds(2),
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Missing reviewed golden", failure.Message);
        Assert.Equal(original, File.ReadAllBytes(actual));
        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        Assert.False(Directory.Exists(Path.Combine(root, "missing")));
    }

    private static SKImage CropForComparison(SKImage image) =>
        image.Subset(new(0, 0, image.Width, image.Height - 1))
            ?? throw new InvalidOperationException("Could not crop the test image.");

    [Fact]
    public async Task ScreenshotComparisonRetriesUntilItMatches()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "retry.expected.png");
        bitmap.Erase(SKColors.Red);
        var actual = SavePng(bitmap, "retry.actual.png");
        var attempts = 0;

        var matched = await Golden.VerifyAsync(
            GoldenInput.Capture(() =>
            {
                if (++attempts == 2)
                    File.Copy(expected, actual, overwrite: true);
                return Task.FromResult(actual);
            }),
            GoldenInput.Path(expected),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, attempts);
        Assert.Equal(actual, matched);
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        Assert.DoesNotContain("expected-" + Path.GetFileName(expected), TestContext.Current.Attachments.Keys);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScreenshotComparisonUsesRequestedRetryInterval(bool resolvedGolden)
    {
        var golden = SampleImage.Host("Console/output");
        var expected = WriteFile(Path.Combine(root, "interval.expected.png"), golden.ReadBytes());
        using var bitmap = SKBitmap.Decode(golden.ReadBytes());
        bitmap.Erase(SKColors.Red);
        var actual = SavePng(bitmap, "interval.actual.png");
        var captures = new List<long>();
        var interval = TimeSpan.FromSeconds(1);
        var input = GoldenInput.Capture(() =>
        {
            captures.Add(Stopwatch.GetTimestamp());
            if (captures.Count == 3)
                File.Copy(expected, actual, overwrite: true);
            return Task.FromResult(actual);
        });

        var matched = resolvedGolden
            ? await golden.VerifyAsync(
                input, timeout: TimeSpan.FromSeconds(10), retryInterval: interval, cancellationToken: TestContext.Current.CancellationToken)
            : await Golden.VerifyAsync(
                input, GoldenInput.Path(expected), timeout: TimeSpan.FromSeconds(10), retryInterval: interval, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(actual, matched);
        Assert.Equal(3, captures.Count);
        Assert.True(Stopwatch.GetElapsedTime(captures[1], captures[2]) >= interval);
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScreenshotComparisonReportsPersistentMismatch(bool wrongSize)
    {
        using var expectedBitmap = new SKBitmap(4, 4);
        expectedBitmap.Erase(SKColors.Blue);
        var expected = SavePng(expectedBitmap, "persistent.expected.png");
        using var actualBitmap = new SKBitmap(wrongSize ? 2 : 4, 4);
        actualBitmap.Erase(SKColors.Red);
        var actual = SavePng(actualBitmap, "persistent.actual.png");

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Golden.VerifyAsync(
                GoldenInput.Capture(() => Task.FromResult(actual)),
                GoldenInput.Path(expected),
                timeout: TimeSpan.FromMilliseconds(150),
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        if (!wrongSize)
        {
            Assert.Contains("Golden images differ", error.Message);
            Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
            Assert.Contains("expected-" + Path.GetFileName(expected), TestContext.Current.Attachments.Keys);
        }
    }

    [Fact]
    public async Task ScreenshotComparisonReportsLastMismatchWhenRetryCaptureTimesOut()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "retry-timeout.expected.png");
        bitmap.Erase(SKColors.Red);
        var actual = SavePng(bitmap, "retry-timeout.actual.png");
        var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;

        try
        {
            var error = await Assert.ThrowsAsync<Xunit.Sdk.FailException>(() =>
                Golden.VerifyAsync(
                    GoldenInput.Capture(() =>
                        ++attempts == 1 ? Task.FromResult(actual) : pending.Task),
                    GoldenInput.Path(expected),
                    timeout: TimeSpan.FromMilliseconds(200),
                    cancellationToken: TestContext.Current.CancellationToken));

            Assert.Equal(2, attempts);
            Assert.Contains("Golden images differ", error.Message);
            Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
            Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
            Assert.Contains("expected-" + Path.GetFileName(expected), TestContext.Current.Attachments.Keys);
        }
        finally
        {
            pending.TrySetCanceled(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task ScreenshotComparisonRetriesTransientDimensions()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "dimensions.expected.png");
        using var small = new SKBitmap(2, 2);
        small.Erase(SKColors.Blue);
        var actual = SavePng(small, "dimensions.actual.png");
        var attempts = 0;
        await Golden.VerifyAsync(
            GoldenInput.Capture(() =>
            {
                if (++attempts == 2)
                    File.Copy(expected, actual, overwrite: true);
                return Task.FromResult(actual);
            }),
            GoldenInput.Path(expected),
            new(expectedSize: new(4, 4)),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task ScreenshotComparisonConfirmsFirstMismatchAfterDeadline()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "deadline.expected.png");
        using var small = new SKBitmap(2, 2);
        small.Erase(SKColors.Blue);
        var actual = SavePng(small, "deadline.actual.png");
        var attempts = 0;

        await Golden.VerifyAsync(
            GoldenInput.Capture(async () =>
            {
                if (++attempts == 1)
                {
                    Thread.Sleep(150);
                }
                else
                {
                    File.Copy(expected, actual, overwrite: true);
                    await Task.Delay(20, TestContext.Current.CancellationToken);
                }
                return actual;
            }),
            GoldenInput.Path(expected),
            new(expectedSize: new(4, 4)),
            TimeSpan.FromMilliseconds(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, attempts);
    }

    [Theory]
    [InlineData("missing-reference")]
    [InlineData("corrupt-reference")]
    [InlineData("corrupt-capture")]
    [InlineData("capture-error")]
    [InlineData("canceled-capture")]
    public async Task ScreenshotComparisonDoesNotRetryErrors(string failure)
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "error.expected.png");
        var actual = SavePng(bitmap, "error.actual.png");
        if (failure == "missing-reference")
            File.Delete(expected);
        if (failure == "corrupt-reference")
            File.WriteAllText(expected, "not a PNG");
        if (failure == "corrupt-capture")
            File.WriteAllText(actual, "not a PNG");
        var attempts = 0;
        var sentinel = new InvalidOperationException("Browser capture failed.");

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Golden.VerifyAsync(
                GoldenInput.Capture(() =>
                {
                    attempts++;
                    return failure switch
                    {
                        "capture-error" => Task.FromException<string>(sentinel),
                        "canceled-capture" => Task.FromCanceled<string>(new CancellationToken(canceled: true)),
                        _ => Task.FromResult(actual)
                    };
                }),
                GoldenInput.Path(expected),
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(1, attempts);
        if (failure == "capture-error")
            Assert.Same(sentinel, error);
        if (failure == "canceled-capture")
            Assert.IsAssignableFrom<OperationCanceledException>(error);
        if (failure == "missing-reference")
            Assert.False(File.Exists(expected));
    }

    [Fact]
    public async Task ScreenshotComparisonBoundsCaptureTime()
    {
        using var bitmap = new SKBitmap(1, 1);
        var expected = SavePng(bitmap, "timeout.expected.png");
        var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            await Assert.ThrowsAsync<TimeoutException>(() =>
                Golden.VerifyAsync(
                    GoldenInput.Capture(() => pending.Task),
                    GoldenInput.Path(expected),
                    timeout: TimeSpan.FromMilliseconds(100),
                    cancellationToken: TestContext.Current.CancellationToken));
        }
        finally
        {
            pending.TrySetCanceled(TestContext.Current.CancellationToken);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ScreenshotComparisonRejectsInvalidTimeoutBeforeCapture(int milliseconds)
    {
        var attempts = 0;
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            Golden.VerifyAsync(
                GoldenInput.Capture(() =>
                {
                    attempts++;
                    return Task.FromResult("unused.png");
                }),
                GoldenInput.Path("unused.png"),
                timeout: TimeSpan.FromMilliseconds(milliseconds),
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(0, attempts);
    }

    [Fact]
    public async Task ImageMustDecodeAndHaveTheExpectedSize()
    {
        using var bitmap = new SKBitmap(800, 600);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var expected = SavePng(bitmap, "decode.expected.png");
        Golden.Verify(
            GoldenInput.Path(WriteFile(
                Path.Combine(root, "decode.actual.png"),
                data.ToArray())),
            GoldenInput.Path(expected),
            new(expectedSize: new(800, 600)));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.Run(() => Golden.Verify(
                GoldenInput.Path(WriteFile(
                    Path.Combine(root, "invalid.actual.png"),
                    data.ToArray().AsSpan(0, 12).ToArray())),
                GoldenInput.Path(expected),
                new(expectedSize: new(800, 600)))));
        var invalidSignature = Path.Combine(root, "invalid-signature.actual.png");
        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.Run(() => Golden.Verify(
                GoldenInput.Path(WriteFile(
                    invalidSignature,
                    new byte[] { 1, 2, 3 })),
                GoldenInput.Path(expected),
                new(expectedSize: new(800, 600)))));
        Assert.Contains("Failed to decode image", error.Message);
        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(invalidSignature));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.Run(() => Golden.Verify(
                GoldenInput.Path(WriteFile(
                    Path.Combine(root, "wrong-size.actual.png"),
                    data.ToArray())),
                GoldenInput.Path(expected),
                new(expectedSize: new(400, 300)))));
    }

    [Fact]
    public void RawPixelInputsValidateImageInfoAndBufferSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GoldenInput.Pixels(new byte[1], default));

        var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
        Assert.Throws<ArgumentException>(() =>
            GoldenInput.Pixels(new byte[info.BytesSize - 1], info));

        var pixels = new byte[info.BytesSize];
        Golden.Verify(
            GoldenInput.Pixels(pixels, info),
            GoldenInput.Pixels(pixels.AsSpan(), info));
    }

    [Theory]
    [InlineData(80, 50, 0, true)]
    [InlineData(80, 50, 3, true)]
    [InlineData(80, 50, 4, false)]
    [InlineData(80, 51, 3, true)]
    [InlineData(80, 51, 4, false)]
    [InlineData(800, 600, 259, true)]
    [InlineData(800, 600, 360, true)]
    [InlineData(800, 600, 361, false)]
    [InlineData(512, 512, 177, true)]
    [InlineData(512, 512, 196, true)]
    [InlineData(512, 512, 197, false)]
    public void GoldenComparisonAllowsAtMostPointZeroSevenFivePercentOfPixels(
        int width, int height, int changedPixels, bool accepted)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "expected.png");
        for (var i = 0; i < changedPixels; i++)
            bitmap.SetPixel(i % width, i / width, SKColors.White);
        var actual = SavePng(bitmap, "actual.png");

        if (accepted)
        {
            Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected));
            Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        }
        else
        {
            var error = Assert.Throws<Xunit.Sdk.FailException>(() =>
                Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected)));
            Assert.Contains($"{changedPixels}/{width * height} pixels", error.Message);
            Assert.Contains("exceeding", error.Message);
            Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        }
    }

    [Theory]
    [InlineData(1, 0, 0, 255)]
    [InlineData(0, 0, 0, 254)]
    public void GoldenComparisonCountsEvenSingleChannelAndAlphaChanges(byte red, byte green, byte blue, byte alpha)
    {
        using var bitmap = new SKBitmap(80, 50);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "expected.png");
        bitmap.Erase(new SKColor(red, green, blue, alpha));
        var actual = SavePng(bitmap, "actual.png");

        var error = Assert.Throws<Xunit.Sdk.FailException>(() =>
            Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected)));
        Assert.Contains("4000/4000 pixels", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    private string SavePng(SKBitmap bitmap, string name)
    {
        return WriteFile(Path.Combine(root, name), EncodePng(bitmap));
    }

    private static byte[] EncodePng(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static string WriteFile(string path, byte[] bytes)
    {
        if (Path.GetDirectoryName(path) is { Length: > 0 } directory)
            Directory.CreateDirectory(directory);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    [Fact]
    public async Task ComparisonArgumentsApplyToSavedFilesAndDirectComparisons()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "options.expected.png");
        var actual = Path.Combine(root, "options.actual.png");
        Golden.Verify(
            GoldenInput.Path(WriteFile(actual, File.ReadAllBytes(expected))),
            GoldenInput.Path(expected),
            new(expectedSize: new(16, 12)));
        Golden.Verify(
            GoldenInput.Path(actual),
            GoldenInput.Path(expected),
            new(expectedSize: new(16, 12)));
        Assert.Contains("options.actual.png-2", TestContext.Current.Attachments!.Keys);
        Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(
                GoldenInput.Path(actual),
                GoldenInput.Path(expected),
                new(expectedSize: new(15, 12))));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.Run(() => Golden.Verify(
                GoldenInput.Path(WriteFile(
                    Path.Combine(root, "wrong-size.actual.png"),
                    File.ReadAllBytes(expected))),
                GoldenInput.Path(expected),
                new(expectedSize: new(15, 12)))));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void ImageOptionsRejectInvalidAverageColorErrorLimits(double limit)
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "invalid-options.expected.png");
        var actual = SavePng(bitmap, "invalid-options.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(
                GoldenInput.Path(actual),
                GoldenInput.Path(expected),
                new(maxAverageColorErrorFraction: limit)));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void ImageOptionsApplyAnExplicitPixelBudget(int changedPixels, bool accepted)
    {
        using var bitmap = new SKBitmap(10, 10);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "pixel-options.expected.png");
        for (var i = 0; i < changedPixels; i++)
            bitmap.SetPixel(i, 0, SKColors.White);
        var actual = SavePng(bitmap, "pixel-options.actual.png");
        if (accepted)
            Golden.Verify(
                GoldenInput.Path(actual),
                GoldenInput.Path(expected),
                new(maxErrorPixelFraction: 0.01));
        else
            Assert.Throws<Xunit.Sdk.FailException>(() =>
                Golden.Verify(
                    GoldenInput.Path(actual),
                    GoldenInput.Path(expected),
                    new(maxErrorPixelFraction: 0.01)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(double.NaN)]
    public void ImageOptionsRejectInvalidPixelBudgets(double limit)
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "invalid-pixel-options.expected.png");
        var actual = SavePng(bitmap, "invalid-pixel-options.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(
                GoldenInput.Path(actual),
                GoldenInput.Path(expected),
                new(maxErrorPixelFraction: limit)));
    }

    [Fact]
    public void GoldenComparisonSupportsPageSizesAndRetainsPixelDifferences()
    {
        Directory.CreateDirectory(root);
        var actual = Path.Combine(root, "page.actual.png");
        var expected = Path.Combine(root, "page.expected.png");
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            File.WriteAllBytes(actual, data.ToArray());
        File.Copy(actual, expected);
        Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected));

        bitmap.Erase(SKColors.Blue);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            File.WriteAllBytes(actual, data.ToArray());
        var error = Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected)));
        Assert.Contains("192/192", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void MissingComparisonImageFailsWithoutCreatingAReference()
    {
        Directory.CreateDirectory(root);
        var actual = Path.Combine(root, "missing-reference.actual.png");
        var expected = Path.Combine(root, "missing-reference.png");
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(actual, data.ToArray());
        Assert.Throws<FileNotFoundException>(() =>
            Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected)));
        Assert.True(File.Exists(actual));
        Assert.False(File.Exists(expected));
    }

    [Fact]
    public void AverageColorErrorAllowsExactlyFivePercentButRejectsOneChannelStepOver()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "average.expected.png");
        bitmap.Erase(new SKColor(238, 17, 17));
        var actual = SavePng(bitmap, "average.actual.png");
        Golden.Verify(
            GoldenInput.Path(actual),
            GoldenInput.Path(expected),
            new(maxAverageColorErrorFraction: 0.05));
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));

        bitmap.SetPixel(0, 0, new SKColor(238, 18, 17));
        SavePng(bitmap, "average.actual.png");
        var error = Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(
                GoldenInput.Path(actual),
                GoldenInput.Path(expected),
                new(maxAverageColorErrorFraction: 0.05)));
        Assert.Contains("average RGBA color error", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void AverageColorErrorDoesNotChangeTheHeadlessPixelBudgetOrExplicitExactComparison()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "channel.expected.png");
        bitmap.Erase(new SKColor(247, 8, 8));
        var actual = SavePng(bitmap, "channel.actual.png");
        Golden.Verify(
            GoldenInput.Path(actual),
            GoldenInput.Path(expected),
            new(maxAverageColorErrorFraction: 0.05));
        var error = Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(GoldenInput.Path(actual), GoldenInput.Path(expected)));
        Assert.Contains("192/192", error.Message);
        Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(
                GoldenInput.Path(actual),
                GoldenInput.Path(expected),
                new(maxAverageColorErrorFraction: 0)));
    }

    [Fact]
    public void AverageColorErrorIncludesAlphaInItsFivePercentBoundary()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "alpha.expected.png");
        bitmap.Erase(new SKColor(0, 0, 0, 204));
        var actual = SavePng(bitmap, "alpha.actual.png");
        Golden.Verify(
            GoldenInput.Path(actual),
            GoldenInput.Path(expected),
            new(maxAverageColorErrorFraction: 0.05));

        bitmap.SetPixel(0, 0, new SKColor(0, 0, 0, 203));
        SavePng(bitmap, "alpha.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(
                GoldenInput.Path(actual),
                GoldenInput.Path(expected),
                new(maxAverageColorErrorFraction: 0.05)));
    }

    [Fact]
    public void AverageColorErrorAllowsExactlyTheGpuSixPercentBudget()
    {
        using var bitmap = new SKBitmap(5, 1);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "gpu-budget.expected.png");
        bitmap.Erase(new SKColor(234, 20, 20));
        bitmap.SetPixel(0, 0, new SKColor(233, 20, 20));
        var actual = SavePng(bitmap, "gpu-budget.actual.png");
        Golden.Verify(
            GoldenInput.Path(actual),
            GoldenInput.Path(expected),
            new(maxAverageColorErrorFraction: 0.06));

        bitmap.SetPixel(1, 0, new SKColor(233, 20, 20));
        SavePng(bitmap, "gpu-budget.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            Golden.Verify(
                GoldenInput.Path(actual),
                GoldenInput.Path(expected),
                new(maxAverageColorErrorFraction: 0.06)));
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
            SampleWorkspace.DeleteDirectory(root);
    }
}
