using System.Diagnostics;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Base class for platform integration tests that create and build real applications.
/// </summary>
public abstract class PlatformTestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly string TestDir;
    protected readonly string SkiaVersion;
    protected readonly string HarfBuzzVersion;
    protected readonly string ScreenshotDir;

    protected PlatformTestBase(ITestOutputHelper output)
    {
        Output = output;

        // Resolve symlinks to avoid macOS /var -> /private/var issue that breaks Razor compilation
        // When using full paths with /var/..., Razor compiler fails to find sibling folders
        var tempPath = Path.GetTempPath();
        if (tempPath.StartsWith("/var/") && Directory.Exists("/private/var"))
            tempPath = "/private" + tempPath;

        TestDir = Path.Combine(tempPath, $"skiasharp-{GetType().Name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(TestDir);
        
        // Setup screenshot output directory (repo root/output/logs/testlogs/integration)
        var repoRoot = FindRepoRoot();
        ScreenshotDir = Path.Combine(repoRoot, "output", "logs", "testlogs", "integration");
        Directory.CreateDirectory(ScreenshotDir);
        
        // Write nuget.config to TestDir for package resolution
        File.WriteAllText(Path.Combine(TestDir, "nuget.config"), """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
                <add key="SkiaSharp Preview" value="https://aka.ms/skiasharp-eap/index.json" />
                <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
              </packageSources>
            </configuration>
            """);
        
        // Write global.json to allow latest SDK (prevents inheriting repo's .NET 8 restriction)
        File.WriteAllText(Path.Combine(TestDir, "global.json"), """
            {
              "sdk": {
                "version": "8.0.0",
                "rollForward": "latestMajor"
              }
            }
            """);
        
        SkiaVersion = AppContext.GetData("SkiaSharpVersion") as string 
            ?? throw new InvalidOperationException("SkiaSharpVersion not set");

        HarfBuzzVersion = AppContext.GetData("HarfBuzzSharpVersion") as string 
            ?? throw new InvalidOperationException("HarfBuzzSharpVersion not set");
    }

	public void Dispose()
    {
        try { Directory.Delete(TestDir, recursive: true); } catch { }
    }

    /// <summary>
    /// Saves a screenshot to the output/logs/testlogs/integration directory for manual review.
    /// </summary>
    protected async Task SaveScreenshot(byte[] imageBytes, string name)
    {
        var filename = $"{name}.png";
        var path = Path.Combine(ScreenshotDir, filename);
        await File.WriteAllBytesAsync(path, imageBytes);
        Output.WriteLine($"Screenshot saved: {path}");
    }

    /// <summary>
    /// Verifies a screenshot against a reference image. Saves the screenshot, compares with reference,
    /// saves a diff image, and asserts similarity >= 95%.
    /// </summary>
    /// <param name="screenshot">The screenshot bytes to verify</param>
    /// <param name="name">Name for the screenshot file (without extension)</param>
    /// <param name="platform">Platform name for reference image lookup (e.g., "blazor", "ios"), or null for base reference</param>
    protected async Task VerifyScreenshot(byte[] screenshot, string name, string? platform = null)
    {
        await SaveScreenshot(screenshot, name);
        
        var referenceImage = TestImage.GetReferenceImage(platform);
        var similarity = await CompareAndSaveDiff(referenceImage, screenshot, name);
        Output.WriteLine($"Image similarity: {similarity:F1}%");
        Assert.True(similarity >= 95, $"Image similarity too low: {similarity:F1}% (expected >= 95%)");
    }

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
        
        canvas.DrawBitmap(original, rect, SKRect.Create(rect.Width, rect.Height));
        
        using var image = SKImage.FromBitmap(cropped);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>
    /// Compares two images using SKPixelComparer and saves a diff image.
    /// Returns the similarity percentage (0-100).
    /// </summary>
    protected async Task<double> CompareAndSaveDiff(byte[] expected, byte[] actual, string diffName)
    {
        using var expectedImage = SKImage.FromEncodedData(expected);
        using var actualImage = SKImage.FromEncodedData(actual);
        
        if (expectedImage == null || actualImage == null)
        {
            Output.WriteLine("Failed to decode one or both images for comparison");
            return 0;
        }
        
        // Resize actual to match expected if different sizes
        SKImage compareActual = actualImage;
        if (actualImage.Width != expectedImage.Width || actualImage.Height != expectedImage.Height)
        {
            Output.WriteLine($"Resizing actual ({actualImage.Width}x{actualImage.Height}) to match expected ({expectedImage.Width}x{expectedImage.Height})");
            using var resizedBitmap = new SKBitmap(expectedImage.Width, expectedImage.Height);
            using var canvas = new SKCanvas(resizedBitmap);
            canvas.DrawImage(actualImage, new SKRect(0, 0, expectedImage.Width, expectedImage.Height));
            compareActual = SKImage.FromBitmap(resizedBitmap);
        }
        
        try
        {
            // Compare using SKPixelComparer
            var result = SkiaSharp.Extended.SKPixelComparer.Compare(expectedImage, compareActual);
            var similarity = result.TotalPixels > 0 
                ? (1.0 - (double)result.ErrorPixelCount / result.TotalPixels) * 100 
                : 0;
            
            Output.WriteLine($"Comparison: {result.TotalPixels} total, {result.ErrorPixelCount} errors, {result.AbsoluteError} absolute error");
            
            // Generate and save diff image
            using var diffImage = SkiaSharp.Extended.SKPixelComparer.GenerateDifferenceMask(expectedImage, compareActual);
            using var diffData = diffImage.Encode(SKEncodedImageFormat.Png, 100);
            var diffPath = Path.Combine(ScreenshotDir, $"{diffName}-diff.png");
            await File.WriteAllBytesAsync(diffPath, diffData.ToArray());
            Output.WriteLine($"Diff image saved: {diffPath}");
            
            return similarity;
        }
        finally
        {
            if (compareActual != actualImage)
                compareActual.Dispose();
        }
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "build.cake")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }
        // Fallback to current directory
        return Directory.GetCurrentDirectory();
    }

    protected Task<string> Run(string command, string args, int timeoutSeconds = 120)
        => Run(command, args, TestDir, timeoutSeconds);

    protected async Task<string> Run(string command, string args, string workingDirectory, int timeoutSeconds = 120)
    {
        Output.WriteLine($"$ {command} {args}");
        Output.WriteLine($"  WorkingDirectory: {workingDirectory}");
        
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        // Clear all DOTNET_* and MSBUILD* env vars to prevent SDK pinning from parent process
        var keysToRemove = psi.Environment.Keys
            .Where(k => k.StartsWith("DOTNET_", StringComparison.OrdinalIgnoreCase) ||
                        k.StartsWith("MSBUILD", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var key in keysToRemove)
            psi.Environment.Remove(key);
        
        using var process = Process.Start(psi)!;
        
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        if (!process.WaitForExit(timeoutSeconds * 1000))
        {
            process.Kill();
            throw new TimeoutException($"Command timed out after {timeoutSeconds}s");
        }
        
        var combined = output + error;
        if (process.ExitCode != 0)
        {
            Output.WriteLine(combined);
            throw new Exception($"Command failed with exit code {process.ExitCode}:\n{combined}");
        }
        
        return combined;
    }
}
