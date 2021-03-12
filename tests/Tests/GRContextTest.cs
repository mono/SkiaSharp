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
		public void CreateDefaultContextWithOptionsIsValid()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			var options = new GRContextOptions();
			var grContext = GRContext.CreateGl(options);

			Assert.NotNull(grContext);
		}

		[Obsolete]
		[SkippableFact]
		public void ToGlSizedFormat()
		{
			foreach (GRPixelConfig value in Enum.GetValues(typeof(GRPixelConfig)))
			{
				if (IsEnumValueDeprected(value))
					Assert.Throws<ArgumentOutOfRangeException>(() => value.ToGlSizedFormat());
				else if (value == GRPixelConfig.Unknown)
					Assert.Equal(0u, value.ToGlSizedFormat());
				else
					Assert.NotEqual(0u, value.ToGlSizedFormat());
			}
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateSpecificContextIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var glInterface = GRGlInterface.Create();

				Assert.True(glInterface.Validate());

				var grContext = GRContext.CreateGl(glInterface);

				Assert.NotNull(grContext);
			}
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateSpecificContextWithOptionsIsValid()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			var glInterface = GRGlInterface.Create();

			Assert.True(glInterface.Validate());

			var options = new GRContextOptions();
			var grContext = GRContext.CreateGl(glInterface, options);

			Assert.NotNull(grContext);
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

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void GpuSurfaceReferencesSameContext()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();
			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			Assert.NotNull(surface);

			Assert.Equal(grContext, surface.Context);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void GpuSurfaceCanMakeAnotherSurface()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();

			using var surface1 = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			Assert.NotNull(surface1);

			using var surface2 = SKSurface.Create(surface1.Context, true, new SKImageInfo(100, 100));
			Assert.NotNull(surface2);

			Assert.NotEqual(surface1, surface2);
			Assert.Equal(grContext, surface1.Context);
			Assert.Equal(grContext, surface2.Context);
		}
	}
}
