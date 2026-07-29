using System;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	[Collection(VulkanGpuRenderingCollection.Name)]
	public class GRVkMemoryAllocatorTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void CreateDefaultRejectsNullHandlesAndGetProc()
		{
			Assert.Throws<ArgumentException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: IntPtr.Zero, vkPhysicalDevice: (IntPtr)1, vkDevice: (IntPtr)2,
				maxApiVersion: 0, getProcedureAddress: (n, i, d) => IntPtr.Zero));

			Assert.Throws<ArgumentException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: (IntPtr)1, vkPhysicalDevice: IntPtr.Zero, vkDevice: (IntPtr)2,
				maxApiVersion: 0, getProcedureAddress: (n, i, d) => IntPtr.Zero));

			Assert.Throws<ArgumentException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: (IntPtr)1, vkPhysicalDevice: (IntPtr)2, vkDevice: IntPtr.Zero,
				maxApiVersion: 0, getProcedureAddress: (n, i, d) => IntPtr.Zero));

			Assert.Throws<ArgumentNullException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: (IntPtr)1, vkPhysicalDevice: (IntPtr)2, vkDevice: (IntPtr)3,
				maxApiVersion: 0, getProcedureAddress: null));
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void CreateDefaultReturnsAllocator()
		{
			using var ctx = CreateSilkVkContext();

			using var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				SilkVkContext.ApiVersion,
				(name, instance, device) => ctx.BaseGetProc(name, instance, device));

			Assert.NotNull(allocator);
			Assert.NotEqual(IntPtr.Zero, allocator.Handle);
		}

		// The Ganesh factory ref_sp's whatever comes through GRVkBackendContext.MemoryAllocator,
		// so the Context outlives the managed wrapper's Dispose. Verify the wrapper can be
		// disposed immediately after GRContext.CreateVulkan and the Context still functions.
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GaneshContextRetainsAllocatorAfterWrapperDispose()
		{
			using var ctx = CreateSilkVkContext();
			var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				SilkVkContext.ApiVersion,
				(name, instance, device) => ctx.BaseGetProc(name, instance, device));
			Assert.NotNull(allocator);

			using var extensions = new GRVkExtensions();
			extensions.Initialize((name, instance, device) => ctx.BaseGetProc(name, instance, device),
				ctx.Instance.Handle, ctx.PhysicalDevice.Handle);

			using var backend = new GRVkBackendContext
			{
				VkInstance         = ctx.Instance.Handle,
				VkPhysicalDevice   = ctx.PhysicalDevice.Handle,
				VkDevice           = ctx.Device.Handle,
				VkQueue            = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxAPIVersion      = SilkVkContext.ApiVersion,
				Extensions         = extensions,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				MemoryAllocator    = allocator,
			};

			using var grContext = GRContext.CreateVulkan(backend);
			Assert.NotNull(grContext);

			// Drop the managed wrapper's ref. The Context's own ref must keep the allocator alive.
			allocator.Dispose();

			// Exercise the allocator via a texture-backed surface + draw.
			using var surface = SKSurface.Create(grContext, budgeted: true, new SKImageInfo(64, 64));
			Assert.NotNull(surface);
			surface.Canvas.Clear(SKColors.Purple);
			grContext.Flush(submit: true, synchronous: true);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteContextAcceptsCustomAllocator()
		{
			using var ctx = CreateSilkVkContext();

			var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				SilkVkContext.ApiVersion,
				(name, instance, device) => ctx.BaseGetProc(name, instance, device));
			Assert.NotNull(allocator);

			using var backend = new SKGraphiteVkBackendContext
			{
				VkInstance         = ctx.Instance.Handle,
				VkPhysicalDevice   = ctx.PhysicalDevice.Handle,
				VkDevice           = ctx.Device.Handle,
				VkQueue            = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxApiVersion      = SilkVkContext.ApiVersion,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				MemoryAllocator    = allocator,
			};

			using var graphiteContext = SKGraphiteContext.CreateVulkan(backend);
			Assert.NotNull(graphiteContext);

			// The Context has retained the allocator; managed wrapper can go.
			allocator.Dispose();

			using var recorder = graphiteContext.CreateRecorder();
			Assert.NotNull(recorder);
			using var surface = SKSurface.Create(recorder, new SKImageInfo(64, 64), false, null);
			Assert.NotNull(surface);
			surface.Canvas.Clear(SKColors.Cyan);
			using var recording = recorder.Snap();
			Assert.NotNull(recording);
			Assert.Equal(SKGraphiteInsertStatus.Success, graphiteContext.InsertRecording(recording));
			Assert.True(graphiteContext.Submit(new SKGraphiteSubmitInfo { Sync = true }));
		}
	}
}
