using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample
{
	public partial class DrawingPage : UserControl
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

		private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
		private SKPath currentPath;
		private SKColor currentColor;
		private float brushSize = 4f;
		private SKPoint cursorPosition;
		private bool isCursorOver;

		static bool IsDarkMode
		{
			get
			{
				if (!OperatingSystem.IsWindows()) return false;
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

		public DrawingPage()
		{
			InitializeComponent();
			currentColor = IsDarkMode ? SKColors.White : SKColors.Black;

			foreach (var (name, light, dark) in ColorOptions)
			{
				var btn = new Button
				{
					Text = name,
					Width = 70,
					Height = 30,
					FlatStyle = FlatStyle.Flat,
					BackColor = Color.FromArgb(light.Red, light.Green, light.Blue),
					ForeColor = Color.White,
					Font = new Font("Segoe UI", 8f, FontStyle.Bold),
					Tag = (light, dark),
				};
				btn.FlatAppearance.BorderSize = 0;
				btn.Click += OnColorClick;
				toolbar.Controls.Add(btn);
			}

			var clearBtn = new Button
			{
				Text = "Clear",
				Width = 70,
				Height = 30,
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(120, 120, 120),
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 8f, FontStyle.Bold),
			};
			clearBtn.FlatAppearance.BorderSize = 0;
			clearBtn.Click += OnClearClick;
			toolbar.Controls.Add(clearBtn);
		}

		private void OnColorClick(object sender, EventArgs e)
		{
			if (sender is Button btn && btn.Tag is (SKColor light, SKColor dark))
				currentColor = IsDarkMode ? dark : light;
		}

		private void OnClearClick(object sender, EventArgs e)
		{
			foreach (var (path, _, _) in strokes)
				path.Dispose();
			strokes.Clear();
			currentPath?.Dispose();
			currentPath = null;
			skiaView.Invalidate();
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

			// Scale factor between control size and surface size
			float sx = (float)e.Info.Width / skiaView.Width;
			float sy = (float)e.Info.Height / skiaView.Height;
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

			// Brush indicator at cursor
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

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;

			currentPath = new SKPath();
			currentPath.MoveTo(e.X, e.Y);
			cursorPosition = new SKPoint(e.X, e.Y);
			skiaView.Invalidate();
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			cursorPosition = new SKPoint(e.X, e.Y);
			currentPath?.LineTo(e.X, e.Y);
			skiaView.Invalidate();
		}

		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			if (currentPath != null)
			{
				strokes.Add((currentPath, currentColor, brushSize));
				currentPath = null;
				skiaView.Invalidate();
			}
		}

		private void OnMouseWheel(object sender, MouseEventArgs e)
		{
			brushSize = Math.Max(1f, Math.Min(50f, brushSize + (e.Delta > 0 ? 1f : -1f)));
			skiaView.Invalidate();
		}

		private void OnMouseEnter(object sender, EventArgs e)
		{
			isCursorOver = true;
		}

		private void OnMouseLeave(object sender, EventArgs e)
		{
			isCursorOver = false;
			skiaView.Invalidate();
		}
	}
}
