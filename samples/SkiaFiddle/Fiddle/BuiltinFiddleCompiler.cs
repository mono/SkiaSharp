using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaFiddle.Fiddle;

/// <summary>
/// Fallback compiler kept around for diagnostics; not normally selected. Renders
/// only the default snippet — Roslyn does the real work.
/// </summary>
public class BuiltinFiddleCompiler : IFiddleCompiler
{
    public string Name => "builtin (no compilation)";

    public Task<FiddleCompileResult> CompileAsync(string setupCode, string drawCode)
    {
        var sw = Stopwatch.StartNew();
        sw.Stop();
        return Task.FromResult(new FiddleCompileResult(
            DrawDefault,
            "Built-in compiler — falls back to a fixed sample.",
            sw.ElapsedMilliseconds));
    }

    private static void DrawDefault(SKCanvas canvas, int width, int height, double t)
    {
        canvas.Clear(new SKColor(0x10, 0x14, 0x1E));

        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        var cx = width / 2f;
        var cy = height / 2f;
        var r = Math.Min(width, height) * 0.32f;

        paint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx, cy),
            r,
            new[] { new SKColor(0x46, 0xC1, 0xFF), new SKColor(0x14, 0x42, 0x9D) },
            null,
            SKShaderTileMode.Clamp);
        canvas.DrawCircle(cx, cy, r, paint);

        paint.Shader = null;
        paint.Color = new SKColor(0xE6, 0xF4, 0xFF);

        using var font = new SKFont(SKTypeface.Default, 28) { Embolden = true };
        const string text = "Hello SkiaFiddle";
        var w = font.MeasureText(text);
        canvas.DrawText(text, cx - w / 2, cy - r - 24, font, paint);
    }
}
