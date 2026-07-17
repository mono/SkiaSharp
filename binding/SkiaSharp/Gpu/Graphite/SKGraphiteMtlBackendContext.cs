#nullable disable

using System;
#if __IOS__ || __MACOS__ || __TVOS__
using Metal;
#endif

namespace SkiaSharp
{
	/// <summary>
	/// Caller-supplied Metal handles used to bring up a Graphite Metal context.
	/// <see cref="MtlDevice"/> and <see cref="MtlQueue"/> are id&lt;MTLDevice&gt; and
	/// id&lt;MTLCommandQueue&gt; passed as CFTypeRef-compatible handles.
	/// </summary>
	public unsafe class SKGraphiteMtlBackendContext : IDisposable
	{
		private IntPtr mtlDevice;
		private IntPtr mtlQueue;

		/// <summary>id&lt;MTLDevice&gt; (CFRetainable Obj-C handle).</summary>
		public IntPtr MtlDevice {
			get => mtlDevice;
			set {
				mtlDevice = value;
#if __IOS__ || __MACOS__ || __TVOS__
				device = null;
#endif
			}
		}

		/// <summary>id&lt;MTLCommandQueue&gt; (CFRetainable Obj-C handle).</summary>
		public IntPtr MtlQueue {
			get => mtlQueue;
			set {
				mtlQueue = value;
#if __IOS__ || __MACOS__ || __TVOS__
				queue = null;
#endif
			}
		}

#if __IOS__ || __MACOS__ || __TVOS__
		private IMTLDevice device;
		private IMTLCommandQueue queue;

		/// <summary>
		/// Strongly-typed id&lt;MTLDevice&gt;. Setting this also updates <see cref="MtlDevice"/>;
		/// setting <see cref="MtlDevice"/> directly clears this cached reference.
		/// </summary>
		public IMTLDevice Device {
			get => device;
			set {
				device = value;
				mtlDevice = value?.Handle ?? IntPtr.Zero;
			}
		}

		/// <summary>
		/// Strongly-typed id&lt;MTLCommandQueue&gt;. Setting this also updates <see cref="MtlQueue"/>;
		/// setting <see cref="MtlQueue"/> directly clears this cached reference.
		/// </summary>
		public IMTLCommandQueue Queue {
			get => queue;
			set {
				queue = value;
				mtlQueue = value?.Handle ?? IntPtr.Zero;
			}
		}
#endif

		internal SKGraphiteMtlBackendContextInit ToNative ()
		{
			if (MtlDevice == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (MtlDevice)} must be set before materializing the backend context.");
			if (MtlQueue == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (MtlQueue)} must be set before materializing the backend context.");
			return new SKGraphiteMtlBackendContextInit {
				Device = (void*)MtlDevice,
				Queue  = (void*)MtlQueue,
			};
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}
	}
}
