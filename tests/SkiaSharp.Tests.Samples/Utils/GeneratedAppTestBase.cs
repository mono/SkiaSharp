using SkiaSharp;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

/// <summary>
/// Base class for platform integration tests that create and build real applications.
/// </summary>
public abstract class GeneratedAppTestBase : SampleTestBase
{
    /// <summary>
    /// Declared framework of the preserved Integration probe applications.
    /// </summary>
    protected internal const string BaseFramework = "net10.0";
    protected readonly string TestDir;
    protected readonly string SkiaVersion;
    protected readonly string ScreenshotDir;
    private readonly DotNet workspace;

    protected GeneratedAppTestBase(ITestOutputHelper output) : base(output)
    {
        SkiaVersion = DotNet.Setting("SkiaSharpVersion");
        workspace = PrepareGeneratedProject();
        TestDir = Path.Combine(workspace.Root, $"app-{GetType().Name}");
        Directory.CreateDirectory(TestDir);

        ScreenshotDir = Path.Combine(workspace.DiagnosticsRoot, "screenshots");
        Directory.CreateDirectory(ScreenshotDir);
    }

    private protected Task<LocalWebServer> StartWebServer(string project) =>
        workspace.StartWebServer(project, Output, "Release");

    /// <summary>
    /// Saves a screenshot to the sample test diagnostics directory.
    /// </summary>
    protected Task SaveScreenshot(byte[] imageBytes, string name) =>
        new ScreenshotVerifier(ScreenshotDir, Output).SaveScreenshot(imageBytes, name);

    /// <summary>
    /// Verifies a screenshot against a reference image. Saves the screenshot, compares with reference,
    /// saves a diff image, and asserts similarity >= 95%.
    /// </summary>
    /// <param name="screenshot">The screenshot bytes to verify</param>
    /// <param name="name">Name for the screenshot file (without extension)</param>
    /// <param name="platform">Platform name for reference image lookup (e.g., "blazor", "ios"), or null for base reference</param>
    protected Task VerifyScreenshot(byte[] screenshot, string name, string? platform = null) =>
        new ScreenshotVerifier(ScreenshotDir, Output).VerifyScreenshot(screenshot, name, platform);

    /// <summary>
    /// Crops a PNG image to the specified rectangle.
    /// </summary>
    protected static byte[] CropImage(byte[] pngBytes, SKRectI cropRect)
    {
        using var original = SKBitmap.Decode(pngBytes) ?? throw new InvalidOperationException("Failed to decode image");

        // Clamp to image bounds
        var rect = SKRectI.Intersect(cropRect, SKRectI.Create(original.Width, original.Height));

        using var cropped = new SKBitmap(rect.Width, rect.Height);
        using var canvas = new SKCanvas(cropped);

        canvas.DrawBitmap(original, rect, SKRect.Create(rect.Width, rect.Height), SKSamplingOptions.Default);

        using var image = SKImage.FromBitmap(cropped);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    protected Task<string> RunDotNet(IEnumerable<string> arguments, int timeoutSeconds = 120) =>
        workspace.RunCommand(arguments, TestDir, TimeSpan.FromSeconds(timeoutSeconds), Output);
}
