using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	public unsafe class GRVkBackendContext : IDisposable
	{
		private GRVkGetProcedureAddressDelegate getProc;
#if USE_INTPTR_DELEGATES
		private IntPtr getProcProxy;
#else
		private GRVkGetProcProxyDelegate getProcProxy;
#endif
		private GCHandle getProcHandle;
		private void* getProcContext;

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (getProcHandle.IsAllocated) {
					getProcHandle.Free ();
					getProcHandle = default;
				}
			}
		}

		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}

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

				getProcProxy = default;
				getProcHandle = default;
				getProcContext = default;

				if (value != null) {
					getProcProxy = DelegateProxies.Create (value, DelegateProxies.GRVkGetProcDelegateProxy, out var gch, out var ctx);
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
				fGetProc = getProcProxy,
				fProtectedContext = ProtectedContext ? (byte)1 : (byte)0
			};
	}
}
