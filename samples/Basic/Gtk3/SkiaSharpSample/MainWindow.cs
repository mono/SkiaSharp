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

			// get the screen density for scaling
			var scale = 1f;
			var scaledSize = new SKSize(e.Info.Width / scale, e.Info.Height / scale);

			// handle the device screen density
			canvas.Scale(scale);

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			// draw some text
			var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				TextAlign = SKTextAlign.Center,
				TextSize = 24
			};
			var coord = new SKPoint(scaledSize.Width / 2, (scaledSize.Height + paint.TextSize) / 2);
			canvas.DrawText("SkiaSharp", coord, paint);
		}
	}
}
