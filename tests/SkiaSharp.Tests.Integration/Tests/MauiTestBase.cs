using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Base class for MAUI platform tests with shared Appium and project creation logic.
/// Uses the shared AppiumFixture via the "Appium" collection.
/// </summary>
[Collection("Appium")]
[Trait("Category", "Platform")]
public abstract class MauiTestBase(ITestOutputHelper output) : PlatformTestBase(output)
{
    protected int AppiumPort => AppiumFixture.Port;
    
    // Build configuration - set to "Debug" for faster iteration when debugging tests
    protected const string BuildConfiguration = "Release";
    
    // Fixed canvas size matching TestImage dimensions
    protected const int CanvasWidth = 400;
    protected const int CanvasHeight = 300;

    /// <summary>
    /// Platform display name for logging (e.g., "Android", "iOS", "MacCatalyst", "Windows")
    /// </summary>
    protected abstract string PlatformName { get; }
    
    /// <summary>
    /// Target framework moniker (e.g., "net10.0-android", "net10.0-ios")
    /// </summary>
    protected abstract string TargetFramework { get; }
    
    /// <summary>
    /// Check if this platform can run on the current machine.
    /// Return null if it can run, or a skip reason if it cannot.
    /// </summary>
    protected virtual string? CanRunOnCurrentMachine() => null;
    
    /// <summary>
    /// Configure platform-specific Appium options.
    /// </summary>
    protected abstract void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId);
    
    /// <summary>
    /// Create the platform-specific Appium driver.
    /// </summary>
    protected abstract AppiumDriver CreateDriver(AppiumOptions options);
    
    /// <summary>
    /// Find the built app artifact (apk, app bundle, exe, etc.)
    /// </summary>
    protected abstract string? FindAppArtifact(string projectDir, string projectName);
    
    /// <summary>
    /// Extract ApplicationId from a MAUI project's csproj file.
    /// </summary>
    protected static string GetApplicationId(string projectDir, string projectName)
    {
        var csprojPath = Path.Combine(projectDir, $"{projectName}.csproj");
        var content = File.ReadAllText(csprojPath);
        
        // Look for <ApplicationId>...</ApplicationId>
        var startTag = "<ApplicationId>";
        var endTag = "</ApplicationId>";
        var startIndex = content.IndexOf(startTag);
        if (startIndex < 0)
            throw new InvalidOperationException("ApplicationId not found in csproj");
        
        var valueStart = startIndex + startTag.Length;
        var endIndex = content.IndexOf(endTag, valueStart);
        return content[valueStart..endIndex];
    }

	/// <summary>
	/// Get the screen scale factor. Appium returns coordinates in logical points,
	/// but screenshots are in physical pixels. Override in platform tests.
	/// </summary>
	protected virtual double GetScreenScaleFactor(SKSizeI screenshotSize, SKSizeI windowSize)
	{
		// Default: calculate from screenshot vs window size
		var scaleX = (double)screenshotSize.Width / windowSize.Width;
		var scaleY = (double)screenshotSize.Height / windowSize.Height;
		return Math.Max(scaleX, scaleY);
	}

    // Tests - same for all platforms
    [Fact]
    public Task SKCanvasView() => RunMauiTest("SKCanvasView", "SKPaintSurfaceEventArgs");

    [Fact]
    public Task SKGLView() => RunMauiTest("SKGLView", "SKPaintGLSurfaceEventArgs");

    /// <summary>
    /// Run the standard MAUI test for the specified canvas view type.
    /// </summary>
    private async Task RunMauiTest(string canvasView, string eventArgsType)
    {
        var skipReason = CanRunOnCurrentMachine();
        Skip.If(skipReason != null, skipReason);
        
        Output.WriteLine($"Testing SkiaSharp {SkiaVersion} in MAUI {PlatformName} ({canvasView})");
        
        var projectName = $"Maui{PlatformName.Replace(" ", "")}{canvasView}";
        var projectDir = await CreateMauiProject(projectName, canvasView, eventArgsType);
        
        // Always run from TestDir (which has global.json) using relative path
        var relativeProjectDir = Path.GetRelativePath(TestDir, projectDir);
        await Run("dotnet", $"build {relativeProjectDir} -c {BuildConfiguration} -f {TargetFramework}", timeoutSeconds: 600);
        
        var appPath = FindAppArtifact(projectDir, projectName);
        Assert.NotNull(appPath);
        
        var bundleId = GetApplicationId(projectDir, projectName);
        Output.WriteLine($"App path: {appPath}");
        Output.WriteLine($"Bundle ID: {bundleId}");
        
        await VerifyWithAppium(appPath, bundleId, $"maui-{PlatformName.ToLowerInvariant().Replace(" ", "")}-{canvasView}");
        
        Output.WriteLine($"✅ MAUI {PlatformName} ({canvasView}) passed");
    }

    private async Task<string> CreateMauiProject(string projectName, string canvasView, string eventArgsType)
    {
        var projectDir = Path.Combine(TestDir, projectName);
        var relativeProjectDir = projectName;  // Relative to TestDir
        
        // Create project (run from TestDir to pick up global.json)
        await Run("dotnet", $"new maui -n {projectName} -o {relativeProjectDir}");

        // Add SkiaSharp package (run from TestDir)
        await Run("dotnet", $"add {relativeProjectDir} package SkiaSharp.Views.Maui.Controls --version {SkiaVersion}");
        
        // Update MauiProgram.cs
        var programPath = Path.Combine(projectDir, "MauiProgram.cs");
        var program = await File.ReadAllTextAsync(programPath);
        program = "using SkiaSharp.Views.Maui.Controls.Hosting;\n" + program;
        program = program.Replace(".UseMauiApp<App>()", ".UseMauiApp<App>()\n\t\t\t.UseSkiaSharp()");
        await File.WriteAllTextAsync(programPath, program);
        
        // Get test image draw code
        var drawCode = TestImage.GetDrawCode();
        
        // MainPage with specified canvas view - fixed size matching TestImage
        // IgnorePixelScaling ensures consistent canvas size regardless of display scaling
        await File.WriteAllTextAsync(Path.Combine(projectDir, "MainPage.xaml"), $"""
            <?xml version="1.0" encoding="utf-8" ?>
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                         xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls"
                         x:Class="{projectName}.MainPage"
                         BackgroundColor="Black">
                <Grid HorizontalOptions="Center" VerticalOptions="Center">
                    <skia:{canvasView} x:Name="CanvasView" 
                                       AutomationId="SkiaCanvas"
                                       WidthRequest="{CanvasWidth}"
                                       HeightRequest="{CanvasHeight}"
                                       IgnorePixelScaling="True"
                                       PaintSurface="OnPaint" />
                </Grid>
            </ContentPage>
            """);
        
        await File.WriteAllTextAsync(Path.Combine(projectDir, "MainPage.xaml.cs"), $$"""
            using SkiaSharp;
            using SkiaSharp.Views.Maui;
            namespace {{projectName}};
            public partial class MainPage : ContentPage
            {
                public MainPage() => InitializeComponent();
                private void OnPaint(object sender, {{eventArgsType}} e)
                {
                    var canvas = e.Surface.Canvas;
                    {{drawCode}}
                }
            }
            """);
        
        return projectDir;
    }

    private async Task VerifyWithAppium(string appPath, string bundleId, string screenshotName)
    {
        var options = new AppiumOptions();
        options.AddAdditionalAppiumOption("newCommandTimeout", 120);
        ConfigureAppiumOptions(options, appPath, bundleId);
        
        AppiumDriver? driver = null;
        try
        {
            Output.WriteLine($"Connecting to Appium at port {AppiumPort}...");
            driver = CreateDriver(options);
            Output.WriteLine("Driver connected, waiting for app to render...");

            await Task.Delay(5000);
            
            // Capture diagnostics FIRST before trying to find elements
            Output.WriteLine("Capturing initial diagnostics...");
            
            // Page source
            var pageSource = driver.PageSource;
            var pageSourcePath = Path.Combine(ScreenshotDir, $"{screenshotName}-pagesource.xml");
            await File.WriteAllTextAsync(pageSourcePath, pageSource);
            Output.WriteLine($"Page source saved: {pageSourcePath}");
            
            // Full screenshot
            var screenshot = driver.GetScreenshot();
            var fullScreenshot = screenshot.AsByteArray;
            await SaveScreenshot(fullScreenshot, $"{screenshotName}-full");
            Output.WriteLine("Full screenshot saved");
            
            // Get screenshot size
            var screenshotSize = SKBitmap.DecodeBounds(fullScreenshot).Size;
            Output.WriteLine($"Screenshot size: {screenshotSize}");
            
            // Now find the canvas element
            Output.WriteLine("Looking for canvas element...");
            var canvasElement = driver.FindElement("accessibility id", "SkiaCanvas");
            var canvasLocation = new SKPointI(canvasElement.Location.X, canvasElement.Location.Y);
            var canvasSize = new SKSizeI(canvasElement.Size.Width, canvasElement.Size.Height);
            Output.WriteLine($"Canvas element: {canvasLocation} {canvasSize}");
            
            // Get scale factor - derived class decides how to calculate it
            var windowSize = new SKSizeI(driver.Manage().Window.Size.Width, driver.Manage().Window.Size.Height);
            var scaleFactor = GetScreenScaleFactor(screenshotSize, windowSize);
            Output.WriteLine($"Scale factor: {scaleFactor:F2}");
            
            // Calculate crop region in screenshot pixels, clamped to screenshot bounds
            var cropRect = SKRectI.Create(
                Math.Max(0, (int)(canvasLocation.X * scaleFactor)),
                Math.Max(0, (int)(canvasLocation.Y * scaleFactor)),
                (int)(canvasSize.Width * scaleFactor),
                (int)(canvasSize.Height * scaleFactor));
            cropRect = SKRectI.Intersect(cropRect, SKRectI.Create(screenshotSize));
            
            Output.WriteLine($"Crop region: {cropRect}");
            
            await VerifyCanvasScreenshot(fullScreenshot, screenshotName, cropRect);
        }
        finally
        {
            if (driver != null)
            {
                Output.WriteLine("Quitting driver...");
                driver.Quit();
            }
        }
    }

    /// <summary>
    /// Crop the full screenshot to canvas bounds and verify against reference image.
    /// Saves: original with crop region, cropped image, and diff image.
    /// </summary>
    private async Task VerifyCanvasScreenshot(byte[] fullScreenshot, string screenshotName, SKRectI cropRect)
    {
        Output.WriteLine($"Crop region: {cropRect}");
        
        // Save original with red rectangle showing crop region
        var annotatedScreenshot = DrawCropRegion(fullScreenshot, cropRect);
        await SaveScreenshot(annotatedScreenshot, $"{screenshotName}-region");
        
        // Crop to element bounds
        var croppedScreenshot = CropImage(fullScreenshot, cropRect);
        Assert.True(croppedScreenshot.Length > 1000, "Screenshot should have meaningful content");
        
        // Verify against reference
        await VerifyScreenshot(croppedScreenshot, screenshotName, PlatformName.ToLowerInvariant());
    }

    /// <summary>
    /// Draw a red rectangle on an image to visualize a region.
    /// </summary>
    private static byte[] DrawCropRegion(byte[] pngBytes, SKRectI rect)
    {
        using var bitmap = SKBitmap.Decode(pngBytes);
        using var canvas = new SKCanvas(bitmap);
        
        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0,
            IsAntialias = false
        };
        
        canvas.DrawRect(rect, paint);
        
        // Draw crosshairs at corners for visibility
        var crossSize = 20;
        canvas.DrawLine(rect.Left - crossSize, rect.Top, rect.Left + crossSize, rect.Top, paint);
        canvas.DrawLine(rect.Left, rect.Top - crossSize, rect.Left, rect.Top + crossSize, paint);
        canvas.DrawLine(rect.Right - crossSize, rect.Bottom, rect.Right + crossSize, rect.Bottom, paint);
        canvas.DrawLine(rect.Right, rect.Bottom - crossSize, rect.Right, rect.Bottom + crossSize, paint);
        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
