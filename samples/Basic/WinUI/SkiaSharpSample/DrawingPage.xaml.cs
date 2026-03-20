using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace SkiaSharpSample;

public sealed partial class DrawingPage : Page
{
	static readonly (string Tag, SKColor Light, SKColor Dark)[] colorPalette =
	{
		("Black",  SKColors.Black, SKColors.White),
		("Red",    new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),
		("Blue",   new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),
		("Green",  new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),
		("Orange", new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),
		("Purple", new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),
	};

	readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
	Border? selectedColorBorder;
	SKPath? currentPath;
	SKColor currentColor;
	float brushSize = 4f;
	SKPoint cursorPosition;
	bool isCursorOver;

	bool IsDarkMode => ActualTheme == ElementTheme.Dark;
	SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;

	public DrawingPage()
	{
		InitializeComponent();
		currentColor = IsDarkMode ? SKColors.White : SKColors.Black;
		selectedColorBorder = ColorBlack;

		ActualThemeChanged += OnThemeChanged;
		Loaded += OnLoaded;
	}

	void OnLoaded(object sender, RoutedEventArgs e)
	{
		ApplySwatchColors();
	}

	void OnThemeChanged(FrameworkElement sender, object args)
	{
		if (currentColor == SKColors.Black && IsDarkMode)
			currentColor = SKColors.White;
		else if (currentColor == SKColors.White && !IsDarkMode)
			currentColor = SKColors.Black;

		ApplySwatchColors();
		skiaView.Invalidate();
	}

	void ApplySwatchColors()
	{
		foreach (var child in swatchPanel.Children)
		{
			if (child is Border border && border.Tag is string tag)
			{
				foreach (var (pTag, light, dark) in colorPalette)
				{
					if (pTag == tag)
					{
						var color = IsDarkMode ? dark : light;
						border.Background = new SolidColorBrush(
							Windows.UI.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));
						break;
					}
				}
			}
		}
	}

	private void OnColorTapped(object sender, TappedRoutedEventArgs e)
	{
		if (sender is not Border border || border.Tag is not string tag)
			return;

		foreach (var (pTag, light, dark) in colorPalette)
		{
			if (pTag == tag)
			{
				currentColor = IsDarkMode ? dark : light;

				if (selectedColorBorder != null)
					selectedColorBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
				border.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue);
				selectedColorBorder = border;
				break;
			}
		}
	}

	private void OnClearClicked(object sender, RoutedEventArgs e)
	{
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		currentPath?.Dispose();
		currentPath = null;
		skiaView.Invalidate();
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

		float sx = (float)e.Info.Width / (float)skiaView.ActualWidth;
		float sy = (float)e.Info.Height / (float)skiaView.ActualHeight;
		canvas.Scale(sx, sy);

		foreach (var (path, color, strokeWidth) in strokes)
		{
			paint.Color = color;
			paint.StrokeWidth = strokeWidth;
			canvas.DrawPath(path, paint);
		}

		if (currentPath != null)
		{
			paint.Color = currentColor;
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(currentPath, paint);
		}

		if (isCursorOver)
		{
			using var indicatorPaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = currentColor.WithAlpha(128),
				StrokeWidth = 1.5f,
			};
			canvas.DrawCircle(cursorPosition.X, cursorPosition.Y, brushSize / 2f, indicatorPaint);
		}
	}

	private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(skiaView);
		currentPath = new SKPath();
		currentPath.MoveTo((float)point.Position.X, (float)point.Position.Y);
		cursorPosition = new SKPoint((float)point.Position.X, (float)point.Position.Y);
		isCursorOver = true;
		skiaView.CapturePointer(e.Pointer);
		skiaView.Invalidate();
	}

	private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(skiaView);
		cursorPosition = new SKPoint((float)point.Position.X, (float)point.Position.Y);
		isCursorOver = true;
		currentPath?.LineTo((float)point.Position.X, (float)point.Position.Y);
		skiaView.Invalidate();
	}

	private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
	{
		if (currentPath != null)
		{
			strokes.Add((currentPath, currentColor, brushSize));
			currentPath = null;
			skiaView.Invalidate();
		}
		skiaView.ReleasePointerCapture(e.Pointer);
	}

	private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(skiaView);
		var delta = point.Properties.MouseWheelDelta;
		brushSize = Math.Clamp(brushSize + (delta > 0 ? 1f : -1f), 1f, 50f);
		brushText.Text = $"{brushSize:F0}";
		BrushSlider.Value = brushSize;
		skiaView.Invalidate();
	}

	private void OnBrushSizeChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
	{
		brushSize = (float)e.NewValue;
		if (brushText != null)
			brushText.Text = $"{brushSize:F0}";
		skiaView?.Invalidate();
	}
}
