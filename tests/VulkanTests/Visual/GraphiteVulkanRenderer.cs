using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Graphite GPU backend over Vulkan for the desktop and Android hosts. Brings
	/// Vulkan up through <see cref="SilkVkContext"/> — the maintained,
	/// cross-platform Silk.NET binding — and feeds the raw Vulkan handles to
	/// <see cref="SKGraphiteContext.CreateVulkan"/> via the binding-neutral
	/// <see cref="SKGraphiteVkBackendContext"/> (no SharpVk, no Graphite-specific
	/// wrapper type). It is the Graphite analogue of
	/// <see cref="GaneshVulkanRenderer"/> and, like it, is fully headless —
	/// Instance → PhysicalDevice → graphics Queue → Device with no surface/swapchain.
	///
	/// </summary>
	public sealed class GraphiteVulkanRenderer : IRenderer
	{
		public string Name => "graphite-vulkan";

		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// GPU work is serialized by the VulkanGpuRenderingCollection the driving
			// test class joins (xUnit DisableParallelization), so no in-renderer lock.
			SilkVkContext ctx = null;
			try
			{
				ctx = new SilkVkContext();

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

				using var context = SKGraphiteContext.CreateVulkan(backendContext)
					?? throw new InvalidOperationException("SKGraphiteContext.CreateVulkan returned null.");
				using var recorder = context.CreateRecorder()
					?? throw new InvalidOperationException("SKGraphiteContext.CreateRecorder returned null.");
				using var surface = SKSurface.Create(recorder, info)
					?? throw new InvalidOperationException("SKSurface.Create returned null on Graphite/Vulkan.");

				scene.Draw(surface.Canvas);

				using var recording = recorder.Snap()
					?? throw new InvalidOperationException("Recorder.Snap() returned null.");
				if (context.InsertRecording(recording) != SKGraphiteInsertStatus.Success)
					throw new InvalidOperationException("InsertRecording did not report Success.");
				if (!context.Submit(new SKGraphiteSubmitInfo { Sync = true }))
					throw new InvalidOperationException("Submit(Sync=true) returned false.");

				// Graphite surfaces don't support synchronous SKSurface.ReadPixels in shipping
				// builds; read back through the async rescale-and-read path instead.
				return Task.FromResult(RendererPixels.ReadRgbaGraphite(context, surface, info));
			}
			finally
			{
				ctx?.Dispose();
			}
		}

		public void Dispose()
		{
		}
	}
}
