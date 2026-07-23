using System;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	public class SKSurfaceTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void VkGpuSurfaceIsCreated()
		{
			using var ctx = CreateSilkVkContext();

			using var extensions = new GRVkExtensions();
			extensions.Initialize(ctx.GetProc, ctx.Instance, ctx.PhysicalDevice);

			using var grVkBackendContext = new GRVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxAPIVersion = SilkVkContext.ApiVersion,
				Extensions = extensions,
				GetProcedureAddress = ctx.BaseGetProc,
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void VkGpuSurfaceIsCreatedSharpVkTypes()
		{
			using var ctx = CreateSharpVkContext();

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
