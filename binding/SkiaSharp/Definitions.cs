#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Various predefined font weights for use with <see cref="T:SkiaSharp.SKTypeface" />.
	/// </summary>
	/// <remarks>Font weights can range from anywhere between 100 to 1000 (inclusive).</remarks>
	public enum SKFontStyleWeight
	{
		/// <summary>
		/// The font has no thickness at all.
		/// </summary>
		Invisible = 0,
		/// <summary>
		/// A thin font weight of 100.
		/// </summary>
		Thin = 100,
		/// <summary>
		/// A thin font weight of 200.
		/// </summary>
		ExtraLight = 200,
		/// <summary>
		/// A thin font weight of 300.
		/// </summary>
		Light = 300,
		/// <summary>
		/// A typical font weight of 400. This is the default font weight.
		/// </summary>
		Normal = 400,
		/// <summary>
		/// A thicker font weight of 500.
		/// </summary>
		Medium = 500,
		/// <summary>
		/// A thick font weight of 600.
		/// </summary>
		SemiBold = 600,
		/// <summary>
		/// A thick font weight of 700. This is the default for a bold font.
		/// </summary>
		Bold = 700,
		/// <summary>
		/// A thick font weight of 800.
		/// </summary>
		ExtraBold = 800,
		/// <summary>
		/// A thick font weight of 900.
		/// </summary>
		Black = 900,
		/// <summary>
		/// A thick font weight of 1000.
		/// </summary>
		ExtraBlack = 1000,
	}

	/// <summary>
	/// Various predefined font widths for use with <see cref="T:SkiaSharp.SKTypeface" />.
	/// </summary>
	public enum SKFontStyleWidth
	{
		/// <summary>
		/// A condensed font width of 1.
		/// </summary>
		UltraCondensed = 1,
		/// <summary>
		/// A condensed font width of 2.
		/// </summary>
		ExtraCondensed = 2,
		/// <summary>
		/// A condensed font width of 3.
		/// </summary>
		Condensed = 3,
		/// <summary>
		/// A condensed font width of 4.
		/// </summary>
		SemiCondensed = 4,
		/// <summary>
		/// A normal font width of 5. This is the default font width.
		/// </summary>
		Normal = 5,
		/// <summary>
		/// An expanded font width of 6.
		/// </summary>
		SemiExpanded = 6,
		/// <summary>
		/// An expanded font width of 7.
		/// </summary>
		Expanded = 7,
		/// <summary>
		/// An expanded font width of 8.
		/// </summary>
		ExtraExpanded = 8,
		/// <summary>
		/// An expanded font width of 9.
		/// </summary>
		UltraExpanded = 9,
	}

	/// <summary>
	/// Describes how to interpret the components of a pixel.
	/// </summary>
	public enum SKColorType
	{
		/// <summary>
		/// Unknown encoding.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Represents a 8-bit alpha-only color.
		/// </summary>
		Alpha8 = 1,
		/// <summary>
		/// Represents an opaque 16-bit color with the format RGB, with the red and blue components being 5 bits and the green component being 6 bits.
		/// </summary>
		Rgb565 = 2,
		/// <summary>
		/// Represents a 16-bit color with the format ARGB.
		/// </summary>
		Argb4444 = 3,
		/// <summary>
		/// Represents a 32-bit color with the format RGBA.
		/// </summary>
		Rgba8888 = 4,
		/// <summary>
		/// Represents an opaque 32-bit color with the format RGB, with 8 bits per color component.
		/// </summary>
		Rgb888x = 5,
		/// <summary>
		/// Represents a 32-bit color with the format BGRA.
		/// </summary>
		Bgra8888 = 6,
		/// <summary>
		/// Represents a 32-bit color with the format RGBA, with 10 bits per color component and 2 bits for the alpha component.
		/// </summary>
		Rgba1010102 = 7,
		/// <summary>
		/// Represents an opaque 32-bit color with the format RGB, with 10 bits per color component.
		/// </summary>
		Rgb101010x = 8,
		/// <summary>
		/// Represents an opaque 8-bit grayscale color.
		/// </summary>
		Gray8 = 9,
		/// <summary>
		/// Represents a floating-point based color with the format RGBA.
		/// </summary>
		RgbaF16 = 10,
		RgbaF16Clamped = 11,
		RgbaF32 = 12,
		Rg88 = 13,
		AlphaF16 = 14,
		RgF16 = 15,
		Alpha16 = 16,
		Rg1616 = 17,
		Rgba16161616 = 18,
		Bgra1010102 = 19,
		Bgr101010x = 20,
		Bgr101010xXR = 21,
		Srgba8888 = 22,
		R8Unorm = 23,
		Rgba10x6 = 24,
	}

	/// <summary>
	/// Convenience methods for <see cref="T:SkiaSharp.SKPixelGeometry" />.
	/// </summary>
	public static partial class SkiaExtensions
	{
		/// <summary>
		/// Returns true if the pixel geometry is BGR.
		/// </summary>
		/// <param name="pg">The pixel geometry to test.</param>
		public static bool IsBgr (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.BgrVertical;

		/// <summary>
		/// Returns true if the pixel geometry is RGB.
		/// </summary>
		/// <param name="pg">The pixel geometry to test.</param>
		public static bool IsRgb (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.RgbHorizontal || pg == SKPixelGeometry.RgbVertical;

		/// <summary>
		/// Returns true if the pixel geometry is vertical.
		/// </summary>
		/// <param name="pg">The pixel geometry to test.</param>
		public static bool IsVertical (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrVertical || pg == SKPixelGeometry.RgbVertical;

		/// <summary>
		/// Returns true if the pixel geometry is horizontal.
		/// </summary>
		/// <param name="pg">The pixel geometry to test.</param>
		public static bool IsHorizontal (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.RgbHorizontal;

		// SkImageInfo.cpp - SkColorTypeBytesPerPixel
		/// <param name="colorType"></param>
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
				// 16
				SKColorType.RgbaF32 => 16,
				//
				_ => throw new ArgumentOutOfRangeException (nameof (colorType), $"Unknown color type: '{colorType}'"),
			};

		// SkImageInfoPriv.h - SkColorTypeShiftPerPixel
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
				// 4
				SKColorType.RgbaF32 => 4,
				//
				_ => throw new ArgumentOutOfRangeException (nameof (colorType), $"Unknown color type: '{colorType}'"),
			};

		// SkImageInfo.cpp - SkColorTypeValidateAlphaType
		/// <param name="colorType"></param>
		/// <param name="alphaType"></param>
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
					alphaType = SKAlphaType.Opaque;
					break;

				default:
					throw new ArgumentOutOfRangeException (nameof (colorType), $"Unknown color type: '{colorType}'");
			}

			return alphaType;
		}
	}

	/// <summary>
	/// Additional options to pass to <see cref="M:SkiaSharp.SKCodec.GetPixels(SkiaSharp.SKImageInfo,System.IntPtr,SkiaSharp.SKCodecOptions)" /> or one of the overloads that accepts a <see cref="T:SkiaSharp.SKCodecOptions" />.
	/// </summary>
	public struct SKCodecOptions : IEquatable<SKCodecOptions>
	{
		/// <summary>
		/// Gets the default options.
		/// </summary>
		/// <remarks>The default value is not zero-initialized and without a subset rectangle.</remarks>
		public static readonly SKCodecOptions Default;

		static SKCodecOptions ()
		{
			Default = new SKCodecOptions (SKZeroInitialized.No);
		}

		/// <summary>
		/// Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" /> with the specified zero-initialization.
		/// </summary>
		/// <param name="zeroInitialized">The zero-initialization.</param>
		public SKCodecOptions (SKZeroInitialized zeroInitialized)
		{
			ZeroInitialized = zeroInitialized;
			Subset = null;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		/// <summary>
		/// Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" /> with the specified subset rectangle and zero-initialization.
		/// </summary>
		/// <param name="zeroInitialized">The zero-initialization.</param>
		/// <param name="subset">The subset rectangle.</param>
		public SKCodecOptions (SKZeroInitialized zeroInitialized, SKRectI subset)
		{
			ZeroInitialized = zeroInitialized;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		/// <summary>
		/// Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" /> with the specified subset rectangle.
		/// </summary>
		/// <param name="subset">The subset rectangle.</param>
		public SKCodecOptions (SKRectI subset)
		{
			ZeroInitialized = SKZeroInitialized.No;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		/// <summary>
		/// Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" /> with the specified frame index.
		/// </summary>
		/// <param name="frameIndex">The frame to decode.</param>
		/// <remarks>Only meaningful for multi-frame images.</remarks>
		public SKCodecOptions (int frameIndex)
		{
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = -1;
		}
		/// <summary>
		/// Create a new instance of <see cref="T:SkiaSharp.SKCodecOptions" />.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the zero-initialization.
		/// </summary>
		public SKZeroInitialized ZeroInitialized { readonly get; set; }
		/// <summary>
		/// Gets or sets the subset rectangle.
		/// </summary>
		public SKRectI? Subset { readonly get; set; }
		/// <summary>
		/// Gets a value indicating whether the options has a subset rectangle.
		/// </summary>
		public readonly bool HasSubset => Subset != null;
		/// <summary>
		/// Gets or sets the frame to decode.
		/// </summary>
		/// <remarks>Only meaningful for multi-frame images.</remarks>
		public int FrameIndex { readonly get; set; }
		/// <summary>
		/// Gets or sets a value indicating which frame, if any, the destination bitmap already contains.
		/// </summary>
		/// <remarks>Only meaningful for multi-frame images.
		/// If <see cref="SkiaSharp.SKCodecOptions.FrameIndex" /> needs to be blended with a prior
		/// frame (as reported by `SKCodec.FrameInfo[FrameIndex].RequiredFrame`), the
		/// client can set this to any non-<see cref="SkiaSharp.SKCodecAnimationDisposalMethod.RestorePrevious" />
		/// frame in the range [RequiredFrame, FrameIndex) to indicate that that frame is
		/// already in the destination. <see cref="SkiaSharp.SKCodecOptions.ZeroInitialized" /> is
		/// ignored in this case.
		/// If set to -1, the codec will decode any necessary required frame(s) first.</remarks>
		public int PriorFrame { readonly get; set; }

		/// <param name="obj"></param>
		public readonly bool Equals (SKCodecOptions obj) =>
			ZeroInitialized == obj.ZeroInitialized &&
			Subset == obj.Subset &&
			FrameIndex == obj.FrameIndex &&
			PriorFrame == obj.PriorFrame;

		/// <param name="obj"></param>
		public readonly override bool Equals (object obj) =>
			obj is SKCodecOptions f && Equals (f);

		/// <param name="left"></param>
		/// <param name="right"></param>
		public static bool operator == (SKCodecOptions left, SKCodecOptions right) =>
			left.Equals (right);

		/// <param name="left"></param>
		/// <param name="right"></param>
		public static bool operator != (SKCodecOptions left, SKCodecOptions right) =>
			!left.Equals (right);

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

	/// <summary>
	/// Structure to represent measurements for a font.
	/// </summary>
	public partial struct SKFontMetrics
	{
		private const uint flagsUnderlineThicknessIsValid = (1U << 0);
		private const uint flagsUnderlinePositionIsValid = (1U << 1);
		private const uint flagsStrikeoutThicknessIsValid = (1U << 2);
		private const uint flagsStrikeoutPositionIsValid = (1U << 3);

		/// <summary>
		/// Gets the greatest distance above the baseline for any glyph.
		/// </summary>
		/// <remarks>Will be &lt;= 0.</remarks>
		public readonly float Top => fTop;

		/// <summary>
		/// Gets the recommended distance above the baseline.
		/// </summary>
		/// <remarks>Will be &lt;= 0.</remarks>
		public readonly float Ascent => fAscent;

		/// <summary>
		/// Gets the recommended distance below the baseline.
		/// </summary>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float Descent => fDescent;

		/// <summary>
		/// Gets the greatest distance below the baseline for any glyph.
		/// </summary>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float Bottom => fBottom;

		/// <summary>
		/// Gets the recommended distance to add between lines of text.
		/// </summary>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float Leading => fLeading;

		/// <summary>
		/// Gets the average character width.
		/// </summary>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float AverageCharacterWidth => fAvgCharWidth;

		/// <summary>
		/// Gets the max character width.
		/// </summary>
		/// <remarks>Will be &gt;= 0.</remarks>
		public readonly float MaxCharacterWidth => fMaxCharWidth;

		/// <summary>
		/// Gets the minimum bounding box x value for all glyphs.
		/// </summary>
		public readonly float XMin => fXMin;

		/// <summary>
		/// Gets the maximum bounding box x value for all glyphs.
		/// </summary>
		public readonly float XMax => fXMax;

		/// <summary>
		/// Gets the height of an 'x' in px.
		/// </summary>
		/// <remarks>0 if no 'x' in face.</remarks>
		public readonly float XHeight => fXHeight;

		/// <summary>
		/// Gets the cap height.
		/// </summary>
		/// <remarks>Will be &gt; 0, or 0 if cannot be determined.</remarks>
		public readonly float CapHeight => fCapHeight;

		/// <summary>
		/// Gets the thickness of the underline.
		/// </summary>
		/// <remarks><para>0 - if the thickness can not be determined</para><para>null - if the thickness is not set.</para></remarks>
		public readonly float? UnderlineThickness => GetIfValid (fUnderlineThickness, flagsUnderlineThicknessIsValid);
		/// <summary>
		/// Gets the position of the top of the underline stroke relative to the baseline.
		/// </summary>
		/// <remarks><para>Negative - underline should be drawn above baseline.</para><para>Positive - underline should be drawn below baseline.</para><para>Zero - underline should be drawn on baseline.underline position, or 0 if cannot be determined.</para><para>null - does not have an UnderlinePosition.</para></remarks>
		public readonly float? UnderlinePosition => GetIfValid (fUnderlinePosition, flagsUnderlinePositionIsValid);
		/// <summary>
		/// Gets the thickness of the strikeout.
		/// </summary>
		public readonly float? StrikeoutThickness => GetIfValid (fStrikeoutThickness, flagsStrikeoutThicknessIsValid);
		/// <summary>
		/// Gets the position of the bottom of the strikeout stroke relative to the baseline.
		/// </summary>
		/// <remarks>This value is typically negative when valid.</remarks>
		public readonly float? StrikeoutPosition => GetIfValid (fStrikeoutPosition, flagsStrikeoutPositionIsValid);

		private readonly float? GetIfValid (float value, uint flag) =>
			(fFlags & flag) == flag ? value : (float?)null;
	}

	/// <summary>
	/// Specifies coordinates to divide a bitmap into (<see cref="P:SkiaSharp.SKLattice.XDivs" /> * <see cref="P:SkiaSharp.SKLattice.YDivs" />) rectangles.
	/// </summary>
	/// <remarks>If the lattice divs or bounds are invalid, the entire lattice structure will be ignored on the draw call.</remarks>
	public struct SKLattice : IEquatable<SKLattice>
	{
		/// <summary>
		/// Gets or sets the x-coordinates for the lattice.
		/// </summary>
		public int[] XDivs { readonly get; set; }
		/// <summary>
		/// Gets or sets the y-coordinates for the lattice.
		/// </summary>
		public int[] YDivs { readonly get; set; }
		/// <summary>
		/// Gets or sets the color for each of the lattice rectangles.
		/// </summary>
		public SKLatticeRectType[] RectTypes { readonly get; set; }
		/// <summary>
		/// Gets or sets the optional source image bounds.
		/// </summary>
		public SKRectI? Bounds { readonly get; set; }
		/// <summary>
		/// Gets or sets the array of fill types, one per rectangular grid entry.
		/// </summary>
		public SKColor[] Colors { readonly get; set; }

		/// <param name="obj"></param>
		public readonly bool Equals (SKLattice obj) =>
			XDivs == obj.XDivs &&
			YDivs == obj.YDivs &&
			RectTypes == obj.RectTypes &&
			Bounds == obj.Bounds &&
			Colors == obj.Colors;

		/// <param name="obj"></param>
		public readonly override bool Equals (object obj) =>
			obj is SKLattice f && Equals (f);

		/// <param name="left"></param>
		/// <param name="right"></param>
		public static bool operator == (SKLattice left, SKLattice right) =>
			left.Equals (right);

		/// <param name="left"></param>
		/// <param name="right"></param>
		public static bool operator != (SKLattice left, SKLattice right) =>
			!left.Equals (right);

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

	/// <summary>
	/// Optional metadata to be passed into the PDF factory function.
	/// </summary>
	public struct SKDocumentPdfMetadata : IEquatable<SKDocumentPdfMetadata>
	{
		/// <summary>
		/// Gets the default DPI (72.0 DPI).
		/// </summary>
		public const float DefaultRasterDpi = SKDocument.DefaultRasterDpi;
		/// <summary>
		/// Gets the default encoding quality (101% or lossless).
		/// </summary>
		public const int DefaultEncodingQuality = 101;

		/// <summary>
		/// Gets a new instance of <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> with the values set to the defaults.
		/// </summary>
		public static readonly SKDocumentPdfMetadata Default;

		static SKDocumentPdfMetadata ()
		{
			Default = new SKDocumentPdfMetadata () {
				RasterDpi = DefaultRasterDpi,
				PdfA = false,
				EncodingQuality = 101,
			};
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> with the specified raster DPI.
		/// </summary>
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

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> with the specified encoding quality.
		/// </summary>
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

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKDocumentPdfMetadata" /> with the specified raster DPI and encoding quality.
		/// </summary>
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

		/// <summary>
		/// The document's title.
		/// </summary>
		public string Title { readonly get; set; }
		/// <summary>
		/// The name of the person who created the document.
		/// </summary>
		public string Author { readonly get; set; }
		/// <summary>
		/// The subject of the document.
		/// </summary>
		public string Subject { readonly get; set; }
		/// <summary>
		/// Comma-separated keywords associated with the document.
		/// </summary>
		public string Keywords { readonly get; set; }
		/// <summary>
		/// The name of the product that created the original document, if the document was converted to PDF from another format.
		/// </summary>
		public string Creator { readonly get; set; }
		/// <summary>
		/// The product that is converting this document to PDF.
		/// </summary>
		/// <remarks>Leave empty to get the default, correct value.</remarks>
		public string Producer { readonly get; set; }
		/// <summary>
		/// The date and time the document was created.
		/// </summary>
		public DateTime? Creation { readonly get; set; }
		/// <summary>
		/// The date and time the document was most recently modified.
		/// </summary>
		public DateTime? Modified { readonly get; set; }
		/// <summary>
		/// Gets or sets the DPI (pixels-per-inch) at which features without native PDF support will be rasterized.
		/// </summary>
		/// <remarks>PDF pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.
		/// A larger DPI would create a PDF that reflects the original intent with better
		/// fidelity, but it can make for larger PDF files too, which would use more
		/// memory while rendering, and it would be slower to be processed or sent online
		/// or to printer.</remarks>
		public float RasterDpi { readonly get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether or not make the document PDF/A-2b conformant.
		/// </summary>
		/// <remarks>If true, include XMP metadata, a document UUID, and sRGB output intent
		/// information. This adds length to the document and makes it non-reproducable,
		/// but are necessary features for PDF/A-2b conformance.</remarks>
		public bool PdfA { readonly get; set; }
		/// <summary>
		/// Gets or sets the encoding quality.
		/// </summary>
		/// <remarks>Encoding quality controls the trade-off between size and quality. By default
		/// this is set to 101 percent, which corresponds to lossless encoding. If this
		/// value is set to a value <= 100, and the image is opaque, it will be encoded
		/// (using JPEG) with that quality setting.</remarks>
		public int EncodingQuality { readonly get; set; }

		/// <param name="obj"></param>
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

		/// <param name="obj"></param>
		public readonly override bool Equals (object obj) =>
			obj is SKDocumentPdfMetadata f && Equals (f);

		/// <param name="left"></param>
		/// <param name="right"></param>
		public static bool operator == (SKDocumentPdfMetadata left, SKDocumentPdfMetadata right) =>
			left.Equals (right);

		/// <param name="left"></param>
		/// <param name="right"></param>
		public static bool operator != (SKDocumentPdfMetadata left, SKDocumentPdfMetadata right) =>
			!left.Equals (right);

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

	/// <summary>
	/// High contrast configuration settings for use with <see cref="M:SkiaSharp.SKColorFilter.CreateHighContrast(SkiaSharp.SKHighContrastConfig)" />.
	/// </summary>
	public partial struct SKHighContrastConfig
	{
		/// <summary>
		/// Gets a new instance with the values set to the defaults.
		/// </summary>
		public static readonly SKHighContrastConfig Default;

		static SKHighContrastConfig ()
		{
			Default = new SKHighContrastConfig (false, SKHighContrastConfigInvertStyle.NoInvert, 0.0f);
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKHighContrastConfig" />.
		/// </summary>
		/// <param name="grayscale">Whether or not the color will be converted to grayscale.</param>
		/// <param name="invertStyle">Whether or not to invert brightness, lightness, or neither.</param>
		/// <param name="contrast">The amount to adjust the contrast by, in the range -1.0 through 1.0.</param>
		public SKHighContrastConfig (bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
		{
			fGrayscale = grayscale ? (byte)1 : (byte)0;
			fInvertStyle = invertStyle;
			fContrast = contrast;
		}

		/// <summary>
		/// Gets a value indicating if the configuration is valid.
		/// </summary>
		public readonly bool IsValid =>
			(int)fInvertStyle >= (int)SKHighContrastConfigInvertStyle.NoInvert &&
			(int)fInvertStyle <= (int)SKHighContrastConfigInvertStyle.InvertLightness &&
			fContrast >= -1.0 &&
			fContrast <= 1.0;
	}

	/// <summary>
	/// Options to control the PNG encoding.
	/// </summary>
	public unsafe partial struct SKPngEncoderOptions
	{
		/// <summary>
		/// Gets a new instance of <see cref="T:SkiaSharp.SKPngEncoderOptions" /> with the values set to the defaults.
		/// </summary>
		public static readonly SKPngEncoderOptions Default;

		static SKPngEncoderOptions ()
		{
			Default = new SKPngEncoderOptions (SKPngEncoderFilterFlags.AllFilters, 6);
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKPngEncoderOptions" />.
		/// </summary>
		/// <param name="filterFlags">The filtering flags.</param>
		/// <param name="zLibLevel">The compression level in the range 0..9.</param>
		public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel)
		{
			fICCProfile = default;
			fICCProfileDescription = default;

			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fComments = null;
		}

		/// <summary>
		/// Gets or sets the filtering flags.
		/// </summary>
		/// <remarks><para>If a single filter is chosen, then that filter will be used for every row.</para><para>If multiple filters are chosen, then a heuristic will be used to guess which filter will encode smallest, then apply that filter. This happens on a per row basis, different rows can use different filters.</para><para>Using a single filter (or less filters) is typically faster. Trying all of the filters may help minimize the output file size.</para></remarks>
		public SKPngEncoderFilterFlags FilterFlags => fFilterFlags;

		/// <summary>
		/// Gets or sets the compression level in the range 0..9.
		/// </summary>
		/// <remarks>A value of 0 is a special case to skip compression entirely, creating dramatically larger PNGs.</remarks>
		public int ZLibLevel => fZLibLevel;
	}

	/// <summary>
	/// Options to control the JPEG encoding.
	/// </summary>
	public unsafe partial struct SKJpegEncoderOptions
	{
		/// <summary>
		/// Gets a new instance of <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> with the values set to the defaults.
		/// </summary>
		public static readonly SKJpegEncoderOptions Default;

		static SKJpegEncoderOptions ()
		{
			Default = new SKJpegEncoderOptions (100, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore);
		}

		public SKJpegEncoderOptions (int quality)
		{
			fICCProfile = default;
			fICCProfileDescription = default;
			xmpMetadata = default;

			fQuality = quality;
			fDownsample = SKJpegEncoderDownsample.Downsample420;
			fAlphaOption = SKJpegEncoderAlphaOption.Ignore;
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKJpegEncoderOptions" />.
		/// </summary>
		/// <param name="quality">The quality of the encoding in the range 0 to 100.</param>
		/// <param name="downsample">The downsampling factor for the U and V components.</param>
		/// <param name="alphaOption">The value to control how alpha is handled.</param>
		public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption)
		{
			fICCProfile = default;
			fICCProfileDescription = default;
			xmpMetadata = default;

			fQuality = quality;
			fDownsample = downsample;
			fAlphaOption = alphaOption;
		}

		/// <summary>
		/// Gets or sets the value to control how alpha is handled.
		/// </summary>
		/// <remarks>JPEGs must be opaque, so this instructs the encoder on how to handle input images with alpha.</remarks>
		public SKJpegEncoderAlphaOption AlphaOption => fAlphaOption;

		/// <summary>
		/// Gets or sets the downsampling factor for the U and V components.
		/// </summary>
		/// <remarks>This is only meaningful if the image is not gray, since gray will not be encoded as YUV.</remarks>
		public SKJpegEncoderDownsample Downsample => fDownsample;

		/// <summary>
		/// Gets or sets the quality of the encoding in the range 0 to 100.
		/// </summary>
		public int Quality => fQuality;
	}

	/// <summary>
	/// Options to control the WEBP encoding.
	/// </summary>
	public unsafe partial struct SKWebpEncoderOptions
	{
		/// <summary>
		/// Gets a new instance of <see cref="T:SkiaSharp.SKWebpEncoderOptions" /> with the values set to the defaults.
		/// </summary>
		public static readonly SKWebpEncoderOptions Default;

		static SKWebpEncoderOptions ()
		{
			Default = new SKWebpEncoderOptions (SKWebpEncoderCompression.Lossy, 100);
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKWebpEncoderOptions" />.
		/// </summary>
		/// <param name="compression">The compression level.</param>
		/// <param name="quality">The quality of the encoding in the range 0.0 to 100.0.</param>
		public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality)
		{
			fICCProfile = default;
			fICCProfileDescription = default;

			fCompression = compression;
			fQuality = quality;
		}

		/// <summary>
		/// Gets or sets the compression level.
		/// </summary>
		public SKWebpEncoderCompression Compression => fCompression;

		/// <summary>
		/// Gets or sets the quality of the encoding.
		/// </summary>
		/// <remarks><param>If the compression is lossy, then the quality corresponds to the visual quality. Lower values are smaller, but will have reduced quality.</param><param>If the compression is lossless, then the quality corresponds effort put into compressing the file. Lower values are faster, but higher values have smaller files.</param></remarks>
		public float Quality => fQuality;
	}

	public partial struct SKCubicResampler
	{
		public static readonly SKCubicResampler Mitchell = new (1 / 3.0f, 1 / 3.0f);

		public static readonly SKCubicResampler CatmullRom = new (0.0f, 1 / 2.0f);

		public SKCubicResampler (float b, float c)
		{
			fB = b;
			fC = c;
		}
	}

	public partial struct SKSamplingOptions
	{
		public static readonly SKSamplingOptions Default = new ();

		public SKSamplingOptions (SKFilterMode filter, SKMipmapMode mipmap)
		{
			fUseCubic = default;
			fCubic = default;
			fMaxAniso = default;

			fFilter = filter;
			fMipmap = mipmap;
		}

		public SKSamplingOptions (SKFilterMode filter)
		{
			fUseCubic = default;
			fCubic = default;
			fMaxAniso = default;

			fFilter = filter;
			fMipmap = SKMipmapMode.None;
		}

		public SKSamplingOptions (SKCubicResampler resampler)
		{
			fMaxAniso = default;
			fFilter = default;
			fMipmap = default;

			fUseCubic = 1;
			fCubic = resampler;
		}

		public SKSamplingOptions (int maxAniso)
		{
			fUseCubic = default;
			fCubic = default;
			fFilter = default;
			fMipmap = default;

			fMaxAniso = Math.Max (1, maxAniso);
		}

		public bool IsAniso => MaxAniso != 0;
	}
}
