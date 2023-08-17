#nullable disable

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

		public static SKPMColor[] PreMultiply (SKColor[] colors)
		{
			var pmcolors = new SKPMColor[colors.Length];
			fixed (SKColor* c = colors)
			fixed (SKPMColor* pm = pmcolors) {
				SkiaApi.sk_color_premultiply_array ((uint*)c, colors.Length, (uint*)pm);
			}
			return pmcolors;
		}

		// UnPreMultiply

		public static SKColor UnPreMultiply (SKPMColor pmcolor) =>
			SkiaApi.sk_color_unpremultiply ((uint)pmcolor);

		public static SKColor[] UnPreMultiply (SKPMColor[] pmcolors)
		{
			var colors = new SKColor[pmcolors.Length];
			fixed (SKColor* c = colors)
			fixed (SKPMColor* pm = pmcolors) {
				SkiaApi.sk_color_unpremultiply_array ((uint*)pm, pmcolors.Length, (uint*)c);
			}
			return colors;
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
