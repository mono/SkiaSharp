namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// A linear gradient overdrawn by a translucent circle with a non-default blend
	/// mode — exercises shaders, gradient interpolation, and premultiplied-alpha
	/// blending, which are common sources of divergence across backends.
	/// </summary>
	public sealed class GradientBlendScene : ISkiaScene
	{
		public string Name => "GradientBlend";

		public SKImageInfo Info => new(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);

		public bool IsPlatformDependent => false;

		public void Draw(SKCanvas canvas)
		{
			canvas.Clear(SKColors.White);

			using (var gradient = SKShader.CreateLinearGradient(
				new SKPoint(0, 0),
				new SKPoint(256, 256),
				new[] { SKColors.Orange, SKColors.DeepPink, SKColors.Indigo },
				new[] { 0f, 0.5f, 1f },
				SKShaderTileMode.Clamp))
			using (var paint = new SKPaint { IsAntialias = true, Shader = gradient })
			{
				canvas.DrawRect(new SKRect(16, 16, 240, 240), paint);
			}

			using (var paint = new SKPaint
			{
				IsAntialias = true,
				Color = SKColors.Cyan.WithAlpha(160),
				BlendMode = SKBlendMode.Multiply,
			})
			{
				canvas.DrawCircle(160, 96, 72, paint);
			}
		}
	}
}
