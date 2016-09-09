using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using SkiaSharp;
using SkiaSharp.Views;

namespace BasicSkiaSharp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		// the real draw method
		private static void Draw(SKSurface surface, SKSize size)
		{
			const int stroke = 4;
			const int curve = 20;
			const int textSize = 60;
			const int shrink = stroke / -2;

			var canvas = surface.Canvas;

			canvas.Clear(SKColors.Transparent);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				paint.TextSize = textSize;

				paint.Color = SKColors.Orchid;
				var r = SKRect.Create(SKPoint.Empty, size);
				canvas.DrawRoundRect(r, curve, curve, paint);

				paint.Color = SKColors.GreenYellow;
				canvas.DrawText("Hello WPF World!", 30, textSize + 10, paint);

				paint.Color = SKColors.Orange.WithAlpha(100);
				canvas.DrawOval(SKRect.Create(50, 50, 100, 100), paint);

				paint.IsStroke = true;
				paint.StrokeWidth = stroke;
				paint.Color = SKColors.Black;
				r.Inflate(shrink, shrink);
				canvas.DrawRoundRect(r, curve - stroke, curve - stroke, paint);
			}
		}

		// from software view
		private void SKElement_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			Draw(e.Surface, e.Info.Size);
		}

		// for the hardware host
		private void WindowsFormsHost_Initialized(object sender, EventArgs e)
		{
			var glControl = new SKGLControl();
			glControl.PaintSurface += GlControl_PaintSurface;
			glControl.Dock = DockStyle.Fill;

			var host = (WindowsFormsHost)sender;
			host.Child = glControl;
		}

		// from hardware view
		private void GlControl_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			Draw(e.Surface, new SKSize(e.RenderTarget.Width, e.RenderTarget.Height));
		}
	}
}
