#nullable disable

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	public unsafe class SKGraphiteVkBackendContext : IDisposable
	{
		private SKGraphiteVkGetProcedureAddressDelegate getProc;
		private GCHandle getProcHandle;
		private void* getProcContext;

		// Device-lost state — see GRVkBackendContext for the pattern. Pinned managed
		// delegate + caller-owned native bridge (allocated via gr_vk_device_lost_handler_new).
		// Skia holds the bridge pointer non-owning; this backend context must outlive
		// the SKGraphiteContext it was used to create.
		private GRVkDeviceLostDelegate deviceLost;
		private GCHandle deviceLostHandle;
		private IntPtr nativeDeviceLostHandler;

		public IntPtr VkInstance { get; set; }

		public IntPtr VkPhysicalDevice { get; set; }

		public IntPtr VkDevice { get; set; }

		public IntPtr VkQueue { get; set; }

		public uint GraphicsQueueIndex { get; set; }

		public uint MaxApiVersion { get; set; }

		public bool ProtectedContext { get; set; }

		public SKGraphiteVkGetProcedureAddressDelegate GetProcedureAddress {
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

		// Hand off the GCHandle pinning the GetProc delegate to SKGraphiteContext.CreateVulkan.
		// Returns the current handle and zeros out internal state so subsequent Dispose calls
		// are no-ops. The getProc field (the user's delegate) stays for getter consistency,
		// but the pin that keeps it alive is now owned elsewhere.
		internal GCHandle TransferGetProcHandle ()
		{
			var h = getProcHandle;
			getProcHandle = default;
			getProcContext = null;
			return h;
		}

		public GRVkDeviceLostDelegate DeviceLost {
			get => deviceLost;
			set {
				// See GRVkBackendContext.DeviceLost for the reasoning behind the atomic
				// swap on assignment.
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

		internal SKGraphiteVkBackendContextNative ToNative ()
		{
			if (VkInstance == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (VkInstance)} must be set before materializing the backend context.");
			if (VkPhysicalDevice == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (VkPhysicalDevice)} must be set before materializing the backend context.");
			if (VkDevice == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (VkDevice)} must be set before materializing the backend context.");
			if (VkQueue == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (VkQueue)} must be set before materializing the backend context.");

			return new SKGraphiteVkBackendContextNative {
				fInstance           = VkInstance,
				fPhysicalDevice     = VkPhysicalDevice,
				fDevice             = VkDevice,
				fQueue              = VkQueue,
				fGraphicsQueueIndex = GraphicsQueueIndex,
				fMaxAPIVersion      = MaxApiVersion,
				fGetProcUserData    = getProcContext,
				fGetProc            = getProcContext is not null ? DelegateProxies.SKGraphiteVkGetProxy : null,
				fProtectedContext   = ProtectedContext ? (byte)1 : (byte)0,
				fDeviceLostHandler  = nativeDeviceLostHandler,
			};
		}

		// 0 = not disposed, 1 = disposed. Interlocked.Exchange makes the
		// "claim ownership of the cleanup" step atomic, so a racing Dispose +
		// finalizer can't both fall through to GCHandle.Free.
		private int disposed;

		public void Dispose ()
		{
			DisposeCore ();
			GC.SuppressFinalize (this);
		}

		private void DisposeCore ()
		{
			if (Interlocked.Exchange (ref disposed, 1) != 0)
				return;

			if (getProcHandle.IsAllocated)
				getProcHandle.Free ();
			if (nativeDeviceLostHandler != IntPtr.Zero) {
				SkiaApi.gr_vk_device_lost_handler_delete (nativeDeviceLostHandler);
				nativeDeviceLostHandler = IntPtr.Zero;
			}
			if (deviceLostHandle.IsAllocated) {
				deviceLostHandle.Free ();
				deviceLostHandle = default;
			}
		}

		~SKGraphiteVkBackendContext () => DisposeCore ();
	}
}
