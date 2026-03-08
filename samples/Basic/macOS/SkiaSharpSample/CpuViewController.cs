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
		SKCanvasView? skiaView;

		public override void LoadView()
		{
			skiaView = new SKCanvasView();
			View = skiaView;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			if (skiaView != null)
			{
				skiaView.IgnorePixelScaling = true;
				skiaView.PaintSurface += OnPaintSurface;
			}
		}

		public override void ViewWillDisappear()
		{
			base.ViewWillDisappear();
			if (skiaView != null)
				skiaView.PaintSurface -= OnPaintSurface;
		}

		void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
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

			// Centered "SkiaSharp" text
			using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
			using var font = new SKFont { Size = width * 0.12f };
			canvas.DrawText("SkiaSharp", center.X, center.Y + font.Size / 3f, SKTextAlign.Center, font, textPaint);
		}
	}
}
