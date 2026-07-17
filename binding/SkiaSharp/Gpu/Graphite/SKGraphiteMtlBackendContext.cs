#nullable disable

using System;
#if __IOS__ || __MACOS__ || __TVOS__
using Metal;
#endif

namespace SkiaSharp
{
	public unsafe class SKGraphiteMtlBackendContext : IDisposable
	{
		private IntPtr mtlDevice;
		private IntPtr mtlQueue;

		public IntPtr MtlDevice {
			get => mtlDevice;
			set {
				mtlDevice = value;
#if __IOS__ || __MACOS__ || __TVOS__
				device = null;
#endif
			}
		}

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

		public IMTLDevice Device {
			get => device;
			set {
				device = value;
				mtlDevice = value?.Handle ?? IntPtr.Zero;
			}
		}

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
