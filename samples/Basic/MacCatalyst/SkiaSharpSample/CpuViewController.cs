using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

[Register("CpuViewController")]
public class CpuViewController : UIViewController
{
	SKCanvasView? skiaView;

	public CpuViewController(IntPtr handle)
		: base(handle)
	{
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		skiaView = new SKCanvasView { TranslatesAutoresizingMaskIntoConstraints = false };
		View!.AddSubview(skiaView);
		NSLayoutConstraint.ActivateConstraints(new[]
		{
			skiaView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
			skiaView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
			skiaView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
			skiaView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
		});

		skiaView.PaintSurface += OnPaintSurface;
	}

	void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;
		canvas.Clear(SKColors.White);

		float cx = info.Width / 2f;
		float cy = info.Height / 2f;
		float radius = Math.Min(info.Width, info.Height) * 0.4f;

		// Background gradient
		using var gradientPaint = new SKPaint { IsAntialias = true };
		using var shader = SKShader.CreateRadialGradient(
			new SKPoint(cx, cy), radius,
			new[] { new SKColor(0x44, 0x88, 0xFF), new SKColor(0x88, 0x33, 0xCC) },
			SKShaderTileMode.Clamp);
		gradientPaint.Shader = shader;
		canvas.DrawCircle(cx, cy, radius, gradientPaint);

		// Orbiting circles
		using var circlePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Fill,
		};
		var colors = new SKColor[]
		{
			new(0xFF, 0x60, 0x60, 0xCC),
			new(0x60, 0xFF, 0x60, 0xCC),
			new(0xFF, 0xFF, 0x60, 0xCC),
			new(0xFF, 0x60, 0xFF, 0xCC),
			new(0x60, 0xFF, 0xFF, 0xCC),
		};
		for (int i = 0; i < colors.Length; i++)
		{
			float angle = (float)(i * 2 * Math.PI / colors.Length);
			float orbitRadius = radius * 0.65f;
			float x = cx + orbitRadius * (float)Math.Cos(angle);
			float y = cy + orbitRadius * (float)Math.Sin(angle);

			circlePaint.Color = colors[i];
			canvas.DrawCircle(x, y, radius * 0.15f, circlePaint);
		}

		// Center text
		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
		};
		using var font = new SKFont { Size = radius * 0.2f };
		canvas.DrawText("CPU Canvas", new SKPoint(cx, cy + font.Size * 0.35f),
			SKTextAlign.Center, font, textPaint);

		// Label
		using var labelFont = new SKFont { Size = 28 };
		using var labelPaint = new SKPaint
		{
			Color = new SKColor(0x33, 0x33, 0x33),
			IsAntialias = true,
		};
		canvas.DrawText("SkiaSharp · Mac Catalyst", new SKPoint(cx, info.Height - 40),
			SKTextAlign.Center, labelFont, labelPaint);
	}
}
