namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// A red rounded rectangle on white — exercises rounded-rect rasterization and
	/// flat fills.
	/// </summary>
	public sealed class RedRoundedRectOnWhiteScene : ISkiaScene
	{
		public string Name => "RedRoundedRectOnWhite";

		public SKImageInfo Info => new(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);

		public void Draw(SKCanvas canvas)
		{
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = SKColors.Red,
			};

			var rect = new SKRect(40, 64, 216, 192);
			canvas.DrawRoundRect(rect, 32, 32, paint);
		}
	}
}
