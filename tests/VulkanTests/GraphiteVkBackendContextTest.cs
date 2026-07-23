using System;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	public class GraphiteVkBackendContextTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteVkBackendContextIsBuiltFromRawHandles()
		{
			using var ctx = CreateVkContext();
			using var backendContext = new SKGraphiteVkBackendContext
			{
				VkInstance = (IntPtr)ctx.Instance.RawHandle.ToUInt64(),
				VkPhysicalDevice = (IntPtr)ctx.PhysicalDevice.RawHandle.ToUInt64(),
				VkDevice = (IntPtr)ctx.Device.RawHandle.ToUInt64(),
				VkQueue = (IntPtr)ctx.GraphicsQueue.RawHandle.ToUInt64(),
				GraphicsQueueIndex = ctx.GraphicsFamily,
				GetProcedureAddress = (name, instance, device) => ctx.GetProc(name, instance, device),
			};
			Assert.NotNull(backendContext);

			// The raw handles must be stored as handed in.
			Assert.Equal(ctx.Instance.RawHandle.ToUInt64(), (ulong)backendContext.VkInstance);
			Assert.Equal(ctx.PhysicalDevice.RawHandle.ToUInt64(), (ulong)backendContext.VkPhysicalDevice);
			Assert.Equal(ctx.Device.RawHandle.ToUInt64(), (ulong)backendContext.VkDevice);
			Assert.Equal(ctx.GraphicsQueue.RawHandle.ToUInt64(), (ulong)backendContext.VkQueue);
			Assert.Equal(ctx.GraphicsFamily, backendContext.GraphicsQueueIndex);
			Assert.NotNull(backendContext.GetProcedureAddress);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteVkContextIsCreatedFromRawHandles()
		{
			using var ctx = CreateVkContext();
			using var backendContext = new SKGraphiteVkBackendContext
			{
				VkInstance = (IntPtr)ctx.Instance.RawHandle.ToUInt64(),
				VkPhysicalDevice = (IntPtr)ctx.PhysicalDevice.RawHandle.ToUInt64(),
				VkDevice = (IntPtr)ctx.Device.RawHandle.ToUInt64(),
				VkQueue = (IntPtr)ctx.GraphicsQueue.RawHandle.ToUInt64(),
				GraphicsQueueIndex = ctx.GraphicsFamily,
				GetProcedureAddress = (name, instance, device) => ctx.GetProc(name, instance, device),
			};

			using var graphiteContext = SKGraphiteContext.CreateVulkan(backendContext);

			Assert.NotNull(graphiteContext);
		}
	}
}
