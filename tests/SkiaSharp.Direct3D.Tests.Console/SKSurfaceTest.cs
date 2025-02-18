using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Direct3D.Tests
{
    public class SKSurfaceTest : Direct3DTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void Direct3DGpuSurfaceIsCreated()
		{
			using var ctx = CreateDirect3DContext();

			var grBackendContext = new GRDirect3DBackendContext
			{
				Adapter = ctx.Adapter,
				Device = ctx.Device,
				Queue = ctx.Queue,
			};

			Assert.NotNull(grBackendContext);

			using var grContext = GRContext.CreateDirect3D(grBackendContext);

			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));

			Assert.NotNull(surface);

			var canvas = surface.Canvas;
			Assert.NotNull(canvas);

			canvas.Clear(SKColors.Transparent);

			canvas.Flush();
		}
	}
}
