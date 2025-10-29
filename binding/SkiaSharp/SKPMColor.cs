﻿#nullable disable

using System;

namespace SkiaSharp
{
	public readonly unsafe struct SKPMColor : IEquatable<SKPMColor>
	{
		private readonly uint color;

		public SKPMColor (uint value)
		{
			color = value;
		}

		public readonly byte Alpha => (byte)((color >> SKImageInfo.PlatformColorAlphaShift) & 0xff);
		public readonly byte Red => (byte)((color >> SKImageInfo.PlatformColorRedShift) & 0xff);
		public readonly byte Green => (byte)((color >> SKImageInfo.PlatformColorGreenShift) & 0xff);
		public readonly byte Blue => (byte)((color >> SKImageInfo.PlatformColorBlueShift) & 0xff);

		// PreMultiply

		public static SKPMColor PreMultiply (SKColor color) =>
			SkiaApi.sk_color_premultiply ((uint)color);

		public static SKPMColor[] PreMultiply (SKColor[] colors) =>
			PreMultiply (colors.AsSpan ());

		public static SKPMColor[] PreMultiply (ReadOnlySpan<SKColor> colors)
		{
			var pmcolors = new SKPMColor[colors.Length];
			PreMultiply (pmcolors.AsSpan (), colors);
			return pmcolors;
		}

		public static void PreMultiply (SKPMColor[] pmcolors, SKColor[] colors) =>
			PreMultiply (pmcolors.AsSpan (), colors.AsSpan ());

		public static void PreMultiply (Span<SKPMColor> pmcolors, ReadOnlySpan<SKColor> colors)
		{
			if (pmcolors.Length != colors.Length)
				throw new ArgumentException ("The length of pmcolors must be the same as the length of colors.", nameof (pmcolors));
			fixed (SKColor* c = colors)
			fixed (SKPMColor* pm = pmcolors) {
				SkiaApi.sk_color_premultiply_array ((uint*)c, colors.Length, (uint*)pm);
			}
		}

		// UnPreMultiply

		public static SKColor UnPreMultiply (SKPMColor pmcolor) =>
			SkiaApi.sk_color_unpremultiply ((uint)pmcolor);

		public static SKColor[] UnPreMultiply (SKPMColor[] pmcolors) =>
			UnPreMultiply (pmcolors.AsSpan ());

		public static SKColor[] UnPreMultiply (ReadOnlySpan<SKPMColor> pmcolors)
		{
			var colors = new SKColor[pmcolors.Length];
			UnPreMultiply (colors.AsSpan (), pmcolors);
			return colors;
		}

		public static void UnPreMultiply (SKColor[] colors, SKPMColor[] pmcolors) =>
			UnPreMultiply (colors.AsSpan (), pmcolors.AsSpan ());

		public static void UnPreMultiply (Span<SKColor> colors, ReadOnlySpan<SKPMColor> pmcolors)
		{
			if (colors.Length != pmcolors.Length)
				throw new ArgumentException ("The length of colors must be the same as the length of pmcolors.", nameof (colors));
			fixed (SKColor* c = colors)
			fixed (SKPMColor* pm = pmcolors) {
				SkiaApi.sk_color_unpremultiply_array ((uint*)pm, pmcolors.Length, (uint*)c);
			}
		}

		public static explicit operator SKPMColor (SKColor color) =>
			SKPMColor.PreMultiply (color);

		public static explicit operator SKColor (SKPMColor color) =>
			SKPMColor.UnPreMultiply (color);

		public readonly override string ToString () =>
			$"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";

		public readonly bool Equals (SKPMColor obj) =>
			obj.color == color;

		public readonly override bool Equals (object other) =>
			other is SKPMColor f && Equals (f);

		public static bool operator == (SKPMColor left, SKPMColor right) =>
			left.Equals (right);

		public static bool operator != (SKPMColor left, SKPMColor right) =>
			!left.Equals (right);

		public readonly override int GetHashCode () =>
			color.GetHashCode ();

		public static implicit operator SKPMColor (uint color) =>
			new SKPMColor (color);

		public static explicit operator uint (SKPMColor color) =>
			color.color;
	}
}
