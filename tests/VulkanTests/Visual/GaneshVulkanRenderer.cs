using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over Vulkan for the desktop hosts (Linux and Windows).
	/// Brings Vulkan up through <see cref="SilkVkContext"/> — the maintained,
	/// cross-platform Silk.NET binding — and bridges to Skia with
	/// <see cref="GRSilkNetBackendContext"/>.
	///
	/// <para>
	/// The context is fully <b>headless</b>: it creates only an
	/// <c>Instance</c> → <c>PhysicalDevice</c> → graphics <c>Queue</c> → <c>Device</c>,
	/// with no <c>VK_KHR_surface</c>/swapchain and no window — exactly the inputs
	/// <see cref="GRContext.CreateVulkan"/> needs to render to an offscreen
	/// <see cref="SKSurface"/>.
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
			TestConfig.Current.IsLinux || TestConfig.Current.IsWindows || TestConfig.Current.IsAndroid
				? null
				: "Vulkan is wired up for the Linux, Windows, and Android hosts.";

		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!IsAvailable)
				throw new RendererUnavailableException(UnavailableReason);

			lock (GpuRenderGate.Sync)
			{
				SilkVkContext ctx = null;
				try
				{
					ctx = CreateContextOrSkip();

					using var extensions = GRVkExtensionsSilkNetExtensions.Create(ctx.GetProc, ctx.Instance, ctx.PhysicalDevice);

					using var backendContext = new GRSilkNetBackendContext
					{
						VkInstance = ctx.Instance,
						VkPhysicalDevice = ctx.PhysicalDevice,
						VkDevice = ctx.Device,
						VkQueue = ctx.GraphicsQueue,
						GraphicsQueueIndex = ctx.GraphicsFamily,
						MaxAPIVersion = SilkVkContext.ApiVersion,
						Extensions = extensions,
						GetProcedureAddress = ctx.GetProc,
						VkPhysicalDeviceFeatures = ctx.Features,
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
					ctx?.Dispose();
				}
			}
		}

		public void Dispose()
		{
		}

		// Distinguishes "Vulkan genuinely absent on this host" (legit skip) from a
		// broken binding (real failure). A missing native entry point or method is
		// a regression and MUST fail; an absent driver / ICD is an honest skip.
		private static SilkVkContext CreateContextOrSkip()
		{
			try
			{
				return new SilkVkContext();
			}
			catch (Exception ex) when (ex is not EntryPointNotFoundException and not MissingMethodException)
			{
				throw new RendererUnavailableException(
					$"Unable to create a Vulkan context on this host: {ex.Message}", ex);
			}
		}
	}
}
