namespace SkiaSharp.Tests.Visual
{
	public sealed class GpuOnlyFilledCircleScene : ISkiaScene
	{
		// Name keeps the historical "GpuOnly_" prefix so existing goldens
		// (Goldens/*/GpuOnly_FilledCircle.png) continue to match.
		public string Name => "GpuOnly_FilledCircle";

		public SKImageInfo SuggestedInfo =>
			new SKImageInfo (96, 96, SKColorType.Rgba8888, SKAlphaType.Premul);

		public SceneRequirements Requires => SceneRequirements.Gpu;

		public void Draw (SKCanvas canvas)
		{
			canvas.Clear (SKColors.White);
			using var paint = new SKPaint { Color = SKColors.MediumBlue, IsAntialias = true };
			canvas.DrawCircle (48, 48, 36, paint);
		}
	}
}
