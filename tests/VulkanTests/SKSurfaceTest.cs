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
			using var ctx = CreateVkContext();

			using var grVkBackendContext = new GRVkBackendContext
			{
				VkInstance = (IntPtr)ctx.Instance.RawHandle.ToUInt64(),
				VkPhysicalDevice = (IntPtr)ctx.PhysicalDevice.RawHandle.ToUInt64(),
				VkDevice = (IntPtr)ctx.Device.RawHandle.ToUInt64(),
				VkQueue = (IntPtr)ctx.GraphicsQueue.RawHandle.ToUInt64(),
				GraphicsQueueIndex = ctx.GraphicsFamily,
				GetProcedureAddress = ctx.GetProc
			};

			Assert.NotNull(grVkBackendContext);

			using var grContext = GRContext.CreateVulkan(grVkBackendContext);

			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));

			Assert.NotNull(surface);

			var canvas = surface.Canvas;
			Assert.NotNull(canvas);

			canvas.Clear(SKColors.Transparent);

			canvas.Flush();
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void VkGpuSurfaceIsCreatedSharpVkTypes()
		{
			using var ctx = CreateVkContext();

			using var grVkBackendContext = new GRSharpVkBackendContext
			{
				VkInstance = ctx.Instance,
				VkPhysicalDevice = ctx.PhysicalDevice,
				VkDevice = ctx.Device,
				VkQueue = ctx.GraphicsQueue,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				GetProcedureAddress = ctx.SharpVkGetProc
			};

			Assert.NotNull(grVkBackendContext);

			using var grContext = GRContext.CreateVulkan(grVkBackendContext);

			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));

			Assert.NotNull(surface);

			var canvas = surface.Canvas;
			Assert.NotNull(canvas);

			canvas.Clear(SKColors.Transparent);

			canvas.Flush();
		}
	}
}
