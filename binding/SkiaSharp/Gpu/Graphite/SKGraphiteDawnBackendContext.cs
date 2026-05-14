#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>
	/// Caller-supplied Dawn (WebGPU) handles used to bring up a Graphite Dawn
	/// context. The shim AddRef's each handle when the context is created and
	/// Releases on disposal — caller's refs are unaffected.
	///
	/// As with <see cref="SKGraphiteMtlBackendContext"/>, there is no GetProc
	/// callback and no GCHandle ownership transfer; the caller may dispose
	/// this object any time after <see cref="SKGraphiteContext.CreateDawn"/>.
	///
	/// On WebAssembly (browser) targets the shim runs in "non-yielding" mode
	/// automatically — Emscripten without -s ASYNCIFY cannot pump the Dawn
	/// event loop from inside a C# stack frame. The trade-off is that
	/// <see cref="SKGraphiteSubmitInfo.Sync"/> = true is rejected by
	/// <see cref="SKGraphiteContext.Submit(SKGraphiteSubmitInfo)"/>; use
	/// <see cref="SKGraphiteContext.CheckAsyncWorkCompletion"/> on a JS
	/// timer tick to drive readbacks instead. On every other platform the
	/// shim installs <c>DawnNativeProcessEventsFunction</c> and the sync
	/// path works as expected.
	/// </summary>
	public unsafe class SKGraphiteDawnBackendContext : IDisposable
	{
		private static readonly OSPlatform Browser = OSPlatform.Create ("BROWSER");

		private IntPtr nativeBackendContext;

		/// <summary>WGPUInstance handle.</summary>
		public IntPtr WgpuInstance { get; set; }

		/// <summary>WGPUDevice handle.</summary>
		public IntPtr WgpuDevice   { get; set; }

		/// <summary>WGPUQueue handle.</summary>
		public IntPtr WgpuQueue    { get; set; }

		// True only on browser/WASM where the Dawn event loop cannot be
		// pumped from inside a managed call stack. Not user-settable —
		// see the class-level remarks for the constraint this imposes
		// on Submit(Sync=true).
		internal bool IsNonYielding => RuntimeInformation.IsOSPlatform (Browser);

		internal IntPtr Handle {
			get {
				if (nativeBackendContext == IntPtr.Zero) {
					if (WgpuInstance == IntPtr.Zero)
						throw new InvalidOperationException ($"{nameof (WgpuInstance)} must be set before materializing the backend context.");
					if (WgpuDevice == IntPtr.Zero)
						throw new InvalidOperationException ($"{nameof (WgpuDevice)} must be set before materializing the backend context.");
					if (WgpuQueue == IntPtr.Zero)
						throw new InvalidOperationException ($"{nameof (WgpuQueue)} must be set before materializing the backend context.");
					var native = new SKGraphiteDawnBackendContextInit {
						Instance    = (void*)WgpuInstance,
						Device      = (void*)WgpuDevice,
						Queue       = (void*)WgpuQueue,
						NonYielding = IsNonYielding,
					};
					nativeBackendContext = SkiaApi.sk_graphite_dawn_backend_context_new (&native);
				}
				return nativeBackendContext;
			}
		}

		internal void ReleaseNativeHandle ()
		{
			if (nativeBackendContext != IntPtr.Zero) {
				SkiaApi.sk_graphite_dawn_backend_context_delete (nativeBackendContext);
				nativeBackendContext = IntPtr.Zero;
			}
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				ReleaseNativeHandle ();
		}

		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}
	}
}
