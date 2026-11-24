using System;
using System.IO;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample;

public class CpuPage : Box
{
	static readonly (float X, float Y, float R, SKColor Color)[] circles =
	{
		(0.20f, 0.30f, 0.10f, new SKColor(0xFF, 0x4D, 0x66, 0xCC)),
		(0.75f, 0.25f, 0.08f, new SKColor(0x4D, 0xB3, 0xFF, 0xCC)),
		(0.15f, 0.70f, 0.07f, new SKColor(0xFF, 0x99, 0x1A, 0xCC)),
		(0.80f, 0.70f, 0.12f, new SKColor(0x66, 0xFF, 0xB3, 0xCC)),
		(0.50f, 0.15f, 0.06f, new SKColor(0xB3, 0x4D, 0xFF, 0xCC)),
		(0.40f, 0.80f, 0.09f, new SKColor(0xFF, 0xE6, 0x33, 0xCC)),
	};

	static readonly SKColor[] gradientColors =
	{
		new SKColor(0x44, 0x88, 0xFF),
		new SKColor(0x88, 0x33, 0xCC),
	};

	public CpuPage()
		: base(new GObject.ConstructArgument[] { })
	{
		Hexpand = true;
		Vexpand = true;

		var builder = MainWindow.LoadBuilder("CpuPage.ui");
		var container = (Box)builder.GetObject("cpuContainer");

		var skiaView = new SKDrawingArea();
		skiaView.Hexpand = true;
		skiaView.Vexpand = true;
		skiaView.PaintSurface += OnPaintSurface;
		container.Append(skiaView);

		Append(container);
	}

	private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.Info.Width;
		var height = e.Info.Height;
		var center = new SKPoint(width / 2f, height / 2f);
		var radius = Math.Max(width, height) / 2f;

		canvas.Clear(SKColors.White);

		// Background gradient
		using var shader = SKShader.CreateRadialGradient(center, radius, gradientColors, SKShaderTileMode.Clamp);
		using var bgPaint = new SKPaint
		{
			IsAntialias = true,
			Shader = shader,
		};
		canvas.DrawRect(0, 0, width, height, bgPaint);

		// Circles
		using var circlePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Fill,
		};
		foreach (var (x, y, r, color) in circles)
		{
			circlePaint.Color = color;
			canvas.DrawCircle(x * width, y * height, r * Math.Min(width, height), circlePaint);
		}

		// Centered text
		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
		};
		using var font = new SKFont { Size = width * 0.12f };
		canvas.DrawText("SkiaSharp", center.X, center.Y + font.Size / 3f, SKTextAlign.Center, font, textPaint);
	}
}
