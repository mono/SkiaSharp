using Silk.NET.Vulkan;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	public class SilkNetBackendContextTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void VkGpuSurfaceIsCreatedSilkNetTypes()
		{
			using var ctx = CreateSilkVkContext();

			using var extensions = new GRVkExtensions();
			extensions.Initialize(ctx.GetProc, ctx.Instance, ctx.PhysicalDevice);

			using var grVkBackendContext = new GRSilkNetBackendContext
			{
				VkInstance = ctx.Instance,
				VkPhysicalDevice = ctx.PhysicalDevice,
				VkDevice = ctx.Device,
				VkQueue = ctx.GraphicsQueue,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxAPIVersion = SilkVkContext.ApiVersion,
				Extensions = extensions,
				GetProcedureAddress = ctx.GetProc,
				VkPhysicalDeviceFeatures = ctx.Features,
			};

			using var grContext = GRContext.CreateVulkan(grVkBackendContext);
			Assert.NotNull(grContext);

			var info = new SKImageInfo(64, 64);
			using var surface = SKSurface.Create(grContext, budgeted: true, info);
			Assert.NotNull(surface);

			surface.Canvas.Clear(SKColors.Purple);
			grContext.Flush(submit: true, synchronous: true);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void HandlesAreMappedToBase()
		{
			using var grVkBackendContext = new GRSilkNetBackendContext
			{
				VkInstance = new Instance(0x1001),
				VkPhysicalDevice = new PhysicalDevice(0x1002),
				VkDevice = new Device(0x1003),
				VkQueue = new Queue(0x1004),
				GraphicsQueueIndex = 3,
			};

			var baseType = grVkBackendContext as GRVkBackendContext;
			Assert.NotNull(baseType);

			Assert.Equal(0x1001, (long)baseType.VkInstance);
			Assert.Equal(0x1002, (long)baseType.VkPhysicalDevice);
			Assert.Equal(0x1003, (long)baseType.VkDevice);
			Assert.Equal(0x1004, (long)baseType.VkQueue);
			Assert.Equal(3u, baseType.GraphicsQueueIndex);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void PhysicalDeviceFeaturesIsSetAndUnset()
		{
			using var grVkBackendContext = new GRSilkNetBackendContext();
			var baseType = grVkBackendContext as GRVkBackendContext;

			Assert.Equal(0, (long)baseType.VkPhysicalDeviceFeatures);

			grVkBackendContext.VkPhysicalDeviceFeatures = new PhysicalDeviceFeatures();
			Assert.NotEqual(0, (long)baseType.VkPhysicalDeviceFeatures);

			grVkBackendContext.VkPhysicalDeviceFeatures = null;
			Assert.Equal(0, (long)baseType.VkPhysicalDeviceFeatures);
		}
	}
}
