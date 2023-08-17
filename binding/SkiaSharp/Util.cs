#nullable disable

using System;
using System.Buffers;
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

		internal static int GetPreambleSize (SKData data)
		{
			_ = data ?? throw new ArgumentNullException (nameof (data));

			var buffer = data.AsSpan ();
			var len = buffer.Length;

			if (len >= 2 && buffer[0] == 0xfe && buffer[1] == 0xff)
				return 2;
			else if (len >= 3 && buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
				return 3;
			else if (len >= 3 && buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
				return 3;
			else if (len >= 4 && buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
				return 4;
			else
				return 0;
		}

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

		public static RentedArray<T> RentArray<T> (int length, bool nullIfEmpty = false) =>
			nullIfEmpty && length <= 0
				? default
				: new RentedArray<T> (length);

		public static RentedArray<IntPtr> RentHandlesArray (SKObject[] objects, bool nullIfEmpty = false)
		{
			var handles = RentArray<IntPtr> (objects?.Length ?? 0, nullIfEmpty);
			for (var i = 0; i < handles.Length; i++) {
				handles[i] = objects[i]?.Handle ?? IntPtr.Zero;
			}
			return handles;
		}

		internal readonly ref struct RentedArray<T>
		{
			internal RentedArray (int length)
			{
				Array = ArrayPool<T>.Shared.Rent (length);
				Span = new Span<T> (Array, 0, length);
			}

			public readonly T[] Array;

			public readonly Span<T> Span;

			public int Length => Span.Length;

			public T this[int index] {
				get => Span[index];
				set => Span[index] = value;
			}

			[EditorBrowsable (EditorBrowsableState.Never)]
			public ref T GetPinnableReference () =>
				ref Span.GetPinnableReference ();

			public void Dispose ()
			{
				if (Array != null)
					ArrayPool<T>.Shared.Return (Array);
			}

			public static explicit operator T[] (RentedArray<T> scope) =>
				scope.Array;

			public static implicit operator Span<T> (RentedArray<T> scope) =>
				scope.Span;

			public static implicit operator ReadOnlySpan<T> (RentedArray<T> scope) =>
				scope.Span;
		}

#if NETSTANDARD1_3 || WINDOWS_UWP
		internal static bool IsAssignableFrom (this Type type, Type c) =>
			type.GetTypeInfo ().IsAssignableFrom (c.GetTypeInfo ());
#endif
	}

	public unsafe static class StringUtilities
	{
		internal const string NullTerminator = "\0";

		// GetUnicodeStringLength

		private static int GetUnicodeStringLength (SKTextEncoding encoding) =>
			encoding switch {
				SKTextEncoding.Utf8 => 1,
				SKTextEncoding.Utf16 => 1,
				SKTextEncoding.Utf32 => 2,
				_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported.")
			};

		// GetCharacterByteSize

		internal static int GetCharacterByteSize (this SKTextEncoding encoding) =>
			encoding switch {
				SKTextEncoding.Utf8 => 1,
				SKTextEncoding.Utf16 => 2,
				SKTextEncoding.Utf32 => 4,
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

		public static byte[] GetEncodedText (string text, SKTextEncoding encoding) =>
			GetEncodedText (text.AsSpan (), encoding);

		internal static byte[] GetEncodedText (string text, SKTextEncoding encoding, bool addNull)
		{
			if (!string.IsNullOrEmpty (text) && addNull)
				text += NullTerminator;

			return GetEncodedText (text.AsSpan (), encoding);
		}

		public static byte[] GetEncodedText (ReadOnlySpan<char> text, SKTextEncoding encoding) =>
			encoding switch {
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

			return encoding switch {
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

			fixed (byte* bp = data) {
				return encoding switch {
					SKTextEncoding.Utf8 => Encoding.UTF8.GetString (bp, data.Length),
					SKTextEncoding.Utf16 => Encoding.Unicode.GetString (bp, data.Length),
					SKTextEncoding.Utf32 => Encoding.UTF32.GetString (bp, data.Length),
					_ => throw new ArgumentOutOfRangeException (nameof (encoding), $"Encoding {encoding} is not supported."),
				};
			}
		}
	}
}
