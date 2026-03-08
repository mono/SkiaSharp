using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

		private Panel toolbox;
		private Panel clearPanel;
		private Label sizeLabel;
		private TrackBar brushSlider;
		private readonly List<Button> colorButtons = new();

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

			CreateFloatingToolbox();
			CreateFloatingClearButton();

			Resize += (s, e) => PositionOverlays();
			Load += (s, e) => PositionOverlays();
		}

		private void CreateFloatingToolbox()
		{
			toolbox = new Panel
			{
				BackColor = Color.FromArgb(200, 30, 30, 30),
				Height = 56,
			};
			toolbox.Paint += OnPaintRoundedPanel;

			int x = 14;
			int y = 12;

			foreach (var (name, light, dark) in ColorOptions)
			{
				var displayColor = IsDarkMode ? dark : light;
				var btn = new Button
				{
					Size = new Size(32, 32),
					Location = new Point(x, y),
					FlatStyle = FlatStyle.Flat,
					BackColor = Color.FromArgb(displayColor.Red, displayColor.Green, displayColor.Blue),
					Tag = (light, dark),
					Cursor = Cursors.Hand,
				};
				btn.FlatAppearance.BorderSize = 0;
				btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
				btn.FlatAppearance.MouseDownBackColor = btn.BackColor;

				var circlePath = new GraphicsPath();
				circlePath.AddEllipse(0, 0, 32, 32);
				btn.Region = new Region(circlePath);
				circlePath.Dispose();

				btn.Click += OnColorClick;
				toolbox.Controls.Add(btn);
				colorButtons.Add(btn);

				x += 38;
			}

			x += 12;

			brushSlider = new TrackBar
			{
				Minimum = 1,
				Maximum = 50,
				Value = (int)brushSize,
				TickStyle = TickStyle.None,
				Location = new Point(x, y + 2),
				Size = new Size(140, 28),
				BackColor = Color.FromArgb(50, 50, 50),
			};
			brushSlider.ValueChanged += OnBrushSliderChanged;
			toolbox.Controls.Add(brushSlider);

			x += 148;

			sizeLabel = new Label
			{
				Text = $"{brushSize:F0}px",
				ForeColor = Color.White,
				BackColor = Color.Transparent,
				Font = new Font("Segoe UI", 9f, FontStyle.Bold),
				Location = new Point(x, y + 5),
				AutoSize = true,
			};
			toolbox.Controls.Add(sizeLabel);

			toolbox.Width = x + 48;

			Controls.Add(toolbox);
			toolbox.BringToFront();
			UpdateColorSelection();
		}

		private void CreateFloatingClearButton()
		{
			clearPanel = new Panel
			{
				BackColor = Color.FromArgb(200, 50, 50, 50),
				Size = new Size(72, 36),
			};
			clearPanel.Paint += OnPaintRoundedPanel;
			UpdatePanelRegion(clearPanel, 18);

			var clearBtn = new Button
			{
				Text = "Clear",
				Dock = DockStyle.Fill,
				FlatStyle = FlatStyle.Flat,
				ForeColor = Color.White,
				BackColor = Color.Transparent,
				Font = new Font("Segoe UI", 9f, FontStyle.Bold),
				Cursor = Cursors.Hand,
			};
			clearBtn.FlatAppearance.BorderSize = 0;
			clearBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
			clearBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 100, 100);
			clearBtn.Click += OnClearClick;
			clearPanel.Controls.Add(clearBtn);

			Controls.Add(clearPanel);
			clearPanel.BringToFront();
		}

		private void PositionOverlays()
		{
			if (toolbox != null)
			{
				toolbox.Left = (Width - toolbox.Width) / 2;
				toolbox.Top = Height - toolbox.Height - 16;
				UpdatePanelRegion(toolbox, 16);
			}

			if (clearPanel != null)
			{
				clearPanel.Left = Width - clearPanel.Width - 16;
				clearPanel.Top = 16;
				UpdatePanelRegion(clearPanel, 18);
			}
		}

		private static void UpdatePanelRegion(Panel panel, int radius)
		{
			if (panel.Width <= 0 || panel.Height <= 0)
				return;
			var path = CreateRoundedRect(new Rectangle(0, 0, panel.Width, panel.Height), radius);
			panel.Region?.Dispose();
			panel.Region = new Region(path);
			path.Dispose();
		}

		private static void OnPaintRoundedPanel(object sender, PaintEventArgs e)
		{
			if (sender is not Panel panel)
				return;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using var path = CreateRoundedRect(new Rectangle(0, 0, panel.Width, panel.Height), 16);
			using var brush = new SolidBrush(panel.BackColor);
			e.Graphics.FillPath(brush, path);
		}

		private static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
		{
			var diameter = radius * 2;
			var path = new GraphicsPath();
			path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
			path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
			path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
			path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
			path.CloseFigure();
			return path;
		}

		private void OnBrushSliderChanged(object sender, EventArgs e)
		{
			brushSize = brushSlider.Value;
			sizeLabel.Text = $"{brushSize:F0}px";
			skiaView.Invalidate();
		}

		private void UpdateColorSelection()
		{
			foreach (var btn in colorButtons)
			{
				if (btn.Tag is not (SKColor light, SKColor dark))
					continue;
				var isSelected = (IsDarkMode ? dark : light) == currentColor;
				btn.FlatAppearance.BorderSize = isSelected ? 2 : 0;
				btn.FlatAppearance.BorderColor = IsDarkMode ? Color.White : Color.FromArgb(60, 60, 60);
			}
		}

		private void OnColorClick(object sender, EventArgs e)
		{
			if (sender is Button btn && btn.Tag is (SKColor light, SKColor dark))
			{
				currentColor = IsDarkMode ? dark : light;
				UpdateColorSelection();
			}
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
			var newValue = Math.Max(1, Math.Min(50, brushSlider.Value + (e.Delta > 0 ? 1 : -1)));
			brushSlider.Value = newValue;
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
