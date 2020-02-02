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
			native.colorType = managed.ColorType;
			native.alphaType = managed.AlphaType;
		}

		public static SKImageInfoNative FromManaged (ref SKImageInfo managed) =>
			new SKImageInfoNative {
				colorspace = managed.ColorSpace == null ? IntPtr.Zero : managed.ColorSpace.Handle,
				width = managed.Width,
				height = managed.Height,
				colorType = managed.ColorType,
				alphaType = managed.AlphaType,
			};

		public static SKImageInfo ToManaged (ref SKImageInfoNative native) =>
			new SKImageInfo {
				ColorSpace = SKObject.GetObject<SKColorSpace> (native.colorspace),
				Width = native.width,
				Height = native.height,
				ColorType = native.colorType,
				AlphaType = native.alphaType,
			};
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
			: this (width, height, PlatformColorType, SKAlphaType.Premul, null)
		{
		}

		public SKImageInfo (int width, int height, SKColorType colorType, SKColorSpace colorspace = null)
			: this (width, height, colorType, SKAlphaType.Premul, colorspace)
		{
		}

		public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace = null)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = alphaType;
			ColorSpace = colorspace;
		}

		public readonly int BytesPerPixel =>
			ColorType switch
			{
				SKColorType.Unknown => 0,
				SKColorType.Alpha8 => 1,
				SKColorType.Gray8 => 1,
				SKColorType.Rgb565 => 2,
				SKColorType.Argb4444 => 2,
				SKColorType.R8g8Unnormalized => 2,
				SKColorType.A16Unnormalized => 2,
				SKColorType.A16Float => 2,
				SKColorType.Bgra8888 => 4,
				SKColorType.Rgba8888 => 4,
				SKColorType.Rgb888x => 4,
				SKColorType.Rgba1010102 => 4,
				SKColorType.Rgb101010x => 4,
				SKColorType.R16g16Unnormalized => 4,
				SKColorType.R16g16Float => 4,
				SKColorType.RgbaF16Normalized => 8,
				SKColorType.RgbaF16 => 8,
				SKColorType.R16g16b16a16Unnormalized => 8,
				SKColorType.RgbaF32 => 16,
				_ => throw new ArgumentOutOfRangeException (nameof (ColorType)),
			};

		public readonly int BitsPerPixel => BytesPerPixel * 8;

		public readonly int BytesSize => Width * Height * BytesPerPixel;

		public readonly long BytesSize64 => (long)Width * (long)Height * (long)BytesPerPixel;

		public readonly int RowBytes => Width * BytesPerPixel;

		public readonly long RowBytes64 => (long)Width * (long)BytesPerPixel;

		public readonly bool IsEmpty => Width <= 0 || Height <= 0;

		public readonly bool IsOpaque => AlphaType == SKAlphaType.Opaque;

		public readonly SKSizeI Size => new SKSizeI (Width, Height);

		public readonly SKRectI Rect => SKRectI.Create (Width, Height);

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
	}
}
