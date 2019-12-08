using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GRContextTest : SKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateDefaultContextIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var grContext = GRContext.CreateGl();

				Assert.NotNull(grContext);
			}
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateSpecificContextIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var glInterface = GRGlInterface.CreateNativeGlInterface();

				Assert.True(glInterface.Validate());

				var grContext = GRContext.CreateGl(glInterface);

				Assert.NotNull(grContext);
			}
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void GpuSurfaceIsCreated()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				using (var grContext = GRContext.CreateGl())
				using (var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100))) {
					Assert.NotNull(surface);

					var canvas = surface.Canvas;
					Assert.NotNull(canvas);

					canvas.Clear(SKColors.Transparent);
				}
			}
		}
	}
}
