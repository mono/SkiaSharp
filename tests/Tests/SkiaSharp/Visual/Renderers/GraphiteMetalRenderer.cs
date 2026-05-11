using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Graphite GPU backend over Apple Metal. Available on macOS / iOS / tvOS
	/// only — reports <see cref="IsAvailable"/>=false on every other platform so
	/// the matrix self-trims.
	///
	/// Metal context creation goes through the Objective-C runtime directly:
	/// <c>MTLCreateSystemDefaultDevice</c> + <c>[device newCommandQueue]</c>.
	/// No <c>CAMetalLayer</c>, no <c>MTKView</c>, no <c>NSWindow</c> — fully
	/// offscreen via Graphite's wrapped <c>MTLTexture</c>.
	/// </summary>
	public sealed class GraphiteMetalRenderer : IRenderer
	{
		public string Name => "graphite-metal";
		public RendererCapabilities Caps => RendererCapabilities.Gpu;

		public bool IsAvailable => UnavailableReason == null;

		public string UnavailableReason
		{
			get {
				if (!RuntimeInformation.IsOSPlatform (OSPlatform.OSX)
					&& !RuntimeInformation.IsOSPlatform (OSPlatform.Create ("IOS"))
					&& !RuntimeInformation.IsOSPlatform (OSPlatform.Create ("TVOS")))
					return "Metal is only available on Apple platforms";
				if (!SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Metal))
					return "SK_GRAPHITE/SK_METAL not built into libSkiaSharp (rebuild with SUPPORT_GRAPHITE=true)";
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

				using var bc = new SKGraphiteMtlBackendContext { MtlDevice = device, MtlQueue = queue };
				using var ctx = SKGraphiteContext.CreateMetal (bc)
					?? throw new InvalidOperationException ("SKGraphiteContext.CreateMetal returned null");
				using var recorder = ctx.CreateRecorder ()
					?? throw new InvalidOperationException ("CreateRecorder returned null");
				using var surface = SKSurface.Create (recorder, info)
					?? throw new InvalidOperationException ("SKSurface.Create returned null on Metal");

				scene.Draw (surface.Canvas);

				using (var recording = recorder.Snap ()
					?? throw new InvalidOperationException ("Recorder.Snap returned null")) {
					var status = ctx.InsertRecording (recording);
					if (status != SKGraphiteInsertStatus.Success)
						throw new InvalidOperationException ($"InsertRecording status = {status}");
				}

				var rgba = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
				var pixels = new byte[rgba.BytesSize];
				unsafe {
					fixed (byte* p = pixels) {
						if (!ctx.ReadPixels (surface, rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
							throw new InvalidOperationException ("Context.ReadPixels failed on Metal");
					}
				}
				return Task.FromResult (pixels);
			} finally {
				if (queue  != IntPtr.Zero) ObjcRelease (queue);
				if (device != IntPtr.Zero) ObjcRelease (device);
			}
		}

		public void Dispose () { }

		// ---- Apple-only P/Invoke (lazy: DllNotFoundException on non-Apple
		// hosts surfaces in UnavailableReason via the OS probe above) ----

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
