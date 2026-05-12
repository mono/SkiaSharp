using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over desktop OpenGL.
	///
	/// <para>
	/// Platform-dispatched: on Linux uses <see cref="EglLoader"/> (EGL +
	/// Mesa <c>llvmpipe</c> for software-deterministic output); on Windows
	/// uses <see cref="WglLoader"/> (HWND_MESSAGE + <c>wglCreateContextAttribsARB</c>).
	/// macOS is not covered — Apple deprecated desktop GL in favor of Metal.
	/// </para>
	///
	/// <para>
	/// GL contexts are thread-affine. On Linux EGL allows cross-thread binding,
	/// so the process-singleton <see cref="EglLoader.Shared"/> is reused across
	/// threads. WGL on Windows does NOT — its DC pixel-format state and the
	/// context's bind state are tied to the thread that created them. So the
	/// renderer's worker thread does the full WGL bring-up itself; we never
	/// touch WGL from another thread.
	/// </para>
	/// </summary>
	public sealed class GaneshGlRenderer : IRenderer
	{
		public string Name => "ganesh-gl";
		public RendererCapabilities Caps => RendererCapabilities.Gpu;

		public bool IsAvailable
		{
			get {
				// Cheap, side-effect-free probe. The actual WGL bring-up on
				// Windows happens on the worker thread inside RenderAsync;
				// if it fails there, the test gets a hard failure (not a
				// skip) with the underlying Win32 error in the message.
				if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux))   return EglLoader.Shared.IsAvailable;
				if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) return WglLoader.ProbeOpenGl32Loadable ();
				return false;
			}
		}

		public string UnavailableReason
		{
			get {
				if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
					return EglLoader.Shared.IsAvailable ? null
						: $"EGL loader unavailable: {EglLoader.Shared.FailureReason}";
				}
				if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
					return WglLoader.ProbeOpenGl32Loadable () ? null
						: "opengl32.dll not loadable on this host";
				}
				return "ganesh-gl uses EGL+llvmpipe on Linux and WGL on Windows (macOS uses Metal — no desktop GL)";
			}
		}

		// Worker thread + work queue. Lazy: spun up on first RenderAsync,
		// joined on Dispose. On Windows the worker also owns the WGL state
		// (creates window, DC, context on first iteration).
		private readonly object _startLock = new object ();
		private Thread _worker;
		private BlockingCollection<WorkItem> _queue;

		private sealed class WorkItem
		{
			public ISkiaScene Scene;
			public SKImageInfo Info;
			public CancellationToken Ct;
			public TaskCompletionSource<byte[]> Tcs;
		}

		public Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (UnavailableReason);
			EnsureWorker ();
			var tcs = new TaskCompletionSource<byte[]> (TaskCreationOptions.RunContinuationsAsynchronously);
			_queue.Add (new WorkItem { Scene = scene, Info = info, Ct = ct, Tcs = tcs }, ct);
			return tcs.Task;
		}

		private void EnsureWorker ()
		{
			if (_worker != null) return;
			lock (_startLock) {
				if (_worker != null) return;
				_queue = new BlockingCollection<WorkItem> ();
				_worker = new Thread (WorkerLoop) {
					IsBackground = true,
					Name = "GaneshGlRenderer-Worker",
				};
				_worker.Start ();
			}
		}

		private void WorkerLoop ()
		{
			WglLoader wgl = null;
			GRGlGetProcedureAddressDelegate getProc;
			try {
				if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
					EglLoader.Shared.MakeCurrent ();
					getProc = name => EglLoader.Shared.GetProc (name);
				} else if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
					wgl = new WglLoader ();
					wgl.Initialize (); // creates window+DC+context on THIS thread
					getProc = name => wgl.GetProc (name);
				} else {
					throw new PlatformNotSupportedException (
						"ganesh-gl is only available on Linux (EGL) and Windows (WGL)");
				}
			} catch (Exception ex) {
				while (_queue.TryTake (out var it))
					it.Tcs.TrySetException (ex);
				wgl?.Dispose ();
				return;
			}

			try {
				foreach (var item in _queue.GetConsumingEnumerable ()) {
					try {
						item.Ct.ThrowIfCancellationRequested ();
						var bytes = RenderOnWorker (getProc, item.Scene, item.Info);
						item.Tcs.SetResult (bytes);
					} catch (Exception ex) {
						item.Tcs.TrySetException (ex);
					}
				}
			} finally {
				wgl?.Dispose ();
			}
		}

		private byte[] RenderOnWorker (GRGlGetProcedureAddressDelegate getProc, ISkiaScene scene, SKImageInfo info)
		{
			using var glInterface = GRGlInterface.CreateOpenGl (getProc)
				?? throw new InvalidOperationException ("GRGlInterface.CreateOpenGl returned null");
			using var ctx = GRContext.CreateGl (glInterface)
				?? throw new InvalidOperationException ("GRContext.CreateGl returned null");
			using var surface = SKSurface.Create (ctx, budgeted: true, info)
				?? throw new InvalidOperationException ("SKSurface.Create returned null on Ganesh/GL");
			scene.Draw (surface.Canvas);
			ctx.Flush ();
			ctx.Submit (synchronous: true);

			var rgba = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = new byte[rgba.BytesSize];
			unsafe {
				fixed (byte* p = pixels) {
					if (!surface.ReadPixels (rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
						throw new InvalidOperationException ("SKSurface.ReadPixels failed on Ganesh/GL");
				}
			}
			return pixels;
		}

		public void Dispose ()
		{
			_queue?.CompleteAdding ();
			_worker?.Join ();
			_queue?.Dispose ();
			_queue = null;
			_worker = null;
		}
	}
}
