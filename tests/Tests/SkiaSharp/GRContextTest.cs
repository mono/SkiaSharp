using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GRContextTest : SKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void CreateDefaultContextIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var grContext = GRContext.CreateGl();

				Assert.NotNull(grContext);
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void AbandonContextIsAbandoned()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var grContext = GRContext.CreateGl();

				Assert.False(grContext.IsAbandoned);

				grContext.AbandonContext();

				Assert.True(grContext.IsAbandoned);
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void CreateDefaultContextWithOptionsIsValid()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			var options = new GRContextOptions();
			var grContext = GRContext.CreateGl(options);

			Assert.NotNull(grContext);
		}

		[SkippableFact]
		public void ToGlSizedFormat()
		{
			var unknowns = new[] {
				SKColorType.Unknown,
				SKColorType.Rgb101010x,
				SKColorType.RgbaF32,
				SKColorType.Bgra1010102,
				SKColorType.Bgr101010x,
				SKColorType.Bgr101010xXR,
				SKColorType.Rgba10x6,
			};

			foreach (SKColorType value in Enum.GetValues(typeof(SKColorType)))
			{
				if (Array.IndexOf(unknowns, value) != -1)
					Assert.Equal(0u, value.ToGlSizedFormat());
				else
					Assert.NotEqual(0u, value.ToGlSizedFormat());
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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
