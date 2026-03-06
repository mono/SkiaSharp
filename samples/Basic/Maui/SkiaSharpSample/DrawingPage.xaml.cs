using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace SkiaSharpSample
{
	public partial class DrawingPage : ContentPage
	{
		readonly List<(SKPath Path, SKColor Color, float Width)> strokes = new();
		SKPath? currentPath;
		SKColor currentColor = SKColors.Black;
		float brushSize = 8f;

		public DrawingPage()
		{
			InitializeComponent();
		}

		private void OnTouch(object sender, SKTouchEventArgs e)
		{
			switch (e.ActionType)
			{
				case SKTouchAction.Pressed:
					currentPath = new SKPath();
					currentPath.MoveTo(e.Location);
					break;

				case SKTouchAction.Moved:
					currentPath?.LineTo(e.Location);
					skiaView.InvalidateSurface();
					break;

				case SKTouchAction.Released:
					if (currentPath != null)
					{
						strokes.Add((currentPath, currentColor, brushSize));
						currentPath = null;
						skiaView.InvalidateSurface();
					}
					break;

				case SKTouchAction.WheelChanged:
					brushSize = Math.Clamp(brushSize + e.WheelDelta, 1f, 100f);
					brushLabel.Text = $"Brush: {brushSize:F0}px";
					break;
			}
			e.Handled = true;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeCap = SKStrokeCap.Round,
				StrokeJoin = SKStrokeJoin.Round,
			};

			foreach (var (path, color, width) in strokes)
			{
				paint.Color = color;
				paint.StrokeWidth = width;
				canvas.DrawPath(path, paint);
			}

			if (currentPath != null)
			{
				paint.Color = currentColor;
				paint.StrokeWidth = brushSize;
				canvas.DrawPath(currentPath, paint);
			}
		}

		private void OnColorClicked(object sender, EventArgs e)
		{
			if (sender is Button btn && btn.BackgroundColor is Color mauiColor)
			{
				currentColor = new SKColor(
					(byte)(mauiColor.Red * 255),
					(byte)(mauiColor.Green * 255),
					(byte)(mauiColor.Blue * 255),
					(byte)(mauiColor.Alpha * 255));
			}
		}

		private void OnClearClicked(object sender, EventArgs e)
		{
			foreach (var (path, _, _) in strokes)
				path.Dispose();
			strokes.Clear();
			currentPath?.Dispose();
			currentPath = null;
			skiaView.InvalidateSurface();
		}
	}
}
