namespace SkiaSharp.Tests.Visual
{
	public sealed class RedRoundedRectOnWhiteScene : ISkiaScene
	{
		public string Name => "RedRoundedRectOnWhite";

		public SKImageInfo SuggestedInfo =>
			new SKImageInfo (256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);

		public void Draw (SKCanvas canvas)
		{
			canvas.Clear (SKColors.White);
			using var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
			canvas.DrawRoundRect (SKRect.Create (32, 32, 192, 192), 24, 24, paint);
		}
	}
}
