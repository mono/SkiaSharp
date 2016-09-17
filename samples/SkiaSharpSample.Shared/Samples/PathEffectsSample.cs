using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class PathEffectsSample : SampleBase
	{
		[Preserve]
		public PathEffectsSample()
		{
		}

		public override string Title => "Path Effects";

		public override SampleCategories Category => SampleCategories.PathEffects | SampleCategories.Paths;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			var step = height / 4;

			using (var paint = new SKPaint())
			using (var effect = SKPathEffect.CreateDash(new[] { 15f, 5f }, 0))
			{
				paint.IsStroke = true;
				paint.StrokeWidth = 4;
				paint.PathEffect = effect;
				canvas.DrawLine(10, step, width - 10 - 10, step, paint);
			}

			using (var paint = new SKPaint())
			using (var effect = SKPathEffect.CreateDiscrete(10, 10))
			{
				paint.IsStroke = true;
				paint.StrokeWidth = 4;
				paint.PathEffect = effect;
				canvas.DrawLine(10, step * 2, width - 10 - 10, step * 2, paint);
			}

			using (var paint = new SKPaint())
			using (var dashEffect = SKPathEffect.CreateDash(new[] { 15f, 5f }, 0))
			using (var discreteEffect = SKPathEffect.CreateDiscrete(10, 10))
			using (var effect = SKPathEffect.CreateCompose(dashEffect, discreteEffect))
			{
				paint.IsStroke = true;
				paint.StrokeWidth = 4;
				paint.PathEffect = effect;
				canvas.DrawLine(10, step * 3, width - 10 - 10, step * 3, paint);
			}
		}
	}
}
