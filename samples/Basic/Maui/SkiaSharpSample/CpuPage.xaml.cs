using System;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace SkiaSharpSample
{
	public partial class CpuPage : ContentPage
	{
		readonly (float xf, float yf, float rf, SKColor color)[] circles =
		{
			(0.2f, 0.3f, 0.10f, new SKColor(0xFF, 0x4D, 0x66, 0xCC)),
			(0.75f, 0.25f, 0.08f, new SKColor(0x4D, 0xB3, 0xFF, 0xCC)),
			(0.15f, 0.7f, 0.07f, new SKColor(0xFF, 0x99, 0x1A, 0xCC)),
			(0.8f, 0.7f, 0.12f, new SKColor(0x66, 0xFF, 0xB3, 0xCC)),
			(0.5f, 0.15f, 0.06f, new SKColor(0xB3, 0x4D, 0xFF, 0xCC)),
			(0.4f, 0.8f, 0.09f, new SKColor(0xFF, 0xE6, 0x33, 0xCC)),
		};

		readonly SKPaint bgPaint = new()
		{
			IsAntialias = true
		};

		readonly SKPaint circlePaint = new()
		{
			IsAntialias = true,
			Style = SKPaintStyle.Fill
		};

		readonly SKPaint textPaint = new()
		{
			Color = SKColors.White,
			IsAntialias = true,
			Style = SKPaintStyle.Fill
		};

		readonly SKFont textFont = new()
		{
			Size = 48
		};

		public CpuPage()
		{
			InitializeComponent();
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var width = e.Info.Width;
			var height = e.Info.Height;
			var center = new SKPoint(width / 2f, height / 2f);
			var radius = Math.Max(width, height) / 2f;

			canvas.Clear(SKColors.White);

			using var bgShader = SKShader.CreateRadialGradient(
				center,
				radius,
				new[]
				{
					new SKColor(0x44, 0x88, 0xFF),
					new SKColor(0x88, 0x33, 0xCC)
				},
				SKShaderTileMode.Clamp);
			bgPaint.Shader = bgShader;
			canvas.DrawRect(0, 0, width, height, bgPaint);

			foreach (var (xf, yf, rf, color) in circles)
			{
				circlePaint.Color = color;
				canvas.DrawCircle(
					xf * width,
					yf * height,
					rf * Math.Min(width, height),
					circlePaint);
			}

			canvas.DrawText(
				"SkiaSharp",
				center.X,
				center.Y + textFont.Size / 3f,
				SKTextAlign.Center,
				textFont,
				textPaint);
		}
	}
}
