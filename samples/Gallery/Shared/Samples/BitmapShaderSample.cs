using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class BitmapShaderSample : SampleBase
	{
		[Preserve]
		public BitmapShaderSample()
		{
		}

		public override string Title => "Bitmap Shader";

		public override SampleCategories Category => SampleCategories.BitmapDecoding | SampleCategories.Shaders;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			// load the image from the embedded resource stream
			using (var stream = new SKManagedStream(SampleMedia.Images.ColorWheel))
			using (var source = SKBitmap.Decode(stream))
			{
				var matrix = SKMatrix.CreateRotation(30.0f);

				// create the shader and paint
				using (var shader = SKShader.CreateBitmap(source, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, matrix))
				using (var paint = new SKPaint())
				{
					paint.IsAntialias = true;
					paint.Shader = shader;

					// tile the bitmap
					canvas.Clear(SKColors.White);
					canvas.DrawPaint(paint);
				}
			}
		}
	}
}
