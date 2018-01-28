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
	}
}
