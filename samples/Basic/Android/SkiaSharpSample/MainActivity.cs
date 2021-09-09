using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;

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

			SetContentView(Resource.Layout.main);

			skiaView = FindViewById<SKCanvasView>(Resource.Id.skiaView);
		}

		protected override void OnResume()
		{
			base.OnResume();

			skiaView.PaintSurface += OnPaintSurface;
		}

		protected override void OnPause()
		{
			skiaView.PaintSurface -= OnPaintSurface;

			base.OnPause();
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
}
