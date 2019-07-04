using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class ManipulatedBitmapShaderSample : SampleBase
	{
		[Preserve]
		public ManipulatedBitmapShaderSample()
		{
		}

		public override string Title => "Bitmap Shader (Manipulated)";

		public override SampleCategories Category => SampleCategories.BitmapDecoding | SampleCategories.Shaders;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			// load the image from the embedded resource stream
			using (var stream = new SKManagedStream(SampleMedia.Images.ColorWheel))
			using (var source = SKBitmap.Decode(stream))
			{
				// invert the pixels
				var pixels = source.Pixels;
				for (var i = 0; i < pixels.Length; i++)
				{
					pixels[i] = new SKColor(
						(byte)(255 - pixels[i].Red),
						(byte)(255 - pixels[i].Green),
						(byte)(255 - pixels[i].Blue),
						pixels[i].Alpha);
				}
				source.Pixels = pixels;

				using (var shader = SKShader.CreateBitmap(source, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat))
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
