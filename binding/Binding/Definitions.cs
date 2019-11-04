﻿using System;
using System.Runtime.InteropServices;
using System.Globalization;

using sk_string_t = System.IntPtr;

namespace SkiaSharp
{
	[Obsolete ("Use SKEncodedOrigin instead.")]
	public enum SKCodecOrigin {
		TopLeft = 1,
		TopRight = 2,
		BottomRight = 3,
		BottomLeft = 4,
		LeftTop = 5,
		RightTop = 6,
		RightBottom = 7,
		LeftBottom = 8,
	}

	public enum SKFontStyleWeight {
		Invisible   =   0,
		Thin        = 100,
		ExtraLight  = 200,
		Light       = 300,
		Normal      = 400,
		Medium      = 500,
		SemiBold    = 600,
		Bold        = 700,
		ExtraBold   = 800,
		Black       = 900,
		ExtraBlack  =1000,
	};

	public enum SKFontStyleWidth {
		UltraCondensed   = 1,
		ExtraCondensed   = 2,
		Condensed        = 3,
		SemiCondensed    = 4,
		Normal           = 5,
		SemiExpanded     = 6,
		Expanded         = 7,
		ExtraExpanded    = 8,
		UltraExpanded    = 9,
	}

	public static partial class SkiaExtensions {
		public static bool IsBgr (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.BgrVertical;

		public static bool IsRgb (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.RgbHorizontal || pg == SKPixelGeometry.RgbVertical;

		public static bool IsVertical (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrVertical || pg == SKPixelGeometry.RgbVertical;

		public static bool IsHorizontal (this SKPixelGeometry pg) =>
			pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.RgbHorizontal;
	}

	[Obsolete("Use SKSurfaceProperties instead.")]
	public struct SKSurfaceProps {
		public SKPixelGeometry PixelGeometry { get; set; }
		public SKSurfacePropsFlags Flags { get; set; }
	}

	public struct SKCodecOptions {
		public static readonly SKCodecOptions Default;

		static SKCodecOptions ()
		{
			Default = new SKCodecOptions (SKZeroInitialized.No);
		}
		public SKCodecOptions (SKZeroInitialized zeroInitialized) {
			ZeroInitialized = zeroInitialized;
			Subset = null;
			FrameIndex = 0;
			PriorFrame = -1;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKCodecOptions (SKZeroInitialized zeroInitialized, SKRectI subset) {
			ZeroInitialized = zeroInitialized;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKCodecOptions (SKRectI subset) {
			ZeroInitialized = SKZeroInitialized.No;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKCodecOptions (int frameIndex) {
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = -1;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKCodecOptions (int frameIndex, int priorFrame) {
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = priorFrame;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKZeroInitialized ZeroInitialized { get; set; }
		public SKRectI? Subset { get; set; }
		public bool HasSubset => Subset != null;
		public int FrameIndex { get; set; }
		public int PriorFrame { get; set; }
		public SKTransferFunctionBehavior PremulBehavior { get; set; }
	}

	public partial struct SKFontMetrics
	{
		private const uint flagsUnderlineThicknessIsValid = (1U << 0);
		private const uint flagsUnderlinePositionIsValid  = (1U << 1);
		private const uint flagsStrikeoutThicknessIsValid = (1U << 2);
		private const uint flagsStrikeoutPositionIsValid  = (1U << 3);

		public float Top => fTop;

		public float Ascent => fAscent;

		public float Descent => fDescent;

		public float Bottom => fBottom;

		public float Leading => fLeading;

		public float AverageCharacterWidth => fAvgCharWidth;

		public float MaxCharacterWidth => fMaxCharWidth;

		public float XMin => fXMin;

		public float XMax => fXMax;

		public float XHeight => fXHeight;

		public float CapHeight => fCapHeight;

		public float? UnderlineThickness => GetIfValid(fUnderlineThickness, flagsUnderlineThicknessIsValid);
		public float? UnderlinePosition => GetIfValid(fUnderlinePosition, flagsUnderlinePositionIsValid);
		public float? StrikeoutThickness => GetIfValid(fStrikeoutThickness, flagsStrikeoutThicknessIsValid);
		public float? StrikeoutPosition => GetIfValid(fStrikeoutPosition, flagsStrikeoutPositionIsValid);

		private float? GetIfValid (float value, uint flag) =>
			(fFlags & flag) == flag ? value : (float?)null;
	}

	public struct SKLattice {
		public int[] XDivs { get; set; }
		public int[] YDivs { get; set; }
		public SKLatticeRectType[] RectTypes { get; set; }
		public SKRectI? Bounds { get; set; }
		public SKColor[] Colors { get; set; }
	}

	internal partial struct SKTimeDateTimeInternal {
		public static SKTimeDateTimeInternal Create (DateTime datetime) {
			var zone = datetime.Hour - datetime.ToUniversalTime().Hour;
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

	public struct SKDocumentPdfMetadata {
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

	[Obsolete]
	[Flags]
	public enum SKColorSpaceFlags {
		None = 0,
		NonLinearBlending = 0x1,
	}

	public partial struct SKHighContrastConfig {
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

		public bool IsValid =>
			(int)fInvertStyle >= (int)SKHighContrastConfigInvertStyle.NoInvert &&
			(int)fInvertStyle <= (int)SKHighContrastConfigInvertStyle.InvertLightness &&
			fContrast >= -1.0 &&
			fContrast <= 1.0;
	}

	public unsafe partial struct SKPngEncoderOptions {
		public static readonly SKPngEncoderOptions Default;

		static SKPngEncoderOptions ()
		{
			Default = new SKPngEncoderOptions (SKPngEncoderFilterFlags.AllFilters, 6, SKTransferFunctionBehavior.Respect);
		}

		public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel)
		{
			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fUnpremulBehavior = SKTransferFunctionBehavior.Respect;
			fComments = null;
		}

		public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel, SKTransferFunctionBehavior unpremulBehavior)
		{
			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fUnpremulBehavior = unpremulBehavior;
			fComments = null;
		}

		public SKPngEncoderFilterFlags FilterFlags {
			get => fFilterFlags;
			set => fFilterFlags = value;
		}
		public int ZLibLevel {
			get => fZLibLevel;
			set => fZLibLevel = value;
		}
		public SKTransferFunctionBehavior UnpremulBehavior {
			get => fUnpremulBehavior;
			set => fUnpremulBehavior = value;
		}
	}

	public partial struct SKJpegEncoderOptions {
		public static readonly SKJpegEncoderOptions Default;

		static SKJpegEncoderOptions ()
		{
			Default = new SKJpegEncoderOptions (100, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore, SKTransferFunctionBehavior.Respect);
		}

		public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption)
		{
			fQuality = quality;
			fDownsample = downsample;
			fAlphaOption = alphaOption;
			fBlendBehavior = SKTransferFunctionBehavior.Respect;
		}

		public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption, SKTransferFunctionBehavior blendBehavior)
		{
			fQuality = quality;
			fDownsample = downsample;
			fAlphaOption = alphaOption;
			fBlendBehavior = blendBehavior;
		}
	}

	public partial struct SKWebpEncoderOptions {
		public static readonly SKWebpEncoderOptions Default;

		static SKWebpEncoderOptions ()
		{
			Default = new SKWebpEncoderOptions (SKWebpEncoderCompression.Lossy, 100, SKTransferFunctionBehavior.Respect);
		}

		public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality)
		{
			fCompression = compression;
			fQuality = quality;
			fUnpremulBehavior = SKTransferFunctionBehavior.Respect;
		}

		public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality, SKTransferFunctionBehavior unpremulBehavior)
		{
			fCompression = compression;
			fQuality = quality;
			fUnpremulBehavior = unpremulBehavior;
		}
	}
}
