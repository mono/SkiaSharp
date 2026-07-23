using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpVk;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Graphite GPU backend over Vulkan for the desktop hosts (Linux and Windows).
	/// Shares the SharpVk vehicle used by <see cref="GaneshVulkanRenderer"/> —
	/// same Instance → PhysicalDevice → graphics Queue → Device sequence, feeding
	/// <see cref="SKGraphiteContext.CreateVulkan"/> instead of
	/// <see cref="GRContext.CreateVulkan"/>. Compiled into the
	/// <c>SkiaSharp.Vulkan.Tests</c> satellite so the SharpVk dependency stays
	/// out of the base test assembly.
	/// </summary>
	public sealed class GraphiteVulkanRenderer : IRenderer
	{
		public string Name => "graphite-vulkan";

		public bool IsAvailable => UnavailableReason is null;

		public string UnavailableReason =>
			TestConfig.Current.IsLinux || TestConfig.Current.IsWindows
				? null
				: "Vulkan is wired up for the Linux and Windows desktop hosts.";

		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!IsAvailable)
				throw new RendererUnavailableException(UnavailableReason);

			lock (GpuRenderGate.Sync)
			{
				Instance instance = null;
				Device device = null;
				try
				{
					instance = CreateInstanceOrSkip();

					var physicalDevice = instance.EnumeratePhysicalDevices().FirstOrDefault()
						?? throw new RendererUnavailableException(
							"No Vulkan physical device was found (no driver or software ICD installed).");

					var graphicsFamily = FindGraphicsFamily(physicalDevice);

					device = physicalDevice.CreateDevice(new[]
					{
						new DeviceQueueCreateInfo { QueueFamilyIndex = graphicsFamily, QueuePriorities = new[] { 1f } },
					}, null, null);

					var queue = device.GetQueue(graphicsFamily, 0);

					// Same shim as GaneshVulkanRenderer: dispatch to device → instance →
					// SharpVk's static instance table depending on what the caller has.
					// Uses the typed SharpVk backend context, so the delegate receives
					// SharpVk Instance/Device objects directly.
					var localInstance = instance;
					GRSharpVkGetProcedureAddressDelegate getProc = (name, inst, dev) =>
					{
						if (dev != null)
							return dev.GetProcedureAddress(name);
						if (inst != null)
							return inst.GetProcedureAddress(name);
						return localInstance.GetProcedureAddress(name);
					};

					using var backendContext = new SKGraphiteSharpVkBackendContext
					{
						VkInstance = instance,
						VkPhysicalDevice = physicalDevice,
						VkDevice = device,
						VkQueue = queue,
						GraphicsQueueIndex = graphicsFamily,
						GetProcedureAddress = getProc,
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
					device?.Dispose();
					instance?.Dispose();
				}
			}
		}

		public void Dispose()
		{
		}

		private static Instance CreateInstanceOrSkip()
		{
			try
			{
				return Instance.Create(null, null);
			}
			catch (Exception ex) when (ex is not EntryPointNotFoundException and not MissingMethodException)
			{
				throw new RendererUnavailableException(
					$"Unable to create a Vulkan instance on this host: {ex.Message}", ex);
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

			throw new RendererUnavailableException("This Vulkan device exposes no graphics queue family.");
		}
	}
}
