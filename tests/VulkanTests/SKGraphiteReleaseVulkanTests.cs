using System;
using System.Linq;
using System.Threading.Tasks;
using SharpVk;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Graphite release-callback tests over Vulkan, for the Linux and Windows
	/// desktop hosts. Shares the SharpVk bring-up used by
	/// <c>GraphiteVulkanRenderer</c>, then lets Skia allocate the wrappable backend
	/// texture via <see cref="SKGraphiteRecorder.CreateBackendTexture"/> (no manual
	/// VkImage). Runs on CI wherever a Vulkan device — real or a software ICD — is
	/// present; skips cleanly otherwise.
	/// </summary>
	public sealed class SKGraphiteReleaseVulkanTests : SKGraphiteReleaseTestsBase
	{
		// VK_FORMAT_R8G8B8A8_UNORM matches SKColorType.Rgba8888.
		private const int VK_FORMAT_R8G8B8A8_UNORM = 37;
		private const int VK_IMAGE_TILING_OPTIMAL = 0;
		private const int VK_SHARING_MODE_EXCLUSIVE = 0;
		private const uint VK_IMAGE_ASPECT_COLOR_BIT = 0x00000001;

		// VkImageUsageFlags: TRANSFER_SRC | TRANSFER_DST | SAMPLED | COLOR_ATTACHMENT.
		private const uint ImageUsage = 0x1 | 0x2 | 0x4 | 0x10;

		protected override SKColorType ColorType => SKColorType.Rgba8888;

		protected override string UnsupportedReason =>
			TestConfig.Current.IsLinux || TestConfig.Current.IsWindows
				? null
				: "Vulkan is wired up for the Linux and Windows desktop hosts.";

		protected override Task<GraphiteReleaseHarness> CreateHarnessAsync() =>
			Task.FromResult(CreateHarness());

		private GraphiteReleaseHarness CreateHarness()
		{
			Instance instance;
			try
			{
				instance = Instance.Create(null, null);
			}
			catch (Exception ex) when (ex is not EntryPointNotFoundException and not MissingMethodException)
			{
				Assert.Skip($"Unable to create a Vulkan instance on this host: {ex.Message}");
				throw; // unreachable
			}

			try
			{
				var physicalDevice = instance.EnumeratePhysicalDevices().FirstOrDefault();
				if (physicalDevice is null)
				{
					instance.Dispose();
					Assert.Skip("No Vulkan physical device was found (no driver or software ICD installed).");
				}

				var graphicsFamily = FindGraphicsFamily(physicalDevice);
				var device = physicalDevice.CreateDevice(new[]
				{
					new DeviceQueueCreateInfo { QueueFamilyIndex = graphicsFamily, QueuePriorities = new[] { 1f } },
				}, null, null);
				var queue = device.GetQueue(graphicsFamily, 0);

				var localInstance = instance;
				var localDevice = device;
				var backendContext = new SKGraphiteVkBackendContext
				{
					VkInstance = (IntPtr)instance.RawHandle.ToUInt64(),
					VkPhysicalDevice = (IntPtr)physicalDevice.RawHandle.ToUInt64(),
					VkDevice = (IntPtr)device.RawHandle.ToUInt64(),
					VkQueue = (IntPtr)queue.RawHandle.ToUInt64(),
					GraphicsQueueIndex = graphicsFamily,
					GetProcedureAddress = (name, inst, dev) =>
						dev != IntPtr.Zero ? localDevice.GetProcedureAddress(name)
						: inst != IntPtr.Zero ? localInstance.GetProcedureAddress(name)
						: localInstance.GetProcedureAddress(name),
				};

				var context = SKGraphiteContext.CreateVulkan(backendContext)
					?? throw new InvalidOperationException("SKGraphiteContext.CreateVulkan returned null.");
				var recorder = context.CreateRecorder()
					?? throw new InvalidOperationException("SKGraphiteContext.CreateRecorder returned null.");

				return new VulkanHarness(instance, device, backendContext, context, recorder);
			}
			catch
			{
				instance.Dispose();
				throw;
			}
		}

		private static uint FindGraphicsFamily(PhysicalDevice physicalDevice)
		{
			var families = physicalDevice.GetQueueFamilyProperties();
			for (uint i = 0; i < families.Length; i++)
			{
				if (families[i].QueueFlags.HasFlag(QueueFlags.Graphics))
					return i;
			}

			throw new InvalidOperationException("This Vulkan device exposes no graphics queue family.");
		}

		private sealed class VulkanHarness : GraphiteReleaseHarness
		{
			private readonly Instance instance;
			private readonly Device device;
			private readonly SKGraphiteVkBackendContext backendContext;
			private readonly SKGraphiteContext context;
			private readonly SKGraphiteRecorder recorder;

			public VulkanHarness(Instance instance, Device device, SKGraphiteVkBackendContext backendContext, SKGraphiteContext context, SKGraphiteRecorder recorder)
			{
				this.instance = instance;
				this.device = device;
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
				device.Dispose();
				instance.Dispose();
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
