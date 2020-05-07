using System;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	public class SKSurfaceTest : VKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void VkGpuSurfaceIsCreated()
		{
			using (var ctx = CreateVkContext())
			using (var grVkBackendContext = GRVkBackendContext.Assemble(
				(IntPtr)ctx.Instance.RawHandle.ToUInt64(),
				(IntPtr)ctx.PhysicalDevice.RawHandle.ToUInt64(),
				(IntPtr)ctx.Device.RawHandle.ToUInt64(),
				(IntPtr)ctx.GraphicsQueue.RawHandle.ToUInt64(),
				ctx.GraphicsFamily,
				0,
				0,
				0,
				ctx.GetProc))
			using (var grContext = GRContext.CreateVulkan(grVkBackendContext))
			using (var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100)))
			{
				Assert.NotNull(surface);

				var canvas = surface.Canvas;
				Assert.NotNull(canvas);

				canvas.Clear(SKColors.Transparent);
			}
		}
	}
}
