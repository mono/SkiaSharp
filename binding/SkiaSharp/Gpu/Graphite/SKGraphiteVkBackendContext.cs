#nullable disable

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>Supplies the Vulkan instance, devices, queue, and function loader used to create a Vulkan-backed <see cref="T:SkiaSharp.SKGraphiteContext" />.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Populate the Vulkan handles and the <xref:SkiaSharp.SKGraphiteVkBackendContext.GetProcedureAddress> loader, then pass this object to <xref:SkiaSharp.SKGraphiteContext.CreateVulkan(SkiaSharp.SKGraphiteVkBackendContext)>. Ownership of the delegate that keeps the function loader alive is transferred to the context, so you can dispose this object as soon as the context has been created.
	///
	/// This type implements `IDisposable`.
	/// ]]></format></remarks>
	public unsafe class SKGraphiteVkBackendContext : IDisposable
	{
		private SKGraphiteVkGetProcedureAddressDelegate getProc;
		private GCHandle getProcHandle;
		private void* getProcContext;

		/// <summary>Initializes a new instance of the <see cref="SKGraphiteVkBackendContext" /> class.</summary>
		/// <remarks />
		public SKGraphiteVkBackendContext ()
		{
		}

		/// <summary>Gets or sets the handle to the Vulkan instance.</summary>
		/// <value>A handle to the Vulkan instance.</value>
		/// <remarks />
		public IntPtr VkInstance { get; set; }

		/// <summary>Gets or sets the handle to the Vulkan physical device.</summary>
		/// <value>A handle to the Vulkan physical device.</value>
		/// <remarks />
		public IntPtr VkPhysicalDevice { get; set; }

		/// <summary>Gets or sets the handle to the Vulkan logical device.</summary>
		/// <value>A handle to the Vulkan logical device.</value>
		/// <remarks />
		public IntPtr VkDevice { get; set; }

		/// <summary>Gets or sets the handle to the Vulkan queue.</summary>
		/// <value>A handle to the Vulkan queue.</value>
		/// <remarks />
		public IntPtr VkQueue { get; set; }

		/// <summary>Gets or sets the index of the Vulkan queue family used for graphics operations.</summary>
		/// <value>The graphics queue family index.</value>
		/// <remarks />
		public uint GraphicsQueueIndex { get; set; }

		/// <summary>Gets or sets the maximum Vulkan API version that Skia may use.</summary>
		/// <value>The packed maximum Vulkan API version, or 0 to let Skia choose.</value>
		/// <remarks />
		public uint MaxApiVersion { get; set; }

		/// <summary>Gets or sets a value indicating whether the context uses Vulkan protected content.</summary>
		/// <value><see langword="true" /> if the context uses protected content; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool ProtectedContext { get; set; }

		/// <summary>Gets or sets the delegate that resolves Vulkan function addresses by name.</summary>
		/// <value>The Vulkan function loader delegate, or <see langword="null" /> if none is set.</value>
		/// <remarks />
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

		/// <summary>Releases the resources used by the current instance of the <see cref="T:SkiaSharp.SKGraphiteVkBackendContext" /> class.</summary>
		/// <remarks />
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKGraphiteVkBackendContext" /> before it is reclaimed by garbage collection.</summary>
		/// <remarks />
		~SKGraphiteVkBackendContext () => DisposeCore ();
	}
}
