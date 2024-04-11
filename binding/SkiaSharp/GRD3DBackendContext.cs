using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSharp
{
	public class GRD3DBackendContext
	{
		public nuint Adapter { get; set; }
		public nuint Device { get; set; }
		public nuint Queue { get; set; }
		public bool ProtectedContext { get; set; }

		internal GrD3DBackendContextNative ToNative ()
		{
			return new GrD3DBackendContextNative {
				fAdapter = Adapter,
				fDevice = Device,
				fQueue = Queue,
				fProtectedContext = ProtectedContext
			};
		}
	}
}
