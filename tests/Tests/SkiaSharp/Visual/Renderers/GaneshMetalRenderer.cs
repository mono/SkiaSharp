using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over Apple Metal. macOS / iOS / tvOS only.
	///
	/// <para>
	/// Builds an <see cref="GRMtlBackendContext"/> from the system default
	/// <c>MTLDevice</c> + a fresh <c>MTLCommandQueue</c> via direct P/Invoke
	/// into Metal.framework and libobjc — same pattern as
	/// <see cref="GraphiteMetalRenderer"/>. Per-render context creation; no
	/// long-lived GR state.
	/// </para>
	/// </summary>
	public sealed class GaneshMetalRenderer : IRenderer
	{
		public string Name => "ganesh-metal";
		public RendererCapabilities Caps => RendererCapabilities.Gpu;

		public bool IsAvailable => UnavailableReason == null;

		public string UnavailableReason
		{
			get {
				if (!RuntimeInformation.IsOSPlatform (OSPlatform.OSX)
					&& !RuntimeInformation.IsOSPlatform (OSPlatform.Create ("IOS"))
					&& !RuntimeInformation.IsOSPlatform (OSPlatform.Create ("TVOS")))
					return "Metal is only available on Apple platforms";
				return null;
			}
		}

		public Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (UnavailableReason);
			ct.ThrowIfCancellationRequested ();

			IntPtr device = IntPtr.Zero, queue = IntPtr.Zero;
			try {
				device = MTLCreateSystemDefaultDevice ();
				if (device == IntPtr.Zero)
					throw new InvalidOperationException ("MTLCreateSystemDefaultDevice returned null");

				queue = ObjcSendVoid (device, "newCommandQueue");
				if (queue == IntPtr.Zero)
					throw new InvalidOperationException ("[MTLDevice newCommandQueue] returned null");

				using var bc = new GRMtlBackendContext { DeviceHandle = device, QueueHandle = queue };
				using var ctx = GRContext.CreateMetal (bc)
					?? throw new InvalidOperationException ("GRContext.CreateMetal returned null");
				using var surface = SKSurface.Create (ctx, budgeted: true, info)
					?? throw new InvalidOperationException ("SKSurface.Create returned null on Ganesh/Metal");

				scene.Draw (surface.Canvas);
				ctx.Flush ();
				ctx.Submit (synchronous: true);

				var rgba = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
				var pixels = new byte[rgba.BytesSize];
				unsafe {
					fixed (byte* p = pixels) {
						if (!surface.ReadPixels (rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
							throw new InvalidOperationException ("SKSurface.ReadPixels failed on Ganesh/Metal");
					}
				}
				return Task.FromResult (pixels);
			} finally {
				if (queue  != IntPtr.Zero) ObjcRelease (queue);
				if (device != IntPtr.Zero) ObjcRelease (device);
			}
		}

		public void Dispose () { }

		// ---- Apple-only P/Invoke (DllNotFoundException on non-Apple hosts
		// surfaces via IsAvailable's OS probe above) ----

		[DllImport ("/System/Library/Frameworks/Metal.framework/Metal")]
		private static extern IntPtr MTLCreateSystemDefaultDevice ();

		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_void (IntPtr receiver, IntPtr selector);

		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_void_void (IntPtr receiver, IntPtr selector);

		[DllImport ("/usr/lib/libobjc.dylib", CharSet = CharSet.Ansi)]
		private static extern IntPtr sel_registerName (string name);

		private static IntPtr ObjcSendVoid (IntPtr obj, string selectorName) =>
			objc_msgSend_void (obj, sel_registerName (selectorName));

		private static void ObjcRelease (IntPtr obj) =>
			objc_msgSend_void_void (obj, sel_registerName ("release"));
	}
}
