using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace SkiaSharp.Extended
{
	internal class Base83
	{
		public const string Charset = @"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#$%*+,-.:;=?@[]^_{|}~";

		private static readonly Dictionary<char, int> Lookup;

		static Base83()
		{
			var dic = new Dictionary<char, int>();
			for (var i = 0; i < Charset.Length; i++)
			{
				var c = Charset[i];
				dic[c] = i;
			}

			Lookup = dic;
		}

		public static int Decode(string? encoded, int start = 0, int length = -1)
		{
			if (encoded == null)
				throw new ArgumentNullException(nameof(encoded));

			return Decode(encoded.AsSpan(), start, length);
		}

		public static int Decode(ReadOnlySpan<char> encoded, int start = 0, int length = -1)
		{
			if (length == -1)
				length = encoded.Length;

			if (start < 0 || start > encoded.Length)
				throw new ArgumentOutOfRangeException(nameof(start));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length));

			var end = start + length;

			if (end > encoded.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			if (length == 0)
				return 0;

			var result = 0;

			for (var i = start; i < end; i++)
			{
				var c = encoded[i];
				var v = Lookup[c];

				result = result * 83 + v;
			}

			return result;
		}

		public static string Encode(int data, int length)
		{
			var result = ArrayPool<char>.Shared.Rent(length);

			Encode(data, length, result);

			var str = new string(result, 0, length);

			ArrayPool<char>.Shared.Return(result);

			return str;
		}

		public static void Encode(int data, int length, Span<char> chars, int start = 0)
		{
			if (start > 0)
				chars = chars.Slice(start);

			if (chars.Length < length)
				throw new ArgumentOutOfRangeException(nameof(length));

			for (var i = 0; i < length; i++)
			{
				var digit = data % 83;
				data /= 83;

				chars[length - 1 - i] = Charset[digit];
			}
		}
	}
}
