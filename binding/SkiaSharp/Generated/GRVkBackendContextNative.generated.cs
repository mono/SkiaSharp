using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_backendcontext_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRVkBackendContextNative : IEquatable<GRVkBackendContextNative> {
		// public vk_instance_t* fInstance
		public vk_instance_t fInstance;

		// public vk_physical_device_t* fPhysicalDevice
		public vk_physical_device_t fPhysicalDevice;

		// public vk_device_t* fDevice
		public vk_device_t fDevice;

		// public vk_queue_t* fQueue
		public vk_queue_t fQueue;

		// public uint32_t fGraphicsQueueIndex
		public UInt32 fGraphicsQueueIndex;

		// public uint32_t fMaxAPIVersion
		public UInt32 fMaxAPIVersion;

		// public const gr_vk_extensions_t* fVkExtensions
		public gr_vk_extensions_t fVkExtensions;

		// public const vk_physical_device_features_t* fDeviceFeatures
		public vk_physical_device_features_t fDeviceFeatures;

		// public const vk_physical_device_features_2_t* fDeviceFeatures2
		public vk_physical_device_features_2_t fDeviceFeatures2;

		// public gr_vk_memory_allocator_t* fMemoryAllocator
		public gr_vk_memory_allocator_t fMemoryAllocator;

		// public gr_vk_get_proc fGetProc
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <void*, /* char */ void*, vk_instance_t, vk_device_t, IntPtr> fGetProc;
#else
		public GRVkGetProcProxyDelegate fGetProc;
#endif

		// public void* fGetProcUserData
		public void* fGetProcUserData;

		// public bool fProtectedContext
		public Byte fProtectedContext;

		public readonly bool Equals (GRVkBackendContextNative obj) =>
#pragma warning disable CS8909
			fInstance == obj.fInstance && fPhysicalDevice == obj.fPhysicalDevice && fDevice == obj.fDevice && fQueue == obj.fQueue && fGraphicsQueueIndex == obj.fGraphicsQueueIndex && fMaxAPIVersion == obj.fMaxAPIVersion && fVkExtensions == obj.fVkExtensions && fDeviceFeatures == obj.fDeviceFeatures && fDeviceFeatures2 == obj.fDeviceFeatures2 && fMemoryAllocator == obj.fMemoryAllocator && fGetProc == obj.fGetProc && fGetProcUserData == obj.fGetProcUserData && fProtectedContext == obj.fProtectedContext;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkBackendContextNative f && Equals (f);

		public static bool operator == (GRVkBackendContextNative left, GRVkBackendContextNative right) =>
			left.Equals (right);

		public static bool operator != (GRVkBackendContextNative left, GRVkBackendContextNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fInstance);
			hash.Add (fPhysicalDevice);
			hash.Add (fDevice);
			hash.Add (fQueue);
			hash.Add (fGraphicsQueueIndex);
			hash.Add (fMaxAPIVersion);
			hash.Add (fVkExtensions);
			hash.Add (fDeviceFeatures);
			hash.Add (fDeviceFeatures2);
			hash.Add (fMemoryAllocator);
			hash.Add (fGetProc);
			hash.Add (fGetProcUserData);
			hash.Add (fProtectedContext);
			return hash.ToHashCode ();
		}

	}
}
