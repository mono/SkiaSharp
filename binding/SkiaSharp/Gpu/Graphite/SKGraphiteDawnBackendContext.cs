#nullable disable

using System;

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
	/// </summary>
	public unsafe class SKGraphiteDawnBackendContext : IDisposable
	{
		private IntPtr nativeBackendContext;

		/// <summary>WGPUInstance handle.</summary>
		public IntPtr WgpuInstance { get; set; }

		/// <summary>WGPUDevice handle.</summary>
		public IntPtr WgpuDevice   { get; set; }

		/// <summary>WGPUQueue handle.</summary>
		public IntPtr WgpuQueue    { get; set; }

		/// <summary>
		/// When true, no DawnTickFunction is installed (Skia's "non-yielding"
		/// mode). Required when running over Emscripten without -s ASYNCIFY,
		/// at the cost of disallowing <c>SKGraphiteSubmitInfo.Sync = true</c>
		/// on submit. Defaults to false (the shim installs
		/// <c>DawnNativeProcessEventsFunction</c>).
		/// </summary>
		public bool NonYielding   { get; set; }

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
						NonYielding = NonYielding,
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
