using System;
using WatchKit;
using Foundation;

using SkiaSharp;
using SkiaSharp.Views.watchOS;

namespace SkiaSharpSample.OnWatchExtension
{
	public partial class InterfaceController : WKInterfaceController
	{
		protected InterfaceController(IntPtr handle)
			: base(handle)
		{
		}

		public override void Awake(NSObject context)
		{
			base.Awake(context);

			var scale = WKInterfaceDevice.CurrentDevice.ScreenScale;

			var bitmap = new SKBitmap((int)(ContentFrame.Width * scale), (int)(ContentFrame.Height * scale));

			var colors = new[] { SKColors.Cyan, SKColors.Magenta, SKColors.Yellow, SKColors.Cyan };
			var center = new SKPoint(bitmap.Width / 2, bitmap.Height / 2);

			using (var canvas = new SKCanvas(bitmap))
			{
				canvas.Clear(SKColors.Transparent);

				using (var path = new SKPath())
				{
					path.AddRoundRect(new SKRect(5, 5, bitmap.Width - 5, bitmap.Height - 5), 5, 5);
					canvas.ClipPath(path);
				}

				using (var paint = new SKPaint())
				using (var gradient = SKShader.CreateSweepGradient(center, colors, null))
				{
					paint.IsAntialias = true;
					paint.Shader = gradient;
					canvas.DrawPaint(paint);
					paint.Shader = null;
				}

				using (var paint = new SKPaint())
				using (var tf = SKTypeface.FromFamilyName("San Fransisco"))
				{
					paint.IsAntialias = true;
					paint.Color = SKColors.DarkBlue;
					paint.TextSize = (float)(20 * scale);
					paint.TextAlign = SKTextAlign.Center;
					paint.Typeface = tf;

					canvas.DrawText("SkiaSharp", center.X, (center.Y / 2) + (paint.TextSize / 2), paint);
				}
			}

			var image = bitmap.ToUIImage(scale, UIKit.UIImageOrientation.Up);
			imageView.SetImage(image);
		}
	}
}
