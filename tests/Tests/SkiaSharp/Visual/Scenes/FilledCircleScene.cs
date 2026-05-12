namespace SkiaSharp.Tests.Visual
{
	public sealed class FilledCircleScene : ISkiaScene
	{
		public string Name => "FilledCircle";

		public SKImageInfo SuggestedInfo =>
			new SKImageInfo (96, 96, SKColorType.Rgba8888, SKAlphaType.Premul);

		public void Draw (SKCanvas canvas)
		{
			canvas.Clear (SKColors.White);
			using var paint = new SKPaint { Color = SKColors.MediumBlue, IsAntialias = true };
			canvas.DrawCircle (48, 48, 36, paint);
		}
	}
}
