using System.Collections.Generic;

namespace SkiaSharpSample;

public enum TagKind
{
	Type,
	Method,
	Other,
}

/// <summary>
/// Classifies sample tags as Types vs Methods for visual differentiation in the gallery.
/// </summary>
public static class KnownApis
{
	private static readonly HashSet<string> Types = new(StringComparer.Ordinal)
	{
		// Core
		"SKCanvas", "SKPaint", "SKBitmap", "SKImage", "SKSurface", "SKData",
		"SKColor", "SKColorF", "SKRect", "SKRectI", "SKPoint", "SKSize",

		// Text
		"SKFont", "SKTypeface", "SKFontMetrics", "SKFontManager", "SKShaper",
		"SKTextBlob", "SKFourByteTag", "SKFontVariationPositionCoordinate",

		// Paths & Geometry
		"SKPath", "SKPathBuilder", "SKPathMeasure", "SKPathFillType",
		"SKMatrix", "SKMatrix44", "SKRoundRect",

		// Effects
		"SKShader", "SKShaderTileMode", "SKImageFilter", "SKColorFilter",
		"SKMaskFilter", "SKPathEffect", "SKBlendMode",
		"SKRuntimeEffect", "SKRuntimeEffectUniforms", "SKRuntimeEffectChildren",

		// Images & Color
		"SKCodec", "SKCodecOptions", "SKImageInfo", "SKManagedStream", "SKPixmap",
		"SKColorSpace", "SKColorSpaceXyz", "SKColorSpaceTransferFn",

		// Documents & Encoding
		"SKDocument", "SKDocumentPdfMetadata",
		"SKWebpEncoder", "SKWebpEncoderFrame", "SKWebpEncoderOptions",

		// Vertices
		"SKVertices",

		// External
		"SKSvg", "Animation",
		"HarfBuzz.Blob", "HarfBuzz.Face",
	};

	private static readonly HashSet<string> Methods = new(StringComparer.Ordinal)
	{
		// Draw methods
		"DrawText", "DrawPath", "DrawBitmap", "DrawImage", "DrawRect",
		"DrawRoundRect", "DrawCircle", "DrawLine", "DrawOval",
		"DrawVertices", "DrawPaint", "DrawPicture",
		"DrawBitmapNinePatch", "DrawTextOnPath", "DrawShapedText",

		// Canvas operations
		"Save", "Restore", "Translate", "RotateDegrees", "Scale",
		"SaveLayer", "Concat", "ClipRect", "ClipRoundRect",

		// Create/Factory methods
		"CreateBlur", "CreateDash", "CreateCorner", "CreateDiscrete",
		"Create1DPath", "Create2DPath", "CreateCompose",
		"CreateLinearGradient", "CreateRadialGradient",
		"CreateSweepGradient", "CreateTwoPointConicalGradient",
		"CreatePerlinNoiseFractalNoise", "CreatePerlinNoiseTurbulence",
		"CreateColorMatrix", "CreateHighContrast", "CreateBlendMode",
		"CreateDilate", "CreateErode", "CreateMagnifier",
		"CreateRotationDegrees", "CreateShader", "CreatePdf", "CreateXps",
		"CreateSrgb", "CreateRgb", "CreateCopy",

		// Query methods
		"MeasureText", "GetFillPath", "GetTextPath", "GetBounds",
		"GetTightBounds", "GetIntercepts", "GetPixels",
		"MatchCharacter",

		// Misc
		"ToShader", "Decode", "Clone", "FromData", "FromStream",
		"BeginPage", "EncodeAnimated", "SetColor", "Snapshot",
		"Render", "SeekFrameTime", "PaletteCount",
	};

	public static TagKind Classify(string tag) =>
		Types.Contains(tag) ? TagKind.Type
		: Methods.Contains(tag) ? TagKind.Method
		: TagKind.Other;
}
