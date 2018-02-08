using Android.App;
using Android.OS;
using Android.Support.V7.App;

using SkiaSharp;
using SkiaSharp.Views.Android;

namespace SkiaSharpSample
{
	[Activity(MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		private SKCanvasView skiaView;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Main);

			skiaView = FindViewById<SKCanvasView>(Resource.Id.skiaView);

			skiaView.PaintSurface += OnPaintSurface;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var scale = Resources.DisplayMetrics.Density;
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

		protected override void OnDestroy()
		{
			skiaView.PaintSurface -= OnPaintSurface;
			base.OnDestroy();
		}
	}
}
