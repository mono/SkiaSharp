using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	// [Parallelizable(ParallelScope.None)]
	public class SKCanvasTest : SKTest
	{
		[Test]
		public void CanvasCanRestoreOnGpu()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				using (var grContext = GRContext.Create(GRBackend.OpenGL))
				using (var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100))) {
					var canvas = surface.Canvas;

					Assert.AreEqual(SKMatrix.MakeIdentity(), canvas.TotalMatrix);

					using (new SKAutoCanvasRestore(canvas)) {
						canvas.Translate(10, 10);
						Assert.AreEqual(SKMatrix.MakeTranslation(10, 10), canvas.TotalMatrix);
					}

					Assert.AreEqual(SKMatrix.MakeIdentity(), canvas.TotalMatrix);
				}
			}
		}
	}
}
