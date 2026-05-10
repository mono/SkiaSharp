#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Caller-supplied Metal handles used to bring up a Graphite Metal context.
	/// Both <see cref="MtlDevice"/> and <see cref="MtlQueue"/> must be valid
	/// CFTypeRef-compatible Objective-C handles (id&lt;MTLDevice&gt; and
	/// id&lt;MTLCommandQueue&gt;); the SkiaSharp shim CFRetains them when the
	/// context is created and CFReleases on disposal.
	///
	/// Unlike <see cref="SKGraphiteVkBackendContext"/>, Metal has no GetProc
	/// callback, so there is no GCHandle to pin and no Variant A-style transfer.
	/// The caller may dispose this object at any time after
	/// <see cref="SKGraphiteContext.CreateMetal"/> returns; the resulting
	/// SKGraphiteContext keeps its own retained references to the device/queue
	/// (held inside the Skia-side <c>MtlBackendContext</c>'s sk_cfp fields).
	/// </summary>
	public unsafe class SKGraphiteMtlBackendContext : IDisposable
	{
		private IntPtr nativeBackendContext;

		/// <summary>id&lt;MTLDevice&gt; (CFRetainable Obj-C handle).</summary>
		public IntPtr MtlDevice { get; set; }

		/// <summary>id&lt;MTLCommandQueue&gt; (CFRetainable Obj-C handle).</summary>
		public IntPtr MtlQueue  { get; set; }

		/// <summary>Lazily creates the native backend-context handle.</summary>
		internal IntPtr Handle {
			get {
				if (nativeBackendContext == IntPtr.Zero) {
					var native = new SKGraphiteMtlBackendContextInit {
						Device = (void*)MtlDevice,
						Queue  = (void*)MtlQueue,
					};
					nativeBackendContext = SkiaApi.sk_graphite_mtl_backend_context_new (&native);
				}
				return nativeBackendContext;
			}
		}

		internal void ReleaseNativeHandle ()
		{
			if (nativeBackendContext != IntPtr.Zero) {
				SkiaApi.sk_graphite_mtl_backend_context_delete (nativeBackendContext);
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
