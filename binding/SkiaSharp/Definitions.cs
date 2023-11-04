#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public enum SKFontStyleWeight
	{
		Invisible = 0,
		Thin = 100,
		ExtraLight = 200,
		Light = 300,
		Normal = 400,
		Medium = 500,
		SemiBold = 600,
		Bold = 700,
		ExtraBold = 800,
		Black = 900,
		ExtraBlack = 1000,
	}

	public enum SKFontStyleWidth
	{
		UltraCondensed = 1,
		ExtraCondensed = 2,
		Condensed = 3,
		SemiCondensed = 4,
		Normal = 5,
		SemiExpanded = 6,
		Expanded = 7,
		ExtraExpanded = 8,
		UltraExpanded = 9,
	}

	public enum SKColorType
	{
		Unknown = 0,
		Alpha8 = 1,
		Rgb565 = 2,
		Argb4444 = 3,
		Rgba8888 = 4,
		Rgb888x = 5,
		Bgra8888 = 6,
		Rgba1010102 = 7,
		Rgb101010x = 8,
		Gray8 = 9,
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
	}

	public static partial class SkiaExtensions
	{
		public static bool IsBgr (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.BgrVertical;

		public static bool IsRgb (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.RgbHorizontal || pg == SKPixelGeometry.RgbVertical;

		public static bool IsVertical (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrVertical || pg == SKPixelGeometry.RgbVertical;

		public static bool IsHorizontal (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.RgbHorizontal;

		// SkImageInfo.cpp - SkColorTypeBytesPerPixel
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
				// 4
				SKColorType.RgbaF32 => 4,
				//
				_ => throw new ArgumentOutOfRangeException (nameof (colorType), $"Unknown color type: '{colorType}'"),
			};

		// SkImageInfo.cpp - SkColorTypeValidateAlphaType
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

	public struct SKCodecOptions : IEquatable<SKCodecOptions>
	{
		public static readonly SKCodecOptions Default;

		static SKCodecOptions ()
		{
			Default = new SKCodecOptions (SKZeroInitialized.No);
		}

		public SKCodecOptions (SKZeroInitialized zeroInitialized)
		{
			ZeroInitialized = zeroInitialized;
			Subset = null;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		public SKCodecOptions (SKZeroInitialized zeroInitialized, SKRectI subset)
		{
			ZeroInitialized = zeroInitialized;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		public SKCodecOptions (SKRectI subset)
		{
			ZeroInitialized = SKZeroInitialized.No;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
		}
		public SKCodecOptions (int frameIndex)
		{
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = -1;
		}
		public SKCodecOptions (int frameIndex, int priorFrame)
		{
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = priorFrame;
		}

		public SKZeroInitialized ZeroInitialized { readonly get; set; }
		public SKRectI? Subset { readonly get; set; }
		public readonly bool HasSubset => Subset != null;
		public int FrameIndex { readonly get; set; }
		public int PriorFrame { readonly get; set; }

		public readonly bool Equals (SKCodecOptions obj) =>
			ZeroInitialized == obj.ZeroInitialized &&
			Subset == obj.Subset &&
			FrameIndex == obj.FrameIndex &&
			PriorFrame == obj.PriorFrame;

		public readonly override bool Equals (object obj) =>
			obj is SKCodecOptions f && Equals (f);

		public static bool operator == (SKCodecOptions left, SKCodecOptions right) =>
			left.Equals (right);

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

	public partial struct SKFontMetrics
	{
		private const uint flagsUnderlineThicknessIsValid = (1U << 0);
		private const uint flagsUnderlinePositionIsValid = (1U << 1);
		private const uint flagsStrikeoutThicknessIsValid = (1U << 2);
		private const uint flagsStrikeoutPositionIsValid = (1U << 3);

		public readonly float Top => fTop;

		public readonly float Ascent => fAscent;

		public readonly float Descent => fDescent;

		public readonly float Bottom => fBottom;

		public readonly float Leading => fLeading;

		public readonly float AverageCharacterWidth => fAvgCharWidth;

		public readonly float MaxCharacterWidth => fMaxCharWidth;

		public readonly float XMin => fXMin;

		public readonly float XMax => fXMax;

		public readonly float XHeight => fXHeight;

		public readonly float CapHeight => fCapHeight;

		public readonly float? UnderlineThickness => GetIfValid (fUnderlineThickness, flagsUnderlineThicknessIsValid);
		public readonly float? UnderlinePosition => GetIfValid (fUnderlinePosition, flagsUnderlinePositionIsValid);
		public readonly float? StrikeoutThickness => GetIfValid (fStrikeoutThickness, flagsStrikeoutThicknessIsValid);
		public readonly float? StrikeoutPosition => GetIfValid (fStrikeoutPosition, flagsStrikeoutPositionIsValid);

		private readonly float? GetIfValid (float value, uint flag) =>
			(fFlags & flag) == flag ? value : (float?)null;
	}

	public struct SKLattice : IEquatable<SKLattice>
	{
		public int[] XDivs { readonly get; set; }
		public int[] YDivs { readonly get; set; }
		public SKLatticeRectType[] RectTypes { readonly get; set; }
		public SKRectI? Bounds { readonly get; set; }
		public SKColor[] Colors { readonly get; set; }

		public readonly bool Equals (SKLattice obj) =>
			XDivs == obj.XDivs &&
			YDivs == obj.YDivs &&
			RectTypes == obj.RectTypes &&
			Bounds == obj.Bounds &&
			Colors == obj.Colors;

		public readonly override bool Equals (object obj) =>
			obj is SKLattice f && Equals (f);

		public static bool operator == (SKLattice left, SKLattice right) =>
			left.Equals (right);

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
			var zone = datetime.Hour - datetime.ToUniversalTime ().Hour;
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

	public struct SKDocumentPdfMetadata : IEquatable<SKDocumentPdfMetadata>
	{
		public const float DefaultRasterDpi = SKDocument.DefaultRasterDpi;
		public const int DefaultEncodingQuality = 101;

		public static readonly SKDocumentPdfMetadata Default;

		static SKDocumentPdfMetadata ()
		{
			Default = new SKDocumentPdfMetadata () {
				RasterDpi = DefaultRasterDpi,
				PdfA = false,
				EncodingQuality = 101,
			};
		}

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

		public string Title { readonly get; set; }
		public string Author { readonly get; set; }
		public string Subject { readonly get; set; }
		public string Keywords { readonly get; set; }
		public string Creator { readonly get; set; }
		public string Producer { readonly get; set; }
		public DateTime? Creation { readonly get; set; }
		public DateTime? Modified { readonly get; set; }
		public float RasterDpi { readonly get; set; }
		public bool PdfA { readonly get; set; }
		public int EncodingQuality { readonly get; set; }

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

		public readonly override bool Equals (object obj) =>
			obj is SKDocumentPdfMetadata f && Equals (f);

		public static bool operator == (SKDocumentPdfMetadata left, SKDocumentPdfMetadata right) =>
			left.Equals (right);

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

	public partial struct SKHighContrastConfig
	{
		public static readonly SKHighContrastConfig Default;

		static SKHighContrastConfig ()
		{
			Default = new SKHighContrastConfig (false, SKHighContrastConfigInvertStyle.NoInvert, 0.0f);
		}

		public SKHighContrastConfig (bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
		{
			fGrayscale = grayscale ? (byte)1 : (byte)0;
			fInvertStyle = invertStyle;
			fContrast = contrast;
		}

		public readonly bool IsValid =>
			(int)fInvertStyle >= (int)SKHighContrastConfigInvertStyle.NoInvert &&
			(int)fInvertStyle <= (int)SKHighContrastConfigInvertStyle.InvertLightness &&
			fContrast >= -1.0 &&
			fContrast <= 1.0;
	}

	public unsafe partial struct SKPngEncoderOptions
	{
		public static readonly SKPngEncoderOptions Default;

		static SKPngEncoderOptions ()
		{
			Default = new SKPngEncoderOptions (SKPngEncoderFilterFlags.AllFilters, 6);
		}

		public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel)
		{
			fICCProfile = default;
			fICCProfileDescription = default;

			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fComments = null;
		}

		public SKPngEncoderFilterFlags FilterFlags => fFilterFlags;

		public int ZLibLevel => fZLibLevel;
	}

	public unsafe partial struct SKJpegEncoderOptions
	{
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

		public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption)
		{
			fICCProfile = default;
			fICCProfileDescription = default;
			xmpMetadata = default;

			fQuality = quality;
			fDownsample = downsample;
			fAlphaOption = alphaOption;
		}

		public SKJpegEncoderAlphaOption AlphaOption => fAlphaOption;

		public SKJpegEncoderDownsample Downsample => fDownsample;

		public int Quality => fQuality;
	}

	public unsafe partial struct SKWebpEncoderOptions
	{
		public static readonly SKWebpEncoderOptions Default;

		static SKWebpEncoderOptions ()
		{
			Default = new SKWebpEncoderOptions (SKWebpEncoderCompression.Lossy, 100);
		}

		public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality)
		{
			fICCProfile = default;
			fICCProfileDescription = default;

			fCompression = compression;
			fQuality = quality;
		}

		public SKWebpEncoderCompression Compression => fCompression;

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
