using System.IO;
using Microsoft.Maui.Controls;
using SkiaSharp;

namespace SkiaSharpSample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			imageView.Source = ImageSource.FromStream(() => CreateImage());
		}

		private Stream CreateImage()
		{
			// crate a surface
			var info = new SKImageInfo(256, 256);
			using var surface = SKSurface.Create(info);

			// the the canvas and properties
			var canvas = surface.Canvas;

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			// draw some text
			using var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				TextAlign = SKTextAlign.Center,
				TextSize = 24
			};
			var coord = new SKPoint(info.Width / 2, (info.Height + paint.TextSize) / 2);
			canvas.DrawText("SkiaSharp", coord, paint);

			// encode the file
			using var image = surface.Snapshot();

			var data = image.Encode(SKEncodedImageFormat.Png, 100);
			return data.AsStream(true);
		}
	}
}
