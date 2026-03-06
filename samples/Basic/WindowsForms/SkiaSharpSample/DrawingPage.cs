using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample
{
	public class DrawingPage : UserControl
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

		private readonly SKControl skiaView;
		private readonly FlowLayoutPanel toolbar;
		private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
		private SKPath currentPath;
		private SKColor currentColor = SKColors.Black;
		private float brushSize = 4f;
		private SKPoint cursorPosition;
		private bool isCursorOver;

		public DrawingPage()
		{
			toolbar = new FlowLayoutPanel
			{
				Dock = DockStyle.Top,
				Height = 40,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false,
				Padding = new Padding(4, 4, 4, 0),
				BackColor = Color.FromArgb(245, 245, 245),
			};

			foreach (var (name, color) in ColorOptions)
			{
				var btn = new Button
				{
					Text = name,
					Width = 70,
					Height = 30,
					FlatStyle = FlatStyle.Flat,
					BackColor = Color.FromArgb(color.Red, color.Green, color.Blue),
					ForeColor = name == "Black" ? Color.White : Color.White,
					Font = new Font("Segoe UI", 8f, FontStyle.Bold),
					Tag = color,
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

			skiaView = new SKControl { Dock = DockStyle.Fill };
			skiaView.PaintSurface += OnPaintSurface;
			skiaView.MouseDown += OnMouseDown;
			skiaView.MouseMove += OnMouseMove;
			skiaView.MouseUp += OnMouseUp;
			skiaView.MouseWheel += OnMouseWheel;
			skiaView.MouseEnter += (s, e) => { isCursorOver = true; };
			skiaView.MouseLeave += (s, e) => { isCursorOver = false; skiaView.Invalidate(); };

			Controls.Add(skiaView);
			Controls.Add(toolbar);
		}

		private void OnColorClick(object sender, EventArgs e)
		{
			if (sender is Button btn && btn.Tag is SKColor color)
				currentColor = color;
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
			canvas.Clear(SKColors.White);

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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				skiaView.PaintSurface -= OnPaintSurface;
				skiaView.MouseDown -= OnMouseDown;
				skiaView.MouseMove -= OnMouseMove;
				skiaView.MouseUp -= OnMouseUp;
				skiaView.MouseWheel -= OnMouseWheel;
				skiaView.Dispose();
				toolbar.Dispose();
				foreach (var (path, _, _) in strokes)
					path.Dispose();
				currentPath?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
