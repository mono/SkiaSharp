using System;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	// Plumbing tests for GRVkBackendContext.DeviceLost / SKGraphiteVkBackendContext.DeviceLost
	// (mono/SkiaSharp#4601). Actually triggering VK_ERROR_DEVICE_LOST from managed code is not
	// realistic in CI — the callback is driver-owned and fires at Skia's discretion. These
	// tests instead verify the wiring: property round-trips, native bridge is allocated /
	// freed correctly, Context construction with a handler set does not crash, and Dispose
	// tears down without use-after-free.
	[Collection(VulkanGpuRenderingCollection.Name)]
	public class GRVkDeviceLostTest : VKTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void DeviceLostPropertyRoundTripsOnGRVkBackendContext()
		{
			using var backend = new GRVkBackendContext();
			Assert.Null(backend.DeviceLost);

			GRVkDeviceLostDelegate handler = info => { };
			backend.DeviceLost = handler;
			Assert.Same(handler, backend.DeviceLost);

			backend.DeviceLost = null;
			Assert.Null(backend.DeviceLost);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void DeviceLostPropertyRoundTripsOnSKGraphiteVkBackendContext()
		{
			using var backend = new SKGraphiteVkBackendContext();
			Assert.Null(backend.DeviceLost);

			GRVkDeviceLostDelegate handler = info => { };
			backend.DeviceLost = handler;
			Assert.Same(handler, backend.DeviceLost);

			backend.DeviceLost = null;
			Assert.Null(backend.DeviceLost);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GRContextAcceptsDeviceLostHandlerAndSurvivesDispose()
		{
			using var ctx = CreateSilkVkContext();
			using var extensions = new GRVkExtensions();
			extensions.Initialize((name, instance, device) => ctx.BaseGetProc(name, instance, device),
				ctx.Instance.Handle, ctx.PhysicalDevice.Handle);

			var fired = false;
			using var backend = new GRVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxAPIVersion = SilkVkContext.ApiVersion,
				Extensions = extensions,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				DeviceLost = info => { fired = true; },
			};

			using var grContext = GRContext.CreateVulkan(backend);
			Assert.NotNull(grContext);

			// Exercise a normal draw path — must not spuriously fire the device-lost callback,
			// and Dispose (via `using`) must tear the Context down before the BackendContext,
			// so the pinned delegate is still alive if Skia calls back during Context deletion.
			using var surface = SKSurface.Create(grContext, budgeted: true, new SKImageInfo(64, 64));
			Assert.NotNull(surface);
			surface.Canvas.Clear(SKColors.Purple);
			grContext.Flush(submit: true, synchronous: true);

			Assert.False(fired, "Device-lost handler fired without a lost device.");
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void SKGraphiteContextAcceptsDeviceLostHandlerAndSurvivesDispose()
		{
			using var ctx = CreateSilkVkContext();

			var fired = false;
			using var backend = new SKGraphiteVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxApiVersion = SilkVkContext.ApiVersion,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				DeviceLost = info => { fired = true; },
			};

			using var graphiteContext = SKGraphiteContext.CreateVulkan(backend);
			Assert.NotNull(graphiteContext);

			using var recorder = graphiteContext.CreateRecorder();
			using var surface = SKSurface.Create(recorder, new SKImageInfo(64, 64), false, null);
			Assert.NotNull(surface);
			surface.Canvas.Clear(SKColors.Cyan);
			using var recording = recorder.Snap();
			Assert.NotNull(recording);
			Assert.Equal(SKGraphiteInsertStatus.Success, graphiteContext.InsertRecording(recording));
			Assert.True(graphiteContext.Submit(new SKGraphiteSubmitInfo { Sync = true }));

			Assert.False(fired, "Device-lost handler fired without a lost device.");
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void ReplacingDeviceLostReleasesPreviousBridge()
		{
			using var backend = new GRVkBackendContext();

			// Assign, replace, replace, null — no leak assertion possible from managed code,
			// but if either the native bridge delete or the GCHandle.Free were skipped we'd
			// crash on the second replacement (double-free of the old bridge would corrupt
			// libc's heap; a leaked GCHandle would prevent the delegate from being collected
			// but not fail the test). This just proves the setter tolerates being written
			// several times.
			for (var i = 0; i < 4; i++) {
				backend.DeviceLost = info => { };
			}
			backend.DeviceLost = null;
			Assert.Null(backend.DeviceLost);
		}
	}
}
