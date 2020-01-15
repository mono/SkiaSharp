using System;
using System.Text;

namespace SkiaSharp
{
	internal static class Utils
	{
		internal const float NearlyZero = (1.0f / (1 << 12));

		internal static bool NearlyEqual (float a, float b, float tolerance) =>
			Math.Abs (a - b) <= tolerance;
	}

	public unsafe static class StringUtilities
	{
		private static int GetUnicodeStringLength (SKTextEncoding encoding) =>
			encoding switch
			{
				SKTextEncoding.Utf8 => 1,
				SKTextEncoding.Utf16 => 1,
				SKTextEncoding.Utf32 => 2,
				_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported.")
			};

		public static int GetUnicodeCharacterCode (string character, SKTextEncoding encoding)
		{
			if (character == null)
				throw new ArgumentNullException (nameof (character));

			if (GetUnicodeStringLength (encoding) != character.Length)
				throw new ArgumentException (nameof (character), $"Only a single character can be specified.");

			var bytes = GetEncodedText (character, encoding);
			return BitConverter.ToInt32 (bytes, 0);
		}

		public static byte[] GetEncodedText (string text, SKTextEncoding encoding)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			return encoding switch
			{
				SKTextEncoding.Utf8 => Encoding.UTF8.GetBytes (text),
				SKTextEncoding.Utf16 => Encoding.Unicode.GetBytes (text),
				SKTextEncoding.Utf32 => Encoding.UTF32.GetBytes (text),
				_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported."),
			};
		}

		public static string GetString (ReadOnlySpan<byte> data, SKTextEncoding encoding) =>
			GetString (data, 0, data.Length, encoding);

		public static string GetString (ReadOnlySpan<byte> data, int index, int count, SKTextEncoding encoding)
		{
			data = data.Slice (index, count);
#if __DESKTOP__
			var array = data.ToArray ();
			return encoding switch
			{
				SKTextEncoding.Utf8 => Encoding.UTF8.GetString (array),
				SKTextEncoding.Utf16 => Encoding.Unicode.GetString (array),
				SKTextEncoding.Utf32 => Encoding.UTF32.GetString (array),
				_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported."),
			};
#else
			fixed (byte* bp = data) {
				return encoding switch
				{
					SKTextEncoding.Utf8 => Encoding.UTF8.GetString (bp, data.Length),
					SKTextEncoding.Utf16 => Encoding.Unicode.GetString (bp, data.Length),
					SKTextEncoding.Utf32 => Encoding.UTF32.GetString (bp, data.Length),
					_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported."),
				};
			}
#endif
		}
	}
}
