using System;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	public class GRContextTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void CreateVkContextIsValid()
		{
			using var ctx = CreateSilkVkContext();

			using var extensions = GRVkExtensionsSilkNetExtensions.Create(ctx.GetProc, ctx.Instance, ctx.PhysicalDevice);

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

			Assert.NotNull(grContext);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void CreateVkContextWithOptionsIsValid()
		{
			using var ctx = CreateSilkVkContext();

			using var extensions = GRVkExtensionsSilkNetExtensions.Create(ctx.GetProc, ctx.Instance, ctx.PhysicalDevice);

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

			var options = new GRContextOptions();

			using var grContext = GRContext.CreateVulkan(grVkBackendContext, options);

			Assert.NotNull(grContext);
		}
	}
}
