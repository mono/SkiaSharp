using System;
using System.Threading.Tasks;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	[Obsolete]
	public class BitmapScalerSample : SampleBase
	{
		private List<SKBitmapResizeMethod> methods;
		private SKBitmapResizeMethod method;
		private SKBitmap bitmap;

		[Preserve]
		public BitmapScalerSample()
		{
		}

		public override string Title => "Bitmap Scaling";

		public override SampleCategories Category => SampleCategories.BitmapDecoding;

		protected override Task OnInit()
		{
			methods = Enum.GetValues(typeof(SKBitmapResizeMethod)).Cast<SKBitmapResizeMethod>().ToList();
			method = methods[0];

			using (var stream = new SKManagedStream(SampleMedia.Images.AdobeDng))
			{
				bitmap = SKBitmap.Decode(stream);
			}

			return base.OnInit();
		}

		protected override void OnDestroy()
		{
			bitmap.Dispose();
			bitmap = null;

			base.OnDestroy();
		}

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			if (bitmap != null)
			{
				var bmpRect = SKRectI.Create(width, height).AspectFit(bitmap.Info.Size);

				using (var resized = new SKBitmap(bmpRect.Width, bmpRect.Height))
				using (var paint = new SKPaint())
				{
					bitmap.Resize(resized, method);

					canvas.DrawBitmap(resized, bmpRect);

					paint.TextAlign = SKTextAlign.Center;
					paint.TextSize = 36;
					paint.IsAntialias = true;

					paint.Color = new SKColor(0, 0, 0, 127);
					canvas.DrawRect(new SKRect(0, 0, width, (paint.TextSize * 2) + 30), paint);

					paint.Color = SKColors.White;
					canvas.DrawText(method.ToString(), width / 2, paint.TextSize + 10, paint);
					canvas.DrawText("(tap to change)", width / 2, (paint.TextSize * 2) + 10, paint);
				}
			}
		}

		protected override void OnTapped()
		{
			var idx = methods.IndexOf(method) + 1;
			if (idx >= methods.Count)
			{
				idx = 0;
			}
			method = methods[idx];

			Refresh();

			base.OnTapped();
		}
	}
}
