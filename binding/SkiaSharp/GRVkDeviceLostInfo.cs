#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	/// <summary>
	/// Kind of fault reported for a specific memory address. Mirrors
	/// <c>VkDeviceFaultAddressTypeEXT</c> from the <c>VK_EXT_device_fault</c> extension.
	/// </summary>
	public enum GRVkDeviceFaultAddressType : int
	{
		/// <summary>No fault address was reported.</summary>
		None                       = 0,
		/// <summary>The device attempted to read an invalid address.</summary>
		ReadInvalid                = 1,
		/// <summary>The device attempted to write an invalid address.</summary>
		WriteInvalid               = 2,
		/// <summary>The device attempted to execute an invalid address.</summary>
		ExecuteInvalid             = 3,
		/// <summary>The instruction pointer at the time of the fault is not known.</summary>
		InstructionPointerUnknown  = 4,
		/// <summary>The instruction pointer at the time of the fault was invalid.</summary>
		InstructionPointerInvalid  = 5,
		/// <summary>The instruction pointer at the time of the fault is reported in the address.</summary>
		InstructionPointerFault    = 6,
	}

	/// <summary>
	/// Single fault-address record from <c>VK_EXT_device_fault</c>. Mirrors
	/// <c>VkDeviceFaultAddressInfoEXT</c>.
	/// </summary>
	public readonly struct GRVkDeviceFaultAddressInfo : IEquatable<GRVkDeviceFaultAddressInfo>
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.GRVkDeviceFaultAddressInfo" /> structure.</summary>
		/// <param name="addressType">The kind of fault reported for the address.</param>
		/// <param name="reportedAddress">The address reported by the driver.</param>
		/// <param name="addressPrecision">The power-of-two precision of the reported address.</param>
		/// <remarks />
		public GRVkDeviceFaultAddressInfo (GRVkDeviceFaultAddressType addressType, ulong reportedAddress, ulong addressPrecision)
		{
			AddressType      = addressType;
			ReportedAddress  = reportedAddress;
			AddressPrecision = addressPrecision;
		}

		/// <summary>Gets the kind of fault reported for this address.</summary>
		/// <value>The kind of fault reported.</value>
		/// <remarks />
		public GRVkDeviceFaultAddressType AddressType { get; }

		/// <summary>Gets the address reported by the driver.</summary>
		/// <value>The reported address.</value>
		/// <remarks />
		public ulong ReportedAddress { get; }

		/// <summary>Gets the power-of-two precision of the reported address.</summary>
		/// <value>The precision of the reported address.</value>
		/// <remarks />
		public ulong AddressPrecision { get; }

		/// <summary>Indicates whether this instance is equal to another instance of the same type.</summary>
		/// <param name="other">The instance to compare with this instance.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise <see langword="false" />.</returns>
		/// <remarks />
		public bool Equals (GRVkDeviceFaultAddressInfo other) =>
			AddressType == other.AddressType &&
			ReportedAddress == other.ReportedAddress &&
			AddressPrecision == other.AddressPrecision;

		/// <summary>Indicates whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if the object is an instance of the same type and is equal; otherwise <see langword="false" />.</returns>
		/// <remarks />
		public override bool Equals (object obj) =>
			obj is GRVkDeviceFaultAddressInfo o && Equals (o);

		/// <summary>Determines whether two specified instances are equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GRVkDeviceFaultAddressInfo left, GRVkDeviceFaultAddressInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified instances are not equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GRVkDeviceFaultAddressInfo left, GRVkDeviceFaultAddressInfo right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A hash code for the current instance.</returns>
		/// <remarks />
		public override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add ((int)AddressType);
			hash.Add (ReportedAddress);
			hash.Add (AddressPrecision);
			return hash.ToHashCode ();
		}
	}

	/// <summary>
	/// Single vendor-specific fault record from <c>VK_EXT_device_fault</c>. Mirrors
	/// <c>VkDeviceFaultVendorInfoEXT</c>.
	/// </summary>
	public readonly struct GRVkDeviceFaultVendorInfo : IEquatable<GRVkDeviceFaultVendorInfo>
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.GRVkDeviceFaultVendorInfo" /> structure.</summary>
		/// <param name="description">The driver-supplied description of the fault.</param>
		/// <param name="vendorFaultCode">The vendor-specific fault code.</param>
		/// <param name="vendorFaultData">The vendor-specific fault data.</param>
		/// <remarks />
		public GRVkDeviceFaultVendorInfo (string description, ulong vendorFaultCode, ulong vendorFaultData)
		{
			Description     = description ?? string.Empty;
			VendorFaultCode = vendorFaultCode;
			VendorFaultData = vendorFaultData;
		}

		/// <summary>Gets the driver-supplied description of the fault.</summary>
		/// <value>The description reported by the driver.</value>
		/// <remarks />
		public string Description { get; }

		/// <summary>Gets the vendor-specific fault code.</summary>
		/// <value>An opaque vendor-defined code.</value>
		/// <remarks />
		public ulong VendorFaultCode { get; }

		/// <summary>Gets the vendor-specific fault data.</summary>
		/// <value>An opaque vendor-defined value.</value>
		/// <remarks />
		public ulong VendorFaultData { get; }

		/// <summary>Indicates whether this instance is equal to another instance of the same type.</summary>
		/// <param name="other">The instance to compare with this instance.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise <see langword="false" />.</returns>
		/// <remarks />
		public bool Equals (GRVkDeviceFaultVendorInfo other) =>
			Description == other.Description &&
			VendorFaultCode == other.VendorFaultCode &&
			VendorFaultData == other.VendorFaultData;

		/// <summary>Indicates whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if the object is an instance of the same type and is equal; otherwise <see langword="false" />.</returns>
		/// <remarks />
		public override bool Equals (object obj) =>
			obj is GRVkDeviceFaultVendorInfo o && Equals (o);

		/// <summary>Determines whether two specified instances are equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GRVkDeviceFaultVendorInfo left, GRVkDeviceFaultVendorInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified instances are not equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GRVkDeviceFaultVendorInfo left, GRVkDeviceFaultVendorInfo right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A hash code for the current instance.</returns>
		/// <remarks />
		public override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (Description);
			hash.Add (VendorFaultCode);
			hash.Add (VendorFaultData);
			return hash.ToHashCode ();
		}
	}

	/// <summary>
	/// Diagnostic information reported by Skia when it detects <c>VK_ERROR_DEVICE_LOST</c>.
	/// </summary>
	public sealed unsafe class GRVkDeviceLostInfo : SKObject, ISKSkipObjectRegistration
	{
		private const int VkMaxDescriptionSize = 256;

		private string description;
		private GRVkDeviceFaultAddressInfo[] addressInfos;
		private GRVkDeviceFaultVendorInfo[] vendorInfos;
		private byte[] vendorBinaryData;

		internal GRVkDeviceLostInfo (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		internal static GRVkDeviceLostInfo GetObject (IntPtr handle, bool owns = false) =>
			handle == IntPtr.Zero ? null : new GRVkDeviceLostInfo (handle, owns);

		/// <summary>Gets the human-readable description from Skia.</summary>
		/// <value>The description string, or <see cref="F:System.String.Empty" /> if none was reported.</value>
		/// <remarks />
		public string Description =>
			description ??= GetDescription ();

		/// <summary>Gets the fault address records.</summary>
		/// <value>The fault address records, or an empty array when <c>VK_EXT_device_fault</c> is not enabled.</value>
		/// <remarks />
		public GRVkDeviceFaultAddressInfo[] AddressInfos =>
			addressInfos ??= GetAddressInfos ();

		/// <summary>Gets the vendor-specific fault records.</summary>
		/// <value>The vendor fault records, or an empty array when <c>VK_EXT_device_fault</c> is not enabled.</value>
		/// <remarks />
		public GRVkDeviceFaultVendorInfo[] VendorInfos =>
			vendorInfos ??= GetVendorInfos ();

		/// <summary>Gets the raw vendor binary blob for offline decoding.</summary>
		/// <value>The vendor binary data, or an empty array when unavailable.</value>
		/// <remarks />
		public byte[] VendorBinaryData =>
			vendorBinaryData ??= GetVendorBinaryData ();

		private string GetDescription ()
		{
			if (Handle == IntPtr.Zero)
				return string.Empty;

			using var str = SKString.GetObject (SkiaApi.gr_vk_device_lost_info_get_description (Handle));
			return (string)str ?? string.Empty;
		}

		private GRVkDeviceFaultAddressInfo[] GetAddressInfos ()
		{
			if (Handle == IntPtr.Zero)
				return Array.Empty<GRVkDeviceFaultAddressInfo> ();

			var count = SkiaApi.gr_vk_device_lost_info_get_address_info_count (Handle);
			if (count <= 0)
				return Array.Empty<GRVkDeviceFaultAddressInfo> ();

			var result = new GRVkDeviceFaultAddressInfo[count];
			for (var i = 0; i < count; i++) {
				GRVkDeviceFaultAddressInfoNative addr;
				SkiaApi.gr_vk_device_lost_info_get_address_info (Handle, i, &addr);
				result[i] = new GRVkDeviceFaultAddressInfo (
					(GRVkDeviceFaultAddressType)addr.fAddressType,
					addr.fReportedAddress,
					addr.fAddressPrecision);
			}
			return result;
		}

		private GRVkDeviceFaultVendorInfo[] GetVendorInfos ()
		{
			if (Handle == IntPtr.Zero)
				return Array.Empty<GRVkDeviceFaultVendorInfo> ();

			var count = SkiaApi.gr_vk_device_lost_info_get_vendor_info_count (Handle);
			if (count <= 0)
				return Array.Empty<GRVkDeviceFaultVendorInfo> ();

			var result = new GRVkDeviceFaultVendorInfo[count];
			for (var i = 0; i < count; i++) {
				GRVkDeviceFaultVendorInfoNative vendor;
				SkiaApi.gr_vk_device_lost_info_get_vendor_info (Handle, i, &vendor);
				var span = new ReadOnlySpan<byte> (vendor.fDescription, VkMaxDescriptionSize);
				var nul = span.IndexOf ((byte)0);
				if (nul >= 0)
					span = span.Slice (0, nul);
				var vendorDesc = span.Length > 0
					? StringUtilities.GetString (span, SKTextEncoding.Utf8)
					: string.Empty;
				result[i] = new GRVkDeviceFaultVendorInfo (
					vendorDesc,
					vendor.fVendorFaultCode,
					vendor.fVendorFaultData);
			}
			return result;
		}

		private byte[] GetVendorBinaryData ()
		{
			if (Handle == IntPtr.Zero)
				return Array.Empty<byte> ();

			IntPtr size = IntPtr.Zero;
			var ptr = SkiaApi.gr_vk_device_lost_info_get_vendor_binary_data (Handle, &size);
			if (ptr == null || (int)size <= 0)
				return Array.Empty<byte> ();

			var result = new byte[(int)size];
			Marshal.Copy ((IntPtr)ptr, result, 0, result.Length);
			return result;
		}
	}
}
