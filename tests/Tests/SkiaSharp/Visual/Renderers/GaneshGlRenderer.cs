using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over desktop OpenGL on Linux. Headless via EGL +
	/// Mesa <c>llvmpipe</c> (software rasterizer; deterministic across hosts).
	///
	/// <para>
	/// GL contexts are THREAD-AFFINE — they can only be current on one thread
	/// at a time, and GL commands must come from that thread. This renderer
	/// owns a dedicated worker thread for its lifetime: <see cref="RenderAsync"/>
	/// posts the work + awaits the result, the worker brings up + tears down
	/// the GRContext + SKSurface against the EGL context it holds current.
	/// </para>
	///
	/// <para>
	/// Linux only. Windows uses a different loader (WGL + HWND_MESSAGE pbuffer);
	/// macOS GL is dead — Apple deprecated it for Metal. Other Linux software
	/// stacks (X11 / Wayland) are bypassed entirely thanks to EGL's
	/// platform-device extension — no display server is contacted.
	/// </para>
	/// </summary>
	public sealed class GaneshGlRenderer : IRenderer
	{
		public string Name => "ganesh-gl";
		public RendererCapabilities Caps => RendererCapabilities.Gpu;

		public bool IsAvailable => OperatingSystem.IsLinux () && EglLoader.Shared.IsAvailable;

		public string UnavailableReason
		{
			get {
				if (!OperatingSystem.IsLinux ())
					return "ganesh-gl uses EGL+llvmpipe on Linux only (Windows uses WGL, macOS uses Metal)";
				return EglLoader.Shared.IsAvailable
					? null
					: $"EGL loader unavailable: {EglLoader.Shared.FailureReason}";
			}
		}

		// Worker thread + work queue. Lazy: spun up on first RenderAsync,
		// joined on Dispose. The EGL context is bound once on the worker
		// (GL is thread-affine), but the GRContext + SKSurface are created
		// PER render — matches the Vulkan/Metal renderers' lifetime model
		// and keeps GarbageCleanupFixture happy (no long-lived GRContext).
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
			try {
				EglLoader.Shared.MakeCurrent ();
			} catch (Exception ex) {
				while (_queue.TryTake (out var it))
					it.Tcs.TrySetException (ex);
				return;
			}

			foreach (var item in _queue.GetConsumingEnumerable ()) {
				try {
					item.Ct.ThrowIfCancellationRequested ();
					var bytes = RenderOnWorker (item.Scene, item.Info);
					item.Tcs.SetResult (bytes);
				} catch (Exception ex) {
					item.Tcs.TrySetException (ex);
				}
			}
		}

		private byte[] RenderOnWorker (ISkiaScene scene, SKImageInfo info)
		{
			using var glInterface = GRGlInterface.CreateOpenGl (name => EglLoader.Shared.GetProc (name))
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
