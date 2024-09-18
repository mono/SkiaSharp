using System;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample
{
	public class MainWindow : Window
	{
		private SKDrawingArea skiaView;

		public MainWindow()
			: this(new Builder("MainWindow.glade"))
		{
		}

		private MainWindow(Builder builder)
			: base(builder.GetObject("MainWindow").Handle)
		{
			builder.Autoconnect(this);
			DeleteEvent += OnWindowDeleteEvent;

			skiaView = new SKDrawingArea();
			skiaView.IgnorePixelScaling = true;
			skiaView.PaintSurface += OnPaintSurface;
			skiaView.Show();
			Child = skiaView;
		}

		private void OnWindowDeleteEvent(object sender, DeleteEventArgs a)
		{
			Application.Quit();
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			// draw some text
			using var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill
			};
			using var font = new SKFont
			{
				Size = 24 * ScaleFactor
			};
			var coord = new SKPoint(e.Info.Width / 2, (e.Info.Height + font.Size) / 2);
			canvas.DrawText("SkiaSharp", coord, SKTextAlign.Center, font, paint);
		}
	}
}
