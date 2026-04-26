using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace SkiaSharpSample;

public sealed partial class DrawingPage : Page
{
	private static readonly Dictionary<string, (SKColor Light, SKColor Dark)> ColorMap = new()
	{
		["Black"] = (SKColors.Black, SKColors.White),
		["Red"] = (new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),
		["Blue"] = (new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),
		["Green"] = (new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),
		["Orange"] = (new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),
		["Purple"] = (new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),
	};

	private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
	private SKPath? currentPath;
	private SKColor currentColor;
	private float brushSize = 4f;
	private SKPoint cursorPosition;
	private bool isCursorOver;

	bool IsDarkMode => ActualTheme == ElementTheme.Dark;
	SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;
	SKColor ResolveColor((SKColor Light, SKColor Dark) pair) => IsDarkMode ? pair.Dark : pair.Light;

	public DrawingPage()
	{
		InitializeComponent();
		currentColor = IsDarkMode ? SKColors.White : SKColors.Black;

		ActualThemeChanged += (s, e) =>
		{
			if (currentColor == SKColors.Black && IsDarkMode)
				currentColor = SKColors.White;
			else if (currentColor == SKColors.White && !IsDarkMode)
				currentColor = SKColors.Black;
			skiaView.Invalidate();
		};
	}

	private void OnColorTapped(object? sender, TappedRoutedEventArgs e)
	{
		if (sender is Border border && border.Tag is string tag && ColorMap.TryGetValue(tag, out var pair))
			currentColor = ResolveColor(pair);
	}

	private void OnClearClicked(object? sender, RoutedEventArgs e)
	{
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		currentPath?.Dispose();
		currentPath = null;
		skiaView.Invalidate();
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
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

	private void OnPointerPressed(object? sender, PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(skiaView);
		currentPath = new SKPath();
		currentPath.MoveTo((float)point.Position.X, (float)point.Position.Y);
		cursorPosition = new SKPoint((float)point.Position.X, (float)point.Position.Y);
		isCursorOver = true;
		skiaView.CapturePointer(e.Pointer);
		skiaView.Invalidate();
	}

	private void OnPointerMoved(object? sender, PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(skiaView);
		cursorPosition = new SKPoint((float)point.Position.X, (float)point.Position.Y);
		isCursorOver = true;
		currentPath?.LineTo((float)point.Position.X, (float)point.Position.Y);
		skiaView.Invalidate();
	}

	private void OnPointerReleased(object? sender, PointerRoutedEventArgs e)
	{
		if (currentPath != null)
		{
			strokes.Add((currentPath, currentColor, brushSize));
			currentPath = null;
			skiaView.Invalidate();
		}
		skiaView.ReleasePointerCapture(e.Pointer);
	}

	private void OnPointerExited(object? sender, PointerRoutedEventArgs e)
	{
		isCursorOver = false;
		skiaView.Invalidate();
	}

	private void OnPointerWheelChanged(object? sender, PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(skiaView);
		var delta = point.Properties.MouseWheelDelta;
		brushSize = Math.Max(1f, Math.Min(50f, brushSize + (delta > 0 ? 1f : -1f)));
		brushSlider.Value = brushSize;
		brushText.Text = $"{brushSize:F0}";
		skiaView.Invalidate();
	}

	private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		brushSize = (float)e.NewValue;
		if (brushText != null)
			brushText.Text = $"{brushSize:F0}";
	}
}
