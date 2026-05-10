using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// Sample visual tests demonstrating the framework. Each test runs once
	/// per available <see cref="VisualSetup"/>; goldens live under
	/// <c>tests/Tests/SkiaSharp/Visual/Goldens/{setup}/</c>.
	///
	/// First run with no golden fails with an "actual" PNG saved to the
	/// failure-inspection dir. Re-run with <c>SKIASHARP_UPDATE_GOLDENS=1</c>
	/// to accept the rendered output as the new golden.
	/// </summary>
	public class RoundedRectVisualTests : VisualTestBase
	{
		[SkippableTheory, MemberData (nameof (AllSetups))]
		public void RedRoundedRectOnWhite (VisualSetup setup) =>
			VerifyDraw (setup, nameof (RedRoundedRectOnWhite),
				new SKImageInfo (256, 256, SKColorType.Rgba8888, SKAlphaType.Premul),
				canvas => {
					canvas.Clear (SKColors.White);
					using var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
					canvas.DrawRoundRect (SKRect.Create (32, 32, 192, 192), 24, 24, paint);
				});

		[SkippableTheory, MemberData (nameof (AllSetups))]
		public void DiagonalLines (VisualSetup setup) =>
			VerifyDraw (setup, nameof (DiagonalLines),
				new SKImageInfo (128, 128, SKColorType.Rgba8888, SKAlphaType.Premul),
				canvas => {
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
				});

		// Example of a setup-restricted test: this would only make sense on GPU
		// backends (raster has no concept of multisampling beyond CPU AA).
		[SkippableTheory, MemberData (nameof (GpuSetups))]
		public void GpuOnly_FilledCircle (VisualSetup setup) =>
			VerifyDraw (setup, nameof (GpuOnly_FilledCircle),
				new SKImageInfo (96, 96, SKColorType.Rgba8888, SKAlphaType.Premul),
				canvas => {
					canvas.Clear (SKColors.White);
					using var paint = new SKPaint { Color = SKColors.MediumBlue, IsAntialias = true };
					canvas.DrawCircle (48, 48, 36, paint);
				});
	}
}
