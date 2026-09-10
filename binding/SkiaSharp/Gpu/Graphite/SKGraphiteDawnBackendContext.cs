#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>Supplies the Dawn (WebGPU) instance, device, and queue used to create a Dawn-backed <see cref="T:SkiaSharp.SKGraphiteContext" />.</summary>
	/// <remarks>
	///       <format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Populate the <xref:SkiaSharp.SKGraphiteDawnBackendContext.WgpuInstance>, <xref:SkiaSharp.SKGraphiteDawnBackendContext.WgpuDevice>, and <xref:SkiaSharp.SKGraphiteDawnBackendContext.WgpuQueue> handles, then pass this object to <xref:SkiaSharp.SKGraphiteContext.CreateDawn(SkiaSharp.SKGraphiteDawnBackendContext)>. You can dispose it as soon as the context has been created.
	///
	/// On non-yielding environments such as the browser (WASM), the Dawn event loop cannot be pumped from inside a managed call frame, so synchronous submits are rejected; drive readbacks with <xref:SkiaSharp.SKGraphiteContext.CheckAsyncWorkCompletion> instead.
	/// ]]></format>
	///     </remarks>
	public unsafe class SKGraphiteDawnBackendContext : IDisposable
	{
		private static readonly OSPlatform Browser = OSPlatform.Create ("BROWSER");

		/// <summary>Gets or sets the handle to the WebGPU instance.</summary>
		/// <value>A handle to the WebGPU instance.</value>
		/// <remarks />
		public IntPtr WgpuInstance { get; set; }

		/// <summary>Gets or sets the handle to the WebGPU device.</summary>
		/// <value>A handle to the WebGPU device.</value>
		/// <remarks />
		public IntPtr WgpuDevice   { get; set; }

		/// <summary>Gets or sets the handle to the WebGPU queue.</summary>
		/// <value>A handle to the WebGPU queue.</value>
		/// <remarks />
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

		/// <param name="disposing">
		///           <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKGraphiteDawnBackendContext" /> and optionally releases the managed resources.</summary>
		/// <remarks />
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>Releases the resources used by the current instance of the <see cref="T:SkiaSharp.SKGraphiteDawnBackendContext" /> class.</summary>
		/// <remarks />
		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}
	}
}
