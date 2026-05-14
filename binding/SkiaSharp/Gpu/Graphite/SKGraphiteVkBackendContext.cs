#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>
	/// Caller-supplied Vulkan handles + procedure-address callback used to bring up a
	/// Graphite Vulkan context. Construct, populate the properties, then pass to
	/// <see cref="SKGraphiteContext.CreateVulkan(SKGraphiteVkBackendContext)"/>.
	///
	/// After a successful CreateVulkan call the caller may safely <see cref="Dispose"/>
	/// this object; the Vulkan handles themselves
	/// (<see cref="VkInstance"/>/<see cref="VkDevice"/>/<see cref="VkQueue"/>) are never
	/// freed by SkiaSharp and remain owned by the caller for the lifetime of the
	/// SKGraphiteContext.
	/// </summary>
	public unsafe class SKGraphiteVkBackendContext : IDisposable
	{
		private SKGraphiteVkGetProcedureAddressDelegate getProc;
		private GCHandle getProcHandle;
		private void* getProcContext;

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
			};
		}

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
	}
}
