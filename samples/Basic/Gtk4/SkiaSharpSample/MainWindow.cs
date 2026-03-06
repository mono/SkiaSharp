using System;
using System.Collections.Generic;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample
{
	public class MainWindow : ApplicationWindow
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

		// Drawing page state
		private SKDrawingArea drawingSkiaView;
		private Label brushSizeLabel;
		private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
		private SKPath currentPath;
		private SKColor currentColor = SKColors.Black;
		private float brushSize = 4f;
		private SKPoint cursorPosition;
		private bool isCursorOver;
		private double dragStartX, dragStartY;

		public MainWindow(Application app)
			: base(new GObject.ConstructArgument[] { })
		{
			Application = app;
			Title = "SkiaSharp on Gtk4";
			SetDefaultSize(1024, 768);

			var box = Box.New(Orientation.Horizontal, 0);

			var stack = Stack.New();
			stack.TransitionType = StackTransitionType.Crossfade;
			stack.TransitionDuration = 300;
			stack.Hexpand = true;

			var sidebar = StackSidebar.New();
			sidebar.Stack = stack;
			sidebar.WidthRequest = 200;

			// CPU Page
			var cpuPage = CreateCpuPage();
			stack.AddTitled(cpuPage, "cpu", "CPU Canvas");

			// Drawing Page
			var drawingPage = CreateDrawingPage();
			stack.AddTitled(drawingPage, "drawing", "Drawing");

			box.Append(sidebar);
			box.Append(stack);

			Child = box;
		}

		// --- CPU Page ---

		private Widget CreateCpuPage()
		{
			var skiaView = new SKDrawingArea();
			skiaView.PaintSurface += OnCpuPaintSurface;
			return skiaView;
		}

		private void OnCpuPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var width = e.Info.Width;
			var height = e.Info.Height;
			var center = new SKPoint(width / 2f, height / 2f);
			var radius = Math.Max(width, height) / 2f;

			canvas.Clear(SKColors.White);

			using var bgShader = SKShader.CreateRadialGradient(
				center, radius,
				new[] { new SKColor(0x44, 0x88, 0xFF), new SKColor(0x88, 0x33, 0xCC) },
				SKShaderTileMode.Clamp);
			using var bgPaint = new SKPaint { IsAntialias = true, Shader = bgShader };
			canvas.DrawRect(0, 0, width, height, bgPaint);

			var circles = new[]
			{
				(0.2f, 0.3f, 0.10f, new SKColor(0xFF, 0x4D, 0x66, 0xCC)),
				(0.75f, 0.25f, 0.08f, new SKColor(0x4D, 0xB3, 0xFF, 0xCC)),
				(0.15f, 0.7f, 0.07f, new SKColor(0xFF, 0x99, 0x1A, 0xCC)),
				(0.8f, 0.7f, 0.12f, new SKColor(0x66, 0xFF, 0xB3, 0xCC)),
				(0.5f, 0.15f, 0.06f, new SKColor(0xB3, 0x4D, 0xFF, 0xCC)),
				(0.4f, 0.8f, 0.09f, new SKColor(0xFF, 0xE6, 0x33, 0xCC)),
			};
			using var circlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
			foreach (var (xf, yf, rf, color) in circles)
			{
				circlePaint.Color = color;
				canvas.DrawCircle(xf * width, yf * height, rf * Math.Min(width, height), circlePaint);
			}

			using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
			using var font = new SKFont { Size = 48 };
			canvas.DrawText("SkiaSharp", center.X, center.Y + font.Size / 3f, SKTextAlign.Center, font, textPaint);
		}

		// --- Drawing Page ---

		private Widget CreateDrawingPage()
		{
			var vbox = Box.New(Orientation.Vertical, 0);

			drawingSkiaView = new SKDrawingArea();
			drawingSkiaView.Vexpand = true;
			drawingSkiaView.PaintSurface += OnDrawingPaintSurface;

			// GTK4 gesture controllers
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
				brushSize = Math.Max(1f, Math.Min(50f, brushSize + (args.Dy < 0 ? 1f : -1f)));
				brushSizeLabel.SetLabel($"Brush: {brushSize:0}px");
				drawingSkiaView.QueueDraw();
			};
			drawingSkiaView.AddController(scrollController);

			vbox.Append(drawingSkiaView);

			// Toolbar
			var toolbar = Box.New(Orientation.Horizontal, 8);
			toolbar.MarginStart = 4;
			toolbar.MarginEnd = 4;
			toolbar.MarginTop = 4;
			toolbar.MarginBottom = 4;

			foreach (var (name, color) in ColorOptions)
			{
				var btn = Button.NewWithLabel(name);
				var provider = new CssProvider();
				provider.LoadFromData(
					$"button {{ background: rgb({color.Red},{color.Green},{color.Blue}); color: white; font-weight: bold; font-size: 9pt; min-width: 70px; border: none; }}",
					-1);
				btn.GetStyleContext().AddProvider(provider, 600);
				var capturedColor = color;
				btn.OnClicked += (sender, args) => currentColor = capturedColor;
				toolbar.Append(btn);
			}

			var clearBtn = Button.NewWithLabel("Clear");
			var clearProvider = new CssProvider();
			clearProvider.LoadFromData(
				"button { background: rgb(120,120,120); color: white; font-weight: bold; font-size: 9pt; min-width: 70px; border: none; }",
				-1);
			clearBtn.GetStyleContext().AddProvider(clearProvider, 600);
			clearBtn.OnClicked += OnClearClicked;
			toolbar.Append(clearBtn);

			brushSizeLabel = Label.New($"Brush: {brushSize:0}px");
			brushSizeLabel.MarginStart = 12;
			toolbar.Append(brushSizeLabel);

			vbox.Append(toolbar);

			return vbox;
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
}
