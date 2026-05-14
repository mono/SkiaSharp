#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>
	/// Caller-supplied Dawn (WebGPU) handles used to bring up a Graphite Dawn
	/// context. The resulting <see cref="SKGraphiteContext"/> retains the
	/// instance/device/queue handles for its lifetime and the caller may drop
	/// their own references once <see cref="SKGraphiteContext.CreateDawn"/>
	/// returns.
	///
	/// On WebAssembly (browser) targets the resulting context runs in
	/// non-yielding mode: <see cref="SKGraphiteSubmitInfo.Sync"/> = true is
	/// rejected by <see cref="SKGraphiteContext.Submit(SKGraphiteSubmitInfo)"/>.
	/// Use <see cref="SKGraphiteContext.CheckAsyncWorkCompletion"/> to drive
	/// readbacks instead.
	/// </summary>
	public unsafe class SKGraphiteDawnBackendContext
	{
		private static readonly OSPlatform Browser = OSPlatform.Create ("BROWSER");

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
	}
}
