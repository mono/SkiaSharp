using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpVk;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over Vulkan for the desktop hosts (Linux and Windows).
	/// Reuses the existing SharpVk vehicle — the same managed Vulkan binding and
	/// <see cref="GRSharpVkBackendContext"/> bridge that <c>SkiaSharp.Vulkan.Tests</c>
	/// already exercises — rather than reinventing a loader.
	///
	/// <para>
	/// The context is fully <b>headless</b>: it creates only an
	/// <c>Instance</c> → <c>PhysicalDevice</c> → graphics <c>Queue</c> → <c>Device</c>,
	/// with no <c>VK_KHR_surface</c>/swapchain and no window — exactly the inputs
	/// <see cref="GRContext.CreateVulkan"/> needs to render to an offscreen
	/// <see cref="SKSurface"/>. This file lives under <c>Renderers/Desktop/</c> and
	/// is compiled only into the desktop host (Console), so the SharpVk dependency
	/// never reaches the MAUI device or WASM builds. Android Vulkan is a separate
	/// device-host renderer.
	/// </para>
	///
	/// <para>
	/// On a host without a Vulkan ICD (the default macOS agent, or a Linux/Windows
	/// agent missing a driver / software ICD such as Lavapipe) instance creation or
	/// device enumeration fails and the cell <b>skips</b> with a reason. A missing
	/// native entry point — a broken binding rather than an absent driver — is
	/// rethrown so it fails.
	/// </para>
	/// </summary>
	public sealed class GaneshVulkanRenderer : IRenderer
	{
		public string Name => "ganesh-vulkan";

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

					// SharpVk exposes the static "instance" functions on the
					// Instance, so fall back to it when no device/instance is
					// supplied — the same shim Win32VkContext uses.
					var localInstance = instance;
					GRSharpVkGetProcedureAddressDelegate getProc = (name, inst, dev) =>
					{
						if (dev != null)
							return dev.GetProcedureAddress(name);
						if (inst != null)
							return inst.GetProcedureAddress(name);
						return localInstance.GetProcedureAddress(name);
					};

					using var backendContext = new GRSharpVkBackendContext
					{
						VkInstance = instance,
						VkPhysicalDevice = physicalDevice,
						VkDevice = device,
						VkQueue = queue,
						GraphicsQueueIndex = graphicsFamily,
						GetProcedureAddress = getProc,
						VkPhysicalDeviceFeatures = physicalDevice.GetFeatures(),
					};

					using var grContext = GRContext.CreateVulkan(backendContext)
						?? throw new InvalidOperationException("GRContext.CreateVulkan returned null.");
					using var surface = SKSurface.Create(grContext, budgeted: true, info)
						?? throw new InvalidOperationException("SKSurface.Create returned null on Ganesh/Vulkan.");

					scene.Draw(surface.Canvas);
					grContext.Flush(submit: true, synchronous: true);

					return Task.FromResult(RendererPixels.ReadRgba(surface, info));
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
