using SkiaSharp;
using SkiaSharp.Debugger;
using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Debugger.Tests;

public class PrintNativeJsonTests
{
    private readonly ITestOutputHelper _output;
    public PrintNativeJsonTests(ITestOutputHelper output) => _output = output;

    [SkippableFact]
    public void PrintRealCommandJson()
    {
        Skip.IfNot(SKDebugCanvas.IsNativeAvailable, "Native debugger not built");
        
        using var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(new SKRect(0, 0, 400, 300));
        canvas.Clear(SKColors.White);
        canvas.Save();
        using var paint = new SKPaint { Color = SKColors.Blue, IsAntialias = true };
        canvas.DrawRect(50, 50, 200, 100, paint);
        using var paint2 = new SKPaint { Color = SKColors.Red, IsAntialias = true };
        canvas.DrawCircle(300, 150, 80, paint2);
        canvas.Restore();
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        
        using var dc = new SKDebugCanvas(400, 300);
        var count = dc.LoadSkp(data.ToArray());
        _output.WriteLine($"Command count: {count}");
        
        using var surface = SKSurface.Create(new SKImageInfo(400, 300));
        var json = dc.GetCommandListJson(surface.Canvas);
        _output.WriteLine($"JSON ({json.Length} chars):");
        _output.WriteLine(json);
    }
}
