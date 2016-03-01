using System;
using System.Collections.Generic;
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

	}
}