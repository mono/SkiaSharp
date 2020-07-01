using Xunit;
using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	public class PlaceholderTest
	{
		private const string SKIA = "libSkiaSharp";

		[Fact]
		public void CheckVersion()
		{
			var str = Marshal.PtrToStringAnsi(sk_version_get_string());
			var milestone = sk_version_get_milestone();
			var increment = sk_version_get_increment();

			Assert.True(milestone > 0);
			Assert.True(increment >= 0);
			Assert.Equal($"{milestone}.{increment}", str);
		}

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr sk_version_get_string();

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		static extern int sk_version_get_milestone();

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		static extern int sk_version_get_increment();
	}
}
