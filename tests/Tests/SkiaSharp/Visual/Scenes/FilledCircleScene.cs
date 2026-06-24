namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// A single antialiased filled circle on white — exercises filled-path
	/// rasterization and edge antialiasing.
	/// </summary>
	public sealed class FilledCircleScene : ISkiaScene
	{
		public string Name => "FilledCircle";

		public SKImageInfo Info => new(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);

		public void Draw(SKCanvas canvas)
		{
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = SKColors.MediumPurple,
			};

			canvas.DrawCircle(128, 128, 96, paint);
		}
	}
}
