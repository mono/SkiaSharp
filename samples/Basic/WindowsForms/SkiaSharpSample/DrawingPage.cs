using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample;

public partial class DrawingPage : UserControl
{
	static readonly (SKColor Light, SKColor Dark)[] colorPalette =
	[
		(SKColors.Black, SKColors.White),
		(new(0xE5, 0x39, 0x35), new(0xEF, 0x53, 0x50)),
		(new(0x1E, 0x88, 0xE5), new(0x42, 0xA5, 0xF5)),
		(new(0x43, 0xA0, 0x47), new(0x66, 0xBB, 0x6A)),
		(new(0xFB, 0x8C, 0x00), new(0xFF, 0xA7, 0x26)),
		(new(0x8E, 0x24, 0xAA), new(0xAB, 0x47, 0xBC)),
	];

	readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = [];
	SKPath? currentPath;
	SKColor currentColor;
	float brushSize = 4f;
	SKPoint cursorPosition;
	bool isCursorOver;
	int selectedColorIndex;

	SKColor CanvasBackground => IsDarkMode() ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;

	public DrawingPage()
	{
		InitializeComponent();
		currentColor = IsDarkMode() ? SKColors.White : SKColors.Black;

		ApplySwatchColors();
		UpdateColorSelection();
	}

	void ApplySwatchColors()
	{
		var dark = IsDarkMode();
		Button[] buttons = [btnBlack, btnRed, btnBlue, btnGreen, btnOrange, btnPurple];
		for (int i = 0; i < buttons.Length && i < colorPalette.Length; i++)
		{
			var c = dark ? colorPalette[i].Dark : colorPalette[i].Light;
			buttons[i].BackColor = Color.FromArgb(c.Red, c.Green, c.Blue);
			buttons[i].FlatAppearance.MouseOverBackColor = buttons[i].BackColor;
			buttons[i].FlatAppearance.MouseDownBackColor = buttons[i].BackColor;
			buttons[i].Tag = i;

			var path = new GraphicsPath();
			path.AddEllipse(0, 0, buttons[i].Width, buttons[i].Height);
			buttons[i].Region?.Dispose();
			buttons[i].Region = new Region(path);
			path.Dispose();
		}
	}

	void UpdateColorSelection()
	{
		Button[] buttons = [btnBlack, btnRed, btnBlue, btnGreen, btnOrange, btnPurple];
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].FlatAppearance.BorderSize = i == selectedColorIndex ? 2 : 0;
			buttons[i].FlatAppearance.BorderColor = Color.DodgerBlue;
		}
	}

	void OnColorClick(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.Tag is int index && index < colorPalette.Length)
		{
			var (light, dark) = colorPalette[index];
			currentColor = IsDarkMode() ? dark : light;
			selectedColorIndex = index;
			UpdateColorSelection();
		}
	}

	void OnClearClick(object sender, EventArgs e)
	{
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		currentPath?.Dispose();
		currentPath = null;
		skiaView.Invalidate();
	}

	void OnBrushSliderChanged(object sender, EventArgs e)
	{
		brushSize = brushSlider.Value;
		sizeLabel.Text = $"{brushSize:F0}";
		skiaView.Invalidate();
	}

	void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
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

	void OnMouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left) return;
		currentPath = new SKPath();
		currentPath.MoveTo(e.X, e.Y);
		cursorPosition = new SKPoint(e.X, e.Y);
		skiaView.Invalidate();
	}

	void OnMouseMove(object sender, MouseEventArgs e)
	{
		cursorPosition = new SKPoint(e.X, e.Y);
		currentPath?.LineTo(e.X, e.Y);
		skiaView.Invalidate();
	}

	void OnMouseUp(object sender, MouseEventArgs e)
	{
		if (currentPath != null)
		{
			strokes.Add((currentPath, currentColor, brushSize));
			currentPath = null;
			skiaView.Invalidate();
		}
	}

	void OnMouseWheel(object sender, MouseEventArgs e)
	{
		var newValue = Math.Clamp(brushSlider.Value + (e.Delta > 0 ? 1 : -1), 1, 50);
		brushSlider.Value = newValue;
	}

	void OnMouseEnter(object sender, EventArgs e) => isCursorOver = true;

	void OnMouseLeave(object sender, EventArgs e)
	{
		isCursorOver = false;
		skiaView.Invalidate();
	}

	bool IsDarkMode()
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
