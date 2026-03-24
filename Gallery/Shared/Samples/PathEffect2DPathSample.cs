using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class PathEffect2DPathSample : SampleBase
	{
		[Preserve]
		public PathEffect2DPathSample()
		{
		}

		public override string Title => "2D Path Effect";

		public override SampleCategories Category => SampleCategories.PathEffects | SampleCategories.Paths;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var blockSize = 30;

			// create the path
			var path = new SKPath();
			// the rect must be offset as the path uses the center
			var rect = SKRect.Create(blockSize / -2, blockSize / -2, blockSize, blockSize);
			path.AddRect(rect);

			// move the path around: across 1 block
			var offsetMatrix = SKMatrix.CreateScale(2 * blockSize, blockSize);
			// each row, move across a bit / offset
			offsetMatrix = SKMatrix.Concat(offsetMatrix, SKMatrix.CreateSkew(0.5f, 0));

			// create the paint
			var paint = new SKPaint
			{
				PathEffect = SKPathEffect.Create2DPath(offsetMatrix, path),
				Color = SKColors.LightGray
			};

			// draw a rectangle
			canvas.DrawRect(SKRect.Create(width + blockSize, height + blockSize), paint);
		}
	}
}
