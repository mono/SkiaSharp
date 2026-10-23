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
    public async Task TypedGoldenComparisonUsesPackagedBytesAndRetainsStrictPolicy()
    {
        var request = SampleImage.Host("Console/output");
        var reference = SampleImage.Store.Resolve(request);
        Assert.NotNull(reference.Location);
        Assert.StartsWith(AppContext.BaseDirectory, reference.Location);
        var actual = Path.Combine(root, "typed.actual.png");
        SampleImage.Verifier.Verify(request, GoldenImageSource.WritePng(reference.ReadBytes(), actual),
            new() { ExpectedSize = new(800, 600), MaxErrorPixelFraction = 0 });
        using var bitmap = SKBitmap.Decode(reference.ReadBytes());
        Assert.NotNull(bitmap);
        bitmap.Erase(SKColors.Red);
        actual = SavePng(bitmap, "typed-mismatch.actual.png");
        var error = Assert.Throws<Xunit.Sdk.FailException>(() =>
            SampleImage.Verifier.Verify(request, actual, new() { MaxErrorPixelFraction = 0 }));
        Assert.Contains("Golden images differ", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void MissingTypedGoldenReportsOrderedAttemptsAndRetainsActual()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var actual = SavePng(bitmap, "typed-missing.actual.png");
        var request = new GoldenRequest("missing-profile", "Missing/case", new Version(12, 6, 2), "arm64", "engine");
        var resolution = SampleImage.Store.Resolve(request);
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Verifier.Verify(request, actual));
        Assert.Null(resolution.Key);
        Assert.Contains(resolution.Describe(), error.Message);
        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        Assert.False(File.Exists(Path.Combine(SampleImage.Root, resolution.Candidates[0])));
    }

    [Fact]
    public void InvalidTypedCaptureReportsTheComparisonFailureWithoutDiagnosticMasking()
    {
        var actual = Path.Combine(root, "invalid-typed.actual.png");
        Directory.CreateDirectory(root);
        File.WriteAllBytes(actual, new byte[] { 1, 2, 3 });

        var error = Assert.ThrowsAny<Exception>(() =>
            SampleImage.Verifier.Verify(SampleImage.Host("Console/output"), actual));

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
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Verifier.Verify(actual, expected));
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
    public async Task ScreenshotComparisonRetriesUntilItMatches()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        var expected = SavePng(bitmap, "retry.expected.png");
        bitmap.Erase(SKColors.Red);
        var actual = SavePng(bitmap, "retry.actual.png");
        var attempts = 0;

        var matched = await SampleImage.Verifier.VerifyAsync(
            () =>
            {
                if (++attempts == 2)
                    File.Copy(expected, actual, overwrite: true);
                return Task.FromResult(GoldenImageSource.FromFile(actual));
            },
            expected,
            null,
            SampleImage.DefaultTimeout,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, attempts);
        Assert.Equal(actual, matched);
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        Assert.DoesNotContain("expected-" + Path.GetFileName(expected), TestContext.Current.Attachments.Keys);
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
            SampleImage.Verifier.VerifyAsync(
                () => Task.FromResult(GoldenImageSource.FromFile(actual)),
                expected,
                null,
                TimeSpan.FromMilliseconds(150),
                TestContext.Current.CancellationToken));

        Assert.Contains(Path.GetFileName(actual), TestContext.Current.Attachments!.Keys);
        if (!wrongSize)
        {
            Assert.Contains("Golden images differ", error.Message);
            Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
            Assert.Contains("expected-" + Path.GetFileName(expected), TestContext.Current.Attachments.Keys);
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
        await SampleImage.Verifier.VerifyAsync(
            () =>
            {
                if (++attempts == 2)
                    File.Copy(expected, actual, overwrite: true);
                return Task.FromResult(GoldenImageSource.FromFile(actual));
            },
            expected,
            new() { ExpectedSize = new(4, 4) },
            SampleImage.DefaultTimeout,
            TestContext.Current.CancellationToken);
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
            SampleImage.Verifier.VerifyAsync(
                () =>
                {
                    attempts++;
                    return failure switch
                    {
                        "capture-error" => Task.FromException<GoldenImageSource>(sentinel),
                        "canceled-capture" => Task.FromCanceled<GoldenImageSource>(new CancellationToken(canceled: true)),
                        _ => Task.FromResult(GoldenImageSource.FromFile(actual))
                    };
                },
                expected,
                null,
                SampleImage.DefaultTimeout,
                TestContext.Current.CancellationToken));

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
        var pending = new TaskCompletionSource<GoldenImageSource>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            await Assert.ThrowsAsync<TimeoutException>(() =>
                SampleImage.Verifier.VerifyAsync(
                    () => pending.Task,
                    "unused.png",
                    null,
                    TimeSpan.FromMilliseconds(100),
                    TestContext.Current.CancellationToken));
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
            SampleImage.Verifier.VerifyAsync(
                () =>
                {
                    attempts++;
                    return Task.FromResult(GoldenImageSource.FromFile("unused.png"));
                },
                "unused.png",
                null,
                TimeSpan.FromMilliseconds(milliseconds),
                TestContext.Current.CancellationToken));
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
        var options = new GoldenComparisonOptions { ExpectedSize = new(800, 600) };
        SampleImage.Verifier.Verify(
            GoldenImageSource.WritePng(data.ToArray(), Path.Combine(root, "decode.actual.png")),
            expected,
            options);
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.Run(() => SampleImage.Verifier.Verify(
                GoldenImageSource.WritePng(
                    data.ToArray().AsSpan(0, 12).ToArray(),
                    Path.Combine(root, "invalid.actual.png")),
                expected,
                options)));
        var invalidSignature = Path.Combine(root, "invalid-signature.actual.png");
        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.Run(() => SampleImage.Verifier.Verify(
                GoldenImageSource.WritePng(new byte[] { 1, 2, 3 }, invalidSignature),
                expected,
                options)));
        Assert.Contains("Expected a PNG image", error.Message);
        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(invalidSignature));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.Run(() => SampleImage.Verifier.Verify(
                GoldenImageSource.WritePng(data.ToArray(), Path.Combine(root, "wrong-size.actual.png")),
                expected,
                new GoldenComparisonOptions { ExpectedSize = new(400, 300) })));
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
            SampleImage.Verifier.Verify(actual, expected);
            Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        }
        else
        {
            var error = Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Verifier.Verify(actual, expected));
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

        var error = Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Verifier.Verify(actual, expected));
        Assert.Contains("4000/4000 pixels", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    private string SavePng(SKBitmap bitmap, string name)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, name);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
        return path;
    }

    [Fact]
    public async Task ImageOptionsApplyToSavedFilesAndDirectComparisonsWithoutMutation()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "options.expected.png");
        var actual = Path.Combine(root, "options.actual.png");
        var options = new GoldenComparisonOptions { ExpectedSize = new(16, 12) };

        SampleImage.Verifier.Verify(GoldenImageSource.WritePng(File.ReadAllBytes(expected), actual), expected, options);
        SampleImage.Verifier.Verify(actual, expected, options);
        Assert.Contains("options.actual.png-2", TestContext.Current.Attachments!.Keys);
        Assert.Equal(new SKSizeI(16, 12), options.ExpectedSize);
        Assert.Equal(0.00075, options.MaxErrorPixelFraction);
        Assert.Null(options.MaxAverageColorErrorFraction);

        var wrongSize = new GoldenComparisonOptions { ExpectedSize = new(15, 12) };
        Assert.ThrowsAny<Exception>(() => SampleImage.Verifier.Verify(actual, expected, wrongSize));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.Run(() => SampleImage.Verifier.Verify(
                GoldenImageSource.WritePng(
                    File.ReadAllBytes(expected),
                    Path.Combine(root, "wrong-size.actual.png")),
                expected,
                wrongSize)));
        Assert.Equal(new SKSizeI(16, 12), options.ExpectedSize);
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
        Assert.ThrowsAny<Exception>(() => SampleImage.Verifier.Verify(actual, expected,
            new() { MaxAverageColorErrorFraction = limit }));
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
        var options = new GoldenComparisonOptions { MaxErrorPixelFraction = 0.01 };
        if (accepted)
            SampleImage.Verifier.Verify(actual, expected, options);
        else
            Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Verifier.Verify(actual, expected, options));
        Assert.Equal(0.00075, new GoldenComparisonOptions().MaxErrorPixelFraction);
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
            SampleImage.Verifier.Verify(actual, expected, new() { MaxErrorPixelFraction = limit }));
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
        SampleImage.Verifier.Verify(actual, expected);

        bitmap.Erase(SKColors.Blue);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            File.WriteAllBytes(actual, data.ToArray());
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Verifier.Verify(actual, expected));
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
        Assert.Throws<FileNotFoundException>(() => SampleImage.Verifier.Verify(actual, expected));
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
        SampleImage.Verifier.Verify(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));

        bitmap.SetPixel(0, 0, new SKColor(238, 18, 17));
        SavePng(bitmap, "average.actual.png");
        var error = Assert.ThrowsAny<Exception>(() =>
            SampleImage.Verifier.Verify(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 }));
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
        SampleImage.Verifier.Verify(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Verifier.Verify(actual, expected));
        Assert.Contains("192/192", error.Message);
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Verifier.Verify(actual, expected, new() { MaxAverageColorErrorFraction = 0 }));
    }

    [Fact]
    public void AverageColorErrorIncludesAlphaInItsFivePercentBoundary()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "alpha.expected.png");
        bitmap.Erase(new SKColor(0, 0, 0, 204));
        var actual = SavePng(bitmap, "alpha.actual.png");
        SampleImage.Verifier.Verify(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });

        bitmap.SetPixel(0, 0, new SKColor(0, 0, 0, 203));
        SavePng(bitmap, "alpha.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Verifier.Verify(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 }));
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
        SampleImage.Verifier.Verify(actual, expected, new() { MaxAverageColorErrorFraction = 0.06 });

        bitmap.SetPixel(1, 0, new SKColor(233, 20, 20));
        SavePng(bitmap, "gpu-budget.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Verifier.Verify(actual, expected, new() { MaxAverageColorErrorFraction = 0.06 }));
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
            SampleWorkspace.DeleteDirectory(root);
    }
}
