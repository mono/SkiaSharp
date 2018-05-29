using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKCanvasTest : SKTest
	{
		[Trait(Category, GpuCategory)]
		[SkippableFact]
		public void CanvasCanRestoreOnGpu()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				using (var grContext = GRContext.Create(GRBackend.OpenGL))
				using (var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100))) {
					var canvas = surface.Canvas;

					Assert.Equal(SKMatrix.MakeIdentity(), canvas.TotalMatrix);

					using (new SKAutoCanvasRestore(canvas)) {
						canvas.Translate(10, 10);
						Assert.Equal(SKMatrix.MakeTranslation(10, 10), canvas.TotalMatrix);
					}

					Assert.Equal(SKMatrix.MakeIdentity(), canvas.TotalMatrix);
				}
			}
		}

		[SkippableFact]
		public void CanvasCanClipRoundRect()
		{
			using (var canvas = new SKNWayCanvas(100, 100))
			{
				canvas.ClipRoundRect(new SKRoundRect(new SKRect(10, 10, 50, 50), 5, 5));
			}
		}

		[SkippableFact]
		public void CanvasCanDrawRoundRect()
		{
			using (var canvas = new SKNWayCanvas(100, 100))
			using (var paint = new SKPaint())
			{
				canvas.DrawRoundRect(new SKRoundRect(new SKRect(10, 10, 50, 50), 5, 5), paint);
			}
		}

		[SkippableFact]
		public void NWayCanvasCanBeConstructed()
		{
			using (var canvas = new SKNWayCanvas(100, 100))
			{
				Assert.NotNull(canvas);
			}
		}

		[SkippableFact]
		public void NWayCanvasDrawsToMultipleCanvases()
		{
			using (var firstBitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var first = new SKCanvas(firstBitmap))
			using (var secondBitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var second = new SKCanvas(secondBitmap))
			{
				first.Clear(SKColors.Red);
				second.Clear(SKColors.Green);

				using (var canvas = new SKNWayCanvas(100, 100))
				{
					canvas.AddCanvas(first);
					canvas.AddCanvas(second);

					canvas.Clear(SKColors.Blue);

					Assert.Equal(SKColors.Blue, firstBitmap.GetPixel(0, 0));
					Assert.Equal(SKColors.Blue, secondBitmap.GetPixel(0, 0));

					canvas.Clear(SKColors.Orange);
				}

				Assert.Equal(SKColors.Orange, firstBitmap.GetPixel(0, 0));
				Assert.Equal(SKColors.Orange, secondBitmap.GetPixel(0, 0));
			}
		}
	}
}
