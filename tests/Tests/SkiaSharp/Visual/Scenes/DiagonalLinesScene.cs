namespace SkiaSharp.Tests.Visual
{
	public sealed class DiagonalLinesScene : ISkiaScene
	{
		public string Name => "DiagonalLines";

		public SKImageInfo SuggestedInfo =>
			new SKImageInfo (128, 128, SKColorType.Rgba8888, SKAlphaType.Premul);

		public void Draw (SKCanvas canvas)
		{
			canvas.Clear (SKColors.Black);
			using var paint = new SKPaint {
				Color = SKColors.Cyan,
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 2,
			};
			for (int i = 0; i < 8; i++) {
				float t = i * 16;
				canvas.DrawLine (t, 0, 128, 128 - t, paint);
				canvas.DrawLine (0, t, 128 - t, 128, paint);
			}
		}
	}
}
