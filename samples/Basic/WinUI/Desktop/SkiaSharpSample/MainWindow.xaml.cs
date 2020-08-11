using Windows.Graphics.Display;
using Microsoft.UI.Xaml;

using SkiaSharp;
using SkiaSharp.Views.WinUI;

namespace SkiaSharpSample
{
	public sealed partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
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
		}
	}
}
