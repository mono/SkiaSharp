using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SkiaSharp;
using SkiaSharp.Views;

namespace BasicSkiaSharp
{
	[Activity(Label = "BasicSkiaSharp", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity, ISurfaceHolderCallback
	{
		private MySoftwareView softwareView;

		private SKSurfaceView surfaceView;

		private SKGLSurfaceView hardwareView;
		private MyHardwareViewRenderer hardwarRenderer;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Main);

			var linearLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout);
			var landscape = Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape;
			linearLayout.Orientation = landscape ? Orientation.Horizontal : Orientation.Vertical;

			var softwareLayout = FindViewById<LinearLayout>(Resource.Id.softwareLayout);
			var surfaceLayout = FindViewById<LinearLayout>(Resource.Id.surfaceLayout);
			var hardwareLayout = FindViewById<LinearLayout>(Resource.Id.hardwareLayout);

			var match = ViewGroup.LayoutParams.MatchParent;

			// layout the various views

			// the software view
			softwareView = new MySoftwareView(this);
			softwareLayout.AddView(softwareView, new LinearLayout.LayoutParams(match, match, 1));

			// the software view using a surface
			surfaceView = new SKSurfaceView(this);
			surfaceView.SetZOrderOnTop(true);
			surfaceView.Holder.SetFormat(Format.Translucent);
			surfaceView.Holder.AddCallback(this);
			surfaceLayout.AddView(surfaceView, new LinearLayout.LayoutParams(match, match, 1));

			// the hardware view
			hardwareView = new SKGLSurfaceView(this);
			hardwareView.SetZOrderOnTop(true);
			hardwareView.Holder.SetFormat(Format.Translucent);
			hardwarRenderer = new MyHardwareViewRenderer();
			hardwareView.SetRenderer(hardwarRenderer);
			hardwareLayout.AddView(hardwareView, new LinearLayout.LayoutParams(match, match, 1));
		}

		protected override void OnResume()
		{
			base.OnResume();

			softwareView.PostInvalidateDelayed(5000);
		}

		// the real draw method
		private static void Draw(SKSurface surface, SKSize size)
		{
			const int stroke = 4;
			const int curve = 20;
			const int textSize = 60;
			const int shrink = stroke / -2;

			var canvas = surface.Canvas;

			canvas.Clear(SKColors.Transparent);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				paint.TextSize = textSize;

				paint.Color = SKColors.Orchid;
				var r = SKRect.Create(SKPoint.Empty, size);
				canvas.DrawRoundRect(r, curve, curve, paint);

				paint.Color = SKColors.GreenYellow;
				canvas.DrawText("Hello Droid World!", 30, textSize + 10, paint);

				paint.Color = SKColors.Orange.WithAlpha(100);
				canvas.DrawOval(SKRect.Create(50, 50, 100, 100), paint);

				paint.IsStroke = true;
				paint.StrokeWidth = stroke;
				paint.Color = SKColors.Black;
				r.Inflate(shrink, shrink);
				canvas.DrawRoundRect(r, curve - stroke, curve - stroke, paint);
			}
		}

		// drawing the surface after creation
		void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
		{
			// asynchronous drawing to the surface
			var lockedSurface = surfaceView.LockSurface();
			MainActivity.Draw(lockedSurface.Surface, lockedSurface.ImageInfo.Size);
			surfaceView.UnlockSurfaceAndPost(lockedSurface);
		}

		// don't care about this
		void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder) { }
		void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder) { }

		// the custom view
		private class MySoftwareView : SKView
		{
			public MySoftwareView(Context context)
				: base(context)
			{
			}

			protected override void OnDraw(SKSurface surface, SKImageInfo info)
			{
				base.OnDraw(surface, info);

				MainActivity.Draw(surface, info.Size);
			}
		}

		// the custom renderer
		private class MyHardwareViewRenderer : SKGLSurfaceView.ISKRenderer
		{
			public void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
			{
				MainActivity.Draw(surface, new SKSizeI(renderTarget.Width, renderTarget.Height));
			}
		}
	}
}
