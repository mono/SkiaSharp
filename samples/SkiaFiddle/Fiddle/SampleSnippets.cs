using System.Collections.Generic;

namespace SkiaFiddle.Fiddle;

/// <summary>
/// A bundled sample. <see cref="Setup"/> goes in the Setup box (class scope —
/// fields, methods, ctor) and runs once per Run. <see cref="Draw"/> goes in
/// the Draw box and runs every frame as the body of <c>Draw(SKCanvas, int, int, double)</c>.
/// </summary>
public record FiddleSample(string Name, string Draw, string Setup = "");

public static class SampleSnippets
{
    public const string DefaultDraw =
        """
        canvas.Clear(new SKColor(0x10, 0x14, 0x1E));

        using var paint = new SKPaint { IsAntialias = true };
        var cx = width / 2f;
        var cy = height / 2f;
        var r = Math.Min(width, height) * 0.32f;

        paint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx, cy), r,
            new[] { new SKColor(0x46, 0xC1, 0xFF), new SKColor(0x14, 0x42, 0x9D) },
            null, SKShaderTileMode.Clamp);
        canvas.DrawCircle(cx, cy, r, paint);

        paint.Shader = null;
        paint.Color = new SKColor(0xE6, 0xF4, 0xFF);

        using var font = new SKFont(SKTypeface.Default, 28) { Embolden = true };
        const string text = "Hello SkiaFiddle";
        var w = font.MeasureText(text);
        canvas.DrawText(text, cx - w / 2, cy - r - 24, font, paint);
        """;

    public const string DefaultSetup =
        """
        // Setup runs once per Run. Declare class-scope fields here:
        //   static readonly SKRuntimeEffect Effect = SKRuntimeEffect.CreateShader(sksl, out _);
        // ... and reference them from Draw below.
        """;

    public const string OrbitsSetup =
        """
        // Per-planet config — built once, referenced every frame.
        static readonly (float Radius, float Speed, SKColor Color, float Size)[] Planets =
        {
            (90,  1.6f,  new SKColor(0x9E, 0xB7, 0xFF),  6),
            (140, 1.0f,  new SKColor(0xFF, 0xC1, 0x07), 10),
            (200, 0.6f,  new SKColor(0x46, 0xC1, 0xFF), 14),
            (270, 0.35f, new SKColor(0xE9, 0x1E, 0x63), 18),
        };
        """;

    public const string OrbitsDraw =
        """
        canvas.Clear(new SKColor(0x05, 0x07, 0x12));

        var cx = width / 2f;
        var cy = height / 2f;

        using var sun = new SKPaint { IsAntialias = true };
        sun.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx, cy), 60,
            new[] { new SKColor(0xFF, 0xE8, 0x80), new SKColor(0xFF, 0x77, 0x10), new SKColor(0xFF, 0x33, 0x00, 0) },
            new[] { 0f, 0.6f, 1f },
            SKShaderTileMode.Clamp);
        canvas.DrawCircle(cx, cy, 60, sun);

        using var orbitPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0x33, 0x44, 0x66, 0x66),
            StrokeWidth = 1,
        };
        foreach (var p in Planets)
            canvas.DrawCircle(cx, cy, p.Radius, orbitPaint);

        using var planet = new SKPaint { IsAntialias = true };
        foreach (var p in Planets)
        {
            var angle = t * p.Speed;
            var x = cx + p.Radius * (float)Math.Cos(angle);
            var y = cy + p.Radius * (float)Math.Sin(angle);
            planet.Color = p.Color;
            canvas.DrawCircle(x, y, p.Size, planet);

            for (int i = 1; i <= 12; i++)
            {
                var a = angle - i * 0.06f;
                var tx = cx + p.Radius * (float)Math.Cos(a);
                var ty = cy + p.Radius * (float)Math.Sin(a);
                planet.Color = p.Color.WithAlpha((byte)(p.Color.Alpha * (1 - i / 12f) * 0.4f));
                canvas.DrawCircle(tx, ty, p.Size * (1 - i / 14f), planet);
            }
        }
        """;

    public const string PlasmaSetup =
        """
        // SkSL fragment shader — compiled once on the GPU.
        const string Sksl = @"
        uniform float2 iResolution;
        uniform float  iTime;

        half4 main(float2 fragCoord) {
            float2 uv = fragCoord / iResolution.xy;
            uv = uv * 2.0 - 1.0;
            uv.x *= iResolution.x / iResolution.y;

            float v = 0.0;
            v += sin(uv.x * 6.0 + iTime);
            v += sin((uv.y * 6.0 + iTime) / 2.0);
            v += sin((uv.x * 6.0 + uv.y * 6.0 + iTime) / 2.0);
            float cx = uv.x + 0.5 * sin(iTime / 5.0);
            float cy = uv.y + 0.5 * cos(iTime / 3.0);
            v += sin(sqrt(cx*cx + cy*cy + 1.0) + iTime);
            v = v / 2.0;

            float3 col = float3(
                0.5 + 0.5 * sin(3.14159 * v),
                0.5 + 0.5 * sin(3.14159 * v + 2.094),
                0.5 + 0.5 * sin(3.14159 * v + 4.188)
            );
            return half4(col, 1.0);
        }";

        static readonly SKRuntimeEffect Effect = SKRuntimeEffect.CreateShader(Sksl, out _);
        """;

    public const string PlasmaDraw =
        """
        if (Effect is null) { canvas.Clear(SKColors.Red); return; }

        using var u = new SKRuntimeEffectUniforms(Effect);
        u["iResolution"] = new[] { (float)width, (float)height };
        u["iTime"] = (float)t;

        using var shader = Effect.ToShader(u);
        using var paint = new SKPaint { Shader = shader };
        canvas.DrawRect(0, 0, width, height, paint);
        """;

    public const string RippleSetup =
        """
        const string Sksl = @"
        uniform float2 iResolution;
        uniform float  iTime;

        half4 main(float2 fragCoord) {
            float2 p = (fragCoord - 0.5 * iResolution) / iResolution.y;
            float r = length(p);
            float a = atan(p.y, p.x);

            float ripple = sin(20.0 * r - 4.0 * iTime) * 0.5 + 0.5;
            float spokes = sin(8.0 * a + iTime) * 0.5 + 0.5;
            float v = mix(ripple, ripple * spokes, 0.6);

            float3 inner = float3(0.10, 0.55, 1.00);
            float3 outer = float3(0.95, 0.20, 0.55);
            float3 col = mix(inner, outer, smoothstep(0.0, 0.7, r));
            col *= 0.4 + 0.8 * v;
            col = pow(col, float3(1.0/2.2));
            return half4(col, 1.0);
        }";

        static readonly SKRuntimeEffect Effect = SKRuntimeEffect.CreateShader(Sksl, out _);
        """;

    public const string RippleDraw =
        """
        if (Effect is null) { canvas.Clear(SKColors.Red); return; }

        using var u = new SKRuntimeEffectUniforms(Effect);
        u["iResolution"] = new[] { (float)width, (float)height };
        u["iTime"] = (float)t;

        using var shader = Effect.ToShader(u);
        using var paint = new SKPaint { Shader = shader };
        canvas.DrawRect(0, 0, width, height, paint);
        """;

    public const string ColorGridDraw =
        """
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint { IsAntialias = true };
        const int cols = 12, rows = 8;
        float cellW = width / (float)cols;
        float cellH = height / (float)rows;
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            var hue = ((x + y * cols) * 360f / (cols * rows) + (float)t * 30f) % 360f;
            paint.Color = SKColor.FromHsl(hue, 80, 55);
            canvas.DrawRect(x * cellW, y * cellH, cellW, cellH, paint);
        }
        """;

    public const string SineWaveDraw =
        """
        canvas.Clear(new SKColor(0xF7, 0xF8, 0xFA));
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            Color = new SKColor(0x16, 0x82, 0xFF),
        };
        using var path = new SKPath();
        float midY = height / 2f;
        float amp = height * 0.3f;
        for (int x = 0; x <= width; x++)
        {
            float u = x / (float)width;
            float y = midY + amp * (float)Math.Sin(u * Math.PI * 4 + t * 2);
            if (x == 0) path.MoveTo(x, y); else path.LineTo(x, y);
        }
        canvas.DrawPath(path, paint);
        """;

    public static IReadOnlyList<FiddleSample> All { get; } = new[]
    {
        new FiddleSample("Default (static)", DefaultDraw, DefaultSetup),
        new FiddleSample("Animated · Orbits", OrbitsDraw, OrbitsSetup),
        new FiddleSample("Shader · Plasma", PlasmaDraw, PlasmaSetup),
        new FiddleSample("Shader · Ripple", RippleDraw, RippleSetup),
        new FiddleSample("Animated · Sine wave", SineWaveDraw),
        new FiddleSample("Animated · Color grid", ColorGridDraw),
    };
}
