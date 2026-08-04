#nullable disable

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
		private GCHandle getProcHandle;
		private void* getProcContext;

		// Device-lost callback state (mono/SkiaSharp#4601). GCHandle pins the caller's
		// managed delegate; nativeDeviceLostHandler is the caller-owned bridge allocated
		// by gr_vk_device_lost_handler_new that Skia's VulkanBackendContext references
		// non-owning. Both live for the BackendContext's lifetime; Skia's Context holds
		// a raw pointer to the bridge and will call the callback whenever it detects
		// VK_ERROR_DEVICE_LOST, so callers MUST keep this GRVkBackendContext alive at
		// least as long as the GRContext built from it. (Same rule as GetProcedureAddress
		// today; unlike GetProc, Skia actually calls this one after Make.)
		private GRVkDeviceLostDelegate deviceLost;
		private GCHandle deviceLostHandle;
		private IntPtr nativeDeviceLostHandler;

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (getProcHandle.IsAllocated) {
					getProcHandle.Free ();
					getProcHandle = default;
				}
				if (nativeDeviceLostHandler != IntPtr.Zero) {
					SkiaApi.gr_vk_device_lost_handler_delete (nativeDeviceLostHandler);
					nativeDeviceLostHandler = IntPtr.Zero;
				}
				if (deviceLostHandle.IsAllocated) {
					deviceLostHandle.Free ();
					deviceLostHandle = default;
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

		// Optional VK_ERROR_DEVICE_LOST callback. Skia stores the callback pointer on
		// the Context and calls it whenever it detects device loss; the invocation can
		// happen on any driver-owned thread. The pinned managed delegate must remain
		// alive for the whole lifetime of any GRContext built from this backend
		// context — dispose this GRVkBackendContext AFTER the corresponding GRContext.
		public GRVkDeviceLostDelegate DeviceLost {
			get => deviceLost;
			set {
				// Tear down previous state atomically. A setter that swapped in a new
				// delegate while the old bridge was still installed on a live Context
				// would let Skia call a freed GCHandle; the intended pattern is
				// "assign once before CreateVulkan", so replacement resets everything.
				if (nativeDeviceLostHandler != IntPtr.Zero) {
					SkiaApi.gr_vk_device_lost_handler_delete (nativeDeviceLostHandler);
					nativeDeviceLostHandler = IntPtr.Zero;
				}
				if (deviceLostHandle.IsAllocated) {
					deviceLostHandle.Free ();
					deviceLostHandle = default;
				}

				deviceLost = value;
				if (value != null) {
					DelegateProxies.Create (value, out var gch, out var ctx);
					deviceLostHandle = gch;
					nativeDeviceLostHandler = SkiaApi.gr_vk_device_lost_handler_new (
						DelegateProxies.GRVkDeviceLostProxy,
						(void*)ctx);
					if (nativeDeviceLostHandler == IntPtr.Zero) {
						gch.Free ();
						deviceLostHandle = default;
						deviceLost = null;
						throw new InvalidOperationException (
							"gr_vk_device_lost_handler_new failed (Vulkan not built into libSkiaSharp?)");
					}
				}
			}
		}

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
				fProtectedContext = ProtectedContext ? (byte)1 : (byte)0,
				fDeviceLostHandler = nativeDeviceLostHandler,
			};
	}
}
