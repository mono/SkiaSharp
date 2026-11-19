using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GRVkDeviceLostLayoutTest : SKTest
	{
		private const int VkMaxDescriptionSize = 256;

		[Fact]
		public void VendorInfoMatchesTheNativeLayout ()
		{
			Assert.Equal (VkMaxDescriptionSize + 8 + 8, Marshal.SizeOf<GRVkDeviceFaultVendorInfoNative> ());
			Assert.Equal (VkMaxDescriptionSize, (int)Marshal.OffsetOf<GRVkDeviceFaultVendorInfoNative> (nameof (GRVkDeviceFaultVendorInfoNative.fVendorFaultCode)));
			Assert.Equal (VkMaxDescriptionSize + 8, (int)Marshal.OffsetOf<GRVkDeviceFaultVendorInfoNative> (nameof (GRVkDeviceFaultVendorInfoNative.fVendorFaultData)));
		}

		[Fact]
		public void AddressInfoMatchesTheNativeLayout ()
		{
			var expectedAlignment = RuntimeInformation.ProcessArchitecture == Architecture.X86 && !RuntimeInformation.IsOSPlatform (OSPlatform.Windows) ? 4 : 8;
			Assert.Equal (expectedAlignment, (int)Marshal.OffsetOf<GRVkDeviceFaultAddressInfoNative> (nameof (GRVkDeviceFaultAddressInfoNative.fReportedAddress)));
			Assert.Equal (expectedAlignment + 8, (int)Marshal.OffsetOf<GRVkDeviceFaultAddressInfoNative> (nameof (GRVkDeviceFaultAddressInfoNative.fAddressPrecision)));
		}
	}
}
