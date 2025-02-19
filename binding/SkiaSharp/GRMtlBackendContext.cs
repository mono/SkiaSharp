#nullable disable

using System;
#if __IOS__ || __MACOS__ || __TVOS__
using Metal;
#endif

namespace SkiaSharp
{
	public class GRMtlBackendContext : IDisposable
	{
		private IntPtr _deviceHandle, _queueHandle;

		public IntPtr DeviceHandle {
			get => _deviceHandle;
			set {
				_deviceHandle = value;
#if __IOS__ || __MACOS__
				_device = null;
#endif
			}
		}

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

		public IMTLDevice Device {
			get => _device;
			set {
				_device = value;
				_deviceHandle = _device.Handle;
			}
		}

		public IMTLCommandQueue Queue {
			get => _queue;
			set {
				_queue = value;
				_queueHandle = _queue.Handle;
			}
		}
#endif

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
