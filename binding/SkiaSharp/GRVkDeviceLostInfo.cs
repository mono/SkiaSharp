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
	/// <c>VkDeviceFaultVendorInfoEXT</c>. The <see cref="Description"/> is the
	/// driver-supplied string (up to 256 characters); <see cref="VendorFaultCode"/>
	/// and <see cref="VendorFaultData"/> are opaque vendor values.
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
			Description     = description;
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
	/// Everything Skia's <c>VulkanDeviceLostProc</c> hands the caller when it detects
	/// <c>VK_ERROR_DEVICE_LOST</c>. The instance owns all its data — the arrays are
	/// snapshots taken during the callback — so a delegate may safely store this
	/// object past the return of the callback (e.g. to write it to a log later).
	/// </summary>
	public sealed class GRVkDeviceLostInfo
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.GRVkDeviceLostInfo" /> class.</summary>
		/// <param name="description">Skia's description of the device loss.</param>
		/// <param name="addressInfos">The fault-address records, or <see langword="null" /> for none.</param>
		/// <param name="vendorInfos">The vendor-specific fault records, or <see langword="null" /> for none.</param>
		/// <param name="vendorBinaryData">The raw vendor binary blob, or <see langword="null" /> for none.</param>
		/// <remarks />
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
