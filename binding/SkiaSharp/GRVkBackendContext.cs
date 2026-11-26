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
