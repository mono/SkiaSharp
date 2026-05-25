using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample;

public class DrawingPage : Box
{
	private static readonly (string Name, SKColor Light, SKColor Dark)[] ColorOptions = new[]
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
			var settings = Gtk.Settings.GetDefault();
			if (settings == null)
				return false;
			if (settings.GtkApplicationPreferDarkTheme)
				return true;
			var themeName = settings.GtkThemeName;
			return themeName?.Contains("dark", StringComparison.OrdinalIgnoreCase) ?? false;
		}
	}

	private static SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;

	private SKDrawingArea drawingSkiaView;
	private Scale brushScale;
	private Label brushSizeLabel;
	private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
	private SKPath currentPath;
	private SKColor currentColor;
	private float brushSize = 4f;
	private SKPoint cursorPosition;
	private bool isCursorOver;
	private double dragStartX, dragStartY;

	public DrawingPage()
		: base(new GObject.ConstructArgument[] { })
	{
		Hexpand = true;
		Vexpand = true;

		currentColor = IsDarkMode ? SKColors.White : SKColors.Black;

		var builder = MainWindow.LoadBuilder("DrawingPage.ui");
		var overlay = (Overlay)builder.GetObject("drawingOverlay");

		var drawingContainer = (Box)builder.GetObject("drawingContainer");
		drawingSkiaView = new SKDrawingArea();
		drawingSkiaView.Hexpand = true;
		drawingSkiaView.Vexpand = true;
		drawingSkiaView.PaintSurface += OnDrawingPaintSurface;
		drawingContainer.Append(drawingSkiaView);

		SetupGestures();
		SetupToolbox(builder);

		Append(overlay);
	}

	private void SetupToolbox(Builder builder)
	{
		var drawingToolbox = (Box)builder.GetObject("drawingToolbox");

		// Create circular color swatch buttons
		foreach (var (name, light, dark) in ColorOptions)
		{
			var btn = Button.New();
			var provider = new CssProvider();
			provider.LoadFromData(
				$"button {{ background: rgb({light.Red},{light.Green},{light.Blue}); min-width: 28px; min-height: 28px; padding: 0; border-radius: 14px; border: 2px solid rgba(0,0,0,0.2); }}",
				-1);
			btn.GetStyleContext().AddProvider(provider, 600);
			var capturedLight = light;
			var capturedDark = dark;
			btn.OnClicked += (sender, args) => currentColor = IsDarkMode ? capturedDark : capturedLight;
			drawingToolbox.Append(btn);
		}

		// Brush size slider
		brushScale = Scale.NewWithRange(Orientation.Horizontal, 1, 50, 1);
		brushScale.SetValue(brushSize);
		brushScale.DrawValue = false;
		brushScale.SetSizeRequest(120, -1);
		var scaleProvider = new CssProvider();
		scaleProvider.LoadFromData(
			"scale { min-height: 20px; } " +
			"scale trough { background: rgba(255,255,255,0.3); border-radius: 4px; min-height: 4px; } " +
			"scale slider { background: white; border-radius: 8px; min-width: 16px; min-height: 16px; }",
			-1);
		brushScale.GetStyleContext().AddProvider(scaleProvider, 600);
		var adj = brushScale.GetAdjustment();
		adj.OnValueChanged += (s, a) =>
		{
			brushSize = (float)brushScale.GetValue();
			brushSizeLabel.SetLabel($"{brushSize:0}px");
			drawingSkiaView.QueueDraw();
		};
		drawingToolbox.Append(brushScale);

		// Brush size label
		brushSizeLabel = Label.New($"{brushSize:0}px");
		var labelProvider = new CssProvider();
		labelProvider.LoadFromData("label { color: white; font-size: 11px; }", -1);
		brushSizeLabel.GetStyleContext().AddProvider(labelProvider, 600);
		drawingToolbox.Append(brushSizeLabel);

		// Floating clear button (top-right overlay)
		var clearBtn = (Button)builder.GetObject("btnClear");
		clearBtn.OnClicked += OnClearClicked;
		var clearCss = new CssProvider();
		clearCss.LoadFromData(
			"button { background-color: rgba(30, 30, 30, 0.6); border-radius: 18px; padding: 6px 16px; color: white; border: none; }",
			-1);
		clearBtn.GetStyleContext().AddProvider(clearCss, 600);

		// Translucent dark background for the floating toolbox
		var toolboxCss = new CssProvider();
		toolboxCss.LoadFromData(
			"box { background-color: rgba(30, 30, 30, 0.8); border-radius: 24px; padding: 12px 20px; }",
			-1);
		drawingToolbox.GetStyleContext().AddProvider(toolboxCss, 600);
	}

	private void SetupGestures()
	{
		var dragGesture = GestureDrag.New();
		dragGesture.OnDragBegin += OnDragBegin;
		dragGesture.OnDragUpdate += OnDragUpdate;
		dragGesture.OnDragEnd += OnDragEnd;
		drawingSkiaView.AddController(dragGesture);

		var motionController = EventControllerMotion.New();
		motionController.OnEnter += (sender, args) =>
		{
			isCursorOver = true;
			cursorPosition = new SKPoint((float)args.X, (float)args.Y);
		};
		motionController.OnLeave += (sender, args) =>
		{
			isCursorOver = false;
			drawingSkiaView.QueueDraw();
		};
		motionController.OnMotion += (sender, args) =>
		{
			cursorPosition = new SKPoint((float)args.X, (float)args.Y);
			if (currentPath == null)
				drawingSkiaView.QueueDraw();
		};
		drawingSkiaView.AddController(motionController);

		var scrollController = EventControllerScroll.New(EventControllerScrollFlags.Vertical);
		scrollController.OnScroll += (sender, args) =>
		{
			var newSize = Math.Max(1f, Math.Min(50f, brushSize + (args.Dy < 0 ? 1f : -1f)));
			brushScale.SetValue(newSize);
			return false;
		};
		drawingSkiaView.AddController(scrollController);
	}

	private void OnDrawingPaintSurface(object sender, SKPaintSurfaceEventArgs e)
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

		float sx = (float)e.Info.Width / drawingSkiaView.GetAllocatedWidth();
		float sy = (float)e.Info.Height / drawingSkiaView.GetAllocatedHeight();
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

	private void OnDragBegin(GestureDrag sender, GestureDrag.DragBeginSignalArgs args)
	{
		dragStartX = args.StartX;
		dragStartY = args.StartY;
		currentPath = new SKPath();
		currentPath.MoveTo((float)dragStartX, (float)dragStartY);
		cursorPosition = new SKPoint((float)dragStartX, (float)dragStartY);
		drawingSkiaView.QueueDraw();
	}

	private void OnDragUpdate(GestureDrag sender, GestureDrag.DragUpdateSignalArgs args)
	{
		var x = dragStartX + args.OffsetX;
		var y = dragStartY + args.OffsetY;
		cursorPosition = new SKPoint((float)x, (float)y);
		currentPath?.LineTo((float)x, (float)y);
		drawingSkiaView.QueueDraw();
	}

	private void OnDragEnd(GestureDrag sender, GestureDrag.DragEndSignalArgs args)
	{
		if (currentPath != null)
		{
			strokes.Add((currentPath, currentColor, brushSize));
			currentPath = null;
			drawingSkiaView.QueueDraw();
		}
	}

	private void OnClearClicked(Button sender, EventArgs args)
	{
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		currentPath?.Dispose();
		currentPath = null;
		drawingSkiaView.QueueDraw();
	}
}
