#nullable disable

using System;

namespace SkiaSharp
{
	// Managed shape of Skia's VulkanDeviceLostProc payload. See #4601. All arrays are
	// snapshots taken during the callback — the wrapper owns its data, so a delegate
	// may store the received GRVkDeviceLostInfo past the return of the callback.

	/// <summary>
	/// Kind of fault reported for a specific memory address. Mirrors
	/// <c>VkDeviceFaultAddressTypeEXT</c> from the <c>VK_EXT_device_fault</c> extension.
	/// </summary>
	public enum GRVkDeviceFaultAddressType : int
	{
		None                       = 0,
		ReadInvalid                = 1,
		WriteInvalid               = 2,
		ExecuteInvalid             = 3,
		InstructionPointerUnknown  = 4,
		InstructionPointerInvalid  = 5,
		InstructionPointerFault    = 6,
	}

	/// <summary>
	/// Single fault-address record from <c>VK_EXT_device_fault</c>. Mirrors
	/// <c>VkDeviceFaultAddressInfoEXT</c>.
	/// </summary>
	public readonly struct GRVkDeviceFaultAddressInfo : IEquatable<GRVkDeviceFaultAddressInfo>
	{
		public GRVkDeviceFaultAddressInfo (GRVkDeviceFaultAddressType addressType, ulong reportedAddress, ulong addressPrecision)
		{
			AddressType      = addressType;
			ReportedAddress  = reportedAddress;
			AddressPrecision = addressPrecision;
		}

		public GRVkDeviceFaultAddressType AddressType { get; }

		public ulong ReportedAddress { get; }

		public ulong AddressPrecision { get; }

		public bool Equals (GRVkDeviceFaultAddressInfo other) =>
			AddressType == other.AddressType &&
			ReportedAddress == other.ReportedAddress &&
			AddressPrecision == other.AddressPrecision;

		public override bool Equals (object obj) =>
			obj is GRVkDeviceFaultAddressInfo o && Equals (o);

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
	/// <c>VkDeviceFaultVendorInfoEXT</c>. The <see cref="Description"/> is the
	/// driver-supplied string (up to 256 characters); <see cref="VendorFaultCode"/>
	/// and <see cref="VendorFaultData"/> are opaque vendor values.
	/// </summary>
	public readonly struct GRVkDeviceFaultVendorInfo : IEquatable<GRVkDeviceFaultVendorInfo>
	{
		public GRVkDeviceFaultVendorInfo (string description, ulong vendorFaultCode, ulong vendorFaultData)
		{
			Description     = description;
			VendorFaultCode = vendorFaultCode;
			VendorFaultData = vendorFaultData;
		}

		public string Description { get; }

		public ulong VendorFaultCode { get; }

		public ulong VendorFaultData { get; }

		public bool Equals (GRVkDeviceFaultVendorInfo other) =>
			Description == other.Description &&
			VendorFaultCode == other.VendorFaultCode &&
			VendorFaultData == other.VendorFaultData;

		public override bool Equals (object obj) =>
			obj is GRVkDeviceFaultVendorInfo o && Equals (o);

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
	/// Everything Skia's <c>VulkanDeviceLostProc</c> hands the caller when it detects
	/// <c>VK_ERROR_DEVICE_LOST</c>. The instance owns all its data — the arrays are
	/// snapshots taken during the callback — so a delegate may safely store this
	/// object past the return of the callback (e.g. to write it to a log later).
	/// </summary>
	public sealed class GRVkDeviceLostInfo
	{
		public GRVkDeviceLostInfo (
			string description,
			GRVkDeviceFaultAddressInfo[] addressInfos,
			GRVkDeviceFaultVendorInfo[] vendorInfos,
			byte[] vendorBinaryData)
		{
			Description      = description ?? string.Empty;
			AddressInfos     = addressInfos ?? Array.Empty<GRVkDeviceFaultAddressInfo> ();
			VendorInfos      = vendorInfos ?? Array.Empty<GRVkDeviceFaultVendorInfo> ();
			VendorBinaryData = vendorBinaryData ?? Array.Empty<byte> ();
		}

		/// <summary>Human-readable description from Skia. Empty string is possible but never null.</summary>
		public string Description { get; }

		/// <summary>Fault address records (empty when <c>VK_EXT_device_fault</c> is not enabled).</summary>
		public GRVkDeviceFaultAddressInfo[] AddressInfos { get; }

		/// <summary>Vendor-specific fault records (empty when <c>VK_EXT_device_fault</c> is not enabled).</summary>
		public GRVkDeviceFaultVendorInfo[] VendorInfos { get; }

		/// <summary>Raw vendor binary blob for offline decoding (empty when unavailable).</summary>
		public byte[] VendorBinaryData { get; }
	}
}
