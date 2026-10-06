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

	/// <summary>Describe an image's dimensions and pixel type.</summary>
	/// <remarks />
	public unsafe struct SKImageInfo : IEquatable<SKImageInfo>
	{
		/// <summary>An empty <see cref="T:SkiaSharp.SKImageInfo" />.</summary>
		/// <remarks />
		public static readonly SKImageInfo Empty;
		/// <summary>The current 32-bit color for the current platform.</summary>
		/// <remarks>On Windows, it is typically <see cref="F:SkiaSharp.SKColorType.Bgra8888" />, and on Unix-based systems (macOS, Linux) it is typically <see cref="F:SkiaSharp.SKColorType.Rgba8888" />.</remarks>
		public static readonly SKColorType PlatformColorType;
		/// <summary>The number of bits to shift left for the alpha color component.</summary>
		/// <remarks />
		public static readonly int PlatformColorAlphaShift;
		/// <summary>The number of bits to shift left for the red color component.</summary>
		/// <remarks />
		public static readonly int PlatformColorRedShift;
		/// <summary>The number of bits to shift left for the green color component.</summary>
		/// <remarks />
		public static readonly int PlatformColorGreenShift;
		/// <summary>The number of bits to shift left for the blue color component.</summary>
		/// <remarks />
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

		/// <summary>Gets or sets the width.</summary>
		/// <value>The width in pixels.</value>
		/// <remarks />
		public int Width { get; set; }

		/// <summary>Gets or sets the height.</summary>
		/// <value>The height in pixels.</value>
		/// <remarks />
		public int Height { get; set; }

		/// <summary>Gets or sets the color type.</summary>
		/// <value>One of the enumeration values that specifies the color type.</value>
		/// <remarks />
		public SKColorType ColorType { get; set; }

		/// <summary>Gets or sets the transparency type for the image info.</summary>
		/// <value>One of the enumeration values that specifies the transparency type.</value>
		/// <remarks />
		public SKAlphaType AlphaType { get; set; }

		/// <summary>Gets or sets the color space.</summary>
		/// <value>The color space, or <see langword="null" /> if not set.</value>
		/// <remarks />
		public SKColorSpace ColorSpace { get; set; }

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified width and height.</summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <remarks />
		public SKImageInfo (int width, int height)
		{
			Width = width;
			Height = height;
			ColorType = PlatformColorType;
			AlphaType = SKAlphaType.Premul;
			ColorSpace = null;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified width, height and color type.</summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="colorType">The color type.</param>
		/// <remarks />
		public SKImageInfo (int width, int height, SKColorType colorType)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = SKAlphaType.Premul;
			ColorSpace = null;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified width, height, color type and transparency type.</summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="colorType">The color type.</param>
		/// <param name="alphaType">The alpha/transparency type.</param>
		/// <remarks />
		public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = alphaType;
			ColorSpace = null;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified width, height, color type, transparency type and color space.</summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="colorType">The color type.</param>
		/// <param name="alphaType">The alpha/transparency type.</param>
		/// <param name="colorspace">The color space.</param>
		/// <remarks />
		public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace)
		{
			Width = width;
			Height = height;
			ColorType = colorType;
			AlphaType = alphaType;
			ColorSpace = colorspace;
		}

		/// <summary>Gets the number of bytes used per pixel.</summary>
		/// <value>The number of bytes used per pixel.</value>
		/// <remarks>This is calculated from the <see cref="P:SkiaSharp.SKImageInfo.ColorType" />. If the color type is <see cref="F:SkiaSharp.SKColorType.Unknown" />, then the value will be 0.</remarks>
		public readonly int BytesPerPixel =>
			ColorType.GetBytesPerPixel ();

		/// <summary>Gets the bit shift per pixel for the color type.</summary>
		/// <value>The number of bits to shift to move from one pixel to the next.</value>
		/// <remarks />
		public readonly int BitShiftPerPixel =>
			ColorType.GetBitShiftPerPixel ();

		/// <summary>Gets the number of bits used per pixel.</summary>
		/// <value>The number of bits used per pixel.</value>
		/// <remarks>This is equivalent to multiplying the <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" /> by 8 (the number of bits in a byte).</remarks>
		public readonly int BitsPerPixel => BytesPerPixel * 8;

		/// <summary>Gets the total number of bytes needed to store the bitmap data.</summary>
		/// <value>The total number of bytes needed to store the bitmap data.</value>
		/// <remarks>This is calculated as: <see cref="P:SkiaSharp.SKImageInfo.Width" /> * <see cref="P:SkiaSharp.SKImageInfo.Height" /> * <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" />.</remarks>
		public readonly int BytesSize => checked(Width * Height * BytesPerPixel);

		/// <summary>Gets the total number of bytes needed to store the bitmap data as a 64-bit integer.</summary>
		/// <value>The total number of bytes needed to store the bitmap data as a 64-bit integer.</value>
		/// <remarks>This is calculated as: <see cref="P:SkiaSharp.SKImageInfo.Width" /> * <see cref="P:SkiaSharp.SKImageInfo.Height" /> * <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" />.</remarks>
		public readonly long BytesSize64 => (long)Width * (long)Height * (long)BytesPerPixel;

		/// <summary>Gets the number of bytes per row.</summary>
		/// <value>The number of bytes per row.</value>
		/// <remarks>This is calculated as: <see cref="P:SkiaSharp.SKImageInfo.Width" /> * <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" />.</remarks>
		public readonly int RowBytes => checked(Width * BytesPerPixel);

		/// <summary>Gets the number of bytes per row as a 64-bit integer.</summary>
		/// <value>The number of bytes per row as a 64-bit integer.</value>
		/// <remarks>This is calculated as: <see cref="P:SkiaSharp.SKImageInfo.Width" /> * <see cref="P:SkiaSharp.SKImageInfo.BytesPerPixel" />.</remarks>
		public readonly long RowBytes64 => (long)Width * (long)BytesPerPixel;

		/// <summary>Gets a value indicating whether the width or height are less or equal than zero.</summary>
		/// <value><see langword="true" /> if the width or height are less than or equal to zero; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public readonly bool IsEmpty => Width <= 0 || Height <= 0;

		/// <summary>Gets a value indicating whether the configured alpha type is opaque.</summary>
		/// <value><see langword="true" /> if the configured alpha type is opaque; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public readonly bool IsOpaque => AlphaType == SKAlphaType.Opaque;

		/// <summary>Gets the current size of the image.</summary>
		/// <value>The current size of the image.</value>
		/// <remarks />
		public readonly SKSizeI Size => new SKSizeI (Width, Height);

		/// <summary>Gets a rectangle with the current width and height.</summary>
		/// <value>The rectangle with the current width and height.</value>
		/// <remarks />
		public readonly SKRectI Rect => SKRectI.Create (Width, Height);

		// uses the supplied stride rather than the info's packed RowBytes, so it is
		// correct for buffers whose row stride differs (e.g. a subset of a larger image)
		internal readonly int GetPixelBytesOffset (int x, int y, int rowBytes) =>
			ColorType == SKColorType.Unknown
				? 0
				: checked(y * rowBytes + (x << ColorType.GetBitShiftPerPixel ()));

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the specified size.</summary>
		/// <param name="size">The new size for the image info.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties but with the specified size.</returns>
		/// <remarks />
		public readonly SKImageInfo WithSize (SKSizeI size) =>
			WithSize (size.Width, size.Height);

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties as this <see cref="T:SkiaSharp.SKImageInfo" />, but with the specified dimensions.</summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageInfo" />.</returns>
		/// <remarks />
		public readonly SKImageInfo WithSize (int width, int height)
		{
			var copy = this;
			copy.Width = width;
			copy.Height = height;
			return copy;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties as this <see cref="T:SkiaSharp.SKImageInfo" />, but with the specified color type.</summary>
		/// <param name="newColorType">The color type.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageInfo" />.</returns>
		/// <remarks />
		public readonly SKImageInfo WithColorType (SKColorType newColorType)
		{
			var copy = this;
			copy.ColorType = newColorType;
			return copy;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties as this <see cref="T:SkiaSharp.SKImageInfo" />, but with the specified color space.</summary>
		/// <param name="newColorSpace">The color space.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageInfo" />.</returns>
		/// <remarks />
		public readonly SKImageInfo WithColorSpace (SKColorSpace newColorSpace)
		{
			var copy = this;
			copy.ColorSpace = newColorSpace;
			return copy;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKImageInfo" /> with the same properties as this <see cref="T:SkiaSharp.SKImageInfo" />, but with the specified transparency type.</summary>
		/// <param name="newAlphaType">The alpha/transparency type.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageInfo" />.</returns>
		/// <remarks />
		public readonly SKImageInfo WithAlphaType (SKAlphaType newAlphaType)
		{
			var copy = this;
			copy.AlphaType = newAlphaType;
			return copy;
		}

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKImageInfo" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKImageInfo" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKImageInfo obj) =>
			ColorSpace == obj.ColorSpace &&
			Width == obj.Width &&
			Height == obj.Height &&
			ColorType == obj.ColorType &&
			AlphaType == obj.AlphaType;

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKImageInfo f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKImageInfo" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKImageInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKImageInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKImageInfo left, SKImageInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKImageInfo" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKImageInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKImageInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKImageInfo left, SKImageInfo right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for this instance.</returns>
		/// <remarks />
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
