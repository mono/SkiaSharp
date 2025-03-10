using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp
{
	public class GRD3DBackendContext : IDisposable
	{
		public nint Adapter { get; set; }

		public nint Device { get; set; }

		public nint Queue { get; set; }

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
