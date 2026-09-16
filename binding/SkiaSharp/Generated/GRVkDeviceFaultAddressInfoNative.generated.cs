using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_device_fault_address_info_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRVkDeviceFaultAddressInfoNative : IEquatable<GRVkDeviceFaultAddressInfoNative> {
		// public int32_t fAddressType
		public Int32 fAddressType;

		// public uint64_t fReportedAddress
		public UInt64 fReportedAddress;

		// public uint64_t fAddressPrecision
		public UInt64 fAddressPrecision;

		public readonly bool Equals (GRVkDeviceFaultAddressInfoNative obj) =>
#pragma warning disable CS8909
			fAddressType == obj.fAddressType && fReportedAddress == obj.fReportedAddress && fAddressPrecision == obj.fAddressPrecision;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkDeviceFaultAddressInfoNative f && Equals (f);

		public static bool operator == (GRVkDeviceFaultAddressInfoNative left, GRVkDeviceFaultAddressInfoNative right) =>
			left.Equals (right);

		public static bool operator != (GRVkDeviceFaultAddressInfoNative left, GRVkDeviceFaultAddressInfoNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fAddressType);
			hash.Add (fReportedAddress);
			hash.Add (fAddressPrecision);
			return hash.ToHashCode ();
		}

	}
}
