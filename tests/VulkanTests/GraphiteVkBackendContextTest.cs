using System;
using SkiaSharp.Tests.Visual;
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

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteDeferredRecordingReplaysIntoTargetSurface()
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
			using var recorder = graphiteContext.CreateRecorder();
			Assert.NotNull(recorder);

			var info = new SKImageInfo(32, 32, SKColorType.Rgba8888, SKAlphaType.Premul);
			var vkInfo = new SKGraphiteVkTextureInfo
			{
				SampleCount = 1,
				Format = 37,
				ImageTiling = 0,
				ImageUsageFlags = 0x1 | 0x2 | 0x4 | 0x10 | 0x80,
				SharingMode = 0,
				AspectMask = 0x1,
			};
			using var textureInfo = SKGraphiteTextureInfo.CreateVulkan(vkInfo);
			Assert.NotNull(textureInfo);
			using var backendTexture = recorder.CreateBackendTexture(
				info.Width,
				info.Height,
				textureInfo);
			Assert.NotNull(backendTexture);
			using var targetTextureInfo = backendTexture.TextureInfo;
			Assert.NotNull(targetTextureInfo);
			Assert.Equal(SKGraphiteBackend.Vulkan, targetTextureInfo.Backend);
			Assert.Equal(1, targetTextureInfo.SampleCount);
			Assert.False(targetTextureInfo.Mipmapped);
			using var surface = SKSurface.Create(
				recorder,
				backendTexture,
				info.ColorType);
			Assert.NotNull(surface);

			Assert.Throws<ArgumentOutOfRangeException>(
				() => recorder.CreateDeferredCanvas(default, targetTextureInfo));
			var canvas = recorder.CreateDeferredCanvas(info, targetTextureInfo);
			Assert.NotNull(canvas);
			canvas.Clear(SKColors.Red);
			using var recording = recorder.Snap();
			Assert.NotNull(recording);

			using (var state = SKGraphiteMutableTextureState.CreateVulkan(
				1000001002,
				ctx.GraphicsFamily))
			{
				var invalidOptions = new SKGraphiteInsertRecordingOptions
				{
					TargetTextureState = state,
				};
				var threw = false;
				try
				{
					graphiteContext.InsertRecording(recording, invalidOptions);
				}
				catch (ArgumentException)
				{
					threw = true;
				}
				Assert.True(threw);
			}

			var finished = false;
			var succeeded = false;
			var options = new SKGraphiteInsertRecordingOptions
			{
				TargetSurface = surface,
				Finished = success =>
				{
					finished = true;
					succeeded = success;
				},
			};
			Assert.Equal(
				SKGraphiteInsertStatus.Success,
				graphiteContext.InsertRecording(recording, options));
			Assert.True(graphiteContext.Submit(new SKGraphiteSubmitInfo { Sync = true }));
			for (var i = 0; i < 100 && !finished; i++)
				graphiteContext.CheckAsyncWorkCompletion();
			Assert.True(finished);
			Assert.True(succeeded);

			var pixels = RendererPixels.ReadRgbaGraphite(
				graphiteContext,
				surface,
				info);
			Assert.Equal(255, pixels[0]);
			Assert.Equal(0, pixels[1]);
			Assert.Equal(0, pixels[2]);
			Assert.Equal(255, pixels[3]);

			surface.Dispose();
			recorder.DeleteBackendTexture(backendTexture);
		}

		[Fact]
		public void GraphiteVulkanPresentationWrappersValidateHandles()
		{
			GpuPolicy.RequireOrSkip(GpuBackends.GraphiteVulkan);

			Assert.Throws<ArgumentOutOfRangeException>(
				() => SKGraphiteBackendSemaphore.CreateVulkan(0));

			using var semaphore = SKGraphiteBackendSemaphore.CreateVulkan(1);
			Assert.NotNull(semaphore);
			using var state = SKGraphiteMutableTextureState.CreateVulkan(
				1000001002,
				0);
			Assert.NotNull(state);
		}
	}
}
