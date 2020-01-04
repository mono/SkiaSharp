using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

public partial class MainWindow : Gtk.Window
{
	public MainWindow()
		: base(Gtk.WindowType.Toplevel)
	{
		Build();
		this.Remove(skiaView);

		var surf = SKSurface.Create(new SKImageInfo(200, 200));
		var cnv = surf.Canvas;
		cnv.Clear(SKColors.Azure);
		cnv.DrawText("Text", 50, 25, new SKPaint { Typeface = SKTypeface.FromFamilyName("Arial"), TextSize = 30, IsAntialias = true, Color = SKColors.OliveDrab });
		cnv.DrawText("Text", 50, 60, new SKPaint { Typeface = SKTypeface.FromFamilyName("Arial"), TextSize = 30, IsAntialias = true, Color = SKColors.BlueViolet });

		var i = surf.Snapshot();

		var pix = i.ToPixbuf().ToSKImage().ToPixbuf();

		var img = new Image();
		img.WidthRequest = 200;
		img.HeightRequest = 200;
		img.SetAlignment(0f, 0f);

		img.Pixbuf = pix;

		this.Add(img);

		this.Child.ShowAll();

		skiaView.PaintSurface += OnPaintSurface;
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
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
