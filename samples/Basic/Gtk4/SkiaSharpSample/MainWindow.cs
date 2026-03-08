using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample
{
	public class MainWindow : ApplicationWindow
	{
		public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

		private static readonly SKColor[] gradientColors =
		{
			new SKColor(0x44, 0x88, 0xFF),
			new SKColor(0x88, 0x33, 0xCC),
		};

		private static readonly (float X, float Y, float R, SKColor Color)[] circles =
		{
			(0.2f, 0.3f, 0.10f, new SKColor(0xFF, 0x4D, 0x66, 0xCC)),
			(0.75f, 0.25f, 0.08f, new SKColor(0x4D, 0xB3, 0xFF, 0xCC)),
			(0.15f, 0.7f, 0.07f, new SKColor(0xFF, 0x99, 0x1A, 0xCC)),
			(0.8f, 0.7f, 0.12f, new SKColor(0x66, 0xFF, 0xB3, 0xCC)),
			(0.5f, 0.15f, 0.06f, new SKColor(0xB3, 0x4D, 0xFF, 0xCC)),
			(0.4f, 0.8f, 0.09f, new SKColor(0xFF, 0xE6, 0x33, 0xCC)),
		};

		private static readonly (string ButtonId, string Name, SKColor Light, SKColor Dark)[] ColorOptions = new[]
		{
			("btnBlack", "Black", SKColors.Black, SKColors.White),
			("btnRed", "Red", new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),
			("btnBlue", "Blue", new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),
			("btnGreen", "Green", new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),
			("btnOrange", "Orange", new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),
			("btnPurple", "Purple", new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),
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

		// Drawing page state
		private SKDrawingArea drawingSkiaView;
		private Label brushSizeLabel;
		private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
		private SKPath currentPath;
		private SKColor currentColor;
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
			currentColor = IsDarkMode ? SKColors.White : SKColors.Black;

			// Load layout from the .ui file (editable in GNOME Builder / Cambalache)
			var uiPath = Path.Combine(AppContext.BaseDirectory, "MainWindow.ui");
			var builder = Builder.NewFromFile(uiPath);

			var rootBox = (Box)builder.GetObject("rootBox");
			Child = rootBox;

			// CPU page — inject SKDrawingArea into the placeholder container
			var cpuContainer = (Box)builder.GetObject("cpuContainer");
			var cpuSkiaView = new SKDrawingArea();
			cpuSkiaView.Hexpand = true;
			cpuSkiaView.Vexpand = true;
			cpuSkiaView.PaintSurface += OnCpuPaintSurface;
			cpuContainer.Append(cpuSkiaView);

			// Drawing page — inject SKDrawingArea into the placeholder container
			var drawingContainer = (Box)builder.GetObject("drawingContainer");
			drawingSkiaView = new SKDrawingArea();
			drawingSkiaView.Hexpand = true;
			drawingSkiaView.Vexpand = true;
			drawingSkiaView.PaintSurface += OnDrawingPaintSurface;
			drawingContainer.Append(drawingSkiaView);

			SetupDrawingGestures();
			SetupColorButtons(builder);

			// Clear button
			var clearBtn = (Button)builder.GetObject("btnClear");
			var clearProvider = new CssProvider();
			clearProvider.LoadFromData(
				"button { background: rgb(120,120,120); color: white; font-weight: bold; font-size: 9pt; min-width: 70px; border: none; }",
				-1);
			clearBtn.GetStyleContext().AddProvider(clearProvider, 600);
			clearBtn.OnClicked += OnClearClicked;

			// Brush size label
			brushSizeLabel = (Label)builder.GetObject("brushSizeLabel");

			var contentStack = (Stack)builder.GetObject("contentStack");
			if (DefaultPage == SamplePage.Drawing)
				contentStack.SetVisibleChildName("drawing");
		}

		private void SetupColorButtons(Builder builder)
		{
			foreach (var (buttonId, name, light, dark) in ColorOptions)
			{
				var btn = (Button)builder.GetObject(buttonId);
				var provider = new CssProvider();
				provider.LoadFromData(
					$"button {{ background: rgb({light.Red},{light.Green},{light.Blue}); color: white; font-weight: bold; font-size: 9pt; min-width: 70px; border: none; }}",
					-1);
				btn.GetStyleContext().AddProvider(provider, 600);
				var capturedLight = light;
				var capturedDark = dark;
				btn.OnClicked += (sender, args) => currentColor = IsDarkMode ? capturedDark : capturedLight;
			}
		}

		private void SetupDrawingGestures()
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
				brushSize = Math.Max(1f, Math.Min(50f, brushSize + (args.Dy < 0 ? 1f : -1f)));
				brushSizeLabel.SetLabel($"Brush: {brushSize:0}px");
				drawingSkiaView.QueueDraw();
				return false;
			};
			drawingSkiaView.AddController(scrollController);
		}

		// --- CPU Page Painting ---

		private void OnCpuPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var width = e.Info.Width;
			var height = e.Info.Height;
			var center = new SKPoint(width / 2f, height / 2f);
			var radius = Math.Max(width, height) / 2f;

			canvas.Clear(SKColors.White);

			using var bgShader = SKShader.CreateRadialGradient(
				center, radius, gradientColors, SKShaderTileMode.Clamp);
			using var bgPaint = new SKPaint { IsAntialias = true, Shader = bgShader };
			canvas.DrawRect(0, 0, width, height, bgPaint);

			using var circlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
			foreach (var (xf, yf, rf, color) in circles)
			{
				circlePaint.Color = color;
				canvas.DrawCircle(xf * width, yf * height, rf * Math.Min(width, height), circlePaint);
			}

			using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
			using var font = new SKFont { Size = width * 0.12f };
			canvas.DrawText("SkiaSharp", center.X, center.Y + font.Size / 3f, SKTextAlign.Center, font, textPaint);
		}

		// --- Drawing Page Painting ---

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
}
