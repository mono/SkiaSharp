using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_allocator_default_options_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRVkAllocatorDefaultOptionsNative : IEquatable<GRVkAllocatorDefaultOptionsNative> {
		// public vk_instance_t* fInstance
		public vk_instance_t fInstance;

		// public vk_physical_device_t* fPhysicalDevice
		public vk_physical_device_t fPhysicalDevice;

		// public vk_device_t* fDevice
		public vk_device_t fDevice;

		// public uint32_t fMaxAPIVersion
		public UInt32 fMaxAPIVersion;

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

		// public bool fThreadSafe
		public Byte fThreadSafe;

		public readonly bool Equals (GRVkAllocatorDefaultOptionsNative obj) =>
#pragma warning disable CS8909
			fInstance == obj.fInstance && fPhysicalDevice == obj.fPhysicalDevice && fDevice == obj.fDevice && fMaxAPIVersion == obj.fMaxAPIVersion && fGetProc == obj.fGetProc && fGetProcUserData == obj.fGetProcUserData && fProtectedContext == obj.fProtectedContext && fThreadSafe == obj.fThreadSafe;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkAllocatorDefaultOptionsNative f && Equals (f);

		public static bool operator == (GRVkAllocatorDefaultOptionsNative left, GRVkAllocatorDefaultOptionsNative right) =>
			left.Equals (right);

		public static bool operator != (GRVkAllocatorDefaultOptionsNative left, GRVkAllocatorDefaultOptionsNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fInstance);
			hash.Add (fPhysicalDevice);
			hash.Add (fDevice);
			hash.Add (fMaxAPIVersion);
			hash.Add (fGetProc);
			hash.Add (fGetProcUserData);
			hash.Add (fProtectedContext);
			hash.Add (fThreadSafe);
			return hash.ToHashCode ();
		}

	}
}
