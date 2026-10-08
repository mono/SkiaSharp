using SkiaSharp;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class ScreenshotVerifier(string screenshotDir, ITestOutputHelper output)
{
    internal async Task SaveScreenshot(byte[] imageBytes, string name)
    {
        var path = Path.Combine(screenshotDir, $"{name}.png");
        await File.WriteAllBytesAsync(path, imageBytes);
        output.WriteLine($"Screenshot saved: {path}");
    }

    internal async Task VerifyScreenshot(byte[] screenshot, string name, string? platform = null)
    {
        await SaveScreenshot(screenshot, name);
        var similarity = await CompareAndSaveDiff(TestImage.GetReferenceImage(platform), screenshot, name);
        output.WriteLine($"Image similarity: {similarity:F1}%");
        Assert.True(similarity >= 95, $"Image similarity too low: {similarity:F1}% (expected >= 95%)");
    }

    private async Task<double> CompareAndSaveDiff(byte[] expected, byte[] actual, string diffName)
    {
        using var expectedImage = SKImage.FromEncodedData(expected);
        using var actualImage = SKImage.FromEncodedData(actual);

        if (expectedImage == null || actualImage == null)
            throw new InvalidOperationException($"Could not decode {(expectedImage == null ? "reference" : "actual")} screenshot for {diffName}; saved screenshot: {Path.Combine(screenshotDir, $"{diffName}.png")}");

        SKImage compareActual = actualImage;
        if (actualImage.Width != expectedImage.Width || actualImage.Height != expectedImage.Height)
        {
            output.WriteLine($"Resizing actual ({actualImage.Width}x{actualImage.Height}) to match expected ({expectedImage.Width}x{expectedImage.Height})");
            using var resizedBitmap = new SKBitmap(expectedImage.Width, expectedImage.Height);
            using var canvas = new SKCanvas(resizedBitmap);
            canvas.DrawImage(actualImage, new SKRect(0, 0, expectedImage.Width, expectedImage.Height), SKSamplingOptions.Default);
            compareActual = SKImage.FromBitmap(resizedBitmap);
        }

        try
        {
            var result = SkiaSharp.Extended.SKPixelComparer.Compare(expectedImage, compareActual);
            var similarity = result.TotalPixels > 0
                ? (1.0 - (double)result.ErrorPixelCount / result.TotalPixels) * 100
                : 0;

            output.WriteLine($"Comparison: {result.TotalPixels} total, {result.ErrorPixelCount} errors, {result.AbsoluteError} absolute error");

            using var diffImage = SkiaSharp.Extended.SKPixelComparer.GenerateDifferenceMask(expectedImage, compareActual);
            using var diffData = diffImage.Encode(SKEncodedImageFormat.Png, 100);
            var diffPath = Path.Combine(screenshotDir, $"{diffName}-diff.png");
            await File.WriteAllBytesAsync(diffPath, diffData.ToArray());
            output.WriteLine($"Diff image saved: {diffPath}");

            return similarity;
        }
        finally
        {
            if (compareActual != actualImage)
                compareActual.Dispose();
        }
    }
}
