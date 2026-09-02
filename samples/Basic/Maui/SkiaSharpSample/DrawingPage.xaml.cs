using System;
using System.Collections.Generic;
using System.Linq;
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
	readonly Dictionary<long, SKPoint> activeTouches = new();
	SKPathBuilder? currentBuilder;
	long? strokeTouchId;
	float? previousPinchDistance;
	SKPoint? hoverPoint;
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
			case SKTouchAction.Entered:
				hoverPoint = e.Location;
				skiaView.InvalidateSurface();
				break;

			case SKTouchAction.Pressed:
				activeTouches[e.Id] = e.Location;
				hoverPoint = null;
				if (activeTouches.Count == 1)
				{
					strokeTouchId = e.Id;
					currentBuilder = new SKPathBuilder();
					currentBuilder.MoveTo(e.Location);
				}
				else if (activeTouches.Count >= 2)
				{
					currentBuilder?.Dispose();
					currentBuilder = null;
					strokeTouchId = null;
					previousPinchDistance = activeTouches.Count == 2 ? GetPinchDistance() : null;
				}
				break;

			case SKTouchAction.Moved:
				if (!e.InContact)
				{
					hoverPoint = e.Location;
				}
				else
				{
					activeTouches[e.Id] = e.Location;
					if (activeTouches.Count == 2)
					{
						var distance = GetPinchDistance();
						if (previousPinchDistance is > 0)
							SetBrushSize(brushSize * distance / previousPinchDistance.Value);
						previousPinchDistance = distance;
					}
					else if (activeTouches.Count > 2)
					{
						previousPinchDistance = null;
					}
					else if (strokeTouchId == e.Id)
					{
						currentBuilder?.LineTo(e.Location);
					}
				}
				skiaView.InvalidateSurface();
				break;

			case SKTouchAction.Released:
				activeTouches.Remove(e.Id);
				if (strokeTouchId == e.Id && currentBuilder != null)
				{
					strokes.Add((currentBuilder.Detach(), currentColor, brushSize));
					currentBuilder = null;
				}
				if (strokeTouchId == e.Id)
					strokeTouchId = null;
				previousPinchDistance = activeTouches.Count == 2 ? GetPinchDistance() : null;
				skiaView.InvalidateSurface();
				break;

			case SKTouchAction.Cancelled:
				activeTouches.Remove(e.Id);
				if (strokeTouchId == e.Id)
				{
					currentBuilder?.Dispose();
					currentBuilder = null;
					strokeTouchId = null;
				}
				previousPinchDistance = activeTouches.Count == 2 ? GetPinchDistance() : null;
				break;

			case SKTouchAction.Exited:
				hoverPoint = null;
				skiaView.InvalidateSurface();
				break;

			case SKTouchAction.WheelChanged:
				SetBrushSize(brushSize + e.WheelDelta / 120f);
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

		if (currentBuilder != null)
		{
			using var path = currentBuilder.Snapshot();
			paint.Color = currentColor;
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(path, paint);
		}

		if (hoverPoint is { } hover)
		{
			paint.Color = currentColor.WithAlpha(128);
			paint.StrokeWidth = 1;
			canvas.DrawCircle(hover, brushSize / 2f + 2f, paint);
		}
	}

	private float GetPinchDistance()
	{
		var points = activeTouches.Values.Take(2).ToArray();
		return points.Length == 2 ? SKPoint.Distance(points[0], points[1]) : 0;
	}

	private void SetBrushSize(float value)
	{
		brushSize = Math.Clamp(value, 1f, 50f);
		brushSlider.Value = brushSize;
		brushLabel.Text = $"{brushSize:F0}";
		skiaView.InvalidateSurface();
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
		SetBrushSize((float)e.NewValue);
	}

	private void OnClearClicked(object sender, EventArgs e)
	{
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		currentBuilder?.Dispose();
		currentBuilder = null;
		activeTouches.Clear();
		strokeTouchId = null;
		previousPinchDistance = null;
		skiaView.InvalidateSurface();
	}
}
