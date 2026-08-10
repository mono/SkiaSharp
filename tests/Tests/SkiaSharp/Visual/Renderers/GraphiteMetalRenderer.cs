using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Graphite GPU backend over Apple Metal. Uses the same Metal.framework
	/// + libobjc runtime bring-up as <see cref="GaneshMetalRenderer"/> — system
	/// default MTLDevice + a fresh MTLCommandQueue — feeding
	/// <see cref="SKGraphiteContext.CreateMetal"/> instead of the Ganesh factory.
	/// </summary>
	public sealed class GraphiteMetalRenderer : IRenderer
	{
		public string Name => GpuBackends.GraphiteMetal;

		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// GPU work is serialized by the GpuRenderingCollection the driving test
			// class joins (xUnit DisableParallelization), so no in-renderer lock.
			var device = IntPtr.Zero;
			var queue = IntPtr.Zero;
			try
			{
				device = MTLCreateSystemDefaultDevice();
				if (device == IntPtr.Zero)
					throw new InvalidOperationException(
						"MTLCreateSystemDefaultDevice returned null; no Metal device on this host.");

				// Probe the device BEFORE allocating a command queue. Skia's Graphite
				// Metal init walks MTLGPUFamilyApple9..7 and Mac2 and SK_ABORTs the
				// process if none is supported; the newCommandQueue call itself is
				// also what leaves the dispatch-queue state that hangs shutdown on
				// virtualized Metal, so we never reach it if the probe fails.
				if (!MetalCanDriveGraphite(device))
					throw new InvalidOperationException(
						"MTLDevice does not support any MTLGPUFamily that Skia Graphite requires " +
						"(Apple7+, Mac2). This is usually a virtualized/software Metal driver.");

				queue = ObjcSendVoid(device, "newCommandQueue");
				if (queue == IntPtr.Zero)
					throw new InvalidOperationException("[MTLDevice newCommandQueue] returned null.");

				var backendContext = new SKGraphiteMtlBackendContext { MtlDevice = device, MtlQueue = queue };
				using var context = SKGraphiteContext.CreateMetal(backendContext)
					?? throw new InvalidOperationException("SKGraphiteContext.CreateMetal returned null.");
				using var recorder = context.CreateRecorder()
					?? throw new InvalidOperationException("SKGraphiteContext.CreateRecorder returned null.");
				using var surface = SKSurface.Create(recorder, info)
					?? throw new InvalidOperationException("SKSurface.Create returned null on Graphite/Metal.");

				scene.Draw(surface.Canvas);

				var recording = recorder.Snap();
				if (recording is null)
				{
					// The Apple simulator's Metal cannot compile every pipeline Graphite
					// emits — notably the gradient-shader pipeline (GradientBlend) — so
					// Snap() returns null there for those scenes.
					throw new InvalidOperationException(
						"Graphite/Metal Recorder.Snap() returned null" +
						(IsRunningOnAppleSimulator
							? " on the Apple simulator, whose Metal cannot compile some pipelines Graphite " +
							  "emits (e.g. gradient shaders). Ganesh/Metal renders this scene on the " +
							  "simulator, and Graphite/Metal renders it on macOS and real devices."
							: "."));
				}

				using (recording)
				{
					if (context.InsertRecording(recording) != SKGraphiteInsertStatus.Success)
						throw new InvalidOperationException("InsertRecording did not report Success.");
					if (!context.Submit(new SKGraphiteSubmitInfo { Sync = true }))
						throw new InvalidOperationException("Submit(Sync=true) returned false.");

					// Graphite surfaces don't support synchronous SKSurface.ReadPixels in shipping
					// builds; read back through the async rescale-and-read path instead.
					return Task.FromResult(RendererPixels.ReadRgbaGraphite(context, surface, info));
				}
			}
			finally
			{
				if (queue != IntPtr.Zero)
					ObjcRelease(queue);
				if (device != IntPtr.Zero)
					ObjcRelease(device);
			}
		}

		public void Dispose()
		{
		}

		// MTLGPUFamily values from Metal.framework (see MTLDevice.h). Skia's
		// Graphite backend needs any one of Apple7+, or Mac2. If the device
		// advertises Metal but none of these, CreateMetal SK_ABORTs.
		private const ulong MTLGPUFamilyApple7 = 1007;
		private const ulong MTLGPUFamilyApple8 = 1008;
		private const ulong MTLGPUFamilyApple9 = 1009;
		private const ulong MTLGPUFamilyMac2   = 2002;

		private static bool MetalHasGraphiteCapableFamily(IntPtr device)
		{
			var sel = sel_registerName("supportsFamily:");
			foreach (var f in new[] { MTLGPUFamilyApple9, MTLGPUFamilyApple8, MTLGPUFamilyApple7, MTLGPUFamilyMac2 })
			{
				if (objc_msgSend_supportsFamily(device, sel, f) != 0)
					return true;
			}
			return false;
		}

		// Whether this MTLDevice can drive Skia Graphite. Real hardware must
		// advertise Apple7+ or Mac2 (below that, Skia's Metal init SK_ABORTs). The
		// Apple simulator under-reports its GPU family (typically Apple1/Apple2/
		// Common1) yet is backed by the host Apple Silicon GPU and drives Graphite
		// Metal correctly, so it is whitelisted.
		private static bool MetalCanDriveGraphite(IntPtr device) =>
			MetalHasGraphiteCapableFamily(device) || IsRunningOnAppleSimulator;

		// The iOS/tvOS simulator sets SIMULATOR_* in the app's environment.
		private static bool IsRunningOnAppleSimulator =>
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SIMULATOR_UDID"))
			|| !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SIMULATOR_DEVICE_NAME"));

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern byte objc_msgSend_supportsFamily(IntPtr receiver, IntPtr selector, ulong family);

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
