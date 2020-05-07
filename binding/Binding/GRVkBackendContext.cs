using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	public class GRVkBackendContext : SKObject
	{
		private GCHandle getProcProxy;

		internal GRVkBackendContext (IntPtr h, GCHandle getProcProxy, bool owns)
			: base (h, owns)
		{
			this.getProcProxy = getProcProxy;
		}

		public static unsafe GRVkBackendContext Assemble (
			IntPtr vkInstance,
			IntPtr vkPhysicalDevice,
			IntPtr vkDevice,
			IntPtr vkQueue,
			uint graphicsQueueIndex,
			uint minAPIVersion,
			uint extensions,
			uint features,
			GRVkGetProcDelegate getProc)
		{
			var proxy = DelegateProxies.Create (getProc, DelegateProxies.GRVkGetProcDelegateProxy, out var gch, out var ctx);

			var handle =
				SkiaApi.gr_vkbackendcontext_assemble (
					(void*)ctx,
					vkInstance,
					vkPhysicalDevice,
					vkDevice,
					vkQueue,
					graphicsQueueIndex,
					minAPIVersion,
					extensions,
					features,
					proxy);

			return new GRVkBackendContext (handle, gch, true);
		}

		protected override void DisposeNative ()
		{
			base.DisposeNative ();

			SkiaApi.gr_vkbackendcontext_delete (Handle);
		}

		protected override void DisposeManaged ()
		{
			base.DisposeManaged ();

			getProcProxy.Free ();
		}
	}
}
