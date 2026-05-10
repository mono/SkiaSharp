using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Graphite GPU backend over Vulkan. Targets Mesa Lavapipe (the software
	/// Vulkan ICD) so it runs in headless CI / WSL2 without a real GPU.
	///
	/// Same per-surface context-creation policy as <see cref="GaneshVulkanSetup"/>:
	/// each <see cref="CreateSurface"/> call brings up its own
	/// SKGraphiteContext + Recorder (sharing the process-singleton VkInstance/
	/// VkDevice via <see cref="VulkanLoader.Shared"/>) and the surface owns
	/// teardown.
	/// </summary>
	public sealed class GraphiteVulkanSetup : VisualSetup
	{
		private readonly string failureReason;

		public override string Name              => "graphite-vulkan";
		public override bool   IsAvailable       => failureReason == null;
		public override string UnavailableReason => failureReason;

		public GraphiteVulkanSetup ()
		{
			if (!SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan)) {
				failureReason = "SK_GRAPHITE/SK_VULKAN not built into libSkiaSharp";
				return;
			}
			var vk = VulkanLoader.Shared;
			if (!vk.IsAvailable)
				failureReason = $"Vulkan loader unavailable: {vk.FailureReason}";
		}

		public override VisualSurface CreateSurface (SKImageInfo info)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (failureReason);
			return new GraphiteVulkanSurface (info);
		}

		private sealed class GraphiteVulkanSurface : VisualSurface
		{
			private SKGraphiteContext  context;
			private SKGraphiteRecorder recorder;
			private SKSurface          surface;
			public override SKImageInfo ImageInfo { get; }
			public override SKCanvas    Canvas    => surface.Canvas;

			public GraphiteVulkanSurface (SKImageInfo info)
			{
				ImageInfo = info;
				var vk = VulkanLoader.Shared;

				using (var bc = new SKGraphiteVkBackendContext {
					VkInstance         = vk.Instance,
					VkPhysicalDevice   = vk.PhysicalDevice,
					VkDevice           = vk.Device,
					VkQueue            = vk.Queue,
					GraphicsQueueIndex = vk.QueueFamilyIndex,
					MaxApiVersion      = VulkanLoader.VK_API_VERSION_1_3,
					ProtectedContext   = false,
					GetProcedureAddress = (name, instance, device) => vk.GetProc (name, instance, device),
				}) {
					context = SKGraphiteContext.CreateVulkan (bc);
				}
				// `bc` disposed here; ctx took ownership of the GCHandle (Variant A).

				if (context == null)
					throw new InvalidOperationException ("SKGraphiteContext.CreateVulkan returned null");

				recorder = context.CreateRecorder ();
				if (recorder == null) {
					context.Dispose ();
					throw new InvalidOperationException ("SKGraphiteContext.CreateRecorder returned null");
				}

				surface = SKSurface.Create (recorder, info);
				if (surface == null) {
					recorder.Dispose ();
					context.Dispose ();
					throw new InvalidOperationException ("SKSurface.Create(recorder, info) returned null on Graphite/Vulkan");
				}
			}

			public override byte[] ReadPixels ()
			{
				using (var recording = recorder.Snap ()) {
					if (recording == null)
						throw new InvalidOperationException ("Recorder.Snap returned null — no draws were recorded");
					var status = context.InsertRecording (recording);
					if (status != SKGraphiteInsertStatus.Success)
						throw new InvalidOperationException ($"InsertRecording status = {status}");
				}

				var pixels = new byte[ImageInfo.BytesSize];
				var rgba = new SKImageInfo (ImageInfo.Width, ImageInfo.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
				unsafe {
					fixed (byte* p = pixels) {
						if (!context.ReadPixels (surface, rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
							throw new InvalidOperationException ("SKGraphiteContext.ReadPixels failed");
					}
				}
				return pixels;
			}

			public override void Dispose ()
			{
				surface?.Dispose ();  surface = null;
				recorder?.Dispose (); recorder = null;
				context?.Dispose ();  context = null;
			}
		}
	}
}
