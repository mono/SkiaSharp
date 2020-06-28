using Xunit;
using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	public class Tests
	{
		[Fact]
		public void Passing()
		{
		}

		[Fact]
		public void Failing()
		{
			Assert.True(false);
		}

		[Fact]
		public void Interop()
		{
			var str = Marshal.PtrToStringAnsi(sk_version_get_string());
			Assert.Equal("80.0", str);
		}

		[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr sk_version_get_string();

		[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
		static extern int sk_colortype_get_default_8888();
	}
}
