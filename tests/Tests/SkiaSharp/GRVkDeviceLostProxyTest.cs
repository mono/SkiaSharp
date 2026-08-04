using System;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace SkiaSharp.Tests
{
	// Proxy-level unit tests for GRVkDeviceLostDelegate marshalling. Invokes
	// DelegateProxies.GRVkDeviceLostProxy directly with synthetic native structs
	// — same call shape Skia's device-lost thunk produces — to verify the C ABI
	// payload is unpacked into GRVkDeviceLostInfo correctly.
	//
	// End-to-end firing under a real device loss is unreachable in CI (needs
	// hardware TDR); this covers everything downstream of the C shim.
	public unsafe class GRVkDeviceLostProxyTest : SKTest
	{
		[Fact]
		public void ProxyUnpacksDescriptionAndEmptyVectors()
		{
			GRVkDeviceLostInfo captured = null;
			GRVkDeviceLostDelegate handler = info => { captured = info; };

			DelegateProxies.Create(handler, out var gch, out var userData);
			try
			{
				var descBytes = Encoding.ASCII.GetBytes("device lost from unit test\0");
				fixed (byte* descPtr = descBytes)
				{
					var native = new GRVkDeviceLostInfoNative
					{
						fDescription = descPtr,
						fAddressInfos = null,
						fAddressInfoCount = 0,
						fVendorInfos = null,
						fVendorInfoCount = 0,
						fVendorBinaryData = null,
						fVendorBinaryDataSize = IntPtr.Zero,
					};
					DelegateProxies.GRVkDeviceLostProxy((void*)userData, &native);
				}

				Assert.NotNull(captured);
				Assert.Equal("device lost from unit test", captured.Description);
				Assert.Empty(captured.AddressInfos);
				Assert.Empty(captured.VendorInfos);
				Assert.Empty(captured.VendorBinaryData);
			}
			finally
			{
				gch.Free();
			}
		}

		[Fact]
		public void ProxyUnpacksAddressInfoArray()
		{
			GRVkDeviceLostInfo captured = null;
			GRVkDeviceLostDelegate handler = info => { captured = info; };

			DelegateProxies.Create(handler, out var gch, out var userData);
			try
			{
				var addressInfos = new GRVkDeviceFaultAddressInfoNative[]
				{
					new() { fAddressType = 1 /* ReadInvalid */, fReportedAddress = 0xDEAD_BEEFul, fAddressPrecision = 4 },
					new() { fAddressType = 6 /* InstructionPointerFault */, fReportedAddress = 0xC0FFEE_1234ul, fAddressPrecision = 8 },
				};
				var descBytes = Encoding.ASCII.GetBytes("addr\0");

				fixed (GRVkDeviceFaultAddressInfoNative* addrPtr = addressInfos)
				fixed (byte* descPtr = descBytes)
				{
					var native = new GRVkDeviceLostInfoNative
					{
						fDescription = descPtr,
						fAddressInfos = addrPtr,
						fAddressInfoCount = addressInfos.Length,
						fVendorInfos = null,
						fVendorInfoCount = 0,
						fVendorBinaryData = null,
						fVendorBinaryDataSize = IntPtr.Zero,
					};
					DelegateProxies.GRVkDeviceLostProxy((void*)userData, &native);
				}

				Assert.NotNull(captured);
				Assert.Equal(2, captured.AddressInfos.Length);
				Assert.Equal(GRVkDeviceFaultAddressType.ReadInvalid, captured.AddressInfos[0].AddressType);
				Assert.Equal(0xDEAD_BEEFul, captured.AddressInfos[0].ReportedAddress);
				Assert.Equal(4ul, captured.AddressInfos[0].AddressPrecision);
				Assert.Equal(GRVkDeviceFaultAddressType.InstructionPointerFault, captured.AddressInfos[1].AddressType);
				Assert.Equal(0xC0FFEE_1234ul, captured.AddressInfos[1].ReportedAddress);
			}
			finally
			{
				gch.Free();
			}
		}

		[Fact]
		public void ProxyWalksVendorInfoManuallyBecauseCharArrayLayoutIsInline()
		{
			// Vulkan's VkDeviceFaultVendorInfoEXT is:
			//   char description[256];   // VK_MAX_DESCRIPTION_SIZE
			//   uint64_t vendorFaultCode;
			//   uint64_t vendorFaultData;
			// The generator emits the description as a pointer (fixed-size arrays are
			// unsupported), so the proxy walks the array via byte* arithmetic. Verify
			// that the walk reads each field at the correct offset.
			const int VendorInfoStride = 256 + 8 + 8;

			var buf = new byte[VendorInfoStride * 2];
			// Record 0: description "nvidia-fault", code 0x11, data 0x22
			WriteAscii(buf, 0, "nvidia-fault");
			BitConverter.GetBytes(0x11ul).CopyTo(buf, 256);
			BitConverter.GetBytes(0x22ul).CopyTo(buf, 264);
			// Record 1: description "amd-fault", code 0x33, data 0x44
			WriteAscii(buf, VendorInfoStride, "amd-fault");
			BitConverter.GetBytes(0x33ul).CopyTo(buf, VendorInfoStride + 256);
			BitConverter.GetBytes(0x44ul).CopyTo(buf, VendorInfoStride + 264);

			GRVkDeviceLostInfo captured = null;
			GRVkDeviceLostDelegate handler = info => { captured = info; };

			DelegateProxies.Create(handler, out var gch, out var userData);
			try
			{
				var descBytes = Encoding.ASCII.GetBytes("vendor\0");
				fixed (byte* vendorPtr = buf)
				fixed (byte* descPtr = descBytes)
				{
					var native = new GRVkDeviceLostInfoNative
					{
						fDescription = descPtr,
						fAddressInfos = null,
						fAddressInfoCount = 0,
						fVendorInfos = (GRVkDeviceFaultVendorInfoNative*)vendorPtr,
						fVendorInfoCount = 2,
						fVendorBinaryData = null,
						fVendorBinaryDataSize = IntPtr.Zero,
					};
					DelegateProxies.GRVkDeviceLostProxy((void*)userData, &native);
				}

				Assert.NotNull(captured);
				Assert.Equal(2, captured.VendorInfos.Length);
				Assert.Equal("nvidia-fault", captured.VendorInfos[0].Description);
				Assert.Equal(0x11ul, captured.VendorInfos[0].VendorFaultCode);
				Assert.Equal(0x22ul, captured.VendorInfos[0].VendorFaultData);
				Assert.Equal("amd-fault", captured.VendorInfos[1].Description);
				Assert.Equal(0x33ul, captured.VendorInfos[1].VendorFaultCode);
				Assert.Equal(0x44ul, captured.VendorInfos[1].VendorFaultData);
			}
			finally
			{
				gch.Free();
			}
		}

		[Fact]
		public void ProxyCopiesVendorBinaryBlob()
		{
			var payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0xFF, 0xFE };

			GRVkDeviceLostInfo captured = null;
			GRVkDeviceLostDelegate handler = info => { captured = info; };

			DelegateProxies.Create(handler, out var gch, out var userData);
			try
			{
				var descBytes = Encoding.ASCII.GetBytes("blob\0");
				fixed (byte* dataPtr = payload)
				fixed (byte* descPtr = descBytes)
				{
					var native = new GRVkDeviceLostInfoNative
					{
						fDescription = descPtr,
						fAddressInfos = null,
						fAddressInfoCount = 0,
						fVendorInfos = null,
						fVendorInfoCount = 0,
						fVendorBinaryData = dataPtr,
						fVendorBinaryDataSize = (IntPtr)payload.Length,
					};
					DelegateProxies.GRVkDeviceLostProxy((void*)userData, &native);
				}

				Assert.NotNull(captured);
				Assert.Equal(payload, captured.VendorBinaryData);
			}
			finally
			{
				gch.Free();
			}
		}

		[Fact]
		public void ProxySwallowsManagedExceptionsAcrossFfiBoundary()
		{
			GRVkDeviceLostDelegate handler = info => throw new InvalidOperationException("intentional test throw");

			DelegateProxies.Create(handler, out var gch, out var userData);
			try
			{
				var descBytes = Encoding.ASCII.GetBytes("throwing\0");
				fixed (byte* descPtr = descBytes)
				{
					var native = new GRVkDeviceLostInfoNative { fDescription = descPtr };
					Exception caught = null;
					try { DelegateProxies.GRVkDeviceLostProxy((void*)userData, &native); }
					catch (Exception ex) { caught = ex; }
					Assert.Null(caught);
				}
			}
			finally
			{
				gch.Free();
			}
		}

		private static void WriteAscii(byte[] buf, int offset, string s)
		{
			var bytes = Encoding.ASCII.GetBytes(s);
			Array.Copy(bytes, 0, buf, offset, bytes.Length);
			// NUL terminator lands automatically because buf is zero-initialised
		}
	}
}
