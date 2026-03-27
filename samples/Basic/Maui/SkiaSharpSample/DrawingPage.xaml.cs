using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace SkiaSharpSample;

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

	Button? selectedSwatch;

	public DrawingPage()
	{
		InitializeComponent();
		currentColor = IsDarkMode ? SKColors.White : SKColors.Black;

		// First swatch is pre-selected in XAML (BorderColor=DodgerBlue)
		if (swatchGrid.Children.Count > 0 && swatchGrid.Children[0] is Button firstBtn)
			selectedSwatch = firstBtn;
	}

	void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
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
				brushSlider.Value = brushSize;
				brushLabel.Text = $"{brushSize:F0}";
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
		if (sender is Button btn && btn.Parent is Layout parent)
		{
			var index = parent.Children.IndexOf(btn);
			if (index >= 0 && index < colorPalette.Length)
			{
				var (light, dark) = colorPalette[index];
				currentColor = IsDarkMode ? dark : light;

				if (selectedSwatch != null)
					selectedSwatch.BorderColor = Colors.Transparent;
				btn.BorderColor = Colors.DodgerBlue;
				selectedSwatch = btn;
			}
		}
	}

	private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
	{
		brushSize = (float)e.NewValue;
		brushLabel.Text = $"{brushSize:F0}";
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
