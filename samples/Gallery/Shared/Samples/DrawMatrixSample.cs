using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class DrawMatrixSample : SampleBase
	{
		[Preserve]
		public DrawMatrixSample()
		{
		}

		public override string Title => "Draw Matrix";

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			var size = ((float)height > width ? width : height) * 0.5f;
			var center = new SKPoint((width - size) / 2f, (height - size) / 2f);

			// draw these at specific locations
			var leftRect = SKRect.Create(center.X - size / 2f, center.Y, size, size);
			var rightRect = SKRect.Create(center.X + size / 2f, center.Y, size, size);

			// draw this at the current location / transformation
			var rotatedRect = SKRect.Create(0f, 0f, size, size);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				canvas.Clear(SampleMedia.Colors.XamarinPurple);

				// draw
				paint.Color = SampleMedia.Colors.XamarinDarkBlue;
				canvas.DrawRect(leftRect, paint);

				// save
				canvas.Save();

				// transform
				canvas.Translate(width / 2f, center.Y);
				canvas.RotateDegrees(45);

				// draw
				paint.Color = SampleMedia.Colors.XamarinGreen;
				canvas.DrawRoundRect(rotatedRect, 10, 10, paint);

				// undo transform / restore
				canvas.Restore();

				// draw
				paint.Color = SampleMedia.Colors.XamarinLightBlue;
				canvas.DrawRect(rightRect, paint);
			}
		}
	}
}
