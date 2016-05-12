using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SkiaSharp
{
	static class Util
	{
		public static byte[] GetEncodedText(string text, SKTextEncoding encoding)
		{
			switch (encoding)
			{
				case SKTextEncoding.Utf8:
					return Encoding.UTF8.GetBytes(text);
				case SKTextEncoding.Utf16:
					return Encoding.Unicode.GetBytes(text);
				case SKTextEncoding.Utf32:
					return Encoding.UTF32.GetBytes(text);
				default:
					throw new ArgumentException($"Encoding {encoding} is not supported");
			}
		}

		public static string GetString(IntPtr data, int dataLength, SKTextEncoding encoding)
		{
			if (data == IntPtr.Zero || dataLength <= 0) return "";
			byte[] result = new byte[dataLength];
			Marshal.Copy(data, result, 0, dataLength);
			switch (encoding)
			{
				case SKTextEncoding.Utf8:
					return Encoding.UTF8.GetString(result);
				case SKTextEncoding.Utf16:
					return Encoding.Unicode.GetString(result);
				case SKTextEncoding.Utf32:
					return Encoding.UTF32.GetString(result);
				default:
					throw new ArgumentException($"Encoding {encoding} is not supported");
			}
		}
	}
}