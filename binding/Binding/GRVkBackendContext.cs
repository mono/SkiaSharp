using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	public class GRVkBackendContext : IDisposable
	{
		private GRVkGetProcDelegate getProc;
		private GRVkGetProcProxyDelegate getProcProxy;
		private GCHandle? getProcHandle;
		private IntPtr getProcContext;

		public IntPtr VkInstance { get; set; }

		public IntPtr VkPhysicalDevice { get; set; }

		public IntPtr VkDevice { get; set; }

		public IntPtr VkQueue { get; set; }

		public UInt32 GraphicsQueueIndex { get; set; }

		public UInt32 MaxAPIVersion { get; set; }

		public GRVkExtensions Extensions { get; set; }

		public IntPtr VkDeviceFeatures { get; set; }

		public IntPtr VkDeviceFeatures2 { get; set; }

		public GRVkGetProcDelegate GetProc {
			get => getProc;
			set {
				if (getProcHandle.HasValue) {
					getProcHandle.Value.Free ();
					getProcHandle = null;
				}

				if (value is null) {
					getProcProxy = null;
					getProcContext = IntPtr.Zero;
					getProc = null;
				} else {
					getProcProxy = DelegateProxies.Create (
						value,
						DelegateProxies.GRVkGetProcDelegateProxy,
						out var gch, out var ctx);
					getProcHandle = gch;
					getProcContext = ctx;
					getProc = value;
				}
			}
		}

		public bool ProtectedContext { get; set; }

		public void Dispose ()
		{
			if (getProcHandle.HasValue) {
				getProcHandle.Value.Free ();
				getProcHandle = null;
			}
		}

		internal GRVkBackendContextNative ToNative ()
		{
			return new GRVkBackendContextNative {
				fInstance = VkInstance,
				fDevice = VkDevice,
				fPhysicalDevice = VkPhysicalDevice,
				fQueue = VkQueue,
				fGraphicsQueueIndex = GraphicsQueueIndex,
				fMaxAPIVersion = MaxAPIVersion,
				fVkExtensions = Extensions?.Handle ??IntPtr.Zero,
				fDeviceFeatures = VkDeviceFeatures,
				fDeviceFeatures2 = VkDeviceFeatures2,
				fGetProcContext = getProcContext,
				fGetProc = getProcProxy,
				fProtectedContext = ProtectedContext ? (byte)1 : (byte)0
			};
		}
	}
}
