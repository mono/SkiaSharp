using System;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class ColorMatrixColorFilterSample : SampleBase
	{
		[Preserve]
		public ColorMatrixColorFilterSample()
		{
		}

		public override string Title => "Color Matrix Color Filter";

		public override SampleCategories Category => SampleCategories.ColorFilters;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			// load the image from the embedded resource stream
			using (var stream = new SKManagedStream(SampleMedia.Images.Baboon))
			using (var bitmap = SKBitmap.Decode(stream))
			{
				var f = new Action<SKRect, float[]>((rect, colorMatrix) =>
				{
					using (var cf = SKColorFilter.CreateColorMatrix(colorMatrix))
					using (var paint = new SKPaint())
					{
						paint.ColorFilter = cf;

						canvas.DrawBitmap(bitmap, rect, paint);
					}
				});

				var colorMatrix1 = new float[20] {
					0f, 1f, 0f, 0f, 0f,
					0f, 0f, 1f, 0f, 0f,
					1f, 0f, 0f, 0f, 0f,
					0f, 0f, 0f, 1f, 0f
				};
				var grayscale = new float[20] {
					0.21f, 0.72f, 0.07f, 0.0f, 0.0f,
					0.21f, 0.72f, 0.07f, 0.0f, 0.0f,
					0.21f, 0.72f, 0.07f, 0.0f, 0.0f,
					0.0f,  0.0f,  0.0f,  1.0f, 0.0f
				};
				var colorMatrix3 = new float[20] {
					-1f,  1f,  1f, 0f, 0f,
					 1f, -1f,  1f, 0f, 0f,
					 1f,  1f, -1f, 0f, 0f,
					 0f,  0f,  0f, 1f, 0f
				};
				var colorMatrix4 = new float[20] {
					0.0f, 0.5f, 0.5f, 0f, 0f,
					0.5f, 0.0f, 0.5f, 0f, 0f,
					0.5f, 0.5f, 0.0f, 0f, 0f,
					0.0f, 0.0f, 0.0f, 1f, 0f
				};
				var highContrast = new float[20] {
					4.0f, 0.0f, 0.0f, 0.0f, -4.0f / (4.0f - 1f),
					0.0f, 4.0f, 0.0f, 0.0f, -4.0f / (4.0f - 1f),
					0.0f, 0.0f, 4.0f, 0.0f, -4.0f / (4.0f - 1f),
					0.0f, 0.0f, 0.0f, 1.0f, 0.0f
				};
				var colorMatrix6 = new float[20] {
					0f, 0f, 1f, 0f, 0f,
					1f, 0f, 0f, 0f, 0f,
					0f, 1f, 0f, 0f, 0f,
					0f, 0f, 0f, 1f, 0f
				};
				var sepia = new float[20] {
					0.393f, 0.769f, 0.189f, 0.0f, 0.0f,
					0.349f, 0.686f, 0.168f, 0.0f, 0.0f,
					0.272f, 0.534f, 0.131f, 0.0f, 0.0f,
					0.0f,   0.0f,   0.0f,   1.0f, 0.0f
				};
				var inverter = new float[20] {
					-1f,  0f,  0f, 0f, 1f,
					0f, -1f,  0f, 0f, 1f,
					0f,  0f, -1f, 0f, 1f,
					0f,  0f,  0f, 1f, 0f
				};

				var matices = new[] {
					colorMatrix1, grayscale, highContrast, sepia,
					colorMatrix3, colorMatrix4, colorMatrix6, inverter
				};

				var cols = width < height ? 2 : 4;
				var rows = (matices.Length - 1) / cols + 1;
				var w = (float)width / cols;
				var h = (float)height / rows;

				for (var y = 0; y < rows; y++)
				{
					for (var x = 0; x < cols; x++)
					{
						f(SKRect.Create(x * w, y * h, w, h), matices[y * cols + x]);
					}
				}
			}
		}
	}
}
