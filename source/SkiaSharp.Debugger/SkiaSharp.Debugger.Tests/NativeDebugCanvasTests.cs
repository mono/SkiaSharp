using SkiaSharp;
using SkiaSharp.Debugger;
using Xunit;

namespace SkiaSharp.Debugger.Tests;

public class NativeDebugCanvasTests
{
    [SkippableFact]
    public void NativeIsAvailable()
    {
        Skip.IfNot(SKDebugCanvas.IsNativeAvailable, "Native debugger not built");
        Assert.True(SKDebugCanvas.IsNativeAvailable);
    }

    [SkippableFact]
    public void LoadSkpReturnsCommands()
    {
        Skip.IfNot(SKDebugCanvas.IsNativeAvailable, "Native debugger not built");
        
        var skpBytes = CreateTestSkp();
        using var dc = new SKDebugCanvas(400, 300);
        var count = dc.LoadSkp(skpBytes);
        
        Assert.True(count > 0, $"Expected commands, got {count}");
        Assert.Equal(count, dc.CommandCount);
    }

    [SkippableFact]
    public void GetCommandListJsonReturnsValidJson()
    {
        Skip.IfNot(SKDebugCanvas.IsNativeAvailable, "Native debugger not built");
        
        var skpBytes = CreateTestSkp();
        using var dc = new SKDebugCanvas(400, 300);
        dc.LoadSkp(skpBytes);
        
        using var surface = SKSurface.Create(new SKImageInfo(400, 300));
        var json = dc.GetCommandListJson(surface.Canvas);
        
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("commands", json);
        // Should contain real Skia command names
        Assert.Contains("DrawRect", json);
    }

    [SkippableFact]
    public void DrawToRendersPartialCommands()
    {
        Skip.IfNot(SKDebugCanvas.IsNativeAvailable, "Native debugger not built");
        
        var skpBytes = CreateTestSkp();
        using var dc = new SKDebugCanvas(400, 300);
        var count = dc.LoadSkp(skpBytes);
        
        using var surface = SKSurface.Create(new SKImageInfo(400, 300));
        // Should not throw
        dc.DrawTo(surface.Canvas, 0);
        dc.DrawTo(surface.Canvas, count / 2);
        dc.DrawTo(surface.Canvas, count - 1);
    }

    [SkippableFact]
    public void GetCommandInfoJsonReturnsMatrixAndClip()
    {
        Skip.IfNot(SKDebugCanvas.IsNativeAvailable, "Native debugger not built");
        
        var skpBytes = CreateTestSkp();
        using var dc = new SKDebugCanvas(400, 300);
        dc.LoadSkp(skpBytes);
        
        using var surface = SKSurface.Create(new SKImageInfo(400, 300));
        dc.DrawTo(surface.Canvas, dc.CommandCount - 1);
        
        var info = dc.GetCommandInfoJson();
        Assert.Contains("ClipRect", info);
        Assert.Contains("ViewMatrix", info);
    }

    [SkippableFact] 
    public void OverdrawVizDoesNotCrash()
    {
        Skip.IfNot(SKDebugCanvas.IsNativeAvailable, "Native debugger not built");
        
        var skpBytes = CreateTestSkp();
        using var dc = new SKDebugCanvas(400, 300);
        dc.LoadSkp(skpBytes);
        
        dc.SetOverdrawVis(true);
        using var surface = SKSurface.Create(new SKImageInfo(400, 300));
        dc.Draw(surface.Canvas);
        dc.SetOverdrawVis(false);
    }

    [SkippableFact]
    public void CommandVisibilityToggleWorks()
    {
        Skip.IfNot(SKDebugCanvas.IsNativeAvailable, "Native debugger not built");
        
        var skpBytes = CreateTestSkp();
        using var dc = new SKDebugCanvas(400, 300);
        dc.LoadSkp(skpBytes);
        
        // Toggle first command off then on
        dc.SetCommandVisibility(0, false);
        dc.SetCommandVisibility(0, true);
    }

    private static byte[] CreateTestSkp()
    {
        using var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(new SKRect(0, 0, 400, 300));
        canvas.Clear(SKColors.White);
        canvas.Save();
        using var paint = new SKPaint { Color = SKColors.Blue, IsAntialias = true };
        canvas.DrawRect(50, 50, 200, 100, paint);
        canvas.DrawCircle(300, 150, 80, paint);
        canvas.Restore();
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        return data.ToArray();
    }
}
