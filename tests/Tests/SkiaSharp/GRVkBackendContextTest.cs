using System;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GRVkBackendContextTest : SKTest
	{
		// A GRVkBackendContext whose GetProcedureAddress delegate is set allocates a
		// strong GCHandle rooting that delegate (and everything its closure captures).
		// That handle is freed in Dispose(). If a caller forgets to Dispose(), the only
		// safety net is a finalizer. This mirrors the finalizer that its sibling
		// SKGraphiteVkBackendContext already has for the identical GCHandle.
		[Fact]
		public async Task UndisposedContextDoesNotLeakGetProcedureAddressDelegate()
		{
			var weak = CreateAndAbandon();

			await AssertEx.EventuallyGC(weak);
		}

		private static WeakReference CreateAndAbandon()
		{
			var tracked = new object();

			var context = new GRVkBackendContext();
			GRVkGetProcedureAddressDelegate del = (name, instance, device) => {
				GC.KeepAlive(tracked);
				return IntPtr.Zero;
			};
			context.GetProcedureAddress = del;

			// Intentionally do NOT dispose: a finalizer must free the GCHandle.
			return new WeakReference(del);
		}
	}
}
