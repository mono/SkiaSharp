#nullable disable

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	public unsafe class GRVkBackendContext : IDisposable
	{
		private GRVkGetProcedureAddressDelegate getProc;
		private GCHandle getProcHandle;
		private void* getProcContext;

		// 0 = not disposed, 1 = disposed. Interlocked.Exchange makes the
		// "claim ownership of the cleanup" step atomic, so a racing Dispose +
		// finalizer can't both fall through to GCHandle.Free.
		private int disposed;

		protected virtual void Dispose (bool disposing)
		{
			if (Interlocked.Exchange (ref disposed, 1) != 0)
				return;

			if (getProcHandle.IsAllocated) {
				getProcHandle.Free ();
				getProcHandle = default;
			}
		}

		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}

		// The GetProcedureAddress setter pins the delegate with a strong GCHandle.
		// Without a finalizer a caller who forgets to Dispose () would leak that
		// handle (and everything the delegate closure roots) for the process
		// lifetime. This mirrors the finalizer on the sibling
		// SKGraphiteVkBackendContext.
		~GRVkBackendContext () => Dispose (disposing: false);

		public IntPtr VkInstance { get; set; }

		public IntPtr VkPhysicalDevice { get; set; }

		public IntPtr VkDevice { get; set; }

		public IntPtr VkQueue { get; set; }

		public UInt32 GraphicsQueueIndex { get; set; }

		public UInt32 MaxAPIVersion { get; set; }

		public GRVkExtensions Extensions { get; set; }

		public IntPtr VkPhysicalDeviceFeatures { get; set; }

		public IntPtr VkPhysicalDeviceFeatures2 { get; set; }

		public GRVkGetProcedureAddressDelegate GetProcedureAddress {
			get => getProc;
			set {
				getProc = value;

				if (getProcHandle.IsAllocated)
					getProcHandle.Free ();

				getProcHandle = default;
				getProcContext = null;

				if (value != null) {
					DelegateProxies.Create (value, out var gch, out var ctx);
					getProcHandle = gch;
					getProcContext = (void*)ctx;
				}
			}
		}

		public bool ProtectedContext { get; set; }

		internal GRVkBackendContextNative ToNative () =>
			new GRVkBackendContextNative {
				fInstance = VkInstance,
				fDevice = VkDevice,
				fPhysicalDevice = VkPhysicalDevice,
				fQueue = VkQueue,
				fGraphicsQueueIndex = GraphicsQueueIndex,
				fMaxAPIVersion = MaxAPIVersion,
				fVkExtensions = Extensions?.Handle ?? IntPtr.Zero,
				fDeviceFeatures = VkPhysicalDeviceFeatures,
				fDeviceFeatures2 = VkPhysicalDeviceFeatures2,
				fGetProcUserData = getProcContext,
				fGetProc = getProcContext is not null ? DelegateProxies.GRVkGetProcProxy : null,
				fProtectedContext = ProtectedContext ? (byte)1 : (byte)0
			};
	}
}
