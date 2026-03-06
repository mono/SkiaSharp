using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample
{
	public partial class DrawingPage : UserControl
	{
		private readonly record struct Stroke(SKPath Path, SKColor Color, float Width);

		private readonly List<Stroke> strokes = new();
		private SKPath? currentPath;
		private SKColor currentColor;
		private float brushSize = 4f;
		private float cursorX;
		private float cursorY;
		private bool isDrawing;
		private double dpiScale = 1.0;

		private static readonly Dictionary<string, (SKColor Light, SKColor Dark)> colorMap = new()
		{
			["Black"] = (SKColors.Black, SKColors.White),
			["Red"] = (new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),
			["Blue"] = (new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),
			["Green"] = (new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),
			["Orange"] = (new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),
			["Purple"] = (new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),
		};

		private Border? selectedColorBorder;

		static bool IsDarkMode
		{
			get
			{
				try
				{
					using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
						@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
					return key?.GetValue("AppsUseLightTheme") is int i && i == 0;
				}
				catch { return false; }
			}
		}

		SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;
		static SKColor ResolveColor((SKColor Light, SKColor Dark) pair) => IsDarkMode ? pair.Dark : pair.Light;

		public DrawingPage()
		{
			InitializeComponent();
			currentColor = IsDarkMode ? SKColors.White : SKColors.Black;

			selectedColorBorder = ColorBlack;

			SkCanvas.MouseDown += OnMouseDown;
			SkCanvas.MouseMove += OnMouseMove;
			SkCanvas.MouseUp += OnMouseUp;
			SkCanvas.MouseWheel += OnMouseWheel;
			SkCanvas.MouseLeave += OnMouseLeave;

			Microsoft.Win32.SystemEvents.UserPreferenceChanged += (s, e) =>
			{
				if (currentColor == SKColors.Black && IsDarkMode)
					currentColor = SKColors.White;
				else if (currentColor == SKColors.White && !IsDarkMode)
					currentColor = SKColors.Black;
				SkCanvas.InvalidateVisual();
			};
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear(CanvasBackground);

			// Capture DPI scale for coordinate conversion
			var source = PresentationSource.FromVisual(this);
			if (source != null)
				dpiScale = source.CompositionTarget.TransformToDevice.M11;

			using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };

			// Draw completed strokes
			foreach (var stroke in strokes)
			{
				paint.Color = stroke.Color;
				paint.StrokeWidth = stroke.Width;
				canvas.DrawPath(stroke.Path, paint);
			}

			// Draw current stroke
			if (currentPath != null)
			{
				paint.Color = currentColor;
				paint.StrokeWidth = brushSize;
				canvas.DrawPath(currentPath, paint);
			}

			// Brush indicator at cursor
			var pixelX = (float)(cursorX * dpiScale);
			var pixelY = (float)(cursorY * dpiScale);
			var pixelRadius = (float)(brushSize / 2.0);
			using var indicatorPaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = new SKColor(0, 0, 0, 128),
				StrokeWidth = 1
			};
			canvas.DrawCircle(pixelX, pixelY, pixelRadius, indicatorPaint);
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed)
				return;

			isDrawing = true;
			var pos = e.GetPosition(SkCanvas);
			cursorX = (float)pos.X;
			cursorY = (float)pos.Y;

			currentPath = new SKPath();
			currentPath.MoveTo((float)(pos.X * dpiScale), (float)(pos.Y * dpiScale));
			SkCanvas.CaptureMouse();
			SkCanvas.InvalidateVisual();
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			var pos = e.GetPosition(SkCanvas);
			cursorX = (float)pos.X;
			cursorY = (float)pos.Y;

			if (isDrawing && currentPath != null)
				currentPath.LineTo((float)(pos.X * dpiScale), (float)(pos.Y * dpiScale));

			SkCanvas.InvalidateVisual();
		}

		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!isDrawing || currentPath == null)
				return;

			isDrawing = false;
			strokes.Add(new Stroke(currentPath, currentColor, brushSize));
			currentPath = null;
			SkCanvas.ReleaseMouseCapture();
			SkCanvas.InvalidateVisual();
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			brushSize = Math.Clamp(brushSize + (e.Delta > 0 ? 1f : -1f), 1f, 50f);
			BrushSizeText.Text = $"Brush: {brushSize:F0}px";
			SkCanvas.InvalidateVisual();
		}

		private void OnMouseLeave(object sender, MouseEventArgs e)
		{
			if (!isDrawing)
				SkCanvas.InvalidateVisual();
		}

		private void OnColorPick(object sender, MouseButtonEventArgs e)
		{
			if (sender is not Border border || border.Tag is not string colorName)
				return;

			if (colorMap.TryGetValue(colorName, out var pair))
			{
				currentColor = ResolveColor(pair);

				// Update selection highlight
				if (selectedColorBorder != null)
					selectedColorBorder.BorderBrush = System.Windows.Media.Brushes.Transparent;
				border.BorderBrush = new System.Windows.Media.SolidColorBrush(
					System.Windows.Media.Color.FromRgb(0x00, 0x7A, 0xCC));
				selectedColorBorder = border;
			}
		}

		private void OnClear(object sender, RoutedEventArgs e)
		{
			foreach (var stroke in strokes)
				stroke.Path.Dispose();
			strokes.Clear();

			currentPath?.Dispose();
			currentPath = null;
			isDrawing = false;

			SkCanvas.InvalidateVisual();
		}
	}
}
