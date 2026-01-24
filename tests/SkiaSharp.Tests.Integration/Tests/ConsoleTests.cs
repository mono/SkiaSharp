using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Tests that verify SkiaSharp and HarfBuzzSharp packages work in console applications.
/// </summary>
[Trait("Category", "Platform")]
public class ConsoleTests(ITestOutputHelper output) : PlatformTestBase(output)
{
    [Fact]
    public async Task SkiaSharpConsoleAppRunsSuccessfully()
    {
        Output.WriteLine($"Testing SkiaSharp {SkiaVersion} in Console app");
        var projectName = "SkiaSharpConsoleTest";
        var projectDir = Path.Combine(TestDir, projectName);
        
        // Run from TestDir (which has global.json) using relative paths
        await Run("dotnet", $"new console -n {projectName} -o {projectName}");
        await Run("dotnet", $"add {projectName} package SkiaSharp --version {SkiaVersion}");
        
        var drawCode = TestImage.GetDrawCode();
        var outputPath = Path.Combine(projectDir, "output.png");
        
        File.WriteAllText(Path.Combine(projectDir, "Program.cs"), $$"""
            using SkiaSharp;
            
            using var bitmap = new SKBitmap({{TestImage.Width}}, {{TestImage.Height}});
            using var canvas = new SKCanvas(bitmap);
            
            {{drawCode}}
            
            var outputPath = @"{{outputPath.Replace("\\", "\\\\")}}";
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(outputPath);
            data.SaveTo(stream);
            Console.WriteLine("SUCCESS");
            """);
        
        var result = await Run("dotnet", $"run --project {projectName}", timeoutSeconds: 60);
        Assert.Contains("SUCCESS", result);
        Assert.True(File.Exists(outputPath), "Output PNG should exist");
        
        var actualImage = await File.ReadAllBytesAsync(outputPath);
        await VerifyScreenshot(actualImage, "console-skiasharp");
    }

    [Fact]
    public async Task HarfBuzzSharpConsoleAppRunsSuccessfully()
    {
        Output.WriteLine($"Testing HarfBuzzSharp {HarfBuzzVersion} in Console app");
        var projectName = "HarfBuzzConsoleTest";
        var projectDir = Path.Combine(TestDir, projectName);
        
        // Run from TestDir (which has global.json) using relative paths
        await Run("dotnet", $"new console -n {projectName} -o {projectName}");
        await Run("dotnet", $"add {projectName} package SkiaSharp --version {SkiaVersion}");
        await Run("dotnet", $"add {projectName} package HarfBuzzSharp --version {HarfBuzzVersion}");
        
        var outputPath = Path.Combine(projectDir, "output.png");
        
        // Create a test that shapes text with HarfBuzz and renders to an image
        File.WriteAllText(Path.Combine(projectDir, "Program.cs"), $$"""
            using SkiaSharp;
            using HarfBuzzSharp;
            
            // Shape text with HarfBuzz
            {{TestText.GetShapeCode()}}
            {{TestText.GetOutputCode()}}
            
            // Render to image
            using var bitmap = new SKBitmap(400, 100);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);
            
            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            using var font = new SKFont(SKTypeface.Default, 32);
            canvas.DrawText("{{TestText.SampleText}}", 20, 60, SKTextAlign.Left, font, paint);
            
            var outputPath = @"{{outputPath.Replace("\\", "\\\\")}}";
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(outputPath);
            data.SaveTo(stream);
            Console.WriteLine("SUCCESS");
            """);
        
        var result = await Run("dotnet", $"run --project {projectName}", timeoutSeconds: 60);
        Assert.Contains("SUCCESS", result);
        Assert.True(TestText.ValidateOutput(result), "HarfBuzz output validation failed");
        Assert.True(File.Exists(outputPath), "Output PNG should exist");
        
        var actualImage = await File.ReadAllBytesAsync(outputPath);
        await SaveScreenshot(actualImage, "console-harfbuzzsharp");
        
        Assert.True(actualImage.Length > 500, "Screenshot should have content");
    }
}
