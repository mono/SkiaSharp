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
	private static readonly (string Name, SKColor Color)[] ColorOptions = new[]
	{
		("Black", SKColors.Black),
		("Red", new SKColor(0xE5, 0x39, 0x35)),
		("Blue", new SKColor(0x1E, 0x88, 0xE5)),
		("Green", new SKColor(0x43, 0xA0, 0x47)),
		("Orange", new SKColor(0xFB, 0x8C, 0x00)),
		("Purple", new SKColor(0x8E, 0x24, 0xAA)),
	};

	private SKDrawingArea drawingSkiaView;
	private Scale brushScale;
	private Label brushSizeLabel;
	private Box drawingToolbox;
	private readonly List<(Button Button, CssProvider Style)> swatches = new();
	private int selectedColorIndex;
	private uint layoutCallback;
	private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
	private SKPathBuilder? currentBuilder;
	private SKColor currentColor = SKColors.Black;
	private float brushSize = 4f;
	private SKPoint cursorPosition;
	private bool isCursorOver;
	private double dragStartX, dragStartY;

	public DrawingPage()
		: base(new GObject.ConstructArgument[] { })
	{
		Hexpand = true;
		Vexpand = true;

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
		OnMap += OnMapped;
		OnUnmap += OnUnmapped;

		Append(overlay);
	}

	private void SetupToolbox(Builder builder)
	{
		drawingToolbox = (Box)builder.GetObject("drawingToolbox");
		drawingToolbox.AddCssClass("drawing-toolbox");
		var swatchBox = Box.New(Orientation.Horizontal, 8);
		drawingToolbox.Append(swatchBox);

		// Create circular color swatch buttons
		for (var i = 0; i < ColorOptions.Length; i++)
		{
			var btn = Button.New();
			btn.Valign = Align.Center;
			btn.SetTooltipText(ColorOptions[i].Name);
			var provider = new CssProvider();
			btn.GetStyleContext().AddProvider(provider, 600);
			swatches.Add((btn, provider));
			var index = i;
			btn.OnClicked += (sender, args) =>
			{
				selectedColorIndex = index;
				UpdatePalette();
			};
			swatchBox.Append(btn);
		}
		UpdatePalette();

		var brushControls = Box.New(Orientation.Horizontal, 8);
		brushControls.Halign = Align.Center;
		drawingToolbox.Append(brushControls);

		// Brush size slider
		brushScale = Scale.NewWithRange(Orientation.Horizontal, 1, 50, 1);
		brushScale.SetValue(brushSize);
		brushScale.DrawValue = false;
		brushScale.SetSizeRequest(140, -1);
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
			brushSizeLabel.SetLabel($"{brushSize:0}");
			drawingSkiaView.QueueDraw();
		};
		brushControls.Append(brushScale);

		// Brush size label
		brushSizeLabel = Label.New($"{brushSize:0}");
		brushSizeLabel.SetSizeRequest(24, -1);
		var labelProvider = new CssProvider();
		labelProvider.LoadFromData("label { color: white; font-size: 13px; }", -1);
		brushSizeLabel.GetStyleContext().AddProvider(labelProvider, 600);
		brushControls.Append(brushSizeLabel);

		// Floating clear button (top-right overlay)
		var clearBtn = (Button)builder.GetObject("btnClear");
		clearBtn.OnClicked += OnClearClicked;
		var clearCss = new CssProvider();
		clearCss.LoadFromData(
			"button { background: rgba(30, 30, 30, 0.8); border-radius: 16px; padding: 6px 16px; color: white; border: none; font-size: 13px; }",
			-1);
		clearBtn.GetStyleContext().AddProvider(clearCss, 600);

		// Translucent dark background for the floating toolbox
		var toolboxCss = new CssProvider();
		toolboxCss.LoadFromData(
			"box.drawing-toolbox { background-color: rgba(30, 30, 30, 0.8); border: 1px solid rgba(255,255,255,0.267); border-radius: 24px; padding: 12px 16px; }",
			-1);
		drawingToolbox.GetStyleContext().AddProvider(toolboxCss, 600);
	}

	private void UpdatePalette()
	{
		currentColor = ColorOptions[selectedColorIndex].Color;
		for (var i = 0; i < swatches.Count; i++)
		{
			var color = ColorOptions[i].Color;
			var border = i == selectedColorIndex ? "dodgerblue" : "transparent";
			swatches[i].Style.LoadFromData(
				$"button {{ background: rgb({color.Red},{color.Green},{color.Blue}); min-width: 30px; min-height: 30px; padding: 0; border-radius: 18px; border: 3px solid {border}; }}",
				-1);
		}
		drawingSkiaView.QueueDraw();
	}

	private void OnMapped(Widget sender, EventArgs args)
	{
		UpdatePalette();
		var lastWidth = -1;
		layoutCallback = AddTickCallback((widget, clock) =>
		{
			var width = GetWidth();
			if (width != lastWidth)
			{
				drawingToolbox.SetOrientation(width < 600 ? Orientation.Vertical : Orientation.Horizontal);
				lastWidth = width;
			}
			return true;
		});
	}

	private void OnUnmapped(Widget sender, EventArgs args)
	{
		if (layoutCallback != 0)
		{
			RemoveTickCallback(layoutCallback);
			layoutCallback = 0;
		}
		currentBuilder?.Dispose();
		currentBuilder = null;
		isCursorOver = false;
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
			if (currentBuilder == null)
				drawingSkiaView.QueueDraw();
		};
		drawingSkiaView.AddController(motionController);

		var scrollController = EventControllerScroll.New(EventControllerScrollFlags.Vertical);
		scrollController.OnScroll += (sender, args) =>
		{
			var newSize = Math.Max(1f, Math.Min(50f, brushSize + (args.Dy < 0 ? 1f : -1f)));
			brushScale.SetValue(newSize);
			return true;
		};
		drawingSkiaView.AddController(scrollController);
	}

	private void OnDrawingPaintSurface(object sender, SKPaintSurfaceEventArgs e)
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

		float sx = (float)e.Info.Width / drawingSkiaView.GetAllocatedWidth();
		float sy = (float)e.Info.Height / drawingSkiaView.GetAllocatedHeight();
		canvas.Scale(sx, sy);

		foreach (var (path, color, strokeWidth) in strokes)
		{
			paint.Color = color;
			paint.StrokeWidth = strokeWidth;
			canvas.DrawPath(path, paint);
		}

		if (currentBuilder != null)
		{
			using var path = currentBuilder.Snapshot();
			paint.Color = currentColor;
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(path, paint);
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
		currentBuilder = new SKPathBuilder();
		currentBuilder.MoveTo((float)dragStartX, (float)dragStartY);
		cursorPosition = new SKPoint((float)dragStartX, (float)dragStartY);
		drawingSkiaView.QueueDraw();
	}

	private void OnDragUpdate(GestureDrag sender, GestureDrag.DragUpdateSignalArgs args)
	{
		var x = dragStartX + args.OffsetX;
		var y = dragStartY + args.OffsetY;
		cursorPosition = new SKPoint((float)x, (float)y);
		currentBuilder?.LineTo((float)x, (float)y);
		drawingSkiaView.QueueDraw();
	}

	private void OnDragEnd(GestureDrag sender, GestureDrag.DragEndSignalArgs args)
	{
		if (currentBuilder != null)
		{
			strokes.Add((currentBuilder.Detach(), currentColor, brushSize));
			currentBuilder.Dispose();
			currentBuilder = null;
			drawingSkiaView.QueueDraw();
		}
	}

	private void OnClearClicked(Button sender, EventArgs args)
	{
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		currentBuilder?.Dispose();
		currentBuilder = null;
		drawingSkiaView.QueueDraw();
	}

	public override void Dispose()
	{
		OnUnmapped(this, EventArgs.Empty);
		OnMap -= OnMapped;
		OnUnmap -= OnUnmapped;
		drawingSkiaView.PaintSurface -= OnDrawingPaintSurface;
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		foreach (var (_, style) in swatches)
			style.Dispose();
		base.Dispose();
	}
}
