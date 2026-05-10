using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Graphite GPU backend over Apple Metal. Available only on macOS / iOS /
	/// tvOS. The setup creates the system default <c>MTLDevice</c> + a fresh
	/// <c>MTLCommandQueue</c> via direct P/Invoke into Metal.framework + the
	/// Objective-C runtime (<c>libobjc</c>); both are scoped to the surface's
	/// lifetime so visual tests stay isolated.
	///
	/// Per-surface SKGraphiteContext+Recorder allocation (same policy as the
	/// Vulkan setups) — keeps the assembly-end leak check happy.
	///
	/// On non-Apple platforms <see cref="IsAvailable"/> reports false and the
	/// test framework skips visual tests for this setup.
	/// </summary>
	public sealed class MetalSetup : VisualSetup
	{
		private readonly string failureReason;

		public override string Name              => "metal";
		public override bool   IsAvailable       => failureReason == null;
		public override string UnavailableReason => failureReason;

		public MetalSetup ()
		{
			if (!OperatingSystem.IsMacOS () && !OperatingSystem.IsIOS () && !OperatingSystem.IsTvOS ()) {
				failureReason = "Metal is only available on Apple platforms";
				return;
			}
			if (!SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Metal)) {
				failureReason = "SK_GRAPHITE/SK_METAL not built into libSkiaSharp (rebuild macOS native with SUPPORT_GRAPHITE=true)";
				return;
			}
			// Cheap check: try to create the default device. If the host has
			// no Metal-capable GPU (rare on modern macOS) this surfaces here.
			try {
				var probeDevice = MTLCreateSystemDefaultDevice ();
				if (probeDevice == IntPtr.Zero) {
					failureReason = "MTLCreateSystemDefaultDevice returned null";
					return;
				}
				ObjcRelease (probeDevice);
			} catch (DllNotFoundException ex) {
				failureReason = $"Metal/libobjc not loadable: {ex.Message}";
			}
		}

		public override VisualSurface CreateSurface (SKImageInfo info)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (failureReason);
			return new MetalSurface (info);
		}

		private sealed unsafe class MetalSurface : VisualSurface
		{
			private IntPtr device, queue;
			private SKGraphiteMtlBackendContext bc;
			private SKGraphiteContext context;
			private SKGraphiteRecorder recorder;
			private SKSurface surface;

			public override SKImageInfo ImageInfo { get; }
			public override SKCanvas    Canvas    => surface.Canvas;

			public MetalSurface (SKImageInfo info)
			{
				ImageInfo = info;

				device = MTLCreateSystemDefaultDevice ();
				if (device == IntPtr.Zero)
					throw new InvalidOperationException ("MTLCreateSystemDefaultDevice failed");

				// queue = [device newCommandQueue]
				queue = ObjcSendVoid (device, "newCommandQueue");
				if (queue == IntPtr.Zero) {
					ObjcRelease (device); device = IntPtr.Zero;
					throw new InvalidOperationException ("[MTLDevice newCommandQueue] failed");
				}

				bc = new SKGraphiteMtlBackendContext {
					MtlDevice = device,
					MtlQueue  = queue,
				};
				context = SKGraphiteContext.CreateMetal (bc);
				if (context == null) {
					Dispose ();
					throw new InvalidOperationException ("SKGraphiteContext.CreateMetal returned null");
				}

				recorder = context.CreateRecorder ();
				if (recorder == null) {
					Dispose ();
					throw new InvalidOperationException ("SKGraphiteContext.CreateRecorder returned null");
				}

				surface = SKSurface.Create (recorder, info);
				if (surface == null) {
					Dispose ();
					throw new InvalidOperationException ("SKSurface.Create on Metal returned null");
				}
			}

			public override byte[] ReadPixels ()
			{
				using (var recording = recorder.Snap ()) {
					if (recording == null)
						throw new InvalidOperationException ("Recorder.Snap returned null");
					var status = context.InsertRecording (recording);
					if (status != SKGraphiteInsertStatus.Success)
						throw new InvalidOperationException ($"InsertRecording status = {status}");
				}

				var pixels = new byte[ImageInfo.BytesSize];
				var rgba = new SKImageInfo (ImageInfo.Width, ImageInfo.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
				fixed (byte* p = pixels) {
					if (!context.ReadPixels (surface, rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
						throw new InvalidOperationException ("Context.ReadPixels failed on Metal");
				}
				return pixels;
			}

			public override void Dispose ()
			{
				surface?.Dispose ();   surface = null;
				recorder?.Dispose ();  recorder = null;
				context?.Dispose ();   context = null;
				bc?.Dispose ();        bc = null;
				if (queue  != IntPtr.Zero) { ObjcRelease (queue);  queue  = IntPtr.Zero; }
				if (device != IntPtr.Zero) { ObjcRelease (device); device = IntPtr.Zero; }
			}
		}

		// ---- Apple-only P/Invoke ----

		// Lazy-resolved by the runtime; on Linux this DllImport will throw
		// DllNotFoundException at first use, which the constructor catches.
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
