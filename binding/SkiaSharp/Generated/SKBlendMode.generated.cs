using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_blendmode_t
	/// <summary>Predefined blend modes.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// - **Porter Duff Compositing Operators**
	///   Defined algebra of compositing.
	///   These operators control the results of mixing the four sub-pixel regions
	///   formed by the overlapping of graphical objects that have an alpha or
	///   pixel coverage channel/value.
	/// - **Separable Blend Mode**
	///   Each component of the result color is completely determined by the
	///   corresponding components of the constituent backdrop and source colors.
	/// - **Non-Separable Blend Mode**
	///   Considers all color components in combination as opposed to the
	///   separable ones that look at each component individually.
	/// ]]></format></remarks>
	public enum SKBlendMode {
		// CLEAR_SK_BLENDMODE = 0
		/// <summary>No regions are enabled. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_clr.svg)</summary>
		Clear = 0,
		// SRC_SK_BLENDMODE = 1
		/// <summary>Only the source will be present. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_src.svg)</summary>
		Src = 1,
		// DST_SK_BLENDMODE = 2
		/// <summary>Only the destination will be present. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_dst.svg)</summary>
		Dst = 2,
		// SRCOVER_SK_BLENDMODE = 3
		/// <summary>Source is placed over the destination. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_src-over.svg)</summary>
		SrcOver = 3,
		// DSTOVER_SK_BLENDMODE = 4
		/// <summary>Destination is placed over the source. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_dst-over.svg)</summary>
		DstOver = 4,
		// SRCIN_SK_BLENDMODE = 5
		/// <summary>The source that overlaps the destination, replaces the destination. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_src-in.svg)</summary>
		SrcIn = 5,
		// DSTIN_SK_BLENDMODE = 6
		/// <summary>Destination which overlaps the source, replaces the source. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_dst-in.svg)</summary>
		DstIn = 6,
		// SRCOUT_SK_BLENDMODE = 7
		/// <summary>Source is placed, where it falls outside of the destination. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_src-out.svg)</summary>
		SrcOut = 7,
		// DSTOUT_SK_BLENDMODE = 8
		/// <summary>Destination is placed, where it falls outside of the source. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_dst-out.svg)</summary>
		DstOut = 8,
		// SRCATOP_SK_BLENDMODE = 9
		/// <summary>Source which overlaps the destination, replaces the destination. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_src-atop.svg)</summary>
		SrcATop = 9,
		// DSTATOP_SK_BLENDMODE = 10
		/// <summary>Destination which overlaps the source replaces the source. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_dst-atop.svg)</summary>
		DstATop = 10,
		// XOR_SK_BLENDMODE = 11
		/// <summary>The non-overlapping regions of source and destination are combined. [Porter Duff Compositing Operators] (https://drafts.fxtf.org/compositing-1/examples/PD_xor.svg)</summary>
		Xor = 11,
		// PLUS_SK_BLENDMODE = 12
		/// <summary>Display the sum of the source image and destination image. [Porter Duff Compositing Operators]</summary>
		Plus = 12,
		// MODULATE_SK_BLENDMODE = 13
		/// <summary>Multiplies all components (= alpha and color). [Separable Blend Modes]</summary>
		Modulate = 13,
		// SCREEN_SK_BLENDMODE = 14
		/// <summary>Multiplies the complements of the backdrop and source color values, then complements the result. [Separable Blend Modes]</summary>
		Screen = 14,
		// OVERLAY_SK_BLENDMODE = 15
		/// <summary>Multiplies or screens the colors, depending on the backdrop color value. [Separable Blend Modes]</summary>
		Overlay = 15,
		// DARKEN_SK_BLENDMODE = 16
		/// <summary>Selects the darker of the backdrop and source colors. [Separable Blend Modes]</summary>
		Darken = 16,
		// LIGHTEN_SK_BLENDMODE = 17
		/// <summary>Selects the lighter of the backdrop and source colors. [Separable Blend Modes]</summary>
		Lighten = 17,
		// COLORDODGE_SK_BLENDMODE = 18
		/// <summary>Brightens the backdrop color to reflect the source color. [Separable Blend Modes]</summary>
		ColorDodge = 18,
		// COLORBURN_SK_BLENDMODE = 19
		/// <summary>Darkens the backdrop color to reflect the source color. [Separable Blend Modes]</summary>
		ColorBurn = 19,
		// HARDLIGHT_SK_BLENDMODE = 20
		/// <summary>Multiplies or screens the colors, depending on the source color value. [Separable Blend Modes]</summary>
		HardLight = 20,
		// SOFTLIGHT_SK_BLENDMODE = 21
		/// <summary>Darkens or lightens the colors, depending on the source color value. [Separable Blend Modes]</summary>
		SoftLight = 21,
		// DIFFERENCE_SK_BLENDMODE = 22
		/// <summary>Subtracts the darker of the two constituent colors from the lighter color. [Separable Blend Modes]</summary>
		Difference = 22,
		// EXCLUSION_SK_BLENDMODE = 23
		/// <summary>Produces an effect similar to that of the Difference mode but lower in contrast. [Separable Blend Modes]</summary>
		Exclusion = 23,
		// MULTIPLY_SK_BLENDMODE = 24
		/// <summary>The source color is multiplied by the destination color and replaces the destination [Separable Blend Modes]</summary>
		Multiply = 24,
		// HUE_SK_BLENDMODE = 25
		/// <summary>Creates a color with the hue of the source color and the saturation and luminosity of the backdrop color. [Non-Separable Blend Modes]</summary>
		Hue = 25,
		// SATURATION_SK_BLENDMODE = 26
		/// <summary>Creates a color with the saturation of the source color and the hue and luminosity of the backdrop color. [Non-Separable Blend Modes]</summary>
		Saturation = 26,
		// COLOR_SK_BLENDMODE = 27
		/// <summary>Creates a color with the hue and saturation of the source color and the luminosity of the backdrop color. [Non-Separable Blend Modes]</summary>
		Color = 27,
		// LUMINOSITY_SK_BLENDMODE = 28
		/// <summary>Creates a color with the luminosity of the source color and the hue and saturation of the backdrop color. [Non-Separable Blend Modes]</summary>
		Luminosity = 28,
	}
}
