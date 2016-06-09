using System;
using Android.Views;
using Android.Graphics;
using SkiaSharp;

namespace Skia.Forms.Demo.Droid
{
	public class NativeSkiaView : View
	{
		private Bitmap bitmap;
		readonly SkiaView skiaView;
		readonly ISkiaViewController iskiaView;

		public NativeSkiaView (Android.Content.Context context, SkiaView skiaView) : base (context)
		{
			this.skiaView = skiaView;
			this.iskiaView = skiaView;
			this.Click += OnTapped;
		}

		protected override void OnDraw (Android.Graphics.Canvas canvas)
		{
			base.OnDraw (canvas);

			if (bitmap == null || bitmap.Width != canvas.Width || bitmap.Height != canvas.Height) {
				if (bitmap != null) 
					bitmap.Dispose ();

				bitmap = Bitmap.CreateBitmap (canvas.Width, canvas.Height, Bitmap.Config.Argb8888);
			}

			try
			{
				using (var surface = SKSurface.Create (canvas.Width, canvas.Height, SKColorType.Rgba_8888, SKAlphaType.Premul, bitmap.LockPixels (), canvas.Width * 4)) {
					var skcanvas = surface.Canvas;
					skcanvas.Scale (((float)canvas.Width)/(float)skiaView.Width, ((float)canvas.Height)/(float)skiaView.Height);
					iskiaView.SendDraw (skcanvas);
				}
			}
			finally {
				bitmap.UnlockPixels ();
			}

			canvas.DrawBitmap (bitmap, 0, 0, null);
		}

		void OnTapped(object sender, EventArgs e)
		{
			iskiaView.SendTap ();
		}
	}
}

