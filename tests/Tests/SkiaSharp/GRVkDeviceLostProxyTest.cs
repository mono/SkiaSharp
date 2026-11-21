using System;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace SkiaSharp.Tests
{
	public unsafe class GRVkDeviceLostProxyTest : SKTest
	{
		[Fact]
		public void FaultAddressInfoEqualityWorks ()
		{
			var a = new GRVkDeviceFaultAddressInfo (GRVkDeviceFaultAddressType.ReadInvalid, 0x1000, 16);
			var b = new GRVkDeviceFaultAddressInfo (GRVkDeviceFaultAddressType.ReadInvalid, 0x1000, 16);
			var c = new GRVkDeviceFaultAddressInfo (GRVkDeviceFaultAddressType.WriteInvalid, 0x1000, 16);

			Assert.Equal (a, b);
			Assert.True (a == b);
			Assert.False (a != b);
			Assert.Equal (a.GetHashCode (), b.GetHashCode ());

			Assert.NotEqual (a, c);
			Assert.False (a == c);
			Assert.True (a != c);
		}

		[Fact]
		public void FaultVendorInfoEqualityWorks ()
		{
			var a = new GRVkDeviceFaultVendorInfo ("fault", 1, 2);
			var b = new GRVkDeviceFaultVendorInfo ("fault", 1, 2);
			var c = new GRVkDeviceFaultVendorInfo ("other", 1, 2);

			Assert.Equal (a, b);
			Assert.True (a == b);
			Assert.False (a != b);
			Assert.Equal (a.GetHashCode (), b.GetHashCode ());

			Assert.NotEqual (a, c);
			Assert.False (a == c);
			Assert.True (a != c);
		}

		[Fact]
		public void ProxyIgnoresZeroInfoHandle ()
		{
			var invoked = false;
			GRVkDeviceLostDelegate handler = _ => invoked = true;

			DelegateProxies.Create (handler, out var gch, out var userData);
			try {
				DelegateProxies.GRVkDeviceLostProxy ((void*)userData, IntPtr.Zero);
				Assert.False (invoked);
			} finally {
				gch.Free ();
			}
		}

		[Fact]
		public void NativeGettersHandleNullInfo ()
		{
			Assert.Equal (IntPtr.Zero, SkiaApi.gr_vk_device_lost_info_get_description (IntPtr.Zero));
			Assert.Equal (0, SkiaApi.gr_vk_device_lost_info_get_address_info_count (IntPtr.Zero));
			Assert.Equal (0, SkiaApi.gr_vk_device_lost_info_get_vendor_info_count (IntPtr.Zero));

			IntPtr size = (IntPtr)123;
			Assert.True (SkiaApi.gr_vk_device_lost_info_get_vendor_binary_data (IntPtr.Zero, &size) == null);
			Assert.Equal (IntPtr.Zero, size);
		}
	}
}
