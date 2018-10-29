using Xunit;

namespace SkiaSharp.Tests
{
	public class SKShaderTest : SKTest
	{
		[SkippableFact]
		public void CanDrawWithCreateSweepGradientShader()
		{
			var size = 160;
			var colors = new[] { SKColors.Blue, SKColors.Yellow, SKColors.Green };
			var pos = new[] { 0.0f, 0.25f, 0.50f };
			var modes = new[] { SKShaderTileMode.Clamp, SKShaderTileMode.Repeat, SKShaderTileMode.Mirror };

			var angles = new[] {
				(start: -330, end: -270),
				(start:   30, end:   90),
				(start:  390, end:  450),
				(start:  -30, end:  800),
			};

			using (var bitmap = new SKBitmap(new SKImageInfo(690, 512)))
			using (var canvas = new SKCanvas(bitmap))
			using (var p = new SKPaint())
			{
				var r = SKRect.Create(size, size);

				foreach (var mode in modes)
				{
					using (new SKAutoCanvasRestore(canvas, true))
					{
						foreach (var angle in angles)
						{
							p.Shader = SKShader.CreateSweepGradient(new SKPoint(size / 2f, size / 2f), colors, pos, mode, angle.start, angle.end);

							canvas.DrawRect(r, p);
							canvas.Translate(size * 1.1f, 0);
						}
					}

					canvas.Translate(0, size * 1.1f);
				}
			}
		}
	}
}
