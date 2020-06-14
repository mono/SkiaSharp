using System.Net.Http;
using SkiaSharp;
using SkiaSharp.Views.UWP;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;

namespace SkiaSharpSample
{
	public sealed partial class MainPage : Page
	{
		SKImage image;

		public MainPage()
		{
			InitializeComponent();

			Load();
		}

		private async void Load()
		{
			var client = new HttpClient();
			var stream = await client.GetStreamAsync("https://raw.githubusercontent.com/mono/SkiaSharp/master/images/nuget.png");
			image = SKImage.FromEncodedData(stream);
			skiaview.Invalidate();
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var display = DisplayInformation.GetForCurrentView();
			var scale = display.LogicalDpi / 96.0f;
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

			if (image != null)
			{
				canvas.DrawImage(image, 10, 10);
			}
		}
	}
}
