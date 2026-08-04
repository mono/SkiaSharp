#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>Represents the Vulkan backend context for GPU rendering.</summary>
	/// <remarks />
	public unsafe class GRVkBackendContext : IDisposable
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.GRVkBackendContext" /> class.</summary>
		/// <remarks />
		public GRVkBackendContext()
		{
		}

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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.GRVkBackendContext" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks />
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

		/// <summary>Releases all resources used by the <see cref="T:SkiaSharp.GRVkBackendContext" />.</summary>
		/// <remarks />
		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}

		/// <summary>Gets or sets the Vulkan instance handle.</summary>
		/// <value>A pointer to the Vulkan instance.</value>
		/// <remarks />
		public IntPtr VkInstance { get; set; }

		/// <summary>Gets or sets the Vulkan physical device handle.</summary>
		/// <value>A pointer to the Vulkan physical device.</value>
		/// <remarks />
		public IntPtr VkPhysicalDevice { get; set; }

		/// <summary>Gets or sets the Vulkan logical device handle.</summary>
		/// <value>A pointer to the Vulkan device.</value>
		/// <remarks />
		public IntPtr VkDevice { get; set; }

		/// <summary>Gets or sets the Vulkan graphics queue handle.</summary>
		/// <value>A pointer to the Vulkan queue.</value>
		/// <remarks />
		public IntPtr VkQueue { get; set; }

		/// <summary>Gets or sets the graphics queue family index.</summary>
		/// <value>The index of the graphics queue family.</value>
		/// <remarks />
		public UInt32 GraphicsQueueIndex { get; set; }

		/// <summary>Gets or sets the maximum Vulkan API version supported.</summary>
		/// <value>The maximum Vulkan API version.</value>
		/// <remarks />
		public UInt32 MaxAPIVersion { get; set; }

		/// <summary>Gets or sets the Vulkan extensions.</summary>
		/// <value>The Vulkan extensions.</value>
		/// <remarks />
		public GRVkExtensions Extensions { get; set; }

		/// <summary>Gets or sets the Vulkan physical device features.</summary>
		/// <value>A pointer to the VkPhysicalDeviceFeatures structure.</value>
		/// <remarks />
		public IntPtr VkPhysicalDeviceFeatures { get; set; }

		/// <summary>Gets or sets the Vulkan 1.1+ physical device features.</summary>
		/// <value>A pointer to the VkPhysicalDeviceFeatures2 structure.</value>
		/// <remarks />
		public IntPtr VkPhysicalDeviceFeatures2 { get; set; }

		/// <summary>Gets or sets the delegate used to retrieve Vulkan procedure addresses.</summary>
		/// <value>The delegate that returns Vulkan procedure addresses.</value>
		/// <remarks />
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

		/// <summary>Gets or sets a value indicating whether the context uses protected memory.</summary>
		/// <value><see langword="true" /> if the context uses protected memory; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool ProtectedContext { get; set; }

		/// <summary>Gets or sets the callback invoked when Skia detects that the Vulkan device has been lost.</summary>
		/// <value>The handler to invoke, or <see langword="null" /> to receive no notification.</value>
		/// <remarks>This <see cref="T:SkiaSharp.GRVkBackendContext" /> must outlive any <see cref="T:SkiaSharp.GRContext" /> created from it, because Skia stores the callback without taking ownership. Assigning a new handler replaces the previous one.</remarks>
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
