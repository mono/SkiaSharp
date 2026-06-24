using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over Apple Metal (macOS desktop host). Builds a
	/// <see cref="GRMtlBackendContext"/> from the system default
	/// <c>MTLDevice</c> and a fresh <c>MTLCommandQueue</c> via direct P/Invoke
	/// into Metal.framework and libobjc — the same vehicle the Graphite PR's
	/// Metal renderers use, so the bring-up pattern is shared.
	///
	/// <para>
	/// This file lives under <c>Renderers/Desktop/</c> and is compiled only into
	/// the desktop host (Console). On iOS / Mac Catalyst the MAUI device host
	/// supplies a Metal device through <see cref="TestConfig"/> instead (a P2
	/// follow-up); this desktop renderer is intentionally macOS-only.
	/// </para>
	/// </summary>
	public sealed class GaneshMetalRenderer : IRenderer
	{
		public string Name => "ganesh-metal";

		public bool IsAvailable => UnavailableReason is null;

		public string UnavailableReason =>
			TestConfig.Current.IsMac
				? null
				: "Metal is only available on Apple platforms.";

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
