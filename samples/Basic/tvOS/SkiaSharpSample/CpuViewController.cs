using SkiaSharp;
using SkiaSharp.Views.tvOS;

namespace SkiaSharpSample;

[Register("CpuViewController")]
public class CpuViewController : UIViewController
{
	[Outlet]
	SKCanvasView skiaView { get; set; } = null!;

	public CpuViewController(IntPtr handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		skiaView.IgnorePixelScaling = true;
		skiaView.PaintSurface += OnPaintSurface;
	}

	public override void ViewWillDisappear(bool animated)
	{
		base.ViewWillDisappear(animated);

		if (skiaView != null)
			skiaView.PaintSurface -= OnPaintSurface;
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.Info.Width;
		var height = e.Info.Height;
		var center = new SKPoint(width / 2f, height / 2f);
		var radius = Math.Max(width, height) / 2f;

		canvas.Clear(SKColors.White);

		// Background radial gradient
		using var bgShader = SKShader.CreateRadialGradient(
			center, radius,
			new[] { new SKColor(0x44, 0x88, 0xFF), new SKColor(0x88, 0x33, 0xCC) },
			SKShaderTileMode.Clamp);
		using var bgPaint = new SKPaint { IsAntialias = true, Shader = bgShader };
		canvas.DrawRect(0, 0, width, height, bgPaint);

		// Semi-transparent circles
		var circles = new[]
		{
			(0.2f, 0.3f, 0.10f, new SKColor(0xFF, 0x4D, 0x66, 0xCC)),
			(0.75f, 0.25f, 0.08f, new SKColor(0x4D, 0xB3, 0xFF, 0xCC)),
			(0.15f, 0.7f, 0.07f, new SKColor(0xFF, 0x99, 0x1A, 0xCC)),
			(0.8f, 0.7f, 0.12f, new SKColor(0x66, 0xFF, 0xB3, 0xCC)),
			(0.5f, 0.15f, 0.06f, new SKColor(0xB3, 0x4D, 0xFF, 0xCC)),
			(0.4f, 0.8f, 0.09f, new SKColor(0xFF, 0xE6, 0x33, 0xCC)),
		};
		using var circlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
		foreach (var (xf, yf, rf, color) in circles)
		{
			circlePaint.Color = color;
			canvas.DrawCircle(xf * width, yf * height, rf * Math.Min(width, height), circlePaint);
		}

		// Centered "CPU Canvas" text
		using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
		using var font = new SKFont { Size = 64 };
		canvas.DrawText("CPU Canvas", center.X, center.Y + font.Size / 3f, SKTextAlign.Center, font, textPaint);

		// Platform label
		using var labelFont = new SKFont { Size = 28 };
		canvas.DrawText("SkiaSharp \u00B7 tvOS", center.X, height - 60f, SKTextAlign.Center, labelFont, textPaint);
	}
}
