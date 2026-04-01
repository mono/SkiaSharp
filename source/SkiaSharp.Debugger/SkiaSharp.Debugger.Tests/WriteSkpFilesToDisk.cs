using SkiaSharp;
using SkiaSharp.Debugger;
using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Debugger.Tests;

public class WriteSkpFilesToDisk
{
    // Output to the Blazor app's wwwroot/skp directory
    private static readonly string BlazorSkpDir = Path.GetFullPath(Path.Combine(
        Path.GetDirectoryName(typeof(WriteSkpFilesToDisk).Assembly.Location)!,
        "..", "..", "..", "..", "..", "..",
        "samples", "Basic", "BlazorWebAssembly", "SkiaSharpDebugger", "wwwroot", "skp"));

    private static readonly string TestOutputDir = Path.Combine(
        Path.GetDirectoryName(typeof(WriteSkpFilesToDisk).Assembly.Location)!,
        "skp_output");

    private void Gen(string name, int w, int h, Action<SKCanvas> draw)
    {
        Directory.CreateDirectory(TestOutputDir);
        Directory.CreateDirectory(BlazorSkpDir);

        // Record into an SKPicture
        using var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(new SKRect(0, 0, w, h));
        draw(canvas);
        using var picture = recorder.EndRecording();
        using var data = picture.Serialize();
        var skpBytes = data.ToArray();

        // Write SKP
        var skpPath = Path.Combine(TestOutputDir, name);
        File.WriteAllBytes(skpPath, skpBytes);
        File.WriteAllBytes(Path.Combine(BlazorSkpDir, name), skpBytes);

        // Use native DebugCanvas to extract real command JSON
        if (SKDebugCanvas.IsNativeAvailable)
        {
            using var dc = new SKDebugCanvas(w, h);
            var count = dc.LoadSkp(skpBytes);
            using var surface = SKSurface.Create(new SKImageInfo(w, h));
            var json = dc.GetCommandListJson(surface.Canvas);

            var jsonName = Path.ChangeExtension(name, ".json");
            File.WriteAllText(Path.Combine(TestOutputDir, jsonName), json);
            File.WriteAllText(Path.Combine(BlazorSkpDir, jsonName), json);
            Console.WriteLine($"Generated {name} ({skpBytes.Length} bytes, {count} commands) + {jsonName}");
        }
        else
        {
            Console.WriteLine($"Generated {name} ({skpBytes.Length} bytes) — no debugger JSON (native not available)");
        }
    }

    [SkippableFact]
    public void GenerateAllSkpFiles()
    {
        Skip.IfNot(File.Exists(Path.Combine(
            AppContext.BaseDirectory, "libSkiaSharp.dylib")) ||
            File.Exists(Path.Combine(AppContext.BaseDirectory, "libSkiaSharp.so")),
            "Native library not available");

        Gen("shapes.skp", 600, 600, c => {
            c.Clear(SKColors.White);
            using var blue = new SKPaint { Color = SKColors.Blue, IsAntialias = true };
            using var red = new SKPaint { Color = SKColors.Red.WithAlpha(128), IsAntialias = true };
            using var green = new SKPaint { Color = SKColors.Green, IsAntialias = true };
            using var purple = new SKPaint { Color = SKColors.Purple, IsAntialias = true };
            c.DrawRect(50, 50, 200, 100, blue);
            c.DrawRect(100, 80, 180, 120, red);
            c.DrawOval(new SKRect(300, 50, 500, 200), green);
            c.DrawCircle(200, 350, 80, purple);
            using var stroke = new SKPaint { Color = SKColors.Black, IsStroke = true, StrokeWidth = 3, IsAntialias = true };
            c.DrawLine(50, 450, 550, 450, stroke);
            c.DrawRoundRect(350, 250, 200, 150, 15, 15, blue);
            using var teal = new SKPaint { Color = SKColors.Teal, IsStroke = true, StrokeWidth = 2, IsAntialias = true };
            using var star = new SKPath();
            float cx = 150, cy = 540;
            for (int i = 0; i < 5; i++)
            {
                float angle = (float)(i * 4 * Math.PI / 5 - Math.PI / 2);
                float x = cx + 50 * MathF.Cos(angle);
                float y = cy + 50 * MathF.Sin(angle);
                if (i == 0) star.MoveTo(x, y); else star.LineTo(x, y);
            }
            star.Close();
            c.DrawPath(star, teal);
        });

        Gen("transforms.skp", 600, 500, c => {
            c.Clear(SKColors.White);
            using var p = new SKPaint { IsAntialias = true };
            // Rotating rectangles around center
            for (int i = 0; i < 6; i++) {
                c.Save(); c.Translate(300, 200); c.RotateDegrees(i * 60); c.Translate(0, -100);
                p.Color = new SKColor((byte)(40*i), (byte)(100+20*i), (byte)(200-30*i));
                c.DrawRect(-30, -20, 60, 40, p); c.Restore();
            }
            // Center dot
            p.Color = SKColors.Black;
            c.DrawCircle(300, 200, 10, p);
            // Scaled oval
            c.Save(); c.Translate(300, 400); c.Scale(2f, 0.5f);
            p.Color = SKColors.Orange;
            c.DrawOval(0, 0, 60, 60, p);
            c.Restore();
        });

        Gen("clipping.skp", 600, 500, c => {
            c.Clear(SKColors.White);
            using var p = new SKPaint { IsAntialias = true };
            c.Save(); c.ClipRect(new SKRect(50, 50, 500, 400));
            p.Color = SKColors.LightBlue; c.DrawRect(0, 0, 600, 500, p);
            c.Save(); using var cp = new SKPath(); cp.AddCircle(275, 225, 120); c.ClipPath(cp);
            p.Color = SKColors.Coral; c.DrawRect(0, 0, 600, 500, p);
            p.Color = SKColors.DarkRed; c.DrawCircle(275, 225, 80, p);
            c.Restore();
            p.Color = SKColors.Green.WithAlpha(128); c.DrawRect(200, 150, 200, 100, p);
            c.Restore();
        });

        Gen("text_paths.skp", 600, 500, c => {
            c.Clear(SKColors.White);
            using var tp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            using var f = new SKFont { Size = 32 };
            c.DrawText("SkiaSharp Debugger", 50, 50, SKTextAlign.Left, f, tp);
            tp.Color = SKColors.Blue;
            using var f2 = new SKFont { Size = 24 };
            c.DrawText("Blazor WASM Demo", 50, 90, SKTextAlign.Left, f2, tp);
            using var sp = new SKPaint { Color = SKColors.Teal, IsStroke = true, StrokeWidth = 3, IsAntialias = true };
            using var path = new SKPath();
            path.MoveTo(50, 200); path.CubicTo(150, 100, 350, 300, 500, 200);
            c.DrawPath(path, sp);
            using var op = new SKPaint { Color = SKColors.DarkOrange, IsAntialias = true };
            using var rp = new SKPath();
            rp.AddRoundRect(new SKRect(50, 250, 300, 400), 15, 15);
            c.DrawPath(rp, op);
            using var wp = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var f3 = new SKFont { Size = 18 };
            c.DrawText("Inside rounded rect", 80, 330, SKTextAlign.Left, f3, wp);
            // Gradient arc
            c.Save(); c.Translate(450, 400);
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(-80, -80), new SKPoint(80, 80),
                new[] { SKColors.MediumPurple, SKColors.DeepPink },
                SKShaderTileMode.Clamp);
            using var gp = new SKPaint { IsStroke = true, StrokeWidth = 5, IsAntialias = true, Shader = shader };
            using var arc = new SKPath();
            arc.AddArc(new SKRect(-80, -80, 80, 80), 0, 270);
            c.DrawPath(arc, gp);
            c.Restore();
        });

        // Verify files exist
        Assert.True(File.Exists(Path.Combine(TestOutputDir, "shapes.skp")));
        Assert.True(File.Exists(Path.Combine(TestOutputDir, "transforms.skp")));
        Assert.True(File.Exists(Path.Combine(TestOutputDir, "clipping.skp")));
        Assert.True(File.Exists(Path.Combine(TestOutputDir, "text_paths.skp")));

        if (SKDebugCanvas.IsNativeAvailable)
        {
            Assert.True(File.Exists(Path.Combine(BlazorSkpDir, "shapes.json")));
            Assert.True(File.Exists(Path.Combine(BlazorSkpDir, "transforms.json")));
            Assert.True(File.Exists(Path.Combine(BlazorSkpDir, "clipping.json")));
            Assert.True(File.Exists(Path.Combine(BlazorSkpDir, "text_paths.json")));
        }

        Console.WriteLine($"All files written to: {TestOutputDir}");
        Console.WriteLine($"Blazor SKP dir: {BlazorSkpDir}");
    }
}
