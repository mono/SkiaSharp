using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Graphite GPU backend over Vulkan. Same Lavapipe target as
	/// <see cref="GaneshVulkanRenderer"/>; different Skia pipeline (recorder →
	/// recording → context.InsertRecording → context.Submit).
	/// </summary>
	public sealed class GraphiteVulkanRenderer : IRenderer
	{
		public string Name => "graphite-vulkan";
		public RendererCapabilities Caps => RendererCapabilities.Gpu;

		public bool IsAvailable => VulkanLoader.Shared.IsAvailable
			&& SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan);

		public string UnavailableReason
		{
			get {
				if (!SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan))
					return "SK_GRAPHITE/SK_VULKAN not built into libSkiaSharp";
				if (!VulkanLoader.Shared.IsAvailable)
					return $"Vulkan loader unavailable: {VulkanLoader.Shared.FailureReason}";
				return null;
			}
		}

		public Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (UnavailableReason);
			ct.ThrowIfCancellationRequested ();

			var vk = VulkanLoader.Shared;
			SKGraphiteContext ctx;
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
				ctx = SKGraphiteContext.CreateVulkan (bc)
					?? throw new InvalidOperationException ("SKGraphiteContext.CreateVulkan returned null");
			}
			using (ctx)
			using (var recorder = ctx.CreateRecorder ()
				?? throw new InvalidOperationException ("SKGraphiteContext.CreateRecorder returned null"))
			using (var surface = SKSurface.Create (recorder, info)
				?? throw new InvalidOperationException ("SKSurface.Create returned null on Graphite/Vulkan")) {

				scene.Draw (surface.Canvas);

				using (var recording = recorder.Snap ()
					?? throw new InvalidOperationException ("Recorder.Snap returned null — no draws were recorded")) {
					var status = ctx.InsertRecording (recording);
					if (status != SKGraphiteInsertStatus.Success)
						throw new InvalidOperationException ($"InsertRecording status = {status}");
				}

				var rgba = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
				var pixels = new byte[rgba.BytesSize];
				unsafe {
					fixed (byte* p = pixels) {
						if (!ctx.ReadPixels (surface, rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
							throw new InvalidOperationException ("SKGraphiteContext.ReadPixels failed");
					}
				}
				return Task.FromResult (pixels);
			}
		}

		public void Dispose () { }
	}
}
