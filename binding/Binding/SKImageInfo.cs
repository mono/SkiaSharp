using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	internal partial struct SKImageInfoNative
	{
		public static void UpdateNative (ref SKImageInfo managed, ref SKImageInfoNative native)
		{
			native.colorspace = managed.ColorSpace == null ? IntPtr.Zero : managed.ColorSpace.Handle;
			native.width = managed.Width;
			native.height = managed.Height;
			native.colorType = managed.ColorType;
			native.alphaType = managed.AlphaType;
		}

		public static SKImageInfoNative FromManaged (ref SKImageInfo managed)
		{
			return new SKImageInfoNative {
				colorspace = managed.ColorSpace == null ? IntPtr.Zero : managed.ColorSpace.Handle,
				width = managed.Width,
				height = managed.Height,
				colorType = managed.ColorType,
				alphaType = managed.AlphaType,
			};
		}

		public static SKImageInfo ToManaged (ref SKImageInfoNative native)
		{
			return new SKImageInfo {
				ColorSpace = SKObject.GetObject<SKColorSpace> (native.colorspace),
				Width = native.width,
				Height = native.height,
				ColorType = native.colorType,
				AlphaType = native.alphaType,
			};
		}
	}

	public unsafe struct SKImageInfo
	{
		public static readonly SKImageInfo Empty;
		public static readonly SKColorType PlatformColorType;
		public static readonly int PlatformColorAlphaShift;
		public static readonly int PlatformColorRedShift;
		public static readonly int PlatformColorGreenShift;
		public static readonly int PlatformColorBlueShift;

		static SKImageInfo ()
		{
			PlatformColorType = SkiaApi.sk_colortype_get_default_8888 ();

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

		public int BytesPerPixel {
			get {
				switch (ColorType) {
				case SKColorType.Unknown:
					return 0;
				case SKColorType.Alpha8:
				case SKColorType.Gray8:
					return 1;
				case SKColorType.Rgb565:
				case SKColorType.Argb4444:
					return 2;
				case SKColorType.Bgra8888:
				case SKColorType.Rgba8888:
				case SKColorType.Rgb888x:
				case SKColorType.Rgba1010102:
				case SKColorType.Rgb101010x:
					return 4;
				case SKColorType.RgbaF16:
					return 8;
				}
				throw new ArgumentOutOfRangeException (nameof (ColorType));
			}
		}

		public int BitsPerPixel => BytesPerPixel * 8;

		public int BytesSize => Width * Height * BytesPerPixel;

		public long BytesSize64 => (long)Width * (long)Height * (long)BytesPerPixel;

		public int RowBytes => Width * BytesPerPixel;

		public long RowBytes64 => (long)Width * (long)BytesPerPixel;

		public bool IsEmpty => Width <= 0 || Height <= 0;

		public bool IsOpaque => AlphaType == SKAlphaType.Opaque;

		public SKSizeI Size => new SKSizeI (Width, Height);

		public SKRectI Rect => SKRectI.Create (Width, Height);

		public SKImageInfo WithSize(int width, int height)
		{
			var copy = this;
			copy.Width = width;
			copy.Height = height;
			return copy;
		}

		public SKImageInfo WithColorType (SKColorType newColorType)
		{
			var copy = this;
			copy.ColorType = newColorType;
			return copy;
		}

		public SKImageInfo WithColorSpace (SKColorSpace newColorSpace)
		{
			var copy = this;
			copy.ColorSpace = newColorSpace;
			return copy;
		}

		public SKImageInfo WithAlphaType (SKAlphaType newAlphaType)
		{
			var copy = this;
			copy.AlphaType = newAlphaType;
			return copy;
		}
	}
}
