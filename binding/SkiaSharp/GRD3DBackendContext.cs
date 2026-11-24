using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp
{
	/// <summary>Represents the Direct3D 12 backend context used to create a GPU-backed <see cref="T:SkiaSharp.GRContext" />.</summary>
	/// <remarks />
	public class GRD3DBackendContext : IDisposable
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.GRD3DBackendContext" /> class.</summary>
		/// <remarks />
		public GRD3DBackendContext()
		{
		}

		/// <summary>Gets or sets the native pointer to the DXGI adapter (IDXGIAdapter).</summary>
		/// <value>The native pointer to the DXGI adapter.</value>
		/// <remarks />
		public nint Adapter { get; set; }

		/// <summary>Gets or sets the native pointer to the Direct3D 12 device (ID3D12Device).</summary>
		/// <value>The native pointer to the Direct3D 12 device.</value>
		/// <remarks />
		public nint Device { get; set; }

		/// <summary>Gets or sets the native pointer to the Direct3D 12 command queue (ID3D12CommandQueue).</summary>
		/// <value>The native pointer to the Direct3D 12 command queue.</value>
		/// <remarks />
		public nint Queue { get; set; }

		/// <summary>Gets or sets a value indicating whether the context uses protected content.</summary>
		/// <value><see langword="true" /> if the context uses protected content; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool ProtectedContext { get; set; }

		internal GRD3DBackendContextNative ToNative ()
		{
			return new GRD3DBackendContextNative {
				fAdapter = Adapter,
				fDevice = Device,
				fQueue = Queue,
				fProtectedContext = ProtectedContext ? (byte)1 : (byte)0
			};
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.GRD3DBackendContext" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks />
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>Releases all resources used by this <see cref="T:SkiaSharp.GRD3DBackendContext" />.</summary>
		/// <remarks />
		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}
	}
}
