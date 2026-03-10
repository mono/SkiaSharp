using System;
using System.Collections.Generic;
using Gdk;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample;

public class DrawingPage : Box
{
	private static readonly (string Name, SKColor Light, SKColor Dark)[] ColorOptions =
	{
		("Black", SKColors.Black, SKColors.White),
		("Red", new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),
		("Blue", new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),
		("Green", new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),
		("Orange", new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),
		("Purple", new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),
	};

	private static bool IsDarkMode
	{
		get
		{
			var settings = Settings.Default;
			return settings?.ApplicationPreferDarkTheme == true ||
			       (settings?.ThemeName?.Contains("dark", StringComparison.OrdinalIgnoreCase) ?? false);
		}
	}

	private static SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;

	private readonly SKDrawingArea skiaView;
	private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
	private SKPath currentPath;
	private SKColor currentColor;
	private float brushSize = 4f;
	private SKPoint cursorPosition;
	private Scale brushScale;
	private Label brushSizeLabel;
	private bool isCursorOver;

	public DrawingPage()
		: base(Orientation.Vertical, 0)
	{
		currentColor = IsDarkMode ? SKColors.White : SKColors.Black;

		// Load layout from Glade — provides the overlay, toolbox, and clear button
		var builder = MainWindow.LoadBuilder("SkiaSharpSample.DrawingPage.glade");
		var overlay = (Overlay)builder.GetObject("drawingOverlay");
		var canvas = (Box)builder.GetObject("drawingCanvas");
		var toolbox = (Box)builder.GetObject("drawingToolbox");
		var clearBtn = (Button)builder.GetObject("btnClear");

		// Insert SKDrawingArea into the canvas placeholder
		skiaView = new SKDrawingArea();
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.AddEvents(
			(int)(EventMask.ButtonPressMask |
			      EventMask.ButtonReleaseMask |
			      EventMask.PointerMotionMask |
			      EventMask.ScrollMask |
			      EventMask.EnterNotifyMask |
			      EventMask.LeaveNotifyMask));
		skiaView.ButtonPressEvent += OnButtonPress;
		skiaView.ButtonReleaseEvent += OnButtonRelease;
		skiaView.MotionNotifyEvent += OnMotionNotify;
		skiaView.ScrollEvent += OnScroll;
		skiaView.EnterNotifyEvent += (s, e) => { isCursorOver = true; };
		skiaView.LeaveNotifyEvent += (s, e) => { isCursorOver = false; skiaView.QueueDraw(); };
		canvas.PackStart(skiaView, true, true, 0);

		// Style the clear button
		clearBtn.Clicked += OnClearClicked;
		var clearCss = new CssProvider();
		clearCss.LoadFromData(
			"button { background-color: rgba(30,30,30,0.6); border-radius: 18px; padding: 6px 16px; color: white; border: none; }");
		clearBtn.StyleContext.AddProvider(clearCss, StyleProviderPriority.Application);

		// Style the toolbox container
		var toolboxCss = new CssProvider();
		toolboxCss.LoadFromData(
			"box { background-color: rgba(30,30,30,0.8); border-radius: 24px; padding: 12px 20px; }");
		toolbox.StyleContext.AddProvider(toolboxCss, StyleProviderPriority.Application);

		// Add color swatches to the toolbox
		foreach (var (name, light, dark) in ColorOptions)
		{
			var btn = new Button { Relief = ReliefStyle.None };
			var css = new CssProvider();
			css.LoadFromData(
				$"button {{ background: rgb({light.Red},{light.Green},{light.Blue}); min-width: 28px; min-height: 28px; padding: 0; border-radius: 14px; border: 2px solid rgba(0,0,0,0.2); }}");
			btn.StyleContext.AddProvider(css, StyleProviderPriority.Application);
			var capturedLight = light;
			var capturedDark = dark;
			btn.Clicked += (s, e) => currentColor = IsDarkMode ? capturedDark : capturedLight;
			toolbox.PackStart(btn, false, false, 0);
		}

		// Brush size slider
		var adjustment = new Adjustment(brushSize, 1, 50, 1, 5, 0);
		brushScale = new Scale(Orientation.Horizontal, adjustment) { WidthRequest = 120, DrawValue = false };
		var scaleCss = new CssProvider();
		scaleCss.LoadFromData(
			"scale { min-height: 20px; } " +
			"scale trough { background: rgba(255,255,255,0.3); border-radius: 4px; min-height: 4px; } " +
			"scale slider { background: white; border-radius: 8px; min-width: 16px; min-height: 16px; }");
		brushScale.StyleContext.AddProvider(scaleCss, StyleProviderPriority.Application);
		brushScale.ValueChanged += (s, e) =>
		{
			brushSize = (float)brushScale.Value;
			brushSizeLabel.Text = $"{brushSize:0}px";
			skiaView?.QueueDraw();
		};
		toolbox.PackStart(brushScale, false, false, 0);

		// Brush size label
		brushSizeLabel = new Label($"{brushSize:0}px");
		var labelCss = new CssProvider();
		labelCss.LoadFromData("label { color: white; font-size: 11px; }");
		brushSizeLabel.StyleContext.AddProvider(labelCss, StyleProviderPriority.Application);
		toolbox.PackStart(brushSizeLabel, false, false, 0);

		PackStart(overlay, true, true, 0);
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

		float sx = (float)e.Info.Width / skiaView.AllocatedWidth;
		float sy = (float)e.Info.Height / skiaView.AllocatedHeight;
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

	[GLib.ConnectBefore]
	private void OnButtonPress(object sender, ButtonPressEventArgs e)
	{
		if (e.Event.Button != 1)
			return;
		currentPath = new SKPath();
		currentPath.MoveTo((float)e.Event.X, (float)e.Event.Y);
		cursorPosition = new SKPoint((float)e.Event.X, (float)e.Event.Y);
		skiaView.QueueDraw();
	}

	[GLib.ConnectBefore]
	private void OnButtonRelease(object sender, ButtonReleaseEventArgs e)
	{
		if (currentPath != null)
		{
			strokes.Add((currentPath, currentColor, brushSize));
			currentPath = null;
			skiaView.QueueDraw();
		}
	}

	[GLib.ConnectBefore]
	private void OnMotionNotify(object sender, MotionNotifyEventArgs e)
	{
		cursorPosition = new SKPoint((float)e.Event.X, (float)e.Event.Y);
		currentPath?.LineTo((float)e.Event.X, (float)e.Event.Y);
		skiaView.QueueDraw();
	}

	[GLib.ConnectBefore]
	private void OnScroll(object sender, ScrollEventArgs e)
	{
		bool scrollUp = e.Event.Direction == ScrollDirection.Smooth
			? e.Event.DeltaY < 0
			: e.Event.Direction == ScrollDirection.Up;

		brushSize = scrollUp
			? Math.Min(brushSize + 1, 50)
			: Math.Max(brushSize - 1, 1);

		brushScale.Value = brushSize;
		skiaView?.QueueDraw();
	}

	private void OnClearClicked(object sender, EventArgs e)
	{
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		currentPath?.Dispose();
		currentPath = null;
		skiaView.QueueDraw();
	}
}
