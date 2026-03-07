using System;
using AppKit;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample
{
	[Register("CpuViewController")]
	public class CpuViewController : NSViewController
	{
		[Outlet]
		SKCanvasView? skiaView { get; set; }

		public CpuViewController(IntPtr handle) : base(handle) { }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			if (skiaView != null)
			{
				skiaView.IgnorePixelScaling = true;
				skiaView.PaintSurface += OnPaintSurface;
			}
		}

		void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var width = e.Info.Width;
			var height = e.Info.Height;
			var cx = width / 2f;
			var cy = height / 2f;
			var radius = Math.Min(width, height) * 0.4f;

			canvas.Clear(SKColors.White);

			// Radial gradient background
			using var gradientPaint = new SKPaint();
			using var shader = SKShader.CreateRadialGradient(
				new SKPoint(cx, cy), radius,
				new[] { new SKColor(0x44, 0x88, 0xFF), new SKColor(0x88, 0x33, 0xCC) },
				null, SKShaderTileMode.Clamp);
			gradientPaint.Shader = shader;
			canvas.DrawCircle(cx, cy, radius, gradientPaint);

			// Decorative concentric circles
			using var circlePaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 2,
				Color = new SKColor(255, 255, 255, 100),
			};
			for (int i = 1; i <= 5; i++)
				canvas.DrawCircle(cx, cy, radius * i / 5f, circlePaint);

			// Title text
			using var textPaint = new SKPaint
			{
				Color = SKColors.White,
				IsAntialias = true,
			};
			using var font = new SKFont { Size = 36 };
			canvas.DrawText("SkiaSharp", new SKPoint(cx, cy), SKTextAlign.Center, font, textPaint);

			// Subtitle
			using var subtitleFont = new SKFont { Size = 16 };
			textPaint.Color = new SKColor(255, 255, 255, 180);
			canvas.DrawText("CPU Canvas", new SKPoint(cx, cy + 30), SKTextAlign.Center, subtitleFont, textPaint);
		}
	}
}
