using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

public partial class MainWindow : Gtk.Window
{
	public MainWindow()
		: base(Gtk.WindowType.Toplevel)
	{
		Build();

		skiaView.PaintSurface += OnPaintSurface;
	}

	protected void OnDeleteEvent(object sender, Gtk.DeleteEventArgs a)
	{
		Gtk.Application.Quit();

		a.RetVal = true;
	}

	private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		// the the canvas and properties
		var canvas = e.Surface.Canvas;

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
		var coord = new SKPoint(e.Info.Width / 2, (e.Info.Height + paint.TextSize) / 2);
		canvas.DrawText("SkiaSharp", coord, paint);
	}
}
