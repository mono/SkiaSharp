using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	public class GraphiteSharpVkBackendContextTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteVkBackendContextIsBuiltFromSharpVkTypes()
		{
			using var ctx = CreateVkContext();
			using var backendContext = new SKGraphiteSharpVkBackendContext
			{
				VkInstance = ctx.Instance,
				VkPhysicalDevice = ctx.PhysicalDevice,
				VkDevice = ctx.Device,
				VkQueue = ctx.GraphicsQueue,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				GetProcedureAddress = ctx.SharpVkGetProc,
			};
			Assert.NotNull(backendContext);

			// Setting the typed handles must write the raw handles through to the base.
			var baseType = backendContext as SKGraphiteVkBackendContext;
			Assert.NotNull(baseType);

			Assert.Equal(ctx.Instance.RawHandle.ToUInt64(), (ulong)baseType.VkInstance);
			Assert.Equal(ctx.PhysicalDevice.RawHandle.ToUInt64(), (ulong)baseType.VkPhysicalDevice);
			Assert.Equal(ctx.Device.RawHandle.ToUInt64(), (ulong)baseType.VkDevice);
			Assert.Equal(ctx.GraphicsQueue.RawHandle.ToUInt64(), (ulong)baseType.VkQueue);
			Assert.Equal(ctx.GraphicsFamily, baseType.GraphicsQueueIndex);
			Assert.NotNull(baseType.GetProcedureAddress);

			// The typed getters must round-trip the SharpVk objects.
			Assert.Same(ctx.Instance, backendContext.VkInstance);
			Assert.Same(ctx.Device, backendContext.VkDevice);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteVkContextIsCreatedFromSharpVkTypes()
		{
			using var ctx = CreateVkContext();
			using var backendContext = new SKGraphiteSharpVkBackendContext
			{
				VkInstance = ctx.Instance,
				VkPhysicalDevice = ctx.PhysicalDevice,
				VkDevice = ctx.Device,
				VkQueue = ctx.GraphicsQueue,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				GetProcedureAddress = ctx.SharpVkGetProc,
			};

			using var graphiteContext = SKGraphiteContext.CreateVulkan(backendContext);

			Assert.NotNull(graphiteContext);
		}
	}
}
