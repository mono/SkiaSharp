using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_device_fault_vendor_info_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRVkDeviceFaultVendorInfoNative : IEquatable<GRVkDeviceFaultVendorInfoNative> {
		// public char[256] fDescription
		public /* char */ void* fDescription;

		// public uint64_t fVendorFaultCode
		public UInt64 fVendorFaultCode;

		// public uint64_t fVendorFaultData
		public UInt64 fVendorFaultData;

		public readonly bool Equals (GRVkDeviceFaultVendorInfoNative obj) =>
#pragma warning disable CS8909
			fDescription == obj.fDescription && fVendorFaultCode == obj.fVendorFaultCode && fVendorFaultData == obj.fVendorFaultData;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkDeviceFaultVendorInfoNative f && Equals (f);

		public static bool operator == (GRVkDeviceFaultVendorInfoNative left, GRVkDeviceFaultVendorInfoNative right) =>
			left.Equals (right);

		public static bool operator != (GRVkDeviceFaultVendorInfoNative left, GRVkDeviceFaultVendorInfoNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDescription);
			hash.Add (fVendorFaultCode);
			hash.Add (fVendorFaultData);
			return hash.ToHashCode ();
		}

	}
}
