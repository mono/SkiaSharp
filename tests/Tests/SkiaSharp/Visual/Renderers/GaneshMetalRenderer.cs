using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over Apple Metal. Builds a
	/// <see cref="GRMtlBackendContext"/> from the system default
	/// <c>MTLDevice</c> and a fresh <c>MTLCommandQueue</c> via direct P/Invoke
	/// into Metal.framework and libobjc — the same vehicle the Graphite PR's
	/// Metal renderers use, so the bring-up pattern is shared.
	///
	/// <para>
	/// This renderer lives in the shared harness (not under
	/// <c>Renderers/Desktop/</c>), so it compiles into every host. Because Metal
	/// is reached purely through runtime P/Invoke — not a platform-TFM API — the
	/// same file runs in-process on the macOS Console host <i>and</i> on the
	/// iOS / Mac Catalyst / tvOS MAUI device hosts. It reports unavailable (and so
	/// skips) on any non-Apple platform, where the Metal/libobjc entry points are
	/// never touched.
	/// </para>
	/// </summary>
	public sealed class GaneshMetalRenderer : IRenderer
	{
		public string Name => "ganesh-metal";

		public bool IsAvailable => UnavailableReason is null;

		public string UnavailableReason =>
			IsApplePlatform
				? null
				: "Metal is only available on Apple platforms (macOS, iOS, Mac Catalyst, tvOS).";

		private static bool IsApplePlatform =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsMacOS()
			|| OperatingSystem.IsIOS()
			|| OperatingSystem.IsMacCatalyst()
			|| OperatingSystem.IsTvOS();
#else
			// net48 (Windows-only TFM) predates the OperatingSystem.Is* probes and
			// can never be an Apple platform, so fall back to the TestConfig flag.
			TestConfig.Current.IsMac;
#endif


		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!IsAvailable)
				throw new RendererUnavailableException(UnavailableReason);

			lock (GpuRenderGate.Sync)
			{
				var device = IntPtr.Zero;
				var queue = IntPtr.Zero;
				try
				{
					device = MTLCreateSystemDefaultDevice();
					if (device == IntPtr.Zero)
						throw new RendererUnavailableException("MTLCreateSystemDefaultDevice returned null; no Metal device on this host.");

					queue = ObjcSendVoid(device, "newCommandQueue");
					if (queue == IntPtr.Zero)
						throw new InvalidOperationException("[MTLDevice newCommandQueue] returned null.");

					using var backendContext = new GRMtlBackendContext { DeviceHandle = device, QueueHandle = queue };
					using var grContext = GRContext.CreateMetal(backendContext)
						?? throw new InvalidOperationException("GRContext.CreateMetal returned null.");
					using var surface = SKSurface.Create(grContext, budgeted: true, info)
						?? throw new InvalidOperationException("SKSurface.Create returned null on Ganesh/Metal.");

					scene.Draw(surface.Canvas);
					grContext.Flush(submit: true, synchronous: true);

					return Task.FromResult(RendererPixels.ReadRgba(surface, info));
				}
				finally
				{
					if (queue != IntPtr.Zero)
						ObjcRelease(queue);
					if (device != IntPtr.Zero)
						ObjcRelease(device);
				}
			}
		}

		public void Dispose()
		{
		}

		[DllImport("/System/Library/Frameworks/Metal.framework/Metal")]
		private static extern IntPtr MTLCreateSystemDefaultDevice();

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_retIntPtr(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_retVoid(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.dylib", CharSet = CharSet.Ansi)]
		private static extern IntPtr sel_registerName(string name);

		private static IntPtr ObjcSendVoid(IntPtr obj, string selectorName) =>
			objc_msgSend_retIntPtr(obj, sel_registerName(selectorName));

		private static void ObjcRelease(IntPtr obj) =>
			objc_msgSend_retVoid(obj, sel_registerName("release"));
	}
}
