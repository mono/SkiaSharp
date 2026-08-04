using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_device_lost_info_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRVkDeviceLostInfoNative : IEquatable<GRVkDeviceLostInfoNative> {
		// public const char* fDescription
		public /* char */ void* fDescription;

		// public const gr_vk_device_fault_address_info_t* fAddressInfos
		public GRVkDeviceFaultAddressInfoNative* fAddressInfos;

		// public int32_t fAddressInfoCount
		public Int32 fAddressInfoCount;

		// public const gr_vk_device_fault_vendor_info_t* fVendorInfos
		public GRVkDeviceFaultVendorInfoNative* fVendorInfos;

		// public int32_t fVendorInfoCount
		public Int32 fVendorInfoCount;

		// public const void* fVendorBinaryData
		public void* fVendorBinaryData;

		// public size_t fVendorBinaryDataSize
		public /* size_t */ IntPtr fVendorBinaryDataSize;

		public readonly bool Equals (GRVkDeviceLostInfoNative obj) =>
#pragma warning disable CS8909
			fDescription == obj.fDescription && fAddressInfos == obj.fAddressInfos && fAddressInfoCount == obj.fAddressInfoCount && fVendorInfos == obj.fVendorInfos && fVendorInfoCount == obj.fVendorInfoCount && fVendorBinaryData == obj.fVendorBinaryData && fVendorBinaryDataSize == obj.fVendorBinaryDataSize;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkDeviceLostInfoNative f && Equals (f);

		public static bool operator == (GRVkDeviceLostInfoNative left, GRVkDeviceLostInfoNative right) =>
			left.Equals (right);

		public static bool operator != (GRVkDeviceLostInfoNative left, GRVkDeviceLostInfoNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDescription);
			hash.Add (fAddressInfos);
			hash.Add (fAddressInfoCount);
			hash.Add (fVendorInfos);
			hash.Add (fVendorInfoCount);
			hash.Add (fVendorBinaryData);
			hash.Add (fVendorBinaryDataSize);
			return hash.ToHashCode ();
		}

	}
}
