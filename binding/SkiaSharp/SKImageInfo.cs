#nullable disable

using System;

namespace SkiaSharp
{
	internal partial struct SKImageInfoNative
	{
		public static void UpdateNative (ref SKImageInfo managed, ref SKImageInfoNative native)
		{
			native.colorspace = managed.ColorSpace?.Handle ?? IntPtr.Zero;
			native.width = managed.Width;
			native.height = managed.Height;
			native.colorType = managed.ColorType.ToNative ();
			native.alphaType = managed.AlphaType;
		}

		public static SKImageInfoNative FromManaged (ref SKImageInfo managed) =>
			new SKImageInfoNative {
				colorspace = managed.ColorSpace?.Handle ?? IntPtr.Zero,
				width = managed.Width,
				height = managed.Height,
				colorType = managed.ColorType.ToNative (),
				alphaType = managed.AlphaType,
			};

		public static SKImageInfo ToManaged (ref SKImageInfoNative native) =>
			new SKImageInfo {
				ColorSpace = SKColorSpace.GetObject (native.colorspace),
				Width = native.width,
				Height = native.height,
				ColorType = native.colorType.FromNative (),
				AlphaType = native.alphaType,
			};
	}

	public unsafe struct SKImageInfo : IEquatable<SKImageInfo>
	{
		public static readonly SKImageInfo Empty;
		public static readonly SKColorType PlatformColorType;
		public static readonly int PlatformColorAlphaShift;
		public static readonly int PlatformColorRedShift;
		public static readonly int PlatformColorGreenShift;
		public static readonly int PlatformColorBlueShift;

		static SKImageInfo ()
		{
			PlatformColorType = SkiaApi.sk_colortype_get_default_8888 ().FromNative ();

			fixed (int* a = &PlatformColorAlphaShift)
			fixed (int* r = &PlatformColorRedShift)
			fixed (int* g = &PlatformColorGreenShift)
			fixed (int* b = &PlatformColorBlueShift) {
				SkiaApi.sk_color_get_bit_shift (a, r, g, b);
			}
		}

		public int Width { get; set; }

		public int Height { get; set; }

		public SKColorType ColorType { get; set; }

		public SKAlphaType AlphaType { get; set; }

		public SKColorSpace ColorSpace { get; set; }

		public SKImageInfo (int width, int height)
		{
			Width = width;
			Height = height;
			ColorType = PlatformColorType;
			AlphaType = SKAlphaType.Premul;
			ColorSpace = null;
		}

		public SKImageInfo (int width, int height, SKColorType colorType)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = SKAlphaType.Premul;
			ColorSpace = null;
		}

		public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = alphaType;
			ColorSpace = null;
		}

		public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = alphaType;
			ColorSpace = colorspace;
		}

		public readonly int BytesPerPixel =>
			ColorType.GetBytesPerPixel ();

		public readonly int BitShiftPerPixel =>
			ColorType.GetBitShiftPerPixel ();

		public readonly int BitsPerPixel => BytesPerPixel * 8;

		public readonly int BytesSize => Width * Height * BytesPerPixel;

		public readonly long BytesSize64 => (long)Width * (long)Height * (long)BytesPerPixel;

		public readonly int RowBytes => Width * BytesPerPixel;

		public readonly long RowBytes64 => (long)Width * (long)BytesPerPixel;

		public readonly bool IsEmpty => Width <= 0 || Height <= 0;

		public readonly bool IsOpaque => AlphaType == SKAlphaType.Opaque;

		public readonly SKSizeI Size => new SKSizeI (Width, Height);

		public readonly SKRectI Rect => SKRectI.Create (Width, Height);

		internal readonly int GetPixelBytesOffset (int x, int y) =>
			ColorType == SKColorType.Unknown
				? 0
				: y * RowBytes + (x << ColorType.GetBitShiftPerPixel ());

		public readonly SKImageInfo WithSize (SKSizeI size) =>
			WithSize (size.Width, size.Height);

		public readonly SKImageInfo WithSize (int width, int height)
		{
			var copy = this;
			copy.Width = width;
			copy.Height = height;
			return copy;
		}

		public readonly SKImageInfo WithColorType (SKColorType newColorType)
		{
			var copy = this;
			copy.ColorType = newColorType;
			return copy;
		}

		public readonly SKImageInfo WithColorSpace (SKColorSpace newColorSpace)
		{
			var copy = this;
			copy.ColorSpace = newColorSpace;
			return copy;
		}

		public readonly SKImageInfo WithAlphaType (SKAlphaType newAlphaType)
		{
			var copy = this;
			copy.AlphaType = newAlphaType;
			return copy;
		}

		public readonly bool Equals (SKImageInfo obj) =>
			ColorSpace == obj.ColorSpace &&
			Width == obj.Width &&
			Height == obj.Height &&
			ColorType == obj.ColorType &&
			AlphaType == obj.AlphaType;

		public readonly override bool Equals (object obj) =>
			obj is SKImageInfo f && Equals (f);

		public static bool operator == (SKImageInfo left, SKImageInfo right) =>
			left.Equals (right);

		public static bool operator != (SKImageInfo left, SKImageInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (ColorSpace);
			hash.Add (Width);
			hash.Add (Height);
			hash.Add (ColorType);
			hash.Add (AlphaType);
			return hash.ToHashCode ();
		}
	}
}
