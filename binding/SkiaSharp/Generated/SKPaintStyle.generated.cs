using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_paint_style_t
	/// <summary>Indications on how to draw geometry.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Styles apply to rectangle, oval, path, and text. Bitmaps are always drawn in
	/// <xref:SkiaSharp.SKPaintStyle.Fill>, and lines are always drawn in
	/// <xref:SkiaSharp.SKPaintStyle.Stroke>.
	///
	/// <xref:SkiaSharp.SKPaintStyle.StrokeAndFill> implicitly draws the result with
	/// <xref:SkiaSharp.SKPathFillType.Winding> so if the original path is even-odd,
	/// the results may not appear the same as if it was drawn twice, filled and then
	/// stroked.
	/// ]]></format></remarks>
	public enum SKPaintStyle {
		// FILL_SK_PAINT_STYLE = 0
		/// <summary>Fill the geometry.</summary>
		Fill = 0,
		// STROKE_SK_PAINT_STYLE = 1
		/// <summary>Stroke the geometry.</summary>
		Stroke = 1,
		// STROKE_AND_FILL_SK_PAINT_STYLE = 2
		/// <summary>Fill and stroke the geometry.</summary>
		StrokeAndFill = 2,
	}
}
