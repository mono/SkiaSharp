using System;
using System.Threading.Tasks;
using SkiaSharp.Tests.Visual;
using SkiaSharp.Vulkan.Tests;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Graphite release-callback tests over Vulkan, for the Linux, Windows and
	/// Android hosts. Brings Vulkan up through the maintained, cross-platform
	/// <see cref="SilkVkContext"/> (Silk.NET) and feeds the raw handles to
	/// <see cref="SKGraphiteContext.CreateVulkan"/>, then lets Skia allocate the
	/// wrappable backend texture via
	/// <see cref="SKGraphiteRecorder.CreateBackendTexture"/> (no manual VkImage).
	/// Runs wherever a Vulkan device — real or a software ICD such as Lavapipe — is
	/// present; skips cleanly otherwise.
	/// </summary>
	[Collection(VulkanGpuRenderingCollection.Name)]
	public sealed class SKGraphiteReleaseVulkanTests : SKGraphiteReleaseTestsBase
	{
		// VK_FORMAT_R8G8B8A8_UNORM matches SKColorType.Rgba8888.
		private const int VK_FORMAT_R8G8B8A8_UNORM = 37;
		private const int VK_IMAGE_TILING_OPTIMAL = 0;
		private const int VK_SHARING_MODE_EXCLUSIVE = 0;
		private const uint VK_IMAGE_ASPECT_COLOR_BIT = 0x00000001;

		// VkImageUsageFlags: TRANSFER_SRC | TRANSFER_DST | SAMPLED | COLOR_ATTACHMENT
		// | INPUT_ATTACHMENT. Graphite requires a color-renderable Vulkan texture to
		// carry INPUT_ATTACHMENT usage (see VulkanCaps::getTextureUsage), so a surface
		// can wrap it; without it validate_backend_texture rejects the surface.
		private const uint ImageUsage = 0x1 | 0x2 | 0x4 | 0x10 | 0x80;

		protected override SKColorType ColorType => SKColorType.Rgba8888;

		protected override GpuBackend Backend => GpuBackend.GraphiteVulkan;

		protected override Task<GraphiteReleaseHarness> CreateHarnessAsync() =>
			Task.FromResult(CreateHarness());

		private GraphiteReleaseHarness CreateHarness()
		{
			// No catch: GpuPolicy already established that Vulkan is required here,
			// and CI provisions a software ICD so it succeeds. A failure means the
			// provisioning broke or the agent needs a declared opt-out.
			var ctx = new SilkVkContext();

			try
			{
				var backendContext = new SKGraphiteVkBackendContext
				{
					VkInstance = ctx.Instance.Handle,
					VkPhysicalDevice = ctx.PhysicalDevice.Handle,
					VkDevice = ctx.Device.Handle,
					VkQueue = ctx.GraphicsQueue.Handle,
					GraphicsQueueIndex = ctx.GraphicsFamily,
					MaxApiVersion = SilkVkContext.ApiVersion,
					GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				};

				var context = SKGraphiteContext.CreateVulkan(backendContext)
					?? throw new InvalidOperationException("SKGraphiteContext.CreateVulkan returned null.");
				var recorder = context.CreateRecorder()
					?? throw new InvalidOperationException("SKGraphiteContext.CreateRecorder returned null.");

				return new VulkanHarness(ctx, backendContext, context, recorder);
			}
			catch
			{
				ctx.Dispose();
				throw;
			}
		}

		private sealed class VulkanHarness : GraphiteReleaseHarness
		{
			private readonly SilkVkContext ctx;
			private readonly SKGraphiteVkBackendContext backendContext;
			private readonly SKGraphiteContext context;
			private readonly SKGraphiteRecorder recorder;

			public VulkanHarness(SilkVkContext ctx, SKGraphiteVkBackendContext backendContext, SKGraphiteContext context, SKGraphiteRecorder recorder)
			{
				this.ctx = ctx;
				this.backendContext = backendContext;
				this.context = context;
				this.recorder = recorder;
			}

			public override SKGraphiteContext Context => context;

			public override SKGraphiteRecorder Recorder => recorder;

			public override (SKGraphiteBackendTexture texture, IDisposable owner) CreateBackendTexture(int width, int height)
			{
				var vkInfo = new SKGraphiteVkTextureInfo
				{
					SampleCount = 1,
					Mipmapped = false,
					Flags = 0,
					Format = VK_FORMAT_R8G8B8A8_UNORM,
					ImageTiling = VK_IMAGE_TILING_OPTIMAL,
					ImageUsageFlags = ImageUsage,
					SharingMode = VK_SHARING_MODE_EXCLUSIVE,
					AspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
				};

				using var info = SKGraphiteTextureInfo.CreateVulkan(vkInfo)
					?? throw new InvalidOperationException("SKGraphiteTextureInfo.CreateVulkan returned null.");
				var backendTexture = recorder.CreateBackendTexture(width, height, info)
					?? throw new InvalidOperationException("SKGraphiteRecorder.CreateBackendTexture returned null.");

				return (backendTexture, new VulkanTextureOwner(recorder, backendTexture));
			}

			public override void Dispose()
			{
				recorder.Dispose();
				context.Dispose();
				backendContext.Dispose();
				ctx.Dispose();
			}

			private sealed class VulkanTextureOwner : IDisposable
			{
				private readonly SKGraphiteRecorder recorder;
				private readonly SKGraphiteBackendTexture backendTexture;

				public VulkanTextureOwner(SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture)
				{
					this.recorder = recorder;
					this.backendTexture = backendTexture;
				}

				public void Dispose()
				{
					// Free the Skia-allocated GPU texture while the handle is live,
					// then dispose the managed wrapper.
					recorder.DeleteBackendTexture(backendTexture);
					backendTexture.Dispose();
				}
			}
		}
	}
}
