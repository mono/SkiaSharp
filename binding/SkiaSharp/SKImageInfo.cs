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

	/// <summary>
	/// Describe an image's dimensions and pixel type.
	/// </summary>
	public unsafe struct SKImageInfo : IEquatable<SKImageInfo>
	{
		/// <summary>
		/// An empty <see cref="T:SkiaSharp.SKImageInfo" />.
		/// </summary>
		public static readonly SKImageInfo Empty;
		/// <summary>
		/// The current 32-bit color for the current platform.
		/// </summary>
		/// <remarks>On Windows, it is typically <see cref="F:SkiaSharp.SKColorType.Bgra8888" />, and on Unix-based systems (macOS, Linux) it is typically <see cref="F:SkiaSharp.SKColorType.Rgba8888" />.</remarks>
		public static readonly SKColorType PlatformColorType;
		/// <summary>
		/// The number of bits to shift left for the alpha color component.
		/// </summary>
		public static readonly int PlatformColorAlphaShift;
		/// <summary>
		/// The number of bits to shift left for the red color component.
		/// </summary>
		public static readonly int PlatformColorRedShift;
		/// <summary>
		/// The number of bits to shift left for the green color component.
		/// </summary>
		public static readonly int PlatformColorGreenShift;
		/// <summary>
		/// The number of bits to shift left for the blue color component.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Gets or sets the color type.
		/// </summary>
		public SKColorType ColorType { get; set; }

		/// <summary>
		/// Gets the transparency type for the image info.
		/// </summary>
		public SKAlphaType AlphaType { get; set; }

		/// <summary>
		/// Gets or sets the color space.
		/// </summary>
		public SKColorSpace ColorSpace { get; set; }

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified width and height.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		public SKImageInfo (int width, int height)
		{
			Width = width;
			Height = height;
			ColorType = PlatformColorType;
			AlphaType = SKAlphaType.Premul;
			ColorSpace = null;
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified width, height and color type.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="colorType">The color type.</param>
		public SKImageInfo (int width, int height, SKColorType colorType)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = SKAlphaType.Premul;
			ColorSpace = null;
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified width, height, color type and transparency type.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="colorType">The color type.</param>
		/// <param name="alphaType">The alpha/transparency type.</param>
		public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = alphaType;
			ColorSpace = null;
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified width, height, color type, transparency type and color space.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="colorType">The color type.</param>
		/// <param name="alphaType">The alpha/transparency type.</param>
		/// <param name="colorspace">The color space.</param>
		public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = alphaType;
			ColorSpace = colorspace;
		}

		/// <summary>
		/// Gets the number of bytes used per pixel.
		/// </summary>
		/// <remarks>This is calculated from the <see cref="P:SkiaSharp.SKImageInfo.ColorType" />. If the color type is <see cref="F:SkiaSharp.SKColorType.Unknown" />, then the value will be 0.</remarks>
		public readonly int BytesPerPixel =>
			ColorType.GetBytesPerPixel ();

		public readonly int BitShiftPerPixel =>
			ColorType.GetBitShiftPerPixel ();

		/// <summary>
		/// Gets the number of bits used per pixel.
		/// </summary>
		/// <remarks>This is equivalent to multiplying the <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" /> by 8 (the number of bits in a byte).</remarks>
		public readonly int BitsPerPixel => BytesPerPixel * 8;

		/// <summary>
		/// Gets the total number of bytes needed to store the bitmap data.
		/// </summary>
		/// <remarks>This is calculated as: <see cref="P:SkiaSharp.SKImageInfo.Width" /> * <see cref="P:SkiaSharp.SKImageInfo.Height" /> * <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" />.</remarks>
		public readonly int BytesSize => Width * Height * BytesPerPixel;

		/// <summary>
		/// Gets the total number of bytes needed to store the bitmap data as a 64-bit integer.
		/// </summary>
		/// <remarks>This is calculated as: <see cref="P:SkiaSharp.SKImageInfo.Width" /> * <see cref="P:SkiaSharp.SKImageInfo.Height" /> * <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" />.</remarks>
		public readonly long BytesSize64 => (long)Width * (long)Height * (long)BytesPerPixel;

		/// <summary>
		/// Gets the number of bytes per row.
		/// </summary>
		/// <remarks>This is calculated as: <see cref="P:SkiaSharp.SKImageInfo.Width" /> * <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" />.</remarks>
		public readonly int RowBytes => Width * BytesPerPixel;

		/// <summary>
		/// Gets the number of bytes per row as a 64-bit integer.
		/// </summary>
		/// <remarks>This is calculated as: <see cref="P:SkiaSharp.SKImageInfo.Width" /> * <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" />.</remarks>
		public readonly long RowBytes64 => (long)Width * (long)BytesPerPixel;

		/// <summary>
		/// Gets a value indicating whether the width or height are less or equal than zero.
		/// </summary>
		public readonly bool IsEmpty => Width <= 0 || Height <= 0;

		/// <summary>
		/// Gets a value indicating whether the configured alpha type is opaque.
		/// </summary>
		public readonly bool IsOpaque => AlphaType == SKAlphaType.Opaque;

		/// <summary>
		/// Gets the current size of the image.
		/// </summary>
		public readonly SKSizeI Size => new SKSizeI (Width, Height);

		/// <summary>
		/// Gets a rectangle with the current width and height.
		/// </summary>
		public readonly SKRectI Rect => SKRectI.Create (Width, Height);

		internal readonly int GetPixelBytesOffset (int x, int y) =>
			ColorType == SKColorType.Unknown
				? 0
				: y * RowBytes + (x << ColorType.GetBitShiftPerPixel ());

		/// <param name="size"></param>
		public readonly SKImageInfo WithSize (SKSizeI size) =>
			WithSize (size.Width, size.Height);

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties as this <see cref="T:SkiaSharp.SKImageInfo" />, but with the specified dimensions.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageInfo" />.</returns>
		public readonly SKImageInfo WithSize (int width, int height)
		{
			var copy = this;
			copy.Width = width;
			copy.Height = height;
			return copy;
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties as this <see cref="T:SkiaSharp.SKImageInfo" />, but with the specified color type.
		/// </summary>
		/// <param name="newColorType">The color type.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageInfo" />.</returns>
		public readonly SKImageInfo WithColorType (SKColorType newColorType)
		{
			var copy = this;
			copy.ColorType = newColorType;
			return copy;
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties as this <see cref="T:SkiaSharp.SKImageInfo" />, but with the specified color space.
		/// </summary>
		/// <param name="newColorSpace">The color space.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageInfo" />.</returns>
		public readonly SKImageInfo WithColorSpace (SKColorSpace newColorSpace)
		{
			var copy = this;
			copy.ColorSpace = newColorSpace;
			return copy;
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties as this <see cref="T:SkiaSharp.SKImageInfo" />, but with the specified transparency type.
		/// </summary>
		/// <param name="newAlphaType">The alpha/transparency type.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageInfo" />.</returns>
		public readonly SKImageInfo WithAlphaType (SKAlphaType newAlphaType)
		{
			var copy = this;
			copy.AlphaType = newAlphaType;
			return copy;
		}

		/// <param name="obj"></param>
		public readonly bool Equals (SKImageInfo obj) =>
			ColorSpace == obj.ColorSpace &&
			Width == obj.Width &&
			Height == obj.Height &&
			ColorType == obj.ColorType &&
			AlphaType == obj.AlphaType;

		/// <param name="obj"></param>
		public readonly override bool Equals (object obj) =>
			obj is SKImageInfo f && Equals (f);

		/// <param name="left"></param>
		/// <param name="right"></param>
		public static bool operator == (SKImageInfo left, SKImageInfo right) =>
			left.Equals (right);

		/// <param name="left"></param>
		/// <param name="right"></param>
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
