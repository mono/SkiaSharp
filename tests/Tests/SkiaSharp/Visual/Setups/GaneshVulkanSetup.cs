using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over Vulkan. Targets Mesa Lavapipe (the software
	/// Vulkan ICD) so it runs in headless CI / WSL2 without a real GPU.
	///
	/// The GPU context is created PER SURFACE — visual tests are independent
	/// and the cleanup-leak-check fixture in this assembly asserts no SKObjects
	/// outlive the run. A bit of extra context-creation latency per test (~50ms
	/// on Lavapipe) buys us isolation + clean teardown.
	/// </summary>
	public sealed class GaneshVulkanSetup : VisualSetup
	{
		private readonly string failureReason;

		public override string Name              => "ganesh-vulkan";
		public override bool   IsAvailable       => failureReason == null;
		public override string UnavailableReason => failureReason;

		public GaneshVulkanSetup ()
		{
			var vk = VulkanLoader.Shared;
			if (!vk.IsAvailable)
				failureReason = $"Vulkan loader unavailable: {vk.FailureReason}";
		}

		public override VisualSurface CreateSurface (SKImageInfo info)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (failureReason);
			return new GaneshVulkanSurface (info);
		}

		private sealed class GaneshVulkanSurface : VisualSurface
		{
			private GRVkBackendContext bc;
			private GRContext          context;
			private SKSurface          surface;
			public override SKImageInfo ImageInfo { get; }
			public override SKCanvas    Canvas    => surface.Canvas;

			public GaneshVulkanSurface (SKImageInfo info)
			{
				ImageInfo = info;
				var vk = VulkanLoader.Shared;
				bc = new GRVkBackendContext {
					VkInstance         = vk.Instance,
					VkPhysicalDevice   = vk.PhysicalDevice,
					VkDevice           = vk.Device,
					VkQueue            = vk.Queue,
					GraphicsQueueIndex = vk.QueueFamilyIndex,
					MaxAPIVersion      = VulkanLoader.VK_API_VERSION_1_3,
					ProtectedContext   = false,
					GetProcedureAddress = vk.GetProc,
				};
				context = GRContext.CreateVulkan (bc);
				if (context == null) {
					bc.Dispose ();
					throw new InvalidOperationException ("GRContext.CreateVulkan returned null");
				}
				surface = SKSurface.Create (context, budgeted: true, info);
				if (surface == null) {
					context.Dispose ();
					bc.Dispose ();
					throw new InvalidOperationException ("SKSurface.Create returned null on Ganesh/Vulkan");
				}
			}

			public override byte[] ReadPixels ()
			{
				context.Flush ();
				context.Submit (synchronous: true);
				var pixels = new byte[ImageInfo.BytesSize];
				var rgba = new SKImageInfo (ImageInfo.Width, ImageInfo.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
				unsafe {
					fixed (byte* p = pixels) {
						if (!surface.ReadPixels (rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
							throw new InvalidOperationException ("SKSurface.ReadPixels failed on Ganesh/Vulkan surface");
					}
				}
				return pixels;
			}

			public override void Dispose ()
			{
				surface?.Dispose (); surface = null;
				context?.Dispose (); context = null;
				bc?.Dispose ();      bc = null;
			}
		}
	}
}
