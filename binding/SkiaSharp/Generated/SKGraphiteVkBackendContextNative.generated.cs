using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_vk_backend_context_init_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKGraphiteVkBackendContextNative : IEquatable<SKGraphiteVkBackendContextNative> {
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

		public readonly bool Equals (SKGraphiteVkBackendContextNative obj) =>
#pragma warning disable CS8909
			fInstance == obj.fInstance && fPhysicalDevice == obj.fPhysicalDevice && fDevice == obj.fDevice && fQueue == obj.fQueue && fGraphicsQueueIndex == obj.fGraphicsQueueIndex && fMaxAPIVersion == obj.fMaxAPIVersion && fGetProc == obj.fGetProc && fGetProcUserData == obj.fGetProcUserData && fProtectedContext == obj.fProtectedContext;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteVkBackendContextNative f && Equals (f);

		public static bool operator == (SKGraphiteVkBackendContextNative left, SKGraphiteVkBackendContextNative right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteVkBackendContextNative left, SKGraphiteVkBackendContextNative right) =>
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
			hash.Add (fGetProc);
			hash.Add (fGetProcUserData);
			hash.Add (fProtectedContext);
			return hash.ToHashCode ();
		}

	}
}
