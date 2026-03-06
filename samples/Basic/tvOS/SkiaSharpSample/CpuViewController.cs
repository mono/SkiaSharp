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

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;
		canvas.Clear(SKColors.Black);

		// Gradient background
		using var gradientPaint = new SKPaint();
		using var gradient = SKShader.CreateLinearGradient(
			new SKPoint(0, 0),
			new SKPoint(info.Width, info.Height),
			new[] { new SKColor(0xFF1a1a2e), new SKColor(0xFF16213e), new SKColor(0xFF0f3460) },
			null,
			SKShaderTileMode.Clamp);
		gradientPaint.Shader = gradient;
		canvas.DrawRect(SKRect.Create(info.Width, info.Height), gradientPaint);

		// Concentric circles
		float cx = info.Width / 2f;
		float cy = info.Height / 2f;
		float maxRadius = Math.Min(info.Width, info.Height) * 0.35f;

		for (int i = 5; i >= 0; i--)
		{
			float radius = maxRadius * (0.3f + 0.7f * (i / 5f));
			byte alpha = (byte)(40 + i * 35);
			using var circlePaint = new SKPaint
			{
				Color = new SKColor(100, 180, 255, alpha),
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 3
			};
			canvas.DrawCircle(cx, cy, radius, circlePaint);
		}

		// Filled center circle
		using var centerPaint = new SKPaint
		{
			Color = new SKColor(100, 180, 255, 60),
			IsAntialias = true,
			Style = SKPaintStyle.Fill
		};
		canvas.DrawCircle(cx, cy, maxRadius * 0.15f, centerPaint);

		// Title text
		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true
		};
		using var font = new SKFont { Size = 48 };
		canvas.DrawText("SkiaSharp \u2014 CPU Canvas", new SKPoint(cx, cy + maxRadius + 80), SKTextAlign.Center, font, textPaint);

		// Subtitle
		using var subtitleFont = new SKFont { Size = 28 };
		using var subtitlePaint = new SKPaint
		{
			Color = new SKColor(180, 180, 200),
			IsAntialias = true
		};
		canvas.DrawText("Software rendering with SKCanvasView", new SKPoint(cx, cy + maxRadius + 130), SKTextAlign.Center, subtitleFont, subtitlePaint);
	}
}
