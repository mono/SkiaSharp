using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SkiaSharp
{
	internal static class Utils
	{
		internal const float NearlyZero = (1.0f / (1 << 12));

		internal static bool NearlyEqual (float a, float b, float tolerance)
		{
			return Math.Abs (a - b) <= tolerance;
		}
	}

	public static class StringUtilities
	{
		private static int GetUnicodeStringLength(SKTextEncoding encoding)
		{
			switch (encoding)
			{
				case SKTextEncoding.Utf8:
				case SKTextEncoding.Utf16:
					return 1;
				case SKTextEncoding.Utf32:
					return 2;
				default:
					throw new ArgumentOutOfRangeException(nameof(encoding), $"Encoding {encoding} is not supported.");
			}
		}

		public static int GetUnicodeCharacterCode(string character, SKTextEncoding encoding)
		{
			if (GetUnicodeStringLength(encoding) != character.Length)
				throw new ArgumentException(nameof(character), $"Only a single character can be specified.");

			var bytes = GetEncodedText(character, encoding);
			return BitConverter.ToInt32(bytes, 0);
		}

		public static byte[] GetEncodedText(string text, SKEncoding encoding)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			switch (encoding)
			{
				case SKEncoding.Utf8:
					return Encoding.UTF8.GetBytes(text);
				case SKEncoding.Utf16:
					return Encoding.Unicode.GetBytes(text);
				case SKEncoding.Utf32:
					return Encoding.UTF32.GetBytes(text);
				default:
					throw new ArgumentOutOfRangeException(nameof(encoding), $"Encoding {encoding} is not supported.");
			}
		}

		public static byte[] GetEncodedText(string text, SKTextEncoding encoding)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			switch (encoding)
			{
				case SKTextEncoding.Utf8:
					return Encoding.UTF8.GetBytes(text);
				case SKTextEncoding.Utf16:
					return Encoding.Unicode.GetBytes(text);
				case SKTextEncoding.Utf32:
					return Encoding.UTF32.GetBytes(text);
				default:
					throw new ArgumentOutOfRangeException(nameof(encoding), $"Encoding {encoding} is not supported.");
			}
		}

		public static string GetString(IntPtr data, int dataLength, SKTextEncoding encoding)
		{
			if (data == IntPtr.Zero || dataLength <= 0)
				return "";

			byte[] result = new byte[dataLength];
			Marshal.Copy(data, result, 0, dataLength);
			return GetString(result, encoding);
		}

		public static string GetString(byte[] data, SKTextEncoding encoding)
		{
			return GetString(data, 0, data.Length, encoding);
		}

		public static string GetString(byte[] data, int index, int count, SKTextEncoding encoding)
		{
			switch (encoding)
			{
				case SKTextEncoding.Utf8:
					return Encoding.UTF8.GetString(data, index, count);
				case SKTextEncoding.Utf16:
					return Encoding.Unicode.GetString(data, index, count);
				case SKTextEncoding.Utf32:
					return Encoding.UTF32.GetString(data, index, count);
				default:
					throw new ArgumentOutOfRangeException(nameof(encoding), $"Encoding {encoding} is not supported.");
			}
		}
	}
}
