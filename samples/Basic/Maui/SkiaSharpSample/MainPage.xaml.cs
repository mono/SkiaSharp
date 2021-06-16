using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaSharpSample
{
	public partial class MainPage : ContentPage
	{
		private SKPoint? touchLocation;

		public MainPage()
		{
			InitializeComponent();
		}

		private void OnTouch(object sender, SKTouchEventArgs e)
		{
			if (e.InContact)
				touchLocation = e.Location;
			else
				touchLocation = null;

			skiaView.InvalidateSurface();

			e.Handled = true;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var scale = (float)(e.Info.Width / skiaView.Width);

			// handle the device screen density
			canvas.Scale(scale);

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			// decide what the text looks like
			using var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				TextAlign = SKTextAlign.Center,
				TextSize = 24
			};

			// adjust the location based on the pointer
			var coord = (touchLocation is SKPoint loc)
				? new SKPoint(loc.X / scale, loc.Y / scale)
				: new SKPoint((float)skiaView.Width / 2, ((float)skiaView.Height + paint.TextSize) / 2);

			// draw some text
			canvas.DrawText("SkiaSharp", coord, paint);

			imageView.Source = new SKImageImageSource { Image = e.Surface.Snapshot() };
			imageButton.Source = new SKImageImageSource { Image = e.Surface.Snapshot() };
		}
	}
}
