#nullable disable

using System;
using System.Runtime.CompilerServices;

namespace SkiaSharp
{
	public readonly struct SKColor : IEquatable<SKColor>
	{
		public static readonly SKColor Empty;

		private readonly uint color;

		public SKColor (uint value)
		{
			color = value;
		}

		public SKColor (byte red, byte green, byte blue, byte alpha)
		{
			color = (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
		}

		public SKColor (byte red, byte green, byte blue)
		{
			color = (0xff000000u | (uint)(red << 16) | (uint)(green << 8) | blue);
		}

		public readonly SKColor WithRed (byte red) =>
			new SKColor (red, Green, Blue, Alpha);

		public readonly SKColor WithGreen (byte green) =>
			new SKColor (Red, green, Blue, Alpha);

		public readonly SKColor WithBlue (byte blue) =>
			new SKColor (Red, Green, blue, Alpha);

		public readonly SKColor WithAlpha (byte alpha) =>
			new SKColor (Red, Green, Blue, alpha);

		public readonly byte Alpha => (byte)((color >> 24) & 0xff);
		public readonly byte Red => (byte)((color >> 16) & 0xff);
		public readonly byte Green => (byte)((color >> 8) & 0xff);
		public readonly byte Blue => (byte)((color) & 0xff);

		public readonly float Hue {
			get {
				ToHsv (out var h, out _, out _);
				return h;
			}
		}

		public static SKColor FromHsl (float h, float s, float l, byte a = 255)
		{
			var colorf = SKColorF.FromHsl (h, s, l);

			// RGB results from 0 to 255
			var r = colorf.Red * 255f;
			var g = colorf.Green * 255f;
			var b = colorf.Blue * 255f;

			return new SKColor ((byte)r, (byte)g, (byte)b, a);
		}

		public static SKColor FromHsv (float h, float s, float v, byte a = 255)
		{
			var colorf = SKColorF.FromHsv (h, s, v);

			// RGB results from 0 to 255
			var r = colorf.Red * 255f;
			var g = colorf.Green * 255f;
			var b = colorf.Blue * 255f;

			return new SKColor ((byte)r, (byte)g, (byte)b, a);
		}

		public readonly void ToHsl (out float h, out float s, out float l)
		{
			// RGB from 0 to 255
			var r = Red / 255f;
			var g = Green / 255f;
			var b = Blue / 255f;

			var colorf = new SKColorF (r, g, b);
			colorf.ToHsl (out h, out s, out l);
		}

		public readonly void ToHsv (out float h, out float s, out float v)
		{
			// RGB from 0 to 255
			var r = Red / 255f;
			var g = Green / 255f;
			var b = Blue / 255f;

			var colorf = new SKColorF (r, g, b);
			colorf.ToHsv (out h, out s, out v);
		}

		public readonly override string ToString () =>
			$"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";

		public readonly bool Equals (SKColor obj) =>
			obj.color == color;

		public readonly override bool Equals (object other) =>
			other is SKColor f && Equals (f);

		public static bool operator == (SKColor left, SKColor right) =>
			left.Equals (right);

		public static bool operator != (SKColor left, SKColor right) =>
			!left.Equals (right);

		public readonly override int GetHashCode () =>
			color.GetHashCode ();

		public static implicit operator SKColor (uint color) =>
			new SKColor (color);

		public static explicit operator uint (SKColor color) =>
			color.color;

		public static SKColor Parse (string hexString) =>
			Parse (hexString.AsSpan ());

		public static SKColor Parse (ReadOnlySpan<char> hexString)
		{
			if (!TryParse (hexString, out var color))
				throw new ArgumentException ("Invalid hexadecimal color string.", nameof (hexString));
			return color;
		}

		public static bool TryParse (string hexString, out SKColor color) =>
			TryParse (hexString.AsSpan (), out color);

		public static bool TryParse (ReadOnlySpan<char> hexString, out SKColor color)
		{
			color = SKColor.Empty;

			// Trim surrounding whitespace and any leading '#'. Re-basing the span to index 0 also lets
			// the JIT prove the fixed-length indexes below are in range and drop the bounds checks.
			hexString = hexString.Trim ().TrimStart ('#');

			switch (hexString.Length) {
				case 3: {
					// #RGB -> each nibble is duplicated (e.g. "F" -> 0xFF)
					if (!TryParseNibble (hexString[0], out var r) ||
						!TryParseNibble (hexString[1], out var g) ||
						!TryParseNibble (hexString[2], out var b))
						return false;

					color = new SKColor (
						(byte)(r << 4 | r),
						(byte)(g << 4 | g),
						(byte)(b << 4 | b));
					return true;
				}
				case 4: {
					// #ARGB -> each nibble is duplicated (e.g. "F" -> 0xFF)
					if (!TryParseNibble (hexString[0], out var a) ||
						!TryParseNibble (hexString[1], out var r) ||
						!TryParseNibble (hexString[2], out var g) ||
						!TryParseNibble (hexString[3], out var b))
						return false;

					color = new SKColor (
						(byte)(r << 4 | r),
						(byte)(g << 4 | g),
						(byte)(b << 4 | b),
						(byte)(a << 4 | a));
					return true;
				}
				case 6: {
					// #RRGGBB
					if (!TryParseByte (hexString[0], hexString[1], out var r) ||
						!TryParseByte (hexString[2], hexString[3], out var g) ||
						!TryParseByte (hexString[4], hexString[5], out var b))
						return false;

					color = new SKColor (r, g, b);
					return true;
				}
				case 8: {
					// #AARRGGBB
					if (!TryParseByte (hexString[0], hexString[1], out var a) ||
						!TryParseByte (hexString[2], hexString[3], out var r) ||
						!TryParseByte (hexString[4], hexString[5], out var g) ||
						!TryParseByte (hexString[6], hexString[7], out var b))
						return false;

					color = new SKColor (r, g, b, a);
					return true;
				}
				default:
					return false;
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static bool TryParseNibble (char c, out byte value)
		{
			// Convert a single ASCII hex digit to its 0-15 value, case-insensitively
			// ('a'-'f' and 'A'-'F' both map to 10-15).
			if (c >= '0' && c <= '9') {
				value = (byte)(c - '0');
				return true;
			}
			if (c >= 'a' && c <= 'f') {
				value = (byte)(c - 'a' + 10);
				return true;
			}
			if (c >= 'A' && c <= 'F') {
				value = (byte)(c - 'A' + 10);
				return true;
			}

			value = 0;
			return false;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static bool TryParseByte (char hi, char lo, out byte value)
		{
			if (TryParseNibble (hi, out var h) && TryParseNibble (lo, out var l)) {
				value = (byte)(h << 4 | l);
				return true;
			}

			value = 0;
			return false;
		}
	}
}
