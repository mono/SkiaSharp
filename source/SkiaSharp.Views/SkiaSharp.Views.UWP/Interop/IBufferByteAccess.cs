using System;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;

namespace SkiaSharp.Views.UWP
{
	[ComImport]
	[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IBufferByteAccess
	{
		long Buffer([Out] out IntPtr value);
	}

	internal static class Utils
	{
		internal static IntPtr GetByteBuffer(this IBuffer buffer)
		{
			var byteBuffer = buffer as IBufferByteAccess;
			if (byteBuffer == null)
				throw new InvalidCastException("Unable to convert WriteableBitmap.PixelBuffer to IBufferByteAccess.");

			var hr = byteBuffer.Buffer(out var ptr);
			if (hr < 0)
				throw new InvalidCastException("Unable to retrieve pixel address from WriteableBitmap.PixelBuffer.");

			return ptr;
		}
	}
}
