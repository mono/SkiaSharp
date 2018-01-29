using System;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class BitmapLatticeSample : SampleBase
	{
		[Preserve]
		public BitmapLatticeSample()
		{
		}

		public override string Title => "Bitmap Lattice (9-patch)";

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			using (var stream = new SKManagedStream(SampleMedia.Images.NinePatch))
			using (var bitmap = SKBitmap.Decode(stream))
			{
				var patchCenter = new SKRectI(33, 33, 256 - 33, 256 - 33);

				// 2x3 for portrait, or 3x2 for landscape
				var land = width > height;
				var min = land ? Math.Min(width / 3f, height / 2f) : Math.Min(width / 2f, height / 3f);
				var wide = SKRect.Inflate(SKRect.Create(0, land ? min : (min * 2f), min * 2f, min), -20, -20);
				var tall = SKRect.Inflate(SKRect.Create(land ? (min * 2f) : min, 0, min, min * 2f), -20, -20);
				var square = SKRect.Inflate(SKRect.Create(0, 0, min, min), -20, -20);
				var text = SKRect.Create(land ? min : 0, land ? 0 : min, min, min / 5f);
				text.Offset(text.Width / 2f, text.Height * 1.5f);
				text.Right = text.Left;

				// draw the bitmaps
				canvas.DrawBitmapNinePatch(bitmap, patchCenter, square);
				canvas.DrawBitmapNinePatch(bitmap, patchCenter, tall);
				canvas.DrawBitmapNinePatch(bitmap, patchCenter, wide);

				// describe what we see
				using (var paint = new SKPaint())
				{
					paint.IsAntialias = true;
					paint.TextAlign = SKTextAlign.Center;
					paint.TextSize = text.Height * 0.75f;

					canvas.DrawText("The corners", text.Left, text.Top, paint);
					text.Offset(0, text.Height);
					canvas.DrawText("should always", text.Left, text.Top, paint);
					text.Offset(0, text.Height);
					canvas.DrawText("be square", text.Left, text.Top, paint);
				}
			}
		}
	}
}
