using System.Runtime.InteropServices.JavaScript;
using SkiaSharp;

Console.WriteLine("SkiaSharp Browser WebAssembly Sample");
Console.WriteLine("Platform color type: " + SKImageInfo.PlatformColorType);

// Render the initial CPU page so something is visible immediately
SampleRenderer.RenderCpu(800, 600);

/// <summary>
/// Static render methods exported to JavaScript via [JSExport].
/// JS calls these to render each sample page onto a shared SKBitmap,
/// then reads the pixel buffer back to draw on an HTML canvas.
/// </summary>
public static partial class SampleRenderer
{
	static SKBitmap? bitmap;
	static SKCanvas? canvas;

	// Drawing state
	static readonly List<DrawingStroke> strokes = new();
	static DrawingStroke? currentStroke;
	static SKColor currentColor = StrokeColors[0].Color;
	static float brushSize = 4;
	static SKPoint lastPoint;

	static readonly (float X, float Y, float R, SKColor Color)[] Circles =
	{
		(0.20f, 0.30f, 0.10f, new SKColor(0xFF, 0x4D, 0x66, 0xCC)),
		(0.75f, 0.25f, 0.08f, new SKColor(0x4D, 0xB3, 0xFF, 0xCC)),
		(0.15f, 0.70f, 0.07f, new SKColor(0xFF, 0x99, 0x1A, 0xCC)),
		(0.80f, 0.70f, 0.12f, new SKColor(0x66, 0xFF, 0xB3, 0xCC)),
		(0.50f, 0.15f, 0.06f, new SKColor(0xB3, 0x4D, 0xFF, 0xCC)),
		(0.40f, 0.80f, 0.09f, new SKColor(0xFF, 0xE6, 0x33, 0xCC)),
	};

	static readonly SKColor[] GradientColors =
	{
		new SKColor(0x44, 0x88, 0xFF),
		new SKColor(0x88, 0x33, 0xCC),
	};

	static readonly float[] BlobColors =
	{
		1.0f, 0.3f, 0.4f,
		0.3f, 0.7f, 1.0f,
		1.0f, 0.6f, 0.1f,
		0.4f, 1.0f, 0.7f,
		0.7f, 0.3f, 1.0f,
		1.0f, 0.9f, 0.2f,
	};

	public static readonly (string Name, SKColor Color)[] StrokeColors =
	{
		("#000000", new SKColor(0x00, 0x00, 0x00)),
		("#E53935", new SKColor(0xE5, 0x39, 0x35)),
		("#1E88E5", new SKColor(0x1E, 0x88, 0xE5)),
		("#43A047", new SKColor(0x43, 0xA0, 0x47)),
		("#FB8C00", new SKColor(0xFB, 0x8C, 0x00)),
		("#8E24AA", new SKColor(0x8E, 0x24, 0xAA)),
	};

	const string GpuShader = @"
		uniform float iTime;
		uniform float2 iResolution;
		uniform float2 iTouchPos;
		uniform float iTouchActive;
		uniform float3 iColors[6];

		half4 main(float2 fragCoord) {
			float2 uv = fragCoord / iResolution;
			float aspect = iResolution.x / iResolution.y;
			float2 st = float2(uv.x * aspect, uv.y);
			float t = iTime;

			float field = 0.0;
			float3 weighted = float3(0.0);

			for (int i = 0; i < 6; i++) {
				float fi = float(i);
				float phase = fi * 1.047;
				float speed = 0.3 + fi * 0.07;
				float2 center = float2(
					aspect * 0.5 + 0.4 * sin(t * speed + phase) * cos(t * speed * 0.6 + fi),
					0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7)
				);
				float2 d = st - center;
				float r = length(d);
				float strength = 0.030 / (r * r + 0.002);
				field += strength;
				weighted += iColors[i] * strength;
			}

			if (iTouchActive > 0.5) {
				float2 touchSt = float2(iTouchPos.x * aspect, iTouchPos.y);
				float2 d = st - touchSt;
				float r = length(d);
				float strength = 0.050 / (r * r + 0.002);
				field += strength;
				weighted += float3(1.0, 0.95, 0.9) * strength;
			}

			float3 blobColor = weighted / max(field, 0.001);
			float edge = smoothstep(5.0, 8.0, field);
			float innerGlow = smoothstep(8.0, 20.0, field) * 0.3;
			float3 bg = float3(0.03, 0.02, 0.08);
			bg += float3(0.02, 0.01, 0.03) * sin(t * 0.2 + uv.y * 3.0);
			float halo = smoothstep(3.0, 5.0, field) * (1.0 - edge);
			float3 result = bg;
			result += blobColor * halo * 0.4;
			result = mix(result, blobColor * (1.0 + innerGlow), edge);
			float2 vc = uv - 0.5;
			float vignette = 1.0 - dot(vc, vc) * 0.8;
			result *= vignette;
			return half4(clamp(result, 0.0, 1.0), 1.0);
		}
	";

	static readonly Lazy<SKRuntimeShaderBuilder> shaderBuilder = new(() =>
		SKRuntimeEffect.BuildShader(GpuShader));

	static readonly SKPaint shaderPaint = new();

	static void EnsureBitmap(int width, int height)
	{
		if (bitmap is not null && bitmap.Width == width && bitmap.Height == height)
			return;

		bitmap?.Dispose();
		canvas?.Dispose();
		bitmap = new SKBitmap(new SKImageInfo(width, height));
		canvas = new SKCanvas(bitmap);
	}

	[JSExport]
	public static void RenderCpu(int width, int height)
	{
		EnsureBitmap(width, height);
		var c = canvas!;
		var center = new SKPoint(width / 2f, height / 2f);
		var radius = Math.Max(width, height) / 2f;

		c.Clear(SKColors.White);

		using var shader = SKShader.CreateRadialGradient(center, radius, GradientColors, SKShaderTileMode.Clamp);
		using var bgPaint = new SKPaint { IsAntialias = true, Shader = shader };
		c.DrawRect(0, 0, width, height, bgPaint);

		using var circlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
		foreach (var (x, y, r, color) in Circles)
		{
			circlePaint.Color = color;
			c.DrawCircle(x * width, y * height, r * Math.Min(width, height), circlePaint);
		}

		using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
		using var font = new SKFont { Size = width * 0.12f };
		c.DrawText("SkiaSharp", center.X, center.Y + font.Size / 3f, SKTextAlign.Center, font, textPaint);

		Interop.Render(width, height, bitmap!.GetPixelSpan());
	}

	[JSExport]
	public static float RenderGpu(int width, int height, float time, float touchX, float touchY, bool touchActive)
	{
		EnsureBitmap(width, height);

		// Use a CPU surface backed by our bitmap for shader rendering
		using var surface = SKSurface.Create(bitmap!.Info, bitmap.GetPixels(), bitmap.RowBytes);
		var c = surface.Canvas;

		var builder = shaderBuilder.Value;
		builder.Uniforms["iTime"] = time;
		builder.Uniforms["iResolution"] = new float[] { width, height };
		builder.Uniforms["iTouchPos"] = new float[] { touchActive ? touchX : -1f, touchActive ? touchY : -1f };
		builder.Uniforms["iTouchActive"] = touchActive ? 1f : 0f;
		builder.Uniforms["iColors"] = BlobColors;

		using var sh = builder.Build();
		shaderPaint.Shader = sh;
		c.DrawRect(0, 0, width, height, shaderPaint);

		c.Flush();
		Interop.Render(width, height, bitmap.GetPixelSpan());
		return time;
	}

	[JSExport]
	public static void DrawingPointerDown(int width, int height, float x, float y, int colorIndex, float size)
	{
		currentColor = StrokeColors[Math.Clamp(colorIndex, 0, StrokeColors.Length - 1)].Color;
		brushSize = size;
		lastPoint = new SKPoint(x, y);
		currentStroke = new DrawingStroke(new SKPath(), currentColor, brushSize);
		currentStroke.Path.MoveTo(lastPoint);
		RenderDrawing(width, height);
	}

	[JSExport]
	public static void DrawingPointerMove(int width, int height, float x, float y)
	{
		lastPoint = new SKPoint(x, y);
		currentStroke?.Path.LineTo(lastPoint);
		RenderDrawing(width, height);
	}

	[JSExport]
	public static void DrawingPointerUp(int width, int height)
	{
		if (currentStroke is not null)
		{
			strokes.Add(currentStroke);
			currentStroke = null;
		}
		RenderDrawing(width, height);
	}

	[JSExport]
	public static void DrawingClear(int width, int height)
	{
		foreach (var s in strokes)
			s.Path.Dispose();
		strokes.Clear();
		currentStroke?.Path.Dispose();
		currentStroke = null;
		RenderDrawing(width, height);
	}

	static void RenderDrawing(int width, int height)
	{
		EnsureBitmap(width, height);
		var c = canvas!;

		c.Clear(SKColors.White);

		using var strokePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeCap = SKStrokeCap.Round,
			StrokeJoin = SKStrokeJoin.Round,
		};

		foreach (var stroke in strokes)
		{
			strokePaint.Color = stroke.Color;
			strokePaint.StrokeWidth = stroke.StrokeWidth;
			c.DrawPath(stroke.Path, strokePaint);
		}

		if (currentStroke is not null)
		{
			strokePaint.Color = currentStroke.Color;
			strokePaint.StrokeWidth = currentStroke.StrokeWidth;
			c.DrawPath(currentStroke.Path, strokePaint);
		}

		strokePaint.Color = SKColors.Gray;
		strokePaint.StrokeWidth = 1;
		c.DrawCircle(lastPoint, brushSize / 2f, strokePaint);

		Interop.Render(width, height, bitmap!.GetPixelSpan());
	}

	record DrawingStroke(SKPath Path, SKColor Color, float StrokeWidth);
}

static partial class Interop
{
	[JSImport("renderer.render", "main.js")]
	internal static partial void Render(int width, int height, [JSMarshalAs<JSType.MemoryView>] Span<byte> buffer);
}
