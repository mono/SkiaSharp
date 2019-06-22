using System;
using System.Runtime.InteropServices;
using System.Globalization;

using sk_string_t = System.IntPtr;

namespace SkiaSharp
{
	public enum SKCodecResult {
		Success,
		IncompleteInput,
		ErrorInInput,
		InvalidConversion,
		InvalidScale,
		InvalidParameters,
		InvalidInput,
		CouldNotRewind,
		InternalError,
		Unimplemented,
	}

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

	public enum SKEncodedOrigin {
		TopLeft = 1,
		TopRight = 2,
		BottomRight = 3,
		BottomLeft = 4,
		LeftTop = 5,
		RightTop = 6,
		RightBottom = 7,
		LeftBottom = 8,
		Default = TopLeft,
	}

	public enum SKEncodedImageFormat {
		Bmp,
		Gif,
		Ico,
		Jpeg,
		Png,
		Wbmp,
		Webp,
		Pkm,
		Ktx,
		Astc,
		Dng,
		// Heif // appears to be development still
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
	};

	public enum SKFontStyleSlant {
		Upright = 0,
		Italic  = 1,
		Oblique = 2,
	};

	public enum SKPointMode {
		Points, Lines, Polygon
	}

	public enum SKPathDirection {
		Clockwise,
		CounterClockwise
	}

	public enum SKPathArcSize {
		Small,
		Large
	}

	public enum SKPathFillType
	{
		Winding,
		EvenOdd,
		InverseWinding,
		InverseEvenOdd
	}

	[Flags]
	public enum SKPathSegmentMask {
		Line  = 1 << 0,
		Quad  = 1 << 1,
		Conic = 1 << 2,
		Cubic = 1 << 3,
	}

	public enum SKColorType {
		Unknown,
		Alpha8,
		Rgb565,
		Argb4444,
		Rgba8888,
		Rgb888x,
		Bgra8888,
		Rgba1010102,
		Rgb101010x,
		Gray8,
		RgbaF16
	}

	public enum SKAlphaType {
		Unknown,
		Opaque,
		Premul,
		Unpremul
	}

	public enum SKShaderTileMode {
		Clamp, Repeat, Mirror
	}

	public enum SKBlurStyle {
		Normal, Solid, Outer, Inner
	}

	public enum SKBlendMode {
		Clear,
		Src,
		Dst,
		SrcOver,
		DstOver,
		SrcIn,
		DstIn,
		SrcOut,
		DstOut,
		SrcATop,
		DstATop,
		Xor,
		Plus,
		Modulate,
		Screen,
		Overlay,
		Darken,
		Lighten,
		ColorDodge,
		ColorBurn,
		HardLight,
		SoftLight,
		Difference,
		Exclusion,
		Multiply,
		Hue,
		Saturation,
		Color,
		Luminosity,
	}

	public enum SKPixelGeometry {
		Unknown,
		RgbHorizontal,
		BgrHorizontal,
		RgbVertical,
		BgrVertical
	}

	[Flags]
	public enum SKSurfacePropsFlags {
		None = 0,
		UseDeviceIndependentFonts = 1 << 0,
	}

	public enum SKEncoding {
		Utf8, Utf16, Utf32
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

	public enum SKStrokeCap {
		Butt, Round, Square
	}

	public enum SKStrokeJoin {
		Miter, Round, Bevel,
	}

	public enum SKTextAlign {
		Left, Center, Right
	}

	public enum SKTextEncoding {
		Utf8, Utf16, Utf32, GlyphId
	}

	public enum SKFilterQuality
	{
		None,
		Low,
		Medium,
		High
	}

	[Flags]
	public enum SKCropRectFlags
	{
		HasNone = 0,
		HasLeft = 0x01,
		HasTop = 0x02,
		HasWidth = 0x04,
		HasHeight = 0x08,
		HasAll = 0x0F,
	}

	public enum SKDropShadowImageFilterShadowMode
	{
		DrawShadowAndForeground,
		DrawShadowOnly,
	}

	public enum SKDisplacementMapEffectChannelSelectorType
	{
		Unknown,
		R,
		G,
		B,
		A,
	}

	public enum SKMatrixConvolutionTileMode
	{
		Clamp,
		Repeat,
		ClampToBlack,
	}

	public enum SKPaintStyle
	{
		Fill,
		Stroke,
		StrokeAndFill,
	}

	public enum SKPaintHinting
	{
		NoHinting = 0,
		Slight = 1,
		Normal = 2,
		Full = 3
	}

	public enum SKRegionOperation
	{
		Difference,
		Intersect,
		Union,
		XOR,
		ReverseDifference,
		Replace,
	}

	public enum SKClipOperation
	{
		Difference,
		Intersect,
	}

	[Obsolete("Use SKSurfaceProperties instead.")]
	public struct SKSurfaceProps {
		public SKPixelGeometry PixelGeometry { get; set; }
		public SKSurfacePropsFlags Flags { get; set; }
	}

	public enum SKZeroInitialized {
		Yes,
		No,
	}

	public enum SKCodecScanlineOrder {
		TopDown,
		BottomUp
	}


	public enum SKTransferFunctionBehavior {
		Respect,
		Ignore,
	}


	[StructLayout (LayoutKind.Sequential)]
	internal unsafe struct SKCodecOptionsInternal {
		public SKZeroInitialized fZeroInitialized;
		public SKRectI* fSubset;
		public int fFrameIndex;
		public int fPriorFrame;
		public SKTransferFunctionBehavior fPremulBehavior;
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

	public enum SKCodecAnimationDisposalMethod {
		Keep                     = 1,
		RestoreBackgroundColor   = 2,
		RestorePrevious          = 3,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKCodecFrameInfo {
		private int requiredFrame;
		private int duration;
		private byte fullyRecieved;
		private SKAlphaType alphaType;
		private SKCodecAnimationDisposalMethod disposalMethod;

		public int RequiredFrame {
			get => requiredFrame;
			set => requiredFrame = value;
		}

		public int Duration {
			get => duration;
			set => duration = value;
		}

		public bool FullyRecieved {
			get => fullyRecieved != 0;
			set => fullyRecieved = value ? (byte)1 : (byte)0;
		}

		public SKAlphaType AlphaType {
			get => alphaType;
			set => alphaType = value;
		}

		public SKCodecAnimationDisposalMethod DisposalMethod {
			get => disposalMethod;
			set => disposalMethod = value;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKFontMetrics
	{
		private const uint flagsUnderlineThicknessIsValid = (1U << 0);
		private const uint flagsUnderlinePositionIsValid  = (1U << 1);
		private const uint flagsStrikeoutThicknessIsValid = (1U << 2);
		private const uint flagsStrikeoutPositionIsValid  = (1U << 3);

		uint flags;                     // Bit field to identify which values are unknown
		float top;                      // The greatest distance above the baseline for any glyph (will be <= 0)
		float ascent;                   // The recommended distance above the baseline (will be <= 0)
		float descent;                  // The recommended distance below the baseline (will be >= 0)
		float bottom;                   // The greatest distance below the baseline for any glyph (will be >= 0)
		float leading;                  // The recommended distance to add between lines of text (will be >= 0)
		float avgCharWidth;             // the average character width (>= 0)
		float maxCharWidth;             // the max character width (>= 0)
		float xMin;                     // The minimum bounding box x value for all glyphs
		float xMax;                     // The maximum bounding box x value for all glyphs
		float xHeight;                  // The height of an 'x' in px, or 0 if no 'x' in face
		float capHeight;                // The cap height (> 0), or 0 if cannot be determined.
		float underlineThickness;       // underline thickness, or 0 if cannot be determined
		float underlinePosition;        // underline position, or 0 if cannot be determined
		float strikeoutThickness;
		float strikeoutPosition;

		public float Top => top;

		public float Ascent => ascent;

		public float Descent => descent;

		public float Bottom => bottom;

		public float Leading => leading;

		public float AverageCharacterWidth => avgCharWidth;

		public float MaxCharacterWidth => maxCharWidth;

		public float XMin => xMin;

		public float XMax => xMax;

		public float XHeight => xHeight;

		public float CapHeight => capHeight;

		public float? UnderlineThickness => GetIfValid(underlineThickness, flagsUnderlineThicknessIsValid);
		public float? UnderlinePosition => GetIfValid(underlinePosition, flagsUnderlinePositionIsValid);
		public float? StrikeoutThickness => GetIfValid(strikeoutThickness, flagsStrikeoutThicknessIsValid);
		public float? StrikeoutPosition => GetIfValid(strikeoutPosition, flagsStrikeoutPositionIsValid);

		private float? GetIfValid (float value, uint flag) =>
			(flags & flag) == flag ? value : (float?)null;
	}

	public enum SKPathOp {
		Difference,
		Intersect,
		Union,
		Xor,
		ReverseDifference,
	};

	public enum SKPathConvexity {
		Unknown,
		Convex,
		Concave,
	};

	public enum SKLatticeRectType {
		Default,
		Transparent,
		FixedColor,
	};

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct SKLatticeInternal {
		public int* fXDivs;
		public int* fYDivs;
		public SKLatticeRectType* fRectTypes;
		public int fXCount;
		public int fYCount;
		public SKRectI* fBounds;
		public SKColor* fColors;
	}

	public struct SKLattice {
		public int[] XDivs { get; set; }
		public int[] YDivs { get; set; }
		public SKLatticeRectType[] RectTypes { get; set; }
		public SKRectI? Bounds { get; set; }
		public SKColor[] Colors { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SKTimeDateTimeInternal {
		public Int16 TimeZoneMinutes;
		public UInt16 Year;
		public Byte Month;
		public Byte DayOfWeek;
		public Byte Day;
		public Byte Hour;
		public Byte Minute;
		public Byte Second;

		public static SKTimeDateTimeInternal Create (DateTime datetime) {
			var zone = datetime.Hour - datetime.ToUniversalTime().Hour;
			return new SKTimeDateTimeInternal {
				TimeZoneMinutes = (Int16)(zone * 60),
				Year = (UInt16)datetime.Year,
				Month = (Byte)datetime.Month,
				DayOfWeek = (Byte)datetime.DayOfWeek,
				Day = (Byte)datetime.Day,
				Hour = (Byte)datetime.Hour,
				Minute = (Byte)datetime.Minute,
				Second = (Byte)datetime.Second
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct SKDocumentPdfMetadataInternal {
		public sk_string_t Title;
		public sk_string_t Author;
		public sk_string_t Subject;
		public sk_string_t Keywords;
		public sk_string_t Creator;
		public sk_string_t Producer;
		public SKTimeDateTimeInternal* Creation;
		public SKTimeDateTimeInternal* Modified;
		public float RasterDPI;
		public byte PDFA;
		public int EncodingQuality;
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

	public enum SKColorSpaceGamut {
		Srgb,
		AdobeRgb,
		Dcip3D65,
		Rec2020,
	}

	[Obsolete]
	[Flags]
	public enum SKColorSpaceFlags {
		None = 0,
		NonLinearBlending = 0x1,
	}

	public enum SKColorSpaceRenderTargetGamma {
		Linear,
		Srgb,
	}

	public enum SKColorSpaceType {
		Rgb,
		Cmyk,
		Gray,
	}

	public enum SKNamedGamma {
		Linear,
		Srgb,
		TwoDotTwoCurve,
		NonStandard,
	}

	public enum SKMaskFormat {
		BW,
		A8,
		ThreeD,
		Argb32,
		Lcd16,
	}

	[Flags]
	public enum SKMatrix44TypeMask {
		Identity = 0,
		Translate = 0x01,
		Scale = 0x02,
		Affine = 0x04,
		Perspective = 0x08
	}

	public enum SKVertexMode {
		Triangles,
		TriangleStrip,
		TriangleFan,
	}

	public enum SKImageCachingHint {
		Allow,
		Disallow,
	}

	public enum SKHighContrastConfigInvertStyle {
		NoInvert,
		InvertBrightness,
		InvertLightness,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKHighContrastConfig {
		private byte fGrayscale;
		private SKHighContrastConfigInvertStyle fInvertStyle;
		private float fContrast;

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

		public bool Grayscale {
			get => fGrayscale != 0;
			set => fGrayscale = value ? (byte)1 : (byte)0;
		}
		public SKHighContrastConfigInvertStyle InvertStyle {
			get => fInvertStyle;
			set => fInvertStyle = value;
		}
		public float Contrast {
			get => fContrast;
			set => fContrast = value;
		}

		public bool IsValid =>
			(int)fInvertStyle >= (int)SKHighContrastConfigInvertStyle.NoInvert &&
			(int)fInvertStyle <= (int)SKHighContrastConfigInvertStyle.InvertLightness &&
			fContrast >= -1.0 &&
			fContrast <= 1.0;
	}

	[Flags]
	public enum SKBitmapAllocFlags : uint {
		None = 0,
		ZeroPixels = 1 << 0,
	}

	[Flags]
	public enum SKPngEncoderFilterFlags {
		NoFilters  = 0x00,
		None       = 0x08,
		Sub        = 0x10,
		Up         = 0x20,
		Avg        = 0x40,
		Paeth      = 0x80,
		AllFilters = None | Sub | Up | Avg | Paeth,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPngEncoderOptions {
		private SKPngEncoderFilterFlags fFilterFlags;
		private int fZLibLevel;
		private SKTransferFunctionBehavior fUnpremulBehavior;
		private IntPtr fComments; // TODO: get and set comments

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
			fComments = IntPtr.Zero;
		}

		public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel, SKTransferFunctionBehavior unpremulBehavior)
		{
			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fUnpremulBehavior = unpremulBehavior;
			fComments = IntPtr.Zero;
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

	public enum SKJpegEncoderDownsample {
		Downsample420,
		Downsample422,
		Downsample444,
	}

	public enum SKJpegEncoderAlphaOption {
		Ignore,
		BlendOnBlack,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKJpegEncoderOptions {
		private int fQuality;
		private SKJpegEncoderDownsample fDownsample;
		private SKJpegEncoderAlphaOption fAlphaOption;
		private SKTransferFunctionBehavior fBlendBehavior;

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

		public int Quality {
			get => fQuality;
			set => fQuality = value;
		}
		public SKJpegEncoderDownsample Downsample {
			get => fDownsample;
			set => fDownsample = value;
		}
		public SKJpegEncoderAlphaOption AlphaOption {
			get => fAlphaOption;
			set => fAlphaOption = value;
		}
		public SKTransferFunctionBehavior BlendBehavior {
			get => fBlendBehavior;
			set => fBlendBehavior = value;
		}
	}

	public enum SKWebpEncoderCompression {
		Lossy,
		Lossless,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKWebpEncoderOptions {
		private SKWebpEncoderCompression fCompression;
		private float fQuality;
		private SKTransferFunctionBehavior fUnpremulBehavior;

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

		public SKWebpEncoderCompression Compression {
			get => fCompression;
			set => fCompression = value;
		}
		public float Quality {
			get => fQuality;
			set => fQuality = value;
		}
		public SKTransferFunctionBehavior UnpremulBehavior {
			get => fUnpremulBehavior;
			set => fUnpremulBehavior = value;
		}
	}

	public enum SKRoundRectType {
		Empty,
		Rect,
		Oval,
		Simple,
		NinePatch,
		Complex,
	}

	public enum SKRoundRectCorner {
		UpperLeft,
		UpperRight,
		LowerRight,
		LowerLeft,
	}
}
