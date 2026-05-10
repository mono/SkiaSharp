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
	/// On a successful CreateVulkan call, ownership of the GetProc delegate's pinned handle
	/// AND the underlying native backend-context wrapper transfer to the SKGraphiteContext.
	/// The caller may safely <see cref="Dispose"/> this object immediately afterwards;
	/// double-disposal is a no-op. The Vulkan handles themselves
	/// (<see cref="VkInstance"/>/<see cref="VkDevice"/>/<see cref="VkQueue"/>) are never
	/// freed by SkiaSharp — they remain owned by the caller for the lifetime of the
	/// SKGraphiteContext.
	/// </summary>
	public unsafe class SKGraphiteVkBackendContext : IDisposable
	{
		private SKGraphiteVkGetProcedureAddressDelegate getProc;
		private GCHandle getProcHandle;
		private void* getProcContext;
		private IntPtr nativeBackendContext;

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

		/// <summary>
		/// Internal: lazily creates the native backend-context handle. Allocated once and reused.
		/// Released either on <see cref="Dispose()"/> or via
		/// <see cref="ReleaseNativeHandle"/> when ownership transfers to a SKGraphiteContext.
		/// </summary>
		internal IntPtr Handle {
			get {
				if (nativeBackendContext == IntPtr.Zero) {
					var native = ToNative ();
					nativeBackendContext = SkiaApi.sk_graphite_vk_backend_context_new (&native);
				}
				return nativeBackendContext;
			}
		}

		/// <summary>
		/// Variant-A ownership transfer: hand the GCHandle pinning the GetProc delegate to a
		/// caller (typically <see cref="SKGraphiteContext.CreateVulkan"/>). Returns the current
		/// handle and zeros out the internal state so subsequent Dispose calls are no-ops.
		/// </summary>
		internal GCHandle TransferGetProcHandle ()
		{
			var h = getProcHandle;
			getProcHandle = default;
			getProcContext = null;
			// Note: getProc field (the user's Action) stays — we keep the public reference
			// for getter consistency, but the GCHandle that pins it is now owned elsewhere.
			return h;
		}

		/// <summary>
		/// Release the lazy native <c>sk_graphite_vk_backend_context_t*</c> wrapper. Safe to call
		/// even if no handle was ever materialized. Used by
		/// <see cref="SKGraphiteContext.CreateVulkan"/> after the Skia context has captured
		/// the GetProc by value into its dispatch lambda — the wrapper has nothing left to hold.
		/// </summary>
		internal void ReleaseNativeHandle ()
		{
			if (nativeBackendContext != IntPtr.Zero) {
				SkiaApi.sk_graphite_vk_backend_context_delete (nativeBackendContext);
				nativeBackendContext = IntPtr.Zero;
			}
		}

		internal SKGraphiteVkBackendContextNative ToNative () =>
			new SKGraphiteVkBackendContextNative {
				fInstance           = VkInstance,
				fPhysicalDevice     = VkPhysicalDevice,
				fDevice             = VkDevice,
				fQueue              = VkQueue,
				fGraphicsQueueIndex = GraphicsQueueIndex,
				fMaxAPIVersion      = MaxApiVersion,
				fGetProcUserData    = getProcContext,
#if USE_LIBRARY_IMPORT
				fGetProc            = getProcContext is not null ? DelegateProxies.SKGraphiteVkGetProxy : null,
#else
				fGetProc            = getProcContext is not null ? DelegateProxies.SKGraphiteVkGetProxy : null,
#endif
				fProtectedContext   = ProtectedContext ? (byte)1 : (byte)0,
			};

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				ReleaseNativeHandle ();
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
