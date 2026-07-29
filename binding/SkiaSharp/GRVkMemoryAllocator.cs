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
		public static GRVkMemoryAllocator CreateDefault (
			IntPtr vkInstance,
			IntPtr vkPhysicalDevice,
			IntPtr vkDevice,
			uint maxApiVersion,
			GRVkGetProcedureAddressDelegate getProcedureAddress,
			bool threadSafe = false,
			bool protectedContent = false)
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
