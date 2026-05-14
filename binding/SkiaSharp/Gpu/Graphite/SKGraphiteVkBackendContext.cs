#nullable disable

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>
	/// Caller-supplied Vulkan handles + procedure-address callback used to bring up a
	/// Graphite Vulkan context. The Vulkan handles are never freed by SkiaSharp — caller
	/// retains ownership for the lifetime of the resulting <see cref="SKGraphiteContext"/>.
	/// </summary>
	public sealed unsafe class SKGraphiteVkBackendContext : IDisposable
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
		}

		~SKGraphiteVkBackendContext () => DisposeCore ();
	}
}
