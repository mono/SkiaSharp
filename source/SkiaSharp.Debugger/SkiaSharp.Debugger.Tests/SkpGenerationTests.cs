using System.Text;
using SkiaSharp;
using Xunit;

namespace SkiaSharp.Debugger.Tests;

/// <summary>
/// Tests for SKP generation using SKPictureRecorder and integration with debugger.
/// These tests require native SkiaSharp library - will be skipped if not available.
/// </summary>
public class SkpGenerationTests
{
    // SKP magic bytes: "skiapict" in ASCII
    private static readonly byte[] SkpMagic = Encoding.ASCII.GetBytes("skiapict");

    private static bool IsNativeAvailable()
    {
        try
        {
            // Try to create a minimal SkiaSharp object to check if native is available
            using var paint = new SKPaint();
            return true;
        }
        catch (TypeInitializationException)
        {
            return false;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
    }

    private static void SkipIfNativeUnavailable()
    {
        Skip.If(!IsNativeAvailable(), "Native SkiaSharp library not available. Run 'dotnet cake --target=externals-download' first.");
    }

    [SkippableFact]
    public void SimpleDrawRect_SerializesToNonEmptySkp()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 100, 100));
        using var paint = new SKPaint { Color = SKColors.Red };
        
        canvas.DrawRect(10, 10, 80, 80, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.NotNull(data);
        Assert.True(data.Size > 0, "Serialized SKP should be non-empty");
    }

    [SkippableFact]
    public void SerializedSkp_StartsWithMagicBytes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 200, 200));
        using var paint = new SKPaint { Color = SKColors.Blue };
        
        canvas.DrawCircle(100, 100, 50, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size >= 8, "SKP data should be at least 8 bytes for magic");
        
        var bytes = data.ToArray();
        for (int i = 0; i < 8; i++)
        {
            Assert.Equal(SkpMagic[i], bytes[i]);
        }
    }

    [SkippableFact]
    public void ComplexSkp_WithSaveRestore_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 500, 500));
        using var paint = new SKPaint { Color = SKColors.Green };
        
        canvas.Clear(SKColors.White);
        canvas.Save();
        canvas.Translate(100, 100);
        canvas.DrawRect(0, 0, 50, 50, paint);
        canvas.Restore();
        
        canvas.Save();
        canvas.RotateDegrees(45);
        canvas.DrawRect(200, 0, 50, 50, paint);
        canvas.Restore();
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
        var bytes = data.ToArray();
        Assert.StartsWith("skiapict", Encoding.ASCII.GetString(bytes, 0, 8));
    }

    [SkippableFact]
    public void ComplexSkp_WithClipRect_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 300, 300));
        using var paint = new SKPaint { Color = SKColors.Red };
        
        canvas.Save();
        canvas.ClipRect(new SKRect(50, 50, 250, 250));
        canvas.DrawRect(0, 0, 300, 300, paint); // Clipped
        canvas.Restore();
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }

    [SkippableFact]
    public void ComplexSkp_WithDrawPath_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 400, 400));
        using var paint = new SKPaint { Color = SKColors.Purple, IsStroke = true, StrokeWidth = 3 };
        using var path = new SKPath();
        
        path.MoveTo(50, 200);
        path.CubicTo(100, 50, 300, 350, 350, 200);
        path.Close();
        
        canvas.DrawPath(path, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }

    [SkippableFact]
    public void SkpWithImage_Serializes()
    {
        SkipIfNativeUnavailable();
        
        // Create a simple bitmap image
        var info = new SKImageInfo(64, 64, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var imgPaint = new SKPaint { Color = SKColors.Orange };
        surface.Canvas.Clear(SKColors.Cyan);
        surface.Canvas.DrawCircle(32, 32, 20, imgPaint);
        using var image = surface.Snapshot();
        
        // Record with DrawImage
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 200, 200));
        
        canvas.DrawImage(image, 10, 10);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0, "SKP with image should serialize");
    }

    [SkippableFact]
    public void SkpWithMultipleOperations_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 600, 600));
        
        // Clear
        canvas.Clear(SKColors.White);
        
        // Multiple saves
        canvas.Save();
        canvas.Save();
        
        // Clip operations
        canvas.ClipRect(new SKRect(10, 10, 590, 590));
        
        // Transform operations
        canvas.Translate(50, 50);
        canvas.Scale(0.9f, 0.9f);
        
        // Draw operations
        using var paint1 = new SKPaint { Color = SKColors.Blue, IsAntialias = true };
        using var paint2 = new SKPaint { Color = SKColors.Red, IsStroke = true, StrokeWidth = 2 };
        
        canvas.DrawRect(0, 0, 200, 150, paint1);
        canvas.DrawOval(new SKRect(250, 0, 400, 100), paint2);
        canvas.DrawCircle(500, 75, 50, paint1);
        
        using var path = new SKPath();
        path.MoveTo(0, 200);
        path.LineTo(100, 300);
        path.LineTo(200, 200);
        path.Close();
        canvas.DrawPath(path, paint2);
        
        // Draw round rect
        canvas.DrawRoundRect(new SKRect(250, 200, 450, 350), 20, 20, paint1);
        
        // Restores
        canvas.Restore();
        canvas.Restore();
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 100, "Complex SKP should have substantial size");
    }

    [SkippableFact]
    public void SkpCanBeDeserialized()
    {
        SkipIfNativeUnavailable();
        
        // Create and serialize
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 100, 100));
        using var paint = new SKPaint { Color = SKColors.Teal };
        
        canvas.DrawRect(0, 0, 100, 100, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        // Deserialize
        using var loadedPicture = SKPicture.Deserialize(data);
        
        Assert.NotNull(loadedPicture);
        Assert.Equal(picture.CullRect, loadedPicture.CullRect);
    }

    [SkippableFact]
    public void EmptyRecording_StillSerializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 100, 100));
        // No drawing operations
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.NotNull(data);
        Assert.True(data.Size > 0, "Even empty SKP should have header");
    }

    [SkippableFact]
    public void SkpWithText_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 400, 200));
        using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        using var font = new SKFont { Size = 24 };
        
        canvas.DrawText("Hello SkiaSharp", 50, 100, SKTextAlign.Left, font, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }

    [SkippableFact]
    public void SkpWithShader_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 200, 200));
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(200, 200),
            new[] { SKColors.Red, SKColors.Blue },
            SKShaderTileMode.Clamp);
        using var paint = new SKPaint { Shader = shader };
        
        canvas.DrawRect(0, 0, 200, 200, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }

    [SkippableFact]
    public void SkpWithNestedSaveRestore_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 400, 400));
        using var paint = new SKPaint { Color = SKColors.Indigo };
        
        // Deeply nested save/restore
        canvas.Save();
        canvas.Translate(100, 100);
        canvas.Save();
        canvas.RotateDegrees(30);
        canvas.Save();
        canvas.Scale(1.5f, 1.5f);
        canvas.DrawRect(0, 0, 50, 50, paint);
        canvas.Restore();
        canvas.Restore();
        canvas.Restore();
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }

    [SkippableFact]
    public void SkpWithClipPath_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 300, 300));
        using var paint = new SKPaint { Color = SKColors.Green };
        using var clipPath = new SKPath();
        
        clipPath.AddCircle(150, 150, 100);
        
        canvas.Save();
        canvas.ClipPath(clipPath);
        canvas.DrawRect(0, 0, 300, 300, paint);
        canvas.Restore();
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }

    [SkippableFact]
    public void SkpWithConcat_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 200, 200));
        using var paint = new SKPaint { Color = SKColors.Magenta };
        
        var matrix = SKMatrix.CreateRotationDegrees(45, 100, 100);
        canvas.Concat(in matrix);
        canvas.DrawRect(50, 50, 100, 100, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }

    [SkippableFact]
    public void Picture_GetCullRect_ReturnsRecordedBounds()
    {
        SkipIfNativeUnavailable();
        
        var bounds = new SKRect(10, 20, 310, 420);
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(bounds);
        using var paint = new SKPaint();
        
        canvas.DrawRect(0, 0, 100, 100, paint);
        
        using var picture = recorder.EndRecording();
        
        Assert.Equal(bounds, picture.CullRect);
    }

    [SkippableFact]
    public void MultipleDrawCalls_IncreasesSkpSize()
    {
        SkipIfNativeUnavailable();
        
        // Record with few operations
        using var recorder1 = new SKPictureRecorder();
        using var canvas1 = recorder1.BeginRecording(new SKRect(0, 0, 100, 100));
        using var paint = new SKPaint();
        
        canvas1.DrawRect(0, 0, 50, 50, paint);
        
        using var picture1 = recorder1.EndRecording();
        using var data1 = picture1.Serialize();
        var size1 = data1.Size;
        
        // Record with many operations
        using var recorder2 = new SKPictureRecorder();
        using var canvas2 = recorder2.BeginRecording(new SKRect(0, 0, 100, 100));
        
        for (int i = 0; i < 50; i++)
        {
            canvas2.DrawRect(i, i, 10, 10, paint);
        }
        
        using var picture2 = recorder2.EndRecording();
        using var data2 = picture2.Serialize();
        var size2 = data2.Size;
        
        Assert.True(size2 > size1, "More operations should result in larger SKP");
    }

    [SkippableFact]
    public void SkpWithDrawPoints_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 200, 200));
        using var paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 5, StrokeCap = SKStrokeCap.Round };
        
        var points = new SKPoint[]
        {
            new SKPoint(20, 20),
            new SKPoint(50, 80),
            new SKPoint(100, 30),
            new SKPoint(150, 90),
        };
        
        canvas.DrawPoints(SKPointMode.Points, points, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }

    [SkippableFact]
    public void SkpWithDrawLine_Serializes()
    {
        SkipIfNativeUnavailable();
        
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, 200, 200));
        using var paint = new SKPaint { Color = SKColors.Navy, StrokeWidth = 2, IsStroke = true };
        
        canvas.DrawLine(10, 10, 190, 190, paint);
        canvas.DrawLine(190, 10, 10, 190, paint);
        
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        Assert.True(data.Size > 0);
    }
}
