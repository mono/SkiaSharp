using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over Vulkan. Targets Mesa Lavapipe (software ICD)
	/// for deterministic, headless rendering. GPU resources allocated lazily
	/// in <see cref="RenderAsync"/> — never during construction or discovery.
	/// </summary>
	public sealed class GaneshVulkanRenderer : IRenderer
	{
		public string Name => "ganesh-vulkan";
		public RendererCapabilities Caps => RendererCapabilities.Gpu;

		public bool IsAvailable => VulkanLoader.Shared.IsAvailable;
		public string UnavailableReason => VulkanLoader.Shared.IsAvailable
			? null
			: $"Vulkan loader unavailable: {VulkanLoader.Shared.FailureReason}";

		public Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (UnavailableReason);
			ct.ThrowIfCancellationRequested ();

			var vk = VulkanLoader.Shared;
			using var bc = new GRVkBackendContext {
				VkInstance         = vk.Instance,
				VkPhysicalDevice   = vk.PhysicalDevice,
				VkDevice           = vk.Device,
				VkQueue            = vk.Queue,
				GraphicsQueueIndex = vk.QueueFamilyIndex,
				MaxAPIVersion      = VulkanLoader.VK_API_VERSION_1_3,
				ProtectedContext   = false,
				GetProcedureAddress = vk.GetProc,
			};
			using var ctx = GRContext.CreateVulkan (bc)
				?? throw new InvalidOperationException ("GRContext.CreateVulkan returned null");
			using var surface = SKSurface.Create (ctx, budgeted: true, info)
				?? throw new InvalidOperationException ("SKSurface.Create returned null on Ganesh/Vulkan");

			scene.Draw (surface.Canvas);
			ctx.Flush ();
			ctx.Submit (synchronous: true);

			var rgba = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = new byte[rgba.BytesSize];
			unsafe {
				fixed (byte* p = pixels) {
					if (!surface.ReadPixels (rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
						throw new InvalidOperationException ("SKSurface.ReadPixels failed on Ganesh/Vulkan");
				}
			}
			return Task.FromResult (pixels);
		}

		public void Dispose () { }
	}
}
