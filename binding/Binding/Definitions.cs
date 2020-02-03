using System;
using System.ComponentModel;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKEncodedOrigin instead.")]
	public enum SKCodecOrigin
	{
		TopLeft = 1,
		TopRight = 2,
		BottomRight = 3,
		BottomLeft = 4,
		LeftTop = 5,
		RightTop = 6,
		RightBottom = 7,
		LeftBottom = 8,
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKTextEncoding instead.")]
	public enum SKEncoding
	{
		Utf8 = 0,
		Utf16 = 1,
		Utf32 = 2,
	}

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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public static SKTextEncoding ToTextEncoding (this SKEncoding encoding) =>
			encoding switch
			{
				SKEncoding.Utf8 => SKTextEncoding.Utf8,
				SKEncoding.Utf16 => SKTextEncoding.Utf16,
				SKEncoding.Utf32 => SKTextEncoding.Utf32,
				_ => throw new ArgumentOutOfRangeException (nameof (encoding)),
			};
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKSurfaceProperties instead.")]
	public struct SKSurfaceProps
	{
		public SKPixelGeometry PixelGeometry { get; set; }
		public SKSurfacePropsFlags Flags { get; set; }
	}

	public struct SKCodecOptions
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

		public SKZeroInitialized ZeroInitialized { get; set; }
		public SKRectI? Subset { get; set; }
		public readonly bool HasSubset => Subset != null;
		public int FrameIndex { get; set; }
		public int PriorFrame { get; set; }
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

	public struct SKLattice
	{
		public int[] XDivs { get; set; }
		public int[] YDivs { get; set; }
		public SKLatticeRectType[] RectTypes { get; set; }
		public SKRectI? Bounds { get; set; }
		public SKColor[] Colors { get; set; }
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

	public struct SKDocumentPdfMetadata
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

		public string Title { get; set; }
		public string Author { get; set; }
		public string Subject { get; set; }
		public string Keywords { get; set; }
		public string Creator { get; set; }
		public string Producer { get; set; }
		public DateTime? Creation { get; set; }
		public DateTime? Modified { get; set; }
		public float RasterDpi { get; set; }
		public bool PdfA { get; set; }
		public int EncodingQuality { get; set; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete]
	[Flags]
	public enum SKColorSpaceFlags
	{
		None = 0,
		NonLinearBlending = 0x1,
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
			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fComments = null;
		}
		public SKPngEncoderFilterFlags FilterFlags {
			readonly get => fFilterFlags;
			set => fFilterFlags = value;
		}

		public int ZLibLevel {
			readonly get => fZLibLevel;
			set => fZLibLevel = value;
		}
	}

	public partial struct SKJpegEncoderOptions
	{
		public static readonly SKJpegEncoderOptions Default;

		static SKJpegEncoderOptions ()
		{
			Default = new SKJpegEncoderOptions (100, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore);
		}

		public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption)
		{
			fQuality = quality;
			fDownsample = downsample;
			fAlphaOption = alphaOption;
		}
	}

	public partial struct SKWebpEncoderOptions
	{
		public static readonly SKWebpEncoderOptions Default;

		static SKWebpEncoderOptions ()
		{
			Default = new SKWebpEncoderOptions (SKWebpEncoderCompression.Lossy, 100);
		}

		public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality)
		{
			fCompression = compression;
			fQuality = quality;
		}
	}
}
