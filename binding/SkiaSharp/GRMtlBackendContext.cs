#nullable disable

using System;
#if __IOS__ || __MACOS__ || __TVOS__
using Metal;
#endif

namespace SkiaSharp
{
	/// <summary>Represents the Metal backend context used to create a GPU-backed <see cref="T:SkiaSharp.GRContext" />.</summary>
	/// <remarks />
	public class GRMtlBackendContext : IDisposable
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.GRMtlBackendContext" /> class.</summary>
		/// <remarks />
		public GRMtlBackendContext()
		{
		}

		private IntPtr _deviceHandle, _queueHandle;

		/// <summary>Gets or sets the native handle to the Metal device (MTLDevice).</summary>
		/// <value>The native pointer to the Metal device.</value>
		/// <remarks />
		public IntPtr DeviceHandle {
			get => _deviceHandle;
			set {
				_deviceHandle = value;
#if __IOS__ || __MACOS__
				_device = null;
#endif
			}
		}

		/// <summary>Gets or sets the native handle to the Metal command queue (MTLCommandQueue).</summary>
		/// <value>The native pointer to the Metal command queue.</value>
		/// <remarks />
		public IntPtr QueueHandle {
			get => _queueHandle;
			set {
				_queueHandle = value;
#if __IOS__ || __MACOS__ || __TVOS__
				_queue = null;
#endif
			}
		}

#if __IOS__ || __MACOS__ || __TVOS__
		private IMTLDevice _device;
		private IMTLCommandQueue _queue;

		/// <summary>Gets or sets the Metal device.</summary>
		/// <value>The Metal device.</value>
		/// <remarks />
		public IMTLDevice Device {
			get => _device;
			set {
				_device = value;
				_deviceHandle = _device.Handle;
			}
		}

		/// <summary>Gets or sets the Metal command queue.</summary>
		/// <value>The Metal command queue.</value>
		/// <remarks />
		public IMTLCommandQueue Queue {
			get => _queue;
			set {
				_queue = value;
				_queueHandle = _queue.Handle;
			}
		}
#endif

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.GRMtlBackendContext" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks />
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>Releases all resources used by this <see cref="T:SkiaSharp.GRMtlBackendContext" />.</summary>
		/// <remarks />
		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}
	}
}
