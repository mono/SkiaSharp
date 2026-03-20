using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample;

public partial class DrawingPage : UserControl
{
	record struct Stroke(SKPath Path, SKColor Color, float Width);

	static readonly (string Tag, SKColor Light, SKColor Dark)[] colorPalette =
	[
		("Black",  SKColors.Black, SKColors.White),
		("Red",    new(0xE5, 0x39, 0x35), new(0xEF, 0x53, 0x50)),
		("Blue",   new(0x1E, 0x88, 0xE5), new(0x42, 0xA5, 0xF5)),
		("Green",  new(0x43, 0xA0, 0x47), new(0x66, 0xBB, 0x6A)),
		("Orange", new(0xFB, 0x8C, 0x00), new(0xFF, 0xA7, 0x26)),
		("Purple", new(0x8E, 0x24, 0xAA), new(0xAB, 0x47, 0xBC)),
	];

	readonly List<Stroke> strokes = [];
	SKPath? currentPath;
	SKColor currentColor;
	float brushSize = 4f;
	float cursorX;
	float cursorY;
	bool isDrawing;
	double dpiScale = 1.0;
	Border? selectedColorBorder;

	SKColor CanvasBackground => IsDarkMode() ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;

	public DrawingPage()
	{
		InitializeComponent();
		currentColor = IsDarkMode() ? SKColors.White : SKColors.Black;
		selectedColorBorder = ColorBlack;

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	void OnThemeChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			if (currentColor == SKColors.Black && IsDarkMode())
				currentColor = SKColors.White;
			else if (currentColor == SKColors.White && !IsDarkMode())
				currentColor = SKColors.Black;
			ApplySwatchColors();
			SkCanvas.InvalidateVisual();
		});
	}

	void OnLoaded(object sender, RoutedEventArgs e)
	{
		SystemEvents.UserPreferenceChanged += OnThemeChanged;
		CanvasBorder.MouseDown += OnMouseDown;
		CanvasBorder.MouseMove += OnMouseMove;
		CanvasBorder.MouseUp += OnMouseUp;
		CanvasBorder.MouseWheel += OnMouseWheel;
		CanvasBorder.MouseLeave += OnMouseLeave;
		ApplySwatchColors();
	}

	void OnUnloaded(object sender, RoutedEventArgs e)
	{
		SystemEvents.UserPreferenceChanged -= OnThemeChanged;
		CanvasBorder.MouseDown -= OnMouseDown;
		CanvasBorder.MouseMove -= OnMouseMove;
		CanvasBorder.MouseUp -= OnMouseUp;
		CanvasBorder.MouseWheel -= OnMouseWheel;
		CanvasBorder.MouseLeave -= OnMouseLeave;
	}

	void ApplySwatchColors()
	{
		var dark = IsDarkMode();
		foreach (var child in SwatchPanel.Children)
		{
			if (child is not Border border || border.Tag is not string tag)
				continue;

			foreach (var (pTag, light, darkColor) in colorPalette)
			{
				if (pTag != tag)
					continue;

				var c = dark ? darkColor : light;
				border.Background = new SolidColorBrush(Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue));
				break;
			}
		}
	}

	void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(CanvasBackground);

		var source = PresentationSource.FromVisual(this);
		if (source != null)
			dpiScale = source.CompositionTarget.TransformToDevice.M11;

		using var paint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeCap = SKStrokeCap.Round,
			StrokeJoin = SKStrokeJoin.Round,
		};

		foreach (var stroke in strokes)
		{
			paint.Color = stroke.Color;
			paint.StrokeWidth = stroke.Width;
			canvas.DrawPath(stroke.Path, paint);
		}

		if (currentPath != null)
		{
			paint.Color = currentColor;
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(currentPath, paint);
		}

		using var indicatorPaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = currentColor.WithAlpha(128),
			StrokeWidth = 1.5f,
		};
		canvas.DrawCircle(
			(float)(cursorX * dpiScale),
			(float)(cursorY * dpiScale),
			brushSize / 2f,
			indicatorPaint);
	}

	void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton != MouseButtonState.Pressed)
			return;

		isDrawing = true;
		var pos = e.GetPosition(CanvasBorder);
		cursorX = (float)pos.X;
		cursorY = (float)pos.Y;

		currentPath = new SKPath();
		currentPath.MoveTo((float)(pos.X * dpiScale), (float)(pos.Y * dpiScale));
		CanvasBorder.CaptureMouse();
		SkCanvas.InvalidateVisual();
	}

	void OnMouseMove(object sender, MouseEventArgs e)
	{
		var pos = e.GetPosition(CanvasBorder);
		cursorX = (float)pos.X;
		cursorY = (float)pos.Y;

		if (isDrawing && currentPath != null)
			currentPath.LineTo((float)(pos.X * dpiScale), (float)(pos.Y * dpiScale));

		SkCanvas.InvalidateVisual();
	}

	void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (!isDrawing || currentPath == null)
			return;

		isDrawing = false;
		strokes.Add(new Stroke(currentPath, currentColor, brushSize));
		currentPath = null;
		CanvasBorder.ReleaseMouseCapture();
		SkCanvas.InvalidateVisual();
	}

	void OnMouseWheel(object sender, MouseWheelEventArgs e)
	{
		brushSize = Math.Clamp(brushSize + (e.Delta > 0 ? 1f : -1f), 1f, 50f);
		BrushSizeText.Text = $"{brushSize:F0}";
		BrushSlider.Value = brushSize;
		SkCanvas.InvalidateVisual();
	}

	void OnBrushSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		brushSize = (float)e.NewValue;
		if (BrushSizeText != null)
			BrushSizeText.Text = $"{brushSize:F0}";
		SkCanvas?.InvalidateVisual();
	}

	void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (!isDrawing)
			SkCanvas.InvalidateVisual();
	}

	void OnColorPick(object sender, MouseButtonEventArgs e)
	{
		if (sender is not Border border || border.Tag is not string tag)
			return;

		var dark = IsDarkMode();
		foreach (var (pTag, light, darkColor) in colorPalette)
		{
			if (pTag != tag)
				continue;

			currentColor = dark ? darkColor : light;
			if (selectedColorBorder != null)
				selectedColorBorder.BorderBrush = Brushes.Transparent;
			border.BorderBrush = Brushes.DodgerBlue;
			selectedColorBorder = border;
			break;
		}
	}

	void OnClear(object sender, RoutedEventArgs e)
	{
		foreach (var stroke in strokes)
			stroke.Path.Dispose();
		strokes.Clear();

		currentPath?.Dispose();
		currentPath = null;
		isDrawing = false;

		SkCanvas.InvalidateVisual();
	}

	bool IsDarkMode()
	{
		var window = Window.GetWindow(this);
		if (window != null)
		{
			var mode = window.ThemeMode;
			if (mode == ThemeMode.Dark) return true;
			if (mode == ThemeMode.Light) return false;
		}

		try
		{
			using var key = Registry.CurrentUser.OpenSubKey(
				@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
			return key?.GetValue("AppsUseLightTheme") is int i && i == 0;
		}
		catch { return false; }
	}
}
