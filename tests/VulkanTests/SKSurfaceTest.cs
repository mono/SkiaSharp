using System;
using SharpVk.Interop;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	public class SKSurfaceTest : VKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void VkGpuSurfaceIsCreated()
		{
			var nativeLibrary = new NativeLibrary();

			using (var ctx = CreateVkContext())
			using (var vkInterface = GRVkInterface.Create(
				nativeLibrary.GetProcedureAddress("vkGetInstanceProcAddr"),
				nativeLibrary.GetProcedureAddress("vkGetDeviceProcAddr"),
				(IntPtr)ctx.Instance.RawHandle.ToUInt64(),
				(IntPtr)ctx.Device.RawHandle.ToUInt64(),
				0))
			using (var grVkBackendContext = GRVkBackendContext.Assemble(
				(IntPtr)ctx.Instance.RawHandle.ToUInt64(),
				(IntPtr)ctx.PhysicalDevice.RawHandle.ToUInt64(),
				(IntPtr)ctx.Device.RawHandle.ToUInt64(),
				(IntPtr)ctx.GraphicsQueue.RawHandle.ToUInt64(),
				ctx.GraphicsFamily,
				0,
				0,
				0,
				vkInterface))
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
