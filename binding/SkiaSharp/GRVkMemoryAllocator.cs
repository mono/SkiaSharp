#nullable disable

using System;

namespace SkiaSharp
{
	// Public wrapper around skgpu::VulkanMemoryAllocator (see gr_vk_allocator.h).
	// Implemented as ISKReferenceCounted so SKObject's dispose path calls
	// sk_refcnt_safe_unref on our handle — that's the same virtual-refcount
	// machinery every other SkRefCnt-derived Skia type uses in these bindings.
	//
	// Ganesh (GRVkBackendContext.MemoryAllocator) and Graphite
	// (SKGraphiteVkBackendContext.MemoryAllocator) both consume this handle.
	// A Context created from a backend context takes its own internal ref, so
	// disposing the allocator after context creation is safe.
	/// <summary>Represents a Vulkan memory allocator that Skia uses to sub-allocate device memory.</summary>
	/// <remarks />
	public unsafe class GRVkMemoryAllocator : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
	{
		internal GRVkMemoryAllocator (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// Build Skia's default VMA-backed allocator. `getProcedureAddress` is used
		// synchronously during allocator construction to resolve Vulkan entry points
		// — its GCHandle can be released before this method returns because VMA
		// caches the resolved function pointers, not the getter itself.
		//
		// Returns null if Vulkan support is not compiled into libSkiaSharp or if
		// the underlying VMA initialisation refused the provided device/instance.
		/// <summary>Creates the default Vulkan memory allocator.</summary>
		/// <param name="vkInstance">A handle to the Vulkan instance.</param>
		/// <param name="vkPhysicalDevice">A handle to the Vulkan physical device.</param>
		/// <param name="vkDevice">A handle to the Vulkan logical device.</param>
		/// <param name="maxApiVersion">The highest Vulkan API version the allocator may use.</param>
		/// <param name="getProcedureAddress">The delegate that resolves Vulkan function addresses by name.</param>
		/// <returns>The new allocator, or <see langword="null" /> if Vulkan support is not available or initialization failed.</returns>
		/// <remarks>The returned allocator may be disposed once the context that uses it has been created, because the context takes its own reference.</remarks>
		public static GRVkMemoryAllocator CreateDefault (
			IntPtr vkInstance,
			IntPtr vkPhysicalDevice,
			IntPtr vkDevice,
			uint maxApiVersion,
			GRVkGetProcedureAddressDelegate getProcedureAddress) =>
			CreateDefault (vkInstance, vkPhysicalDevice, vkDevice, maxApiVersion, getProcedureAddress, false, false);

		/// <summary>Creates the default Vulkan memory allocator, optionally making it thread-safe.</summary>
		/// <param name="vkInstance">A handle to the Vulkan instance.</param>
		/// <param name="vkPhysicalDevice">A handle to the Vulkan physical device.</param>
		/// <param name="vkDevice">A handle to the Vulkan logical device.</param>
		/// <param name="maxApiVersion">The highest Vulkan API version the allocator may use.</param>
		/// <param name="getProcedureAddress">The delegate that resolves Vulkan function addresses by name.</param>
		/// <param name="threadSafe"><see langword="true" /> if the allocator may be used from more than one thread; otherwise <see langword="false" />.</param>
		/// <returns>The new allocator, or <see langword="null" /> if Vulkan support is not available or initialization failed.</returns>
		/// <remarks>The returned allocator may be disposed once the context that uses it has been created, because the context takes its own reference.</remarks>
		public static GRVkMemoryAllocator CreateDefault (
			IntPtr vkInstance,
			IntPtr vkPhysicalDevice,
			IntPtr vkDevice,
			uint maxApiVersion,
			GRVkGetProcedureAddressDelegate getProcedureAddress,
			bool threadSafe) =>
			CreateDefault (vkInstance, vkPhysicalDevice, vkDevice, maxApiVersion, getProcedureAddress, threadSafe, false);

		/// <summary>Creates the default Vulkan memory allocator, optionally making it thread-safe and serving protected memory.</summary>
		/// <param name="vkInstance">A handle to the Vulkan instance.</param>
		/// <param name="vkPhysicalDevice">A handle to the Vulkan physical device.</param>
		/// <param name="vkDevice">A handle to the Vulkan logical device.</param>
		/// <param name="maxApiVersion">The highest Vulkan API version the allocator may use.</param>
		/// <param name="getProcedureAddress">The delegate that resolves Vulkan function addresses by name.</param>
		/// <param name="threadSafe"><see langword="true" /> if the allocator may be used from more than one thread; otherwise <see langword="false" />.</param>
		/// <param name="protectedContent"><see langword="true" /> if the allocator serves protected (DRM) memory; otherwise <see langword="false" />.</param>
		/// <returns>The new allocator, or <see langword="null" /> if Vulkan support is not available or initialization failed.</returns>
		/// <remarks>The returned allocator may be disposed once the context that uses it has been created, because the context takes its own reference.</remarks>
		public static GRVkMemoryAllocator CreateDefault (
			IntPtr vkInstance,
			IntPtr vkPhysicalDevice,
			IntPtr vkDevice,
			uint maxApiVersion,
			GRVkGetProcedureAddressDelegate getProcedureAddress,
			bool threadSafe,
			bool protectedContent)
		{
			if (vkInstance == IntPtr.Zero)
				throw new ArgumentException ("Must be non-null.", nameof (vkInstance));
			if (vkPhysicalDevice == IntPtr.Zero)
				throw new ArgumentException ("Must be non-null.", nameof (vkPhysicalDevice));
			if (vkDevice == IntPtr.Zero)
				throw new ArgumentException ("Must be non-null.", nameof (vkDevice));
			if (getProcedureAddress is null)
				throw new ArgumentNullException (nameof (getProcedureAddress));

			DelegateProxies.Create (getProcedureAddress, out var gch, out var ctx);
			try {
				var opts = new GRVkAllocatorDefaultOptionsNative {
					fInstance         = vkInstance,
					fPhysicalDevice   = vkPhysicalDevice,
					fDevice           = vkDevice,
					fMaxAPIVersion    = maxApiVersion,
					fGetProc          = DelegateProxies.GRVkGetProcProxy,
					fGetProcUserData  = (void*)ctx,
					fProtectedContext = protectedContent ? (byte)1 : (byte)0,
					fThreadSafe       = threadSafe ? (byte)1 : (byte)0,
				};
				var handle = SkiaApi.gr_vk_memory_allocator_make_default (opts);
				return handle == IntPtr.Zero ? null : new GRVkMemoryAllocator (handle, true);
			} finally {
				// fGetProc lifetime = Make call only. VMA holds resolved fn ptrs afterwards.
				gch.Free ();
			}
		}
	}
}
