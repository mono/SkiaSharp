using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// The "touch many things" tracker: one representative composite frame rendered onto a reused
// CPU surface, deliberately broad so that many unrelated future optimizations each nudge it a
// little (the opposite of a micro-benchmark that only moves for one change). It walks the
// common draw path end to end — transforms, linear + radial gradient shaders, filled and
// stroked primitives, a built path, a scaled image draw (the CPU image-sampling path from
// #4421), a colour-filter layer, and clipping — then a managed colour-maths step (premultiply
// + SKColor->SKColorF over a small palette, weaving in #4370 / #4385). No fonts or external
// content (machine-dependent); `Complexity` scales how many times the scene is layered.
public class SceneRenderBenchmark
{
	[Params(1, 4)]
	public int Complexity { get; set; }

	private const int Size = 512;
	private const int SourceSize = 128;

	private SKSurface _surface = null!;
	private SKCanvas _canvas = null!;
	private SKImage _image = null!;
	private SKPaint _fill = null!;
	private SKPaint _stroke = null!;
	private SKPaint _linear = null!;
	private SKPaint _radial = null!;
	private SKPaint _filtered = null!;
	private SKPath _path = null!;
	private SKColor[] _palette = null!;
	private SKRect _srcRect;
	private SKRect _dstRect;

	[GlobalSetup]
	public void Setup ()
	{
		_surface = SKSurface.Create (new SKImageInfo (Size, Size));
		_canvas = _surface.Canvas;
		_image = CreateSource ();

		_fill = new SKPaint { Color = SKColors.CornflowerBlue, IsAntialias = true, Style = SKPaintStyle.Fill };
		_stroke = new SKPaint { Color = SKColors.OrangeRed, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };

		_linear = new SKPaint { IsAntialias = true };
		_linear.Shader = SKShader.CreateLinearGradient (
			new SKPoint (0, 0), new SKPoint (Size, Size),
			new[] { SKColors.MidnightBlue, SKColors.Teal, SKColors.Gold },
			SKShaderTileMode.Clamp);

		_radial = new SKPaint { IsAntialias = true };
		_radial.Shader = SKShader.CreateRadialGradient (
			new SKPoint (Size / 2f, Size / 2f), Size / 2f,
			new[] { SKColors.White, SKColors.DarkSlateGray },
			SKShaderTileMode.Clamp);

		_filtered = new SKPaint { IsAntialias = true, Color = SKColors.White };
		_filtered.ColorFilter = SKColorFilter.CreateBlendMode (SKColors.MediumPurple, SKBlendMode.Multiply);

		// A representative palette for the managed colour-maths step. Deterministic + varied alpha.
		var rnd = new Random (42);
		_palette = new SKColor[64];
		for (var i = 0; i < _palette.Length; i++)
			_palette[i] = new SKColor ((byte)rnd.Next (256), (byte)rnd.Next (256), (byte)rnd.Next (256), (byte)rnd.Next (256));

		// Version-appropriate path construction: SKPathBuilder on 4.x+, classic SKPath below.
#if SKIASHARP_4_OR_GREATER
		using (var builder = new SKPathBuilder ()) {
			builder.MoveTo (40, 40);
			builder.LineTo (300, 90);
			builder.CubicTo (360, 200, 120, 260, 200, 380);
			builder.LineTo (60, 300);
			builder.Close ();
			_path = builder.Snapshot ();
		}
#else
		_path = new SKPath ();
		_path.MoveTo (40, 40);
		_path.LineTo (300, 90);
		_path.CubicTo (360, 200, 120, 260, 200, 380);
		_path.LineTo (60, 300);
		_path.Close ();
#endif

		_srcRect = new SKRect (0, 0, SourceSize, SourceSize);
		_dstRect = new SKRect (0, 0, Size * 0.7f, Size * 0.7f); // non-integer scale => sampling
	}

	[GlobalCleanup]
	public void Cleanup ()
	{
		_path.Dispose ();
		_filtered.Dispose ();
		_radial.Dispose ();
		_linear.Dispose ();
		_stroke.Dispose ();
		_fill.Dispose ();
		_image.Dispose ();
		_surface.Dispose ();
	}

	// A busy, non-uniform source image so the scaled draw genuinely samples texels.
	private static SKImage CreateSource ()
	{
		using var surface = SKSurface.Create (new SKImageInfo (SourceSize, SourceSize));
		var canvas = surface.Canvas;
		canvas.Clear (SKColors.White);
		using var fill = new SKPaint { IsAntialias = false };
		for (var y = 0; y < SourceSize; y += 16) {
			for (var x = 0; x < SourceSize; x += 16) {
				fill.Color = ((x + y) / 16 % 2 == 0) ? SKColors.SeaGreen : SKColors.Goldenrod;
				canvas.DrawRect (new SKRect (x, y, x + 12, y + 12), fill);
			}
		}
		return surface.Snapshot ();
	}

	[Benchmark]
	public int RenderFrame ()
	{
		_canvas.Clear (SKColors.White);

		for (var r = 0; r < Complexity; r++) {
			_canvas.Save ();
			_canvas.Translate (r * 8f, r * 8f);
			_canvas.RotateDegrees (r * 5f, Size / 2f, Size / 2f);
			_canvas.Scale (1f - r * 0.05f);

			// Gradient shader fills (linear + radial).
			_canvas.DrawRect (new SKRect (0, 0, Size, Size), _linear);
			_canvas.DrawCircle (Size / 2f, Size / 2f, Size / 3f, _radial);

			// Filled + stroked primitives.
			_canvas.DrawRect (new SKRect (40, 40, 220, 180), _fill);
			_canvas.DrawRoundRect (new SKRect (60, 220, 260, 360), 18, 18, _stroke);
			_canvas.DrawOval (new SKRect (280, 60, 460, 200), _fill);
			_canvas.DrawCircle (360, 300, 70, _stroke);

			// A built path.
			_canvas.DrawPath (_path, _stroke);

			// Scaled image draw (the #4421 CPU sampling path).
			_canvas.DrawImage (_image, _srcRect, _dstRect, SKSamplingOptions.Default, _fill);

			// A colour-filter layer.
			_canvas.DrawRect (new SKRect (150, 150, 380, 320), _filtered);

			// Clipping + a clipped path draw.
			_canvas.ClipRect (new SKRect (80, 80, 420, 420));
			_canvas.DrawPath (_path, _fill);

			_canvas.Restore ();
		}

		// Managed colour maths over the palette (weaves in #4385 + #4370) with a live sink.
		var sink = 0;
		foreach (var c in _palette) {
			sink ^= (int)(uint)SKPMColor.PreMultiply (c);
			SKColorF f = c;
			sink ^= (int)(f.Red * 255f);
		}

		_canvas.Flush ();
		return sink;
	}
}
