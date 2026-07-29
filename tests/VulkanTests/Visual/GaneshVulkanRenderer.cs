using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over Vulkan for the desktop and Android hosts. Brings
	/// Vulkan up through <see cref="SilkVkContext"/> — the maintained,
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
	/// <see cref="GpuPolicy"/> decides where Vulkan is required. On a host where it
	/// is, a missing ICD is a <b>failure</b>, not a skip: CI provisions a software
	/// ICD (Mesa lavapipe on Linux, SwiftShader on Windows) precisely so this
	/// renders, and a silent skip would let that provisioning rot unnoticed.
	/// </para>
	/// </summary>
	public sealed class GaneshVulkanRenderer : IRenderer
	{
		public string Name => "ganesh-vulkan";

		public GpuBackend Backend => GpuBackend.GaneshVulkan;

		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			SilkVkContext ctx = null;
			try
			{
				ctx = new SilkVkContext();

				using var extensions = new GRVkExtensions();
				extensions.Initialize(ctx.GetProc, ctx.Instance, ctx.PhysicalDevice);

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

		public void Dispose()
		{
		}
	}
}
