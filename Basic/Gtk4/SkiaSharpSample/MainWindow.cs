using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample
{
	public class MainWindow : ApplicationWindow
	{
		private readonly SKDrawingArea skiaView;

		public MainWindow(Application app)
			: base(new GObject.ConstructArgument[] { })
		{
			Application = app;
			Title = "SkiaSharp";
			SetDefaultSize(400, 300);

			skiaView = new SKDrawingArea();
			skiaView.PaintSurface += OnPaintSurface;
			Child = skiaView;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// get the canvas and properties
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
				Size = 24
			};
			var coord = new SKPoint(e.Info.Width / 2, (e.Info.Height + font.Size) / 2);
			canvas.DrawText("SkiaSharp", coord, SKTextAlign.Center, font, paint);
		}
	}
}
