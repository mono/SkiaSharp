using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace SkiaSharpSample
{
	public partial class DrawingPage : ContentPage
	{
		static readonly (SKColor Light, SKColor Dark)[] colorPalette =
		{
			(SKColors.Black, SKColors.White),
			(new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),
			(new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),
			(new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),
			(new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),
			(new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),
		};

		readonly List<(SKPath Path, SKColor Color, float Width)> strokes = new();
		SKPath? currentPath;
		SKColor currentColor;
		float brushSize = 4f;

		bool IsDarkMode => Application.Current?.RequestedTheme == AppTheme.Dark;
		SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;

		public DrawingPage()
		{
			InitializeComponent();
			currentColor = IsDarkMode ? SKColors.White : SKColors.Black;
		}

		void OnThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			// Swap black/white when theme changes
			if (currentColor == SKColors.Black && IsDarkMode)
				currentColor = SKColors.White;
			else if (currentColor == SKColors.White && !IsDarkMode)
				currentColor = SKColors.Black;
			skiaView.InvalidateSurface();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			if (Application.Current != null)
				Application.Current.RequestedThemeChanged -= OnThemeChanged;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (Application.Current != null)
				Application.Current.RequestedThemeChanged += OnThemeChanged;
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

				case SKTouchAction.Cancelled:
					currentPath?.Dispose();
					currentPath = null;
					break;

				case SKTouchAction.WheelChanged:
					brushSize = Math.Clamp(brushSize + e.WheelDelta, 1f, 50f);
					brushLabel.Text = $"Brush: {brushSize:F0}px";
					break;
			}
			e.Handled = true;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear(CanvasBackground);

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
				var light = new SKColor(
					(byte)(mauiColor.Red * 255),
					(byte)(mauiColor.Green * 255),
					(byte)(mauiColor.Blue * 255));
				var entry = colorPalette.FirstOrDefault(p => p.Light == light);
				currentColor = entry.Light != default
					? (IsDarkMode ? entry.Dark : entry.Light)
					: light;
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
