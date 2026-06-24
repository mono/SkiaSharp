namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Antialiased diagonal strokes on white — exercises line rasterization and
	/// stroke antialiasing.
	/// </summary>
	public sealed class DiagonalLinesScene : ISkiaScene
	{
		public string Name => "DiagonalLines";

		public SKImageInfo Info => new(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);

		public bool IsPlatformDependent => false;

		public void Draw(SKCanvas canvas)
		{
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 6,
				StrokeCap = SKStrokeCap.Round,
			};

			var colors = new[] { SKColors.Black, SKColors.Crimson, SKColors.RoyalBlue, SKColors.SeaGreen };

			for (var i = 0; i < colors.Length; i++)
			{
				paint.Color = colors[i];
				var offset = 32 + (i * 56);
				canvas.DrawLine(0, offset, offset, 0, paint);
				canvas.DrawLine(256 - offset, 256, 256, 256 - offset, paint);
			}
		}
	}
}
