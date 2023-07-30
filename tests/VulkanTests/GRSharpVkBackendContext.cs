using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	public class SharpVkBackendContextTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
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
				GetProcedureAddress = ctx.SharpVkGetProc,
				VkPhysicalDeviceFeatures = ctx.PhysicalDevice.GetFeatures(),
			};
			Assert.NotNull(grVkBackendContext);

			var baseType = grVkBackendContext as GRVkBackendContext;
			Assert.NotNull(baseType);

			Assert.Equal(ctx.Instance.RawHandle.ToUInt64(), (ulong)baseType.VkInstance);
			Assert.Equal(ctx.PhysicalDevice.RawHandle.ToUInt64(), (ulong)baseType.VkPhysicalDevice);
			Assert.Equal(ctx.Device.RawHandle.ToUInt64(), (ulong)baseType.VkDevice);
			Assert.Equal(ctx.GraphicsQueue.RawHandle.ToUInt64(), (ulong)baseType.VkQueue);
			Assert.NotEqual(0, (long)baseType.VkPhysicalDeviceFeatures);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void PropertyIsSetAndUnset()
		{
			using var grVkBackendContext = new GRSharpVkBackendContext();
			var baseType = grVkBackendContext as GRVkBackendContext;

			Assert.Equal(0, (long)baseType.VkPhysicalDeviceFeatures);

			grVkBackendContext.VkPhysicalDeviceFeatures = new global::SharpVk.PhysicalDeviceFeatures();
			Assert.NotEqual(0, (long)baseType.VkPhysicalDeviceFeatures);

			grVkBackendContext.VkPhysicalDeviceFeatures = null;
			Assert.Equal(0, (long)baseType.VkPhysicalDeviceFeatures);
		}
	}
}
