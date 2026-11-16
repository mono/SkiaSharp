using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_vk_backend_context_init_v2_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKGraphiteVkBackendContextV2Native : IEquatable<SKGraphiteVkBackendContextV2Native> {
		// public size_t fStructSize
		public /* size_t */ IntPtr fStructSize;

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

		// public sk_graphite_vk_get_proc fGetProc
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <void*, /* char */ void*, vk_instance_t, vk_device_t, IntPtr> fGetProc;
#else
		public SKGraphiteVkGetProxyDelegate fGetProc;
#endif

		// public void* fGetProcUserData
		public void* fGetProcUserData;

		// public bool fProtectedContext
		public Byte fProtectedContext;

		// public gr_vk_device_lost_handler_t* fDeviceLostHandler
		public gr_vk_device_lost_handler_t fDeviceLostHandler;

		public readonly bool Equals (SKGraphiteVkBackendContextV2Native obj) =>
#pragma warning disable CS8909
			fStructSize == obj.fStructSize && fInstance == obj.fInstance && fPhysicalDevice == obj.fPhysicalDevice && fDevice == obj.fDevice && fQueue == obj.fQueue && fGraphicsQueueIndex == obj.fGraphicsQueueIndex && fMaxAPIVersion == obj.fMaxAPIVersion && fGetProc == obj.fGetProc && fGetProcUserData == obj.fGetProcUserData && fProtectedContext == obj.fProtectedContext && fDeviceLostHandler == obj.fDeviceLostHandler;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteVkBackendContextV2Native f && Equals (f);

		public static bool operator == (SKGraphiteVkBackendContextV2Native left, SKGraphiteVkBackendContextV2Native right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteVkBackendContextV2Native left, SKGraphiteVkBackendContextV2Native right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fStructSize);
			hash.Add (fInstance);
			hash.Add (fPhysicalDevice);
			hash.Add (fDevice);
			hash.Add (fQueue);
			hash.Add (fGraphicsQueueIndex);
			hash.Add (fMaxAPIVersion);
			hash.Add (fGetProc);
			hash.Add (fGetProcUserData);
			hash.Add (fProtectedContext);
			hash.Add (fDeviceLostHandler);
			return hash.ToHashCode ();
		}

	}
}
