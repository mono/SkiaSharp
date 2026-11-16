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

		// Device-lost callback state (mono/SkiaSharp#4601).
		//
		// The GCHandle pins the caller's delegate and nativeDeviceLostHandler is the native
		// bridge from gr_vk_device_lost_handler_new. Neither is ever freed, and that is
		// deliberate: Skia copies the bridge pointer into GrVkGpu / VulkanSharedContext and
		// stores it raw and non-owning, then gives no notification when the last holder is
		// destroyed - so there is no point at which freeing is provably safe. A C++ embedder
		// has the same contract and simply points fDeviceLostContext at something long-lived.
		// The cost is one small allocation per backend context that opts in.
		//
		// DeviceLost is consequently init-only: there is no replacement path that could free a
		// bridge a live GRContext still points at.
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
				// The device-lost bridge and its GCHandle are intentionally NOT released here:
				// a GRContext created from this backend keeps a raw pointer to the bridge and may
				// call it at any time. See the field comment above.
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
			init {
				// init-only on purpose. Skia copies the bridge pointer into the native context and
				// keeps it raw; a settable property would let a later assignment free a bridge that a
				// live GRContext still points at (use-after-free), while the replacement would never
				// reach that context anyway - the backend context is a creation-time descriptor, so
				// mutating any of its fields after CreateVulkan has no effect on an existing context.
				deviceLost = value;
				if (value is null)
					return;

				DelegateProxies.Create (value, out var gch, out var ctx);
				deviceLostHandle = gch;
				nativeDeviceLostHandler = SkiaApi.gr_vk_device_lost_handler_new (
					DelegateProxies.GRVkDeviceLostProxy,
					(void*)ctx);
				if (nativeDeviceLostHandler == IntPtr.Zero) {
					// Nothing was handed to Skia, so releasing the pin here is safe.
					gch.Free ();
					deviceLostHandle = default;
					deviceLost = null;
					throw new InvalidOperationException (
						"gr_vk_device_lost_handler_new failed (Vulkan not built into libSkiaSharp?)");
				}
			}
		}
		// Revision 1 of the native struct: frozen ABI, no device-lost field. Used by the
		// original gr_direct_context_make_vulkan* entry points.
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
			};

		// Revision 2: adds the device-lost handler. fStructSize lets the native side tell which
		// revision it was handed, so a future field needs no further entry point.
		internal GRVkBackendContextV2Native ToNativeV2 () =>
			new GRVkBackendContextV2Native {
				fStructSize = (IntPtr)Marshal.SizeOf<GRVkBackendContextV2Native> (),
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
