#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public unsafe class SKGraphiteDawnBackendContext : IDisposable
	{
		private static readonly OSPlatform Browser = OSPlatform.Create ("BROWSER");

		public IntPtr WgpuInstance { get; set; }

		public IntPtr WgpuDevice   { get; set; }

		public IntPtr WgpuQueue    { get; set; }

		// True only on browser/WASM where the Dawn event loop cannot be
		// pumped from inside a managed call stack. Not user-settable —
		// see the class-level remarks for the constraint this imposes
		// on Submit(Sync=true).
		internal bool IsNonYielding => RuntimeInformation.IsOSPlatform (Browser);

		internal SKGraphiteDawnBackendContextInit ToNative ()
		{
			if (WgpuInstance == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (WgpuInstance)} must be set before materializing the backend context.");
			if (WgpuDevice == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (WgpuDevice)} must be set before materializing the backend context.");
			if (WgpuQueue == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (WgpuQueue)} must be set before materializing the backend context.");
			return new SKGraphiteDawnBackendContextInit {
				Instance    = (void*)WgpuInstance,
				Device      = (void*)WgpuDevice,
				Queue       = (void*)WgpuQueue,
				NonYielding = IsNonYielding,
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
