using System;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	public class GRContextTest : VKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateVkContextIsValid()
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

			Assert.NotNull(grContext);
		}
	}
}
