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
	/// <see cref="SKGraphiteContext.CreateMetal"/> instead of the Ganesh
	/// factory. Compiled into every host; the Apple-only check gates the P/Invoke
	/// surface so non-Apple hosts skip cleanly.
	/// </summary>
	public sealed class GraphiteMetalRenderer : IRenderer
	{
		public string Name => "graphite-metal";

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

					var backendContext = new SKGraphiteMtlBackendContext { MtlDevice = device, MtlQueue = queue };
					using var context = SKGraphiteContext.CreateMetal(backendContext)
						?? throw new InvalidOperationException("SKGraphiteContext.CreateMetal returned null.");
					using var recorder = context.CreateRecorder()
						?? throw new InvalidOperationException("SKGraphiteContext.CreateRecorder returned null.");
					using var surface = SKSurface.Create(recorder, info)
						?? throw new InvalidOperationException("SKSurface.Create returned null on Graphite/Metal.");

					scene.Draw(surface.Canvas);

					using var recording = recorder.Snap()
						?? throw new InvalidOperationException("Recorder.Snap() returned null.");
					if (context.InsertRecording(recording) != SKGraphiteInsertStatus.Success)
						throw new InvalidOperationException("InsertRecording did not report Success.");
					if (!context.Submit(new SKGraphiteSubmitInfo { Sync = true }))
						throw new InvalidOperationException("Submit(Sync=true) returned false.");

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
