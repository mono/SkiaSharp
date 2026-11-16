using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	// Layout guards for the Vulkan device-lost interop structs (mono/SkiaSharp#4601 review).
	//
	// These types are marshalled across the C ABI, so a change in the binding generator or
	// in the C headers can silently shift a field and corrupt a device-loss report rather
	// than failing to compile. Pin the sizes and offsets here so such a change fails a test.
	public unsafe class GRVkDeviceLostLayoutTest : SKTest
	{
		// VK_MAX_DESCRIPTION_SIZE, mirrored by gr_vk_device_fault_vendor_info_t.fDescription.
		private const int VkMaxDescriptionSize = 256;

		[Fact]
		public void VendorInfoMatchesTheNativeLayout()
		{
			// char fDescription[256]; uint64_t fVendorFaultCode; uint64_t fVendorFaultData;
			//
			// The generator used to emit the fixed char array as a pointer, which made this
			// struct 24 bytes and put both uint64 fields at the wrong offsets. It must stay a
			// fixed buffer.
			Assert.Equal (VkMaxDescriptionSize + 8 + 8, Marshal.SizeOf<GRVkDeviceFaultVendorInfoNative> ());
			Assert.Equal (VkMaxDescriptionSize, (int)Marshal.OffsetOf<GRVkDeviceFaultVendorInfoNative> (nameof (GRVkDeviceFaultVendorInfoNative.fVendorFaultCode)));
			Assert.Equal (VkMaxDescriptionSize + 8, (int)Marshal.OffsetOf<GRVkDeviceFaultVendorInfoNative> (nameof (GRVkDeviceFaultVendorInfoNative.fVendorFaultData)));
		}

		[Fact]
		public void AddressInfoMatchesTheNativeLayout()
		{
			// int32_t fAddressType; uint64_t fReportedAddress; uint64_t fAddressPrecision;
			// (padded to 8-byte alignment by the trailing uint64 fields)
			Assert.Equal (8, (int)Marshal.OffsetOf<GRVkDeviceFaultAddressInfoNative> (nameof (GRVkDeviceFaultAddressInfoNative.fReportedAddress)));
			Assert.Equal (16, (int)Marshal.OffsetOf<GRVkDeviceFaultAddressInfoNative> (nameof (GRVkDeviceFaultAddressInfoNative.fAddressPrecision)));
		}

		[Fact]
		public void BackendContextV2PutsStructSizeFirst()
		{
			// The native side reads fStructSize before touching any revision-2 field, so it
			// must sit at offset 0 and the managed marshalled size must match what the C
			// struct reports — that is the whole basis of the versioned-struct ABI scheme.
			Assert.Equal (0, (int)Marshal.OffsetOf<GRVkBackendContextV2Native> (nameof (GRVkBackendContextV2Native.fStructSize)));
			Assert.Equal (0, (int)Marshal.OffsetOf<SKGraphiteVkBackendContextV2Native> (nameof (SKGraphiteVkBackendContextV2Native.fStructSize)));
		}

		[Fact]
		public void BackendContextV2IsLargerThanV1()
		{
			// v2 exists precisely because it carries a field v1 does not. If these ever match,
			// the versioned entry points are pointless and something has regressed.
			Assert.True (Marshal.SizeOf<GRVkBackendContextV2Native> () > Marshal.SizeOf<GRVkBackendContextNative> (),
				"gr_vk_backendcontext_v2_t must be larger than the frozen gr_vk_backendcontext_t");
			Assert.True (Marshal.SizeOf<SKGraphiteVkBackendContextV2Native> () > Marshal.SizeOf<SKGraphiteVkBackendContextNative> (),
				"sk_graphite_vk_backend_context_init_v2_t must be larger than the frozen init struct");
		}
	}
}
