using System;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	[Collection(VulkanGpuRenderingCollection.Name)]
	public class GraphiteVkBackendContextTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteVkBackendContextIsBuiltFromRawHandles()
		{
			using var ctx = CreateSilkVkContext();
			using var backendContext = new SKGraphiteVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxApiVersion = SilkVkContext.ApiVersion,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
			};
			Assert.NotNull(backendContext);

			// The raw handles must be stored as handed in.
			Assert.Equal((long)ctx.Instance.Handle, (long)backendContext.VkInstance);
			Assert.Equal((long)ctx.PhysicalDevice.Handle, (long)backendContext.VkPhysicalDevice);
			Assert.Equal((long)ctx.Device.Handle, (long)backendContext.VkDevice);
			Assert.Equal((long)ctx.GraphicsQueue.Handle, (long)backendContext.VkQueue);
			Assert.Equal(ctx.GraphicsFamily, backendContext.GraphicsQueueIndex);
			Assert.NotNull(backendContext.GetProcedureAddress);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteVkContextIsCreatedFromRawHandles()
		{
			using var ctx = CreateSilkVkContext();
			using var backendContext = new SKGraphiteVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxApiVersion = SilkVkContext.ApiVersion,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
			};

			using var graphiteContext = SKGraphiteContext.CreateVulkan(backendContext);

			Assert.NotNull(graphiteContext);
		}
	}
}
