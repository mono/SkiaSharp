#nullable disable

#if __IOS__ || __MACOS__
using System;
using Metal;

namespace SkiaSharp
{
	public class GRMtlBackendContext : IDisposable
	{
		public IMTLDevice Device { get; set; }

		public IMTLCommandQueue Queue { get; set; }

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
#endif
