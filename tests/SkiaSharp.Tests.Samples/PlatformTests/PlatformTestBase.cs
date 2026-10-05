using System.Diagnostics;
using SkiaSharp;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples.PlatformTests;

/// <summary>
/// Base class for platform integration tests that create and build real applications.
/// </summary>
public abstract class PlatformTestBase : IDisposable
{
    /// <summary>
    /// Framework of the package consumers, shared with the sample tests.
    /// </summary>
    protected static string BaseFramework => DotNet.Setting("ConsumerTargetFramework");
    protected static string DotNetHost => DotNet.Setting("DotNetHost");

    protected readonly ITestOutputHelper Output;
    protected readonly string TestDir;
    protected readonly string SkiaVersion;
    protected readonly string HarfBuzzVersion;
    protected readonly string ScreenshotDir;
    private readonly DotNet workspace;

    protected PlatformTestBase(ITestOutputHelper output)
    {
        ManualPlatformPolicy.RequireLocalOptIn();
        Output = output;
        SkiaVersion = DotNet.Setting("SkiaSharpVersion");
        HarfBuzzVersion = DotNet.Setting("HarfBuzzSharpVersion");
        workspace = new DotNet();
        TestDir = Path.Combine(workspace.Root, $"platform-{GetType().Name}");
        Directory.CreateDirectory(TestDir);

        ScreenshotDir = Path.Combine(DotNet.Setting("ArtifactsDirectory"), "platform");
        Directory.CreateDirectory(ScreenshotDir);

        WriteNuGetConfig(TestDir);
    }

    protected void WriteNuGetConfig(string directory) =>
        DotNet.WriteNuGetConfig(Path.Combine(directory, "NuGet.Config"), DotNet.Setting("PackageDirectory"));

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(TestDir))
                Directory.Delete(TestDir, recursive: true);
        }
        finally
        {
            workspace.Dispose();
        }
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
        
        canvas.DrawBitmap(original, rect, SKRect.Create(rect.Width, rect.Height), SKSamplingOptions.Default);
        
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
            throw new InvalidOperationException($"Could not decode {(expectedImage == null ? "reference" : "actual")} screenshot for {diffName}; saved screenshot: {Path.Combine(ScreenshotDir, $"{diffName}.png")}");
        
        // Resize actual to match expected if different sizes
        SKImage compareActual = actualImage;
        if (actualImage.Width != expectedImage.Width || actualImage.Height != expectedImage.Height)
        {
            Output.WriteLine($"Resizing actual ({actualImage.Width}x{actualImage.Height}) to match expected ({expectedImage.Width}x{expectedImage.Height})");
            using var resizedBitmap = new SKBitmap(expectedImage.Width, expectedImage.Height);
            using var canvas = new SKCanvas(resizedBitmap);
            canvas.DrawImage(actualImage, new SKRect(0, 0, expectedImage.Width, expectedImage.Height), SKSamplingOptions.Default);
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

    protected Task<string> Run(string command, string args, int timeoutSeconds = 120)
        => Run(command, args, TestDir, timeoutSeconds);

    protected async Task<string> Run(string command, string args, string workingDirectory, int timeoutSeconds = 120)
    {
        Output.WriteLine($"$ {command} {args}");
        Output.WriteLine($"  WorkingDirectory: {workingDirectory}");
        
        var psi = new ProcessStartInfo
        {
            FileName = command == "dotnet" ? DotNetHost : command,
            Arguments = args,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        ConfigureProcess(psi);
        
        using var process = Process.Start(psi)!;
        
        // Start draining both pipes immediately so neither can fill and deadlock,
        // then enforce the timeout via WaitForExit while the reads are in flight.
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        
        if (!process.WaitForExit(timeoutSeconds * 1000))
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException($"Command timed out after {timeoutSeconds}s: {command} {args}\n{await outputTask}\n{await errorTask}");
        }
        
        var output = await outputTask;
        var error = await errorTask;
        
        Output.WriteLine($"Process exited with code {process.ExitCode}");

        var combined = output + error;
        if (process.ExitCode != 0)
        {
            Output.WriteLine(combined);
            throw new Exception($"Command failed with exit code {process.ExitCode}:\n{combined}");
        }
        
        return combined;
    }
    
    /// <summary>
    /// Clears DOTNET_* and MSBUILD* environment variables from a ProcessStartInfo
    /// to prevent SDK pinning from the parent process.
    /// </summary>
    protected static void ClearDotNetEnvironmentVariables(ProcessStartInfo psi)
    {
        var keysToRemove = psi.Environment.Keys
            .Where(k => k.StartsWith("DOTNET_", StringComparison.OrdinalIgnoreCase) ||
                        k.StartsWith("MSBUILD", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var key in keysToRemove)
            psi.Environment.Remove(key);
    }

    protected void ConfigureProcess(ProcessStartInfo psi)
    {
        ClearDotNetEnvironmentVariables(psi);
        var cache = Path.Combine(workspace.Root, "cache");
        psi.Environment["NUGET_PACKAGES"] = Path.Combine(cache, "packages");
        psi.Environment["NUGET_HTTP_CACHE_PATH"] = Path.Combine(cache, "http");
        psi.Environment["NUGET_SCRATCH"] = Path.Combine(cache, "scratch");
        psi.Environment["DOTNET_CLI_HOME"] = Path.Combine(cache, "home");
    }
}
