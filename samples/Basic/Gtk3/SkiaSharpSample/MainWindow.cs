using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gdk;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample
{
	public class MainWindow : Gtk.Window
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
				var settings = Gtk.Settings.Default;
				return settings?.ApplicationPreferDarkTheme == true ||
				       (settings?.ThemeName?.Contains("dark", StringComparison.OrdinalIgnoreCase) ?? false);
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

		public MainWindow()
			: base("SkiaSharp on Gtk3")
		{
			currentColor = IsDarkMode ? SKColors.White : SKColors.Black;
			SetDefaultSize(1024, 768);
			DeleteEvent += (s, e) => Application.Quit();

			// Load layout from the embedded Glade resource
			var builder = new Builder();
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream("SkiaSharpSample.MainWindow.glade"))
			using (var reader = new StreamReader(stream))
			{
				builder.AddFromString(reader.ReadToEnd());
			}

			var rootBox = (Box)builder.GetObject("rootBox");

			// CPU Page: insert SKDrawingArea into the placeholder container
			var cpuPage = (Box)builder.GetObject("cpuPage");
			var cpuSkiaView = new SKDrawingArea();
			cpuSkiaView.PaintSurface += OnCpuPaintSurface;
			cpuPage.PackStart(cpuSkiaView, true, true, 0);

			// Drawing Page: insert SKDrawingArea into the placeholder container
			var drawingCanvasContainer = (Box)builder.GetObject("drawingCanvasContainer");
			drawingSkiaView = new SKDrawingArea();
			drawingSkiaView.PaintSurface += OnDrawingPaintSurface;
			drawingSkiaView.AddEvents(
				(int)(EventMask.ButtonPressMask |
				      EventMask.ButtonReleaseMask |
				      EventMask.PointerMotionMask |
				      EventMask.ScrollMask |
				      EventMask.EnterNotifyMask |
				      EventMask.LeaveNotifyMask));
			drawingSkiaView.ButtonPressEvent += OnDrawingButtonPress;
			drawingSkiaView.ButtonReleaseEvent += OnDrawingButtonRelease;
			drawingSkiaView.MotionNotifyEvent += OnDrawingMotionNotify;
			drawingSkiaView.ScrollEvent += OnDrawingScroll;
			drawingSkiaView.EnterNotifyEvent += (s, e) => { isCursorOver = true; };
			drawingSkiaView.LeaveNotifyEvent += (s, e) => { isCursorOver = false; drawingSkiaView.QueueDraw(); };
			drawingCanvasContainer.PackStart(drawingSkiaView, true, true, 0);

			// Connect color buttons defined in the Glade file
			foreach (var (name, light, dark) in ColorOptions)
			{
				var btn = (Button)builder.GetObject($"btn{name}");
				var provider = new CssProvider();
				provider.LoadFromData(
					$"button {{ background: rgb({light.Red},{light.Green},{light.Blue}); color: white; font-weight: bold; font-size: 9pt; min-width: 70px; border: none; }}");
				btn.StyleContext.AddProvider(provider, StyleProviderPriority.Application);
				var capturedLight = light;
				var capturedDark = dark;
				btn.Clicked += (s, e) => currentColor = IsDarkMode ? capturedDark : capturedLight;
			}

			// Connect clear button
			var clearBtn = (Button)builder.GetObject("btnClear");
			var clearProvider = new CssProvider();
			clearProvider.LoadFromData(
				"button { background: rgb(120,120,120); color: white; font-weight: bold; font-size: 9pt; min-width: 70px; border: none; }");
			clearBtn.StyleContext.AddProvider(clearProvider, StyleProviderPriority.Application);
			clearBtn.Clicked += OnClearClicked;

			// Brush size label
			brushSizeLabel = (Label)builder.GetObject("brushSizeLabel");

			Add(rootBox);
			ShowAll();
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

			float sx = (float)e.Info.Width / drawingSkiaView.AllocatedWidth;
			float sy = (float)e.Info.Height / drawingSkiaView.AllocatedHeight;
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
		private void OnDrawingButtonPress(object sender, ButtonPressEventArgs e)
		{
			if (e.Event.Button != 1)
				return;

			currentPath = new SKPath();
			currentPath.MoveTo((float)e.Event.X, (float)e.Event.Y);
			cursorPosition = new SKPoint((float)e.Event.X, (float)e.Event.Y);
			drawingSkiaView.QueueDraw();
		}

		[GLib.ConnectBefore]
		private void OnDrawingButtonRelease(object sender, ButtonReleaseEventArgs e)
		{
			if (currentPath != null)
			{
				strokes.Add((currentPath, currentColor, brushSize));
				currentPath = null;
				drawingSkiaView.QueueDraw();
			}
		}

		[GLib.ConnectBefore]
		private void OnDrawingMotionNotify(object sender, MotionNotifyEventArgs e)
		{
			cursorPosition = new SKPoint((float)e.Event.X, (float)e.Event.Y);
			currentPath?.LineTo((float)e.Event.X, (float)e.Event.Y);
			drawingSkiaView.QueueDraw();
		}

		[GLib.ConnectBefore]
		private void OnDrawingScroll(object sender, ScrollEventArgs e)
		{
			bool scrollUp;
			if (e.Event.Direction == Gdk.ScrollDirection.Smooth)
				scrollUp = e.Event.DeltaY < 0;
			else
				scrollUp = e.Event.Direction == Gdk.ScrollDirection.Up;

			if (scrollUp)
				brushSize = Math.Min(brushSize + 1, 50);
			else
				brushSize = Math.Max(brushSize - 1, 1);

			brushSizeLabel.Text = $"Brush: {brushSize:0}px";
			drawingSkiaView?.QueueDraw();
		}

		private void OnClearClicked(object sender, EventArgs e)
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
