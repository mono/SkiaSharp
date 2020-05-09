using System;
using System.ComponentModel;
using System.Text;
#if NETSTANDARD1_3 || WINDOWS_UWP
using System.Reflection;
#endif

namespace SkiaSharp
{
	internal unsafe static class Utils
	{
		internal const float NearlyZero = 1.0f / (1 << 12);

		internal static Span<byte> AsSpan (this IntPtr ptr, int size) =>
			new Span<byte> ((void*)ptr, size);

		internal static ReadOnlySpan<byte> AsReadOnlySpan (this IntPtr ptr, int size) =>
			new ReadOnlySpan<byte> ((void*)ptr, size);

		internal static bool NearlyEqual (float a, float b, float tolerance) =>
			Math.Abs (a - b) <= tolerance;

		internal static byte[] GetBytes (this Encoding encoding, ReadOnlySpan<char> text)
		{
			if (text.Length == 0)
				return new byte[0];

			fixed (char* t = text) {
				var byteCount = encoding.GetByteCount (t, text.Length);
				if (byteCount == 0)
					return new byte[0];

				var bytes = new byte[byteCount];
				fixed (byte* b = bytes) {
					encoding.GetBytes (t, text.Length, b, byteCount);
				}
				return bytes;
			}
		}

#if NETSTANDARD1_3 || WINDOWS_UWP
		internal static bool IsAssignableFrom (this Type type, Type c) =>
			type.GetTypeInfo ().IsAssignableFrom (c.GetTypeInfo ());
#endif
	}

	public unsafe static class StringUtilities
	{
		// GetUnicodeStringLength

		private static int GetUnicodeStringLength (SKTextEncoding encoding) =>
			encoding switch
			{
				SKTextEncoding.Utf8 => 1,
				SKTextEncoding.Utf16 => 1,
				SKTextEncoding.Utf32 => 2,
				_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported.")
			};

		// GetUnicodeCharacterCode

		public static int GetUnicodeCharacterCode (string character, SKTextEncoding encoding)
		{
			if (character == null)
				throw new ArgumentNullException (nameof (character));
			if (GetUnicodeStringLength (encoding) != character.Length)
				throw new ArgumentException (nameof (character), $"Only a single character can be specified.");

			var bytes = GetEncodedText (character, encoding);
			return BitConverter.ToInt32 (bytes, 0);
		}

		// GetEncodedText

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetEncodedText(string, SKTextEncoding) instead.")]
		public static byte[] GetEncodedText (string text, SKEncoding encoding) =>
			GetEncodedText (text.AsSpan (), encoding.ToTextEncoding ());

		public static byte[] GetEncodedText (string text, SKTextEncoding encoding) =>
			GetEncodedText (text.AsSpan (), encoding);

		public static byte[] GetEncodedText (ReadOnlySpan<char> text, SKTextEncoding encoding) =>
			encoding switch
			{
				SKTextEncoding.Utf8 => Encoding.UTF8.GetBytes (text),
				SKTextEncoding.Utf16 => Encoding.Unicode.GetBytes (text),
				SKTextEncoding.Utf32 => Encoding.UTF32.GetBytes (text),
				_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported."),
			};

		// GetString

		public static string GetString (IntPtr data, int dataLength, SKTextEncoding encoding) =>
			GetString (data.AsReadOnlySpan (dataLength), 0, dataLength, encoding);

		public static string GetString (byte[] data, SKTextEncoding encoding) =>
			GetString (data, 0, data.Length, encoding);

		public static string GetString (byte[] data, int index, int count, SKTextEncoding encoding)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return encoding switch
			{
				SKTextEncoding.Utf8 => Encoding.UTF8.GetString (data, index, count),
				SKTextEncoding.Utf16 => Encoding.Unicode.GetString (data, index, count),
				SKTextEncoding.Utf32 => Encoding.UTF32.GetString (data, index, count),
				_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported."),
			};
		}

		public static string GetString (ReadOnlySpan<byte> data, SKTextEncoding encoding) =>
			GetString (data, 0, data.Length, encoding);

		public static string GetString (ReadOnlySpan<byte> data, int index, int count, SKTextEncoding encoding)
		{
			data = data.Slice (index, count);

			if (data.Length == 0)
				return string.Empty;

#if __DESKTOP__
			// TODO: improve this copy for old .NET 4.5
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
