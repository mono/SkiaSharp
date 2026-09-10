#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>Various predefined font weights for use with <see cref="T:SkiaSharp.SKTypeface" />.</summary>
	/// <remarks>Font weights can range from anywhere between 100 to 1000 (inclusive).</remarks>
	public enum SKFontStyleWeight
	{
		/// <summary>The font has no thickness at all.</summary>
		Invisible = 0,
		/// <summary>A thin font weight of 100.</summary>
		Thin = 100,
		/// <summary>A thin font weight of 200.</summary>
		ExtraLight = 200,
		/// <summary>A thin font weight of 300.</summary>
		Light = 300,
		/// <summary>A typical font weight of 400. This is the default font weight.</summary>
		Normal = 400,
		/// <summary>A thicker font weight of 500.</summary>
		Medium = 500,
		/// <summary>A thick font weight of 600.</summary>
		SemiBold = 600,
		/// <summary>A thick font weight of 700. This is the default for a bold font.</summary>
		Bold = 700,
		/// <summary>A thick font weight of 800.</summary>
		ExtraBold = 800,
		/// <summary>A thick font weight of 900.</summary>
		Black = 900,
		/// <summary>A thick font weight of 1000.</summary>
		ExtraBlack = 1000,
	}

	/// <summary>Various predefined font widths for use with <see cref="T:SkiaSharp.SKTypeface" />.</summary>
	/// <remarks />
	public enum SKFontStyleWidth
	{
		/// <summary>A condensed font width of 1.</summary>
		UltraCondensed = 1,
		/// <summary>A condensed font width of 2.</summary>
		ExtraCondensed = 2,
		/// <summary>A condensed font width of 3.</summary>
		Condensed = 3,
		/// <summary>A condensed font width of 4.</summary>
		SemiCondensed = 4,
		/// <summary>A normal font width of 5. This is the default font width.</summary>
		Normal = 5,
		/// <summary>An expanded font width of 6.</summary>
		SemiExpanded = 6,
		/// <summary>An expanded font width of 7.</summary>
		Expanded = 7,
		/// <summary>An expanded font width of 8.</summary>
		ExtraExpanded = 8,
		/// <summary>An expanded font width of 9.</summary>
		UltraExpanded = 9,
	}

	/// <summary>Describes how to interpret the components of a pixel.</summary>
	/// <remarks />
	public enum SKColorType
	{
		/// <summary>Unknown encoding.</summary>
		Unknown = 0,
		/// <summary>Represents a 8-bit alpha-only color.</summary>
		Alpha8 = 1,
		/// <summary>Represents an opaque 16-bit color with the format RGB, with the red and blue components being 5 bits and the green component being 6 bits.</summary>
		Rgb565 = 2,
		/// <summary>Represents a 16-bit color with the format ARGB.</summary>
		Argb4444 = 3,
		/// <summary>Represents a 32-bit color with the format RGBA.</summary>
		Rgba8888 = 4,
		/// <summary>Represents an opaque 32-bit color with the format RGB, with 8 bits per color component.</summary>
		Rgb888x = 5,
		/// <summary>Represents a 32-bit color with the format BGRA.</summary>
		Bgra8888 = 6,
		/// <summary>Represents a 32-bit color with the format RGBA, with 10 bits per color component and 2 bits for the alpha component.</summary>
		Rgba1010102 = 7,
		/// <summary>Represents an opaque 32-bit color with the format RGB, with 10 bits per color component.</summary>
		Rgb101010x = 8,
		/// <summary>Represents an opaque 8-bit grayscale color.</summary>
		Gray8 = 9,
		/// <summary>Represents a floating-point based color with the format RGBA.</summary>
		RgbaF16 = 10,
		/// <summary>Represents a half-precision floating-point color with clamped RGBA values in the range 0.0 to 1.0.</summary>
		RgbaF16Clamped = 11,
		/// <summary>Represents a 128-bit single-precision floating-point color with the format RGBA.</summary>
		RgbaF32 = 12,
		/// <summary>Represents a 16-bit color with 8 bits each for red and green channels.</summary>
		Rg88 = 13,
		/// <summary>Represents a 16-bit half-precision floating-point alpha-only color.</summary>
		AlphaF16 = 14,
		/// <summary>Represents a 32-bit half-precision floating-point color with red and green channels.</summary>
		RgF16 = 15,
		/// <summary>Represents a 16-bit alpha-only color.</summary>
		Alpha16 = 16,
		/// <summary>Represents a 32-bit color with 16 bits each for red and green channels.</summary>
		Rg1616 = 17,
		/// <summary>Represents a 64-bit color with 16 bits per RGBA component.</summary>
		Rgba16161616 = 18,
		/// <summary>Represents a 32-bit color with the format BGRA, with 10 bits per color component and 2 bits for alpha.</summary>
		Bgra1010102 = 19,
		/// <summary>Represents an opaque 32-bit color with the format BGR, with 10 bits per color component.</summary>
		Bgr101010x = 20,
		/// <summary>Represents an extended-range 32-bit color with the format BGR, with 10 bits per color component.</summary>
		Bgr101010xXR = 21,
		/// <summary>Represents a 32-bit sRGB color with the format RGBA.</summary>
		Srgba8888 = 22,
		/// <summary>Represents a single-channel 8-bit unsigned normalized red color.</summary>
		R8Unorm = 23,
		/// <summary>Represents a 64-bit color with 10 bits each for RGBA plus 6 bits of padding.</summary>
		Rgba10x6 = 24,
		/// <summary>An 8-bytes-per-pixel format storing blue, green, red, and alpha as 10-bit extended-range (XR) values.</summary>
		Bgra10101010XR = 25,
		/// <summary>An 8-bytes-per-pixel format storing red, green, and blue as 16-bit floating-point values, with a 16-bit padding component.</summary>
		RgbF16F16F16x = 26,
		/// <summary>A 2-bytes-per-pixel format storing a single red channel as a 16-bit normalized unsigned integer.</summary>
		R16Unorm = 27,
		/// <summary>A 2-bytes-per-pixel format storing a single red channel as a 16-bit floating-point value.</summary>
		RF16 = 28,
	}

	/// <summary>Convenience methods for <see cref="T:SkiaSharp.SKPixelGeometry" />.</summary>
	/// <remarks />
	public static partial class SkiaExtensions
	{
		/// <summary>Determines whether the pixel geometry is BGR.</summary>
		/// <param name="pg">The pixel geometry to test.</param>
		/// <returns><see langword="true" /> if the pixel geometry is BGR; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool IsBgr (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.BgrVertical;

		/// <summary>Determines whether the pixel geometry is RGB.</summary>
		/// <param name="pg">The pixel geometry to test.</param>
		/// <returns><see langword="true" /> if the pixel geometry is RGB; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool IsRgb (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.RgbHorizontal || pg == SKPixelGeometry.RgbVertical;

		/// <summary>Determines whether the pixel geometry is vertical.</summary>
		/// <param name="pg">The pixel geometry to test.</param>
		/// <returns><see langword="true" /> if the pixel geometry is vertical; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool IsVertical (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrVertical || pg == SKPixelGeometry.RgbVertical;

		/// <summary>Determines whether the pixel geometry is horizontal.</summary>
		/// <param name="pg">The pixel geometry to test.</param>
		/// <returns><see langword="true" /> if the pixel geometry is horizontal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool IsHorizontal (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.RgbHorizontal;

		// SkImageInfo.cpp - SkColorTypeBytesPerPixel
		/// <summary>Gets the number of bytes per pixel for the specified color type.</summary>
		/// <param name="colorType">The color type to query.</param>
		/// <returns>The number of bytes required to store one pixel.</returns>
		/// <remarks />
		public static int GetBytesPerPixel (this SKColorType colorType) =>
			colorType switch {
				// 0
				SKColorType.Unknown => 0,
				// 1
				SKColorType.Alpha8 => 1,
				SKColorType.Gray8 => 1,
				SKColorType.R8Unorm => 1,
				// 2
				SKColorType.Rgb565 => 2,
				SKColorType.Argb4444 => 2,
				SKColorType.Rg88 => 2,
				SKColorType.Alpha16 => 2,
				SKColorType.AlphaF16 => 2,
				SKColorType.R16Unorm => 2,
				SKColorType.RF16 => 2,
				// 4
				SKColorType.Bgra8888 => 4,
				SKColorType.Bgra1010102 => 4,
				SKColorType.Bgr101010x => 4,
				SKColorType.Bgr101010xXR => 4,
				SKColorType.Rgba8888 => 4,
				SKColorType.Rgb888x => 4,
				SKColorType.Rgba1010102 => 4,
				SKColorType.Rgb101010x => 4,
				SKColorType.Rg1616 => 4,
				SKColorType.RgF16 => 4,
				SKColorType.Srgba8888 => 4,
				// 8
				SKColorType.RgbaF16Clamped => 8,
				SKColorType.RgbaF16 => 8,
				SKColorType.Rgba16161616 => 8,
				SKColorType.Rgba10x6 => 8,
				SKColorType.Bgra10101010XR => 8,
				SKColorType.RgbF16F16F16x => 8,
				// 16
				SKColorType.RgbaF32 => 16,
				//
				_ => throw new ArgumentOutOfRangeException (nameof (colorType), $"Unknown color type: '{colorType}'"),
			};

		// SkImageInfoPriv.h - SkColorTypeShiftPerPixel
		/// <summary>Gets the bit shift value per pixel for the specified color type.</summary>
		/// <param name="colorType">The color type to query.</param>
		/// <returns>The number of bits to shift to calculate byte offsets for pixels.</returns>
		/// <remarks />
		public static int GetBitShiftPerPixel (this SKColorType colorType) =>
			colorType switch {
				// 0
				SKColorType.Unknown => 0,
				// 0
				SKColorType.Alpha8 => 0,
				SKColorType.Gray8 => 0,
				SKColorType.R8Unorm => 0,
				// 1
				SKColorType.Rgb565 => 1,
				SKColorType.Argb4444 => 1,
				SKColorType.Rg88 => 1,
				SKColorType.Alpha16 => 1,
				SKColorType.AlphaF16 => 1,
				SKColorType.R16Unorm => 1,
				SKColorType.RF16 => 1,
				// 2
				SKColorType.Bgra8888 => 2,
				SKColorType.Bgra1010102 => 2,
				SKColorType.Bgr101010x => 2,
				SKColorType.Bgr101010xXR => 2,
				SKColorType.Rgba8888 => 2,
				SKColorType.Rgb888x => 2,
				SKColorType.Rgba1010102 => 2,
				SKColorType.Rgb101010x => 2,
				SKColorType.Rg1616 => 2,
				SKColorType.RgF16 => 2,
				SKColorType.Srgba8888 => 2,
				// 3
				SKColorType.RgbaF16Clamped => 3,
				SKColorType.RgbaF16 => 3,
				SKColorType.Rgba16161616 => 3,
				SKColorType.Rgba10x6 => 3,
				SKColorType.Bgra10101010XR => 3,
				SKColorType.RgbF16F16F16x => 3,
				// 4
				SKColorType.RgbaF32 => 4,
				//
				_ => throw new ArgumentOutOfRangeException (nameof (colorType), $"Unknown color type: '{colorType}'"),
			};

		// SkImageInfo.cpp - SkColorTypeValidateAlphaType
		/// <summary>Returns a valid alpha type for the specified color type.</summary>
		/// <param name="colorType">The color type to validate against.</param>
		/// <param name="alphaType">The alpha type to validate.</param>
		/// <returns>A valid alpha type that is compatible with the color type.</returns>
		/// <remarks />
		public static SKAlphaType GetAlphaType (this SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Premul)
		{
			switch (colorType) {
				case SKColorType.Unknown:
					alphaType = SKAlphaType.Unknown;
					break;

				// opaque or premul
				case SKColorType.Alpha8:
				case SKColorType.Alpha16:
				case SKColorType.AlphaF16:
					if (SKAlphaType.Unpremul == alphaType) {
						alphaType = SKAlphaType.Premul;
					}
					break;

				// any
				case SKColorType.Argb4444:
				case SKColorType.Rgba8888:
				case SKColorType.Bgra8888:
				case SKColorType.Srgba8888:
				case SKColorType.Rgba1010102:
				case SKColorType.Bgra1010102:
				case SKColorType.RgbaF16Clamped:
				case SKColorType.RgbaF16:
				case SKColorType.RgbaF32:
				case SKColorType.Rgba16161616:
				case SKColorType.Rgba10x6:
				case SKColorType.Bgra10101010XR:
					break;

				// opaque
				case SKColorType.Gray8:
				case SKColorType.Rg88:
				case SKColorType.Rg1616:
				case SKColorType.RgF16:
				case SKColorType.Rgb565:
				case SKColorType.Rgb888x:
				case SKColorType.Rgb101010x:
				case SKColorType.Bgr101010x:
				case SKColorType.Bgr101010xXR:
				case SKColorType.R8Unorm:
				case SKColorType.R16Unorm:
				case SKColorType.RF16:
				case SKColorType.RgbF16F16F16x:
					alphaType = SKAlphaType.Opaque;
					break;

				default:
					throw new ArgumentOutOfRangeException (nameof (colorType), $"Unknown color type: '{colorType}'");
			}

			return alphaType;
		}
	}

	/// <summary>Additional options to pass to <see cref="M:SkiaSharp.SKCodec.GetPixels(SkiaSharp.SKImageInfo,System.IntPtr,SkiaSharp.SKCodecOptions)" /> or one of the overloads that accepts a <see cref="T:SkiaSharp.SKCodecOptions" />.</summary>
	/// <remarks />
	public struct SKCodecOptions : IEquatable<SKCodecOptions>
	{
		/// <summary>Gets the default options.</summary>
		/// <remarks>The default value is not zero-initialized and without a subset rectangle.</remarks>
		public static readonly SKCodecOptions Default;

		static SKCodecOptions ()
		{
			Default = new SKCodecOptions (SKZeroInitialized.No);
		}

		/// <summary>Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" /> with the specified zero-initialization.</summary>
		/// <param name="zeroInitialized">The zero-initialization.</param>
		/// <remarks />
		public SKCodecOptions (SKZeroInitialized zeroInitialized)
		{
			ZeroInitialized = zeroInitialized;
			Subset = null;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		/// <summary>Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" /> with the specified subset rectangle and zero-initialization.</summary>
		/// <param name="zeroInitialized">The zero-initialization.</param>
		/// <param name="subset">The subset rectangle.</param>
		/// <remarks />
		public SKCodecOptions (SKZeroInitialized zeroInitialized, SKRectI subset)
		{
			ZeroInitialized = zeroInitialized;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		/// <summary>Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" /> with the specified subset rectangle.</summary>
		/// <param name="subset">The subset rectangle.</param>
		/// <remarks />
		public SKCodecOptions (SKRectI subset)
		{
			ZeroInitialized = SKZeroInitialized.No;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		/// <summary>Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" /> with the specified frame index.</summary>
		/// <param name="frameIndex">The frame to decode.</param>
		/// <remarks>Only meaningful for multi-frame images.</remarks>
		public SKCodecOptions (int frameIndex)
		{
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = -1;
		}
		/// <summary>Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" />.</summary>
		/// <param name="frameIndex">The frame to decode.</param>
		/// <param name="priorFrame">The previous frame to decode.</param>
		/// <remarks>Only meaningful for multi-frame images.</remarks>
		public SKCodecOptions (int frameIndex, int priorFrame)
		{
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = priorFrame;
		}

		/// <summary>Gets or sets the zero-initialization.</summary>
		/// <value>The zero-initialization setting.</value>
		/// <remarks />
		public SKZeroInitialized ZeroInitialized { readonly get; set; }
		/// <summary>Gets or sets the subset rectangle.</summary>
		/// <value>The subset rectangle, or <see langword="null" /> if no subset is specified.</value>
		/// <remarks />
		public SKRectI? Subset { readonly get; set; }
		/// <summary>Gets a value indicating whether the options has a subset rectangle.</summary>
		/// <value><see langword="true" /> if the options has a subset rectangle; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public readonly bool HasSubset => Subset != null;
		/// <summary>Gets or sets the frame to decode.</summary>
		/// <value>The zero-based index of the frame to decode.</value>
		/// <remarks>Only meaningful for multi-frame images.</remarks>
		public int FrameIndex { readonly get; set; }
		/// <summary>Gets or sets a value indicating which frame, if any, the destination bitmap already contains.</summary>
		/// <value>The index of the prior frame, or -1 to indicate no prior frame.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Only meaningful for multi-frame images.
		///
		/// If <xref:SkiaSharp.SKCodecOptions.FrameIndex> needs to be blended with a prior
		/// frame (as reported by `SKCodec.FrameInfo[FrameIndex].RequiredFrame`), the
		/// client can set this to any non-<xref:SkiaSharp.SKCodecAnimationDisposalMethod.RestorePrevious>
		/// frame in the range [RequiredFrame, FrameIndex) to indicate that that frame is
		/// already in the destination. <xref:SkiaSharp.SKCodecOptions.ZeroInitialized> is
		/// ignored in this case.
		///
		/// If set to -1, the codec will decode any necessary required frame(s) first.
		/// ]]></format></remarks>
		public int PriorFrame { readonly get; set; }

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKCodecOptions" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKCodecOptions" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKCodecOptions obj) =>
			ZeroInitialized == obj.ZeroInitialized &&
			Subset == obj.Subset &&
			FrameIndex == obj.FrameIndex &&
			PriorFrame == obj.PriorFrame;

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKCodecOptions f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.SKCodecOptions" /> objects have the same value.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKCodecOptions" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKCodecOptions" /> to compare.</param>
		/// <returns><see langword="true" /> if the value of <paramref name="left" /> is the same as the value of <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKCodecOptions left, SKCodecOptions right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.SKCodecOptions" /> objects have different values.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKCodecOptions" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKCodecOptions" /> to compare.</param>
		/// <returns><see langword="true" /> if the value of <paramref name="left" /> is different from the value of <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKCodecOptions left, SKCodecOptions right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (ZeroInitialized);
			hash.Add (Subset);
			hash.Add (FrameIndex);
			hash.Add (PriorFrame);
			return hash.ToHashCode ();
		}
	}

	/// <summary>Structure to represent measurements for a font.</summary>
	/// <remarks />
	public partial struct SKFontMetrics
	{
		private const uint flagsUnderlineThicknessIsValid = (1U << 0);
		private const uint flagsUnderlinePositionIsValid = (1U << 1);
		private const uint flagsStrikeoutThicknessIsValid = (1U << 2);
		private const uint flagsStrikeoutPositionIsValid = (1U << 3);

		/// <summary>Gets the greatest distance above the baseline for any glyph.</summary>
		/// <value>The greatest distance above the baseline for any glyph.</value>
		/// <remarks>Will be &lt;= 0.</remarks>
		public readonly float Top => fTop;

		/// <summary>Gets the recommended distance above the baseline.</summary>
		/// <value>The recommended distance above the baseline.</value>
		/// <remarks>Will be &lt;= 0.</remarks>
		public readonly float Ascent => fAscent;

		/// <summary>Gets the recommended distance below the baseline.</summary>
		/// <value>The recommended distance below the baseline.</value>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float Descent => fDescent;

		/// <summary>Gets the greatest distance below the baseline for any glyph.</summary>
		/// <value>The greatest distance below the baseline for any glyph.</value>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float Bottom => fBottom;

		/// <summary>Gets the recommended distance to add between lines of text.</summary>
		/// <value>The recommended distance to add between lines of text.</value>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float Leading => fLeading;

		/// <summary>Gets the average character width.</summary>
		/// <value>The average character width.</value>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float AverageCharacterWidth => fAvgCharWidth;

		/// <summary>Gets the max character width.</summary>
		/// <value>The max character width.</value>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float MaxCharacterWidth => fMaxCharWidth;

		/// <summary>Gets the minimum bounding box x value for all glyphs.</summary>
		/// <value>The minimum bounding box x value for all glyphs.</value>
		/// <remarks />
		public readonly float XMin => fXMin;

		/// <summary>Gets the maximum bounding box x value for all glyphs.</summary>
		/// <value>The maximum bounding box x value for all glyphs.</value>
		/// <remarks />
		public readonly float XMax => fXMax;

		/// <summary>Gets the height of an 'x' in px.</summary>
		/// <value>The height of an 'x' in px.</value>
		/// <remarks>0 if no 'x' in face.</remarks>
		public readonly float XHeight => fXHeight;

		/// <summary>Gets the cap height.</summary>
		/// <value>The cap height.</value>
		/// <remarks>Will be &gt; 0, or 0 if cannot be determined.</remarks>
		public readonly float CapHeight => fCapHeight;

		/// <summary>Gets the thickness of the underline.</summary>
		/// <value>The thickness of the underline, or <see langword="null" /> if the font does not have this metric.</value>
		/// <remarks><para>0 - if the thickness can not be determined</para><para>null - if the thickness is not set.</para></remarks>
		public readonly float? UnderlineThickness => GetIfValid (fUnderlineThickness, flagsUnderlineThicknessIsValid);
		/// <summary>Gets the position of the top of the underline stroke relative to the baseline.</summary>
		/// <value>The position of the top of the underline stroke relative to the baseline, or <see langword="null" /> if the font does not have this metric.</value>
		/// <remarks><para>Negative - underline should be drawn above baseline.</para><para>Positive - underline should be drawn below baseline.</para><para>Zero - underline should be drawn on baseline.underline position, or 0 if cannot be determined.</para><para>null - does not have an UnderlinePosition.</para></remarks>
		public readonly float? UnderlinePosition => GetIfValid (fUnderlinePosition, flagsUnderlinePositionIsValid);
		/// <summary>Gets the thickness of the strikeout.</summary>
		/// <value>The thickness of the strikeout, or <see langword="null" /> if the font does not have this metric.</value>
		/// <remarks />
		public readonly float? StrikeoutThickness => GetIfValid (fStrikeoutThickness, flagsStrikeoutThicknessIsValid);
		/// <summary>Gets the position of the bottom of the strikeout stroke relative to the baseline.</summary>
		/// <value>The position of the bottom of the strikeout stroke relative to the baseline, or <see langword="null" /> if the font does not have this metric.</value>
		/// <remarks>This value is typically negative when valid.</remarks>
		public readonly float? StrikeoutPosition => GetIfValid (fStrikeoutPosition, flagsStrikeoutPositionIsValid);

		private readonly float? GetIfValid (float value, uint flag) =>
			(fFlags & flag) == flag ? value : (float?)null;
	}

	/// <summary>Specifies coordinates to divide a bitmap into (<see cref="P:SkiaSharp.SKLattice.XDivs" /> * <see cref="P:SkiaSharp.SKLattice.YDivs" />) rectangles.</summary>
	/// <remarks>If the lattice divs or bounds are invalid, the entire lattice structure will be ignored on the draw call.</remarks>
	public struct SKLattice : IEquatable<SKLattice>
	{
		/// <summary>Gets or sets the x-coordinates for the lattice.</summary>
		/// <value>The array of x-coordinates that divide the lattice.</value>
		/// <remarks />
		public int[] XDivs { readonly get; set; }
		/// <summary>Gets or sets the y-coordinates for the lattice.</summary>
		/// <value>The array of y-coordinates that divide the lattice.</value>
		/// <remarks />
		public int[] YDivs { readonly get; set; }
		/// <summary>Gets or sets the color for each of the lattice rectangles.</summary>
		/// <value>The array of rectangle types for each grid entry.</value>
		/// <remarks />
		public SKLatticeRectType[] RectTypes { readonly get; set; }
		/// <summary>Gets or sets the optional source image bounds.</summary>
		/// <value>The optional source image bounds, or <see langword="null" /> if not specified.</value>
		/// <remarks />
		public SKRectI? Bounds { readonly get; set; }
		/// <summary>Gets or sets the array of fill types, one per rectangular grid entry.</summary>
		/// <value>The array of colors for each grid entry.</value>
		/// <remarks />
		public SKColor[] Colors { readonly get; set; }

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKLattice" /> is equal to the current lattice.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKLattice" /> to compare with the current lattice.</param>
		/// <returns><see langword="true" /> if the specified lattice is equal to the current lattice; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKLattice obj) =>
			XDivs == obj.XDivs &&
			YDivs == obj.YDivs &&
			RectTypes == obj.RectTypes &&
			Bounds == obj.Bounds &&
			Colors == obj.Colors;

		/// <summary>Determines whether the specified object is equal to the current lattice.</summary>
		/// <param name="obj">The object to compare with the current lattice.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current lattice; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKLattice f && Equals (f);

		/// <summary>Indicates whether two <see cref="T:SkiaSharp.SKLattice" /> objects are equal.</summary>
		/// <param name="left">The first lattice to compare.</param>
		/// <param name="right">The second lattice to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKLattice left, SKLattice right) =>
			left.Equals (right);

		/// <summary>Indicates whether two <see cref="T:SkiaSharp.SKLattice" /> objects are different.</summary>
		/// <param name="left">The first lattice to compare.</param>
		/// <param name="right">The second lattice to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKLattice left, SKLattice right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (XDivs);
			hash.Add (YDivs);
			hash.Add (RectTypes);
			hash.Add (Bounds);
			hash.Add (Colors);
			return hash.ToHashCode ();
		}
	}

	internal partial struct SKTimeDateTimeInternal
	{
		public static SKTimeDateTimeInternal Create (DateTime datetime)
		{
			var zone = datetime.ToLocalTime ().Hour - datetime.ToUniversalTime ().Hour;
			return new SKTimeDateTimeInternal {
				fTimeZoneMinutes = (Int16)(zone * 60),
				fYear = (UInt16)datetime.Year,
				fMonth = (Byte)datetime.Month,
				fDayOfWeek = (Byte)datetime.DayOfWeek,
				fDay = (Byte)datetime.Day,
				fHour = (Byte)datetime.Hour,
				fMinute = (Byte)datetime.Minute,
				fSecond = (Byte)datetime.Second
			};
		}
	}

	/// <summary>Optional metadata to be passed into the PDF factory function.</summary>
	/// <remarks />
	public struct SKDocumentPdfMetadata : IEquatable<SKDocumentPdfMetadata>
	{
		/// <summary>Gets the default DPI (72.0 DPI).</summary>
		/// <remarks />
		public const float DefaultRasterDpi = SKDocument.DefaultRasterDpi;
		/// <summary>Gets the default encoding quality (101% or lossless).</summary>
		/// <remarks />
		public const int DefaultEncodingQuality = 101;

		/// <summary>Gets a new instance of <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> with the values set to the defaults.</summary>
		/// <remarks />
		public static readonly SKDocumentPdfMetadata Default;

		static SKDocumentPdfMetadata ()
		{
			Default = new SKDocumentPdfMetadata () {
				RasterDpi = DefaultRasterDpi,
				PdfA = false,
				EncodingQuality = 101,
			};
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> with the specified raster DPI.</summary>
		/// <param name="rasterDpi">The DPI (pixels-per-inch) at which features without native PDF support will be rasterized.</param>
		/// <remarks>PDF pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.</remarks>
		public SKDocumentPdfMetadata (float rasterDpi)
		{
			Title = null;
			Author = null;
			Subject = null;
			Keywords = null;
			Creator = null;
			Producer = null;
			Creation = null;
			Modified = null;
			RasterDpi = rasterDpi;
			PdfA = false;
			EncodingQuality = DefaultEncodingQuality;
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> with the specified encoding quality.</summary>
		/// <param name="encodingQuality">The encoding quality.</param>
		/// <remarks>The encoding quality is between 0 and 100. A quality of 101 indicates lossless encoding.</remarks>
		public SKDocumentPdfMetadata (int encodingQuality)
		{
			Title = null;
			Author = null;
			Subject = null;
			Keywords = null;
			Creator = null;
			Producer = null;
			Creation = null;
			Modified = null;
			RasterDpi = DefaultRasterDpi;
			PdfA = false;
			EncodingQuality = encodingQuality;
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> with the specified raster DPI and encoding quality.</summary>
		/// <param name="rasterDpi">The DPI (pixels-per-inch) at which features without native PDF support will be rasterized.</param>
		/// <param name="encodingQuality">The encoding quality.</param>
		/// <remarks>PDF pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm. The encoding quality is between 0 and 100. A quality of 101 indicates lossless encoding.</remarks>
		public SKDocumentPdfMetadata (float rasterDpi, int encodingQuality)
		{
			Title = null;
			Author = null;
			Subject = null;
			Keywords = null;
			Creator = null;
			Producer = null;
			Creation = null;
			Modified = null;
			RasterDpi = rasterDpi;
			PdfA = false;
			EncodingQuality = encodingQuality;
		}

		/// <summary>The document's title.</summary>
		/// <value>The title, or <see langword="null" /> if not set.</value>
		/// <remarks />
		public string Title { readonly get; set; }
		/// <summary>The name of the person who created the document.</summary>
		/// <value>The name of the author, or <see langword="null" /> if not set.</value>
		/// <remarks />
		public string Author { readonly get; set; }
		/// <summary>The subject of the document.</summary>
		/// <value>The subject, or <see langword="null" /> if not set.</value>
		/// <remarks />
		public string Subject { readonly get; set; }
		/// <summary>Comma-separated keywords associated with the document.</summary>
		/// <value>The comma-separated keywords, or <see langword="null" /> if not set.</value>
		/// <remarks />
		public string Keywords { readonly get; set; }
		/// <summary>The name of the product that created the original document, if the document was converted to PDF from another format.</summary>
		/// <value>The name of the creator product, or <see langword="null" /> if not set.</value>
		/// <remarks />
		public string Creator { readonly get; set; }
		/// <summary>The product that is converting this document to PDF.</summary>
		/// <value>The name of the producer product, or <see langword="null" /> if not set.</value>
		/// <remarks>Leave empty to get the default, correct value.</remarks>
		public string Producer { readonly get; set; }
		/// <summary>The date and time the document was created.</summary>
		/// <value>The creation date, or <see langword="null" /> if not set.</value>
		/// <remarks />
		public DateTime? Creation { readonly get; set; }
		/// <summary>The date and time the document was most recently modified.</summary>
		/// <value>The modification date, or <see langword="null" /> if not set.</value>
		/// <remarks />
		public DateTime? Modified { readonly get; set; }
		/// <summary>Gets or sets the DPI (pixels-per-inch) at which features without native PDF support will be rasterized.</summary>
		/// <value>The raster DPI value.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// PDF pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.
		///
		/// A larger DPI would create a PDF that reflects the original intent with better
		/// fidelity, but it can make for larger PDF files too, which would use more
		/// memory while rendering, and it would be slower to be processed or sent online
		/// or to printer.
		/// ]]></format></remarks>
		public float RasterDpi { readonly get; set; }
		/// <summary>Gets or sets a value indicating whether to make the document PDF/A-2b conformant.</summary>
		/// <value><see langword="true" /> if the document should be PDF/A-2b conformant; otherwise, <see langword="false" />.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If true, include XMP metadata, a document UUID, and sRGB output intent
		/// information. This adds length to the document and makes it non-reproducable,
		/// but are necessary features for PDF/A-2b conformance.
		/// ]]></format></remarks>
		public bool PdfA { readonly get; set; }
		/// <summary>Gets or sets the encoding quality.</summary>
		/// <value>The encoding quality, between 0 and 100, or 101 for lossless encoding.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Encoding quality controls the trade-off between size and quality. By default
		/// this is set to 101 percent, which corresponds to lossless encoding. If this
		/// value is set to a value <= 100, and the image is opaque, it will be encoded
		/// (using JPEG) with that quality setting.
		/// ]]></format></remarks>
		public int EncodingQuality { readonly get; set; }

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKDocumentPdfMetadata obj) =>
			Title == obj.Title &&
			Author == obj.Author &&
			Subject == obj.Subject &&
			Keywords == obj.Keywords &&
			Creator == obj.Creator &&
			Producer == obj.Producer &&
			Creation == obj.Creation &&
			Modified == obj.Modified &&
			RasterDpi == obj.RasterDpi &&
			PdfA == obj.PdfA &&
			EncodingQuality == obj.EncodingQuality;

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKDocumentPdfMetadata f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> objects have the same value.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> to compare.</param>
		/// <returns><see langword="true" /> if the value of <paramref name="left" /> is the same as the value of <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKDocumentPdfMetadata left, SKDocumentPdfMetadata right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> objects have different values.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> to compare.</param>
		/// <returns><see langword="true" /> if the value of <paramref name="left" /> is different from the value of <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKDocumentPdfMetadata left, SKDocumentPdfMetadata right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (Title);
			hash.Add (Author);
			hash.Add (Subject);
			hash.Add (Keywords);
			hash.Add (Creator);
			hash.Add (Producer);
			hash.Add (Creation);
			hash.Add (Modified);
			hash.Add (RasterDpi);
			hash.Add (PdfA);
			hash.Add (EncodingQuality);
			return hash.ToHashCode ();
		}
	}

	/// <summary>High contrast configuration settings for use with <see cref="M:SkiaSharp.SKColorFilter.CreateHighContrast(SkiaSharp.SKHighContrastConfig)" />.</summary>
	/// <remarks />
	public partial struct SKHighContrastConfig
	{
		/// <summary>Gets a new instance with the values set to the defaults.</summary>
		/// <remarks />
		public static readonly SKHighContrastConfig Default;

		static SKHighContrastConfig ()
		{
			Default = new SKHighContrastConfig (false, SKHighContrastConfigInvertStyle.NoInvert, 0.0f);
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKHighContrastConfig" />.</summary>
		/// <param name="grayscale"><see langword="true" /> to convert the color to grayscale; otherwise, <see langword="false" />.</param>
		/// <param name="invertStyle">The style of brightness or lightness inversion to apply, or none.</param>
		/// <param name="contrast">The amount to adjust the contrast by, in the range -1.0 through 1.0.</param>
		/// <remarks />
		public SKHighContrastConfig (bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
		{
			fGrayscale = grayscale ? (byte)1 : (byte)0;
			fInvertStyle = invertStyle;
			fContrast = contrast;
		}

		/// <summary>Gets a value indicating if the configuration is valid.</summary>
		/// <value><see langword="true" /> if the configuration is valid; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public readonly bool IsValid =>
			(int)fInvertStyle >= (int)SKHighContrastConfigInvertStyle.NoInvert &&
			(int)fInvertStyle <= (int)SKHighContrastConfigInvertStyle.InvertLightness &&
			fContrast >= -1.0 &&
			fContrast <= 1.0;
	}

	/// <summary>Options to control the PNG encoding.</summary>
	/// <remarks />
	public unsafe partial struct SKPngEncoderOptions
	{
		/// <summary>Gets a new instance of <see cref="T:SkiaSharp.SKPngEncoderOptions" /> with the values set to the defaults.</summary>
		/// <remarks />
		public static readonly SKPngEncoderOptions Default;

		static SKPngEncoderOptions ()
		{
			Default = new SKPngEncoderOptions (SKPngEncoderFilterFlags.AllFilters, 6);
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKPngEncoderOptions" />.</summary>
		/// <param name="filterFlags">The filtering flags.</param>
		/// <param name="zLibLevel">The compression level in the range 0..9.</param>
		/// <remarks />
		public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel)
		{
			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fComments = null;
		}

		/// <summary>Gets the filtering flags.</summary>
		/// <value>The filtering flags.</value>
		/// <remarks><para>If a single filter is chosen, then that filter will be used for every row.</para><para>If multiple filters are chosen, then a heuristic will be used to guess which filter will encode smallest, then apply that filter. This happens on a per row basis, different rows can use different filters.</para><para>Using a single filter (or less filters) is typically faster. Trying all of the filters may help minimize the output file size.</para></remarks>
		public SKPngEncoderFilterFlags FilterFlags => fFilterFlags;

		/// <summary>Gets the compression level in the range 0..9.</summary>
		/// <value>The compression level in the range 0..9.</value>
		/// <remarks>A value of 0 is a special case to skip compression entirely, creating dramatically larger PNGs.</remarks>
		public int ZLibLevel => fZLibLevel;
	}

	/// <summary>Options to control the JPEG encoding.</summary>
	/// <remarks />
	public unsafe partial struct SKJpegEncoderOptions
	{
		/// <summary>Gets a new instance of <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> with the values set to the defaults.</summary>
		/// <remarks />
		public static readonly SKJpegEncoderOptions Default;

		static SKJpegEncoderOptions ()
		{
			Default = new SKJpegEncoderOptions (100, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore);
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> structure with the specified quality.</summary>
		/// <param name="quality">The quality of the encoding in the range 0 to 100.</param>
		/// <remarks>Uses default values for downsampling (<see cref="F:SkiaSharp.SKJpegEncoderDownsample.Downsample420" />) and alpha option (<see cref="F:SkiaSharp.SKJpegEncoderAlphaOption.Ignore" />).</remarks>
		public SKJpegEncoderOptions (int quality)
		{
			xmpMetadata = default;
			fOrigin = default;
			fHasOrigin = default;

			fQuality = quality;
			fDownsample = SKJpegEncoderDownsample.Downsample420;
			fAlphaOption = SKJpegEncoderAlphaOption.Ignore;
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKJpegEncoderOptions" />.</summary>
		/// <param name="quality">The quality of the encoding in the range 0 to 100.</param>
		/// <param name="downsample">The downsampling factor for the U and V components.</param>
		/// <param name="alphaOption">The value to control how alpha is handled.</param>
		/// <remarks />
		public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption)
		{
			xmpMetadata = default;
			fOrigin = default;
			fHasOrigin = default;

			fQuality = quality;
			fDownsample = downsample;
			fAlphaOption = alphaOption;
		}

		/// <summary>Gets the value to control how alpha is handled.</summary>
		/// <value>One of the enumeration values that specifies how alpha is handled.</value>
		/// <remarks>JPEGs must be opaque, so this instructs the encoder on how to handle input images with alpha.</remarks>
		public SKJpegEncoderAlphaOption AlphaOption => fAlphaOption;

		/// <summary>Gets the downsampling factor for the U and V components.</summary>
		/// <value>One of the enumeration values that specifies the downsampling factor.</value>
		/// <remarks>This is only meaningful if the image is not gray, since gray will not be encoded as YUV.</remarks>
		public SKJpegEncoderDownsample Downsample => fDownsample;

		/// <summary>Gets the quality of the encoding in the range 0 to 100.</summary>
		/// <value>The encoding quality value from 0 (lowest) to 100 (highest).</value>
		/// <remarks />
		public int Quality => fQuality;
	}

	/// <summary>Options to control the WEBP encoding.</summary>
	/// <remarks />
	public unsafe partial struct SKWebpEncoderOptions
	{
		/// <summary>Gets a new instance of <see cref="T:SkiaSharp.SKWebpEncoderOptions" /> with the values set to the defaults.</summary>
		/// <remarks />
		public static readonly SKWebpEncoderOptions Default;

		static SKWebpEncoderOptions ()
		{
			Default = new SKWebpEncoderOptions (SKWebpEncoderCompression.Lossy, 100);
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKWebpEncoderOptions" />.</summary>
		/// <param name="compression">The compression level.</param>
		/// <param name="quality">The quality of the encoding in the range 0.0 to 100.0.</param>
		/// <remarks />
		public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality)
		{
			fCompression = compression;
			fQuality = quality;
		}

		/// <summary>Gets the compression level.</summary>
		/// <value>The compression level.</value>
		/// <remarks />
		public SKWebpEncoderCompression Compression => fCompression;

		/// <summary>Gets the quality of the encoding.</summary>
		/// <value>The quality of the encoding in the range 0.0 to 100.0.</value>
		/// <remarks><param>If the compression is lossy, then the quality corresponds to the visual quality. Lower values are smaller, but will have reduced quality.</param><param>If the compression is lossless, then the quality corresponds effort put into compressing the file. Lower values are faster, but higher values have smaller files.</param></remarks>
		public float Quality => fQuality;
	}

	/// <summary>Represents a single frame in an animated WebP sequence.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `SKWebpEncoderFrame` combines pixel data (as an <xref:SkiaSharp.SKPixmap>) and a display duration for use with <xref:SkiaSharp.SKWebpEncoder.EncodeAnimated*>.
	///
	/// When constructed from an <xref:SkiaSharp.SKBitmap> or <xref:SkiaSharp.SKImage>, the constructor calls `PeekPixels()` to obtain a non-owning view of the pixel data. If pixel data is not available, an exception is thrown.
	///
	/// ## Examples
	///
	/// Creating a two-frame animated WebP:
	///
	/// ```csharp
	/// using var frame1Bitmap = new SKBitmap(100, 100);
	/// using var frame2Bitmap = new SKBitmap(100, 100);
	/// // ... draw into bitmaps ...
	///
	/// var frames = new[]
	/// {
	///     new SKWebpEncoderFrame(frame1Bitmap, TimeSpan.FromMilliseconds(100)),
	///     new SKWebpEncoderFrame(frame2Bitmap, TimeSpan.FromMilliseconds(100)),
	/// };
	/// using var data = SKWebpEncoder.EncodeAnimated(frames, SKWebpEncoderOptions.Default);
	/// ```
	/// ]]></remarks>
	public struct SKWebpEncoderFrame
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKWebpEncoderFrame" /> struct from a pixmap and a display duration.</summary>
		/// <param name="pixmap">The pixmap containing the pixel data for this frame.</param>
		/// <param name="duration">The display duration of this frame.</param>
		/// <remarks />
		public SKWebpEncoderFrame (SKPixmap pixmap, TimeSpan duration)
		{
			Pixmap = pixmap ?? throw new ArgumentNullException (nameof (pixmap));
			Duration = duration;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKWebpEncoderFrame" /> struct from a bitmap and a display duration.</summary>
		/// <param name="bitmap">The bitmap whose pixel data will be used for this frame.</param>
		/// <param name="duration">The display duration of this frame.</param>
		/// <remarks />
		public SKWebpEncoderFrame (SKBitmap bitmap, TimeSpan duration)
		{
			_ = bitmap ?? throw new ArgumentNullException (nameof (bitmap));
			Pixmap = bitmap.PeekPixels () ?? throw new ArgumentException ("Unable to peek pixels from bitmap.", nameof (bitmap));
			Duration = duration;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKWebpEncoderFrame" /> struct from an image and a display duration.</summary>
		/// <param name="image">The image whose pixel data will be used for this frame.</param>
		/// <param name="duration">The display duration of this frame.</param>
		/// <remarks />
		public SKWebpEncoderFrame (SKImage image, TimeSpan duration)
		{
			_ = image ?? throw new ArgumentNullException (nameof (image));
			Pixmap = image.PeekPixels () ?? throw new ArgumentException ("Unable to peek pixels from image. Ensure the image is raster-backed.", nameof (image));
			Duration = duration;
		}

		/// <summary>Gets or sets the pixel data for this frame.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKPixmap" /> containing the pixel data for this frame.</value>
		/// <remarks />
		public SKPixmap Pixmap { readonly get; set; }

		/// <summary>Gets or sets the display duration of this frame.</summary>
		/// <value>The length of time this frame is displayed during animation playback.</value>
		/// <remarks />
		public TimeSpan Duration { readonly get; set; }
	}

	/// <summary>Represents a cubic resampler with configurable B and C parameters for high-quality image scaling.</summary>
	/// <remarks />
	public partial struct SKCubicResampler
	{
		/// <summary>Gets a Mitchell-Netravali cubic resampler with B=1/3 and C=1/3.</summary>
		/// <remarks />
		public static readonly SKCubicResampler Mitchell = new (1 / 3.0f, 1 / 3.0f);

		/// <summary>Gets a Catmull-Rom cubic resampler with B=0 and C=0.5.</summary>
		/// <remarks />
		public static readonly SKCubicResampler CatmullRom = new (0.0f, 1 / 2.0f);

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKCubicResampler" /> struct with the specified B and C parameters.</summary>
		/// <param name="b">The B parameter of the cubic resampler.</param>
		/// <param name="c">The C parameter of the cubic resampler.</param>
		/// <remarks />
		public SKCubicResampler (float b, float c)
		{
			fB = b;
			fC = c;
		}
	}

	/// <summary>Specifies the sampling options used when drawing images, including filter mode, mipmap mode, and cubic resampling.</summary>
	/// <remarks />
	public partial struct SKSamplingOptions
	{
		/// <summary>Gets the default sampling options.</summary>
		/// <remarks />
		public static readonly SKSamplingOptions Default = new ();

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKSamplingOptions" /> struct with the specified filter and mipmap modes.</summary>
		/// <param name="filter">The filter mode to use for image sampling.</param>
		/// <param name="mipmap">The mipmap mode to use for image sampling.</param>
		/// <remarks />
		public SKSamplingOptions (SKFilterMode filter, SKMipmapMode mipmap)
		{
			fUseCubic = default;
			fCubic = default;
			fMaxAniso = default;

			fFilter = filter;
			fMipmap = mipmap;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKSamplingOptions" /> struct with the specified filter mode and no mipmap filtering.</summary>
		/// <param name="filter">The filter mode to use for image sampling.</param>
		/// <remarks />
		public SKSamplingOptions (SKFilterMode filter)
		{
			fUseCubic = default;
			fCubic = default;
			fMaxAniso = default;

			fFilter = filter;
			fMipmap = SKMipmapMode.None;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKSamplingOptions" /> struct with the specified cubic resampler.</summary>
		/// <param name="resampler">The cubic resampler to use for image sampling.</param>
		/// <remarks />
		public SKSamplingOptions (SKCubicResampler resampler)
		{
			fMaxAniso = default;
			fFilter = default;
			fMipmap = default;

			fUseCubic = 1;
			fCubic = resampler;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKSamplingOptions" /> struct with the specified anisotropic filtering level.</summary>
		/// <param name="maxAniso">The maximum anisotropic filtering level. Values less than 1 are clamped to 1.</param>
		/// <remarks />
		public SKSamplingOptions (int maxAniso)
		{
			fUseCubic = default;
			fCubic = default;
			fFilter = default;
			fMipmap = default;

			fMaxAniso = Math.Max (1, maxAniso);
		}

		/// <summary>Gets a value indicating whether anisotropic filtering is enabled.</summary>
		/// <value><see langword="true" /> if anisotropic filtering is enabled; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsAniso => MaxAniso != 0;
	}
}
