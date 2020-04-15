﻿using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public enum SKPaintHinting
	{
		NoHinting = 0,
		Slight = 1,
		Normal = 2,
		Full = 3,
	}

	public unsafe class SKPaint : SKObject
	{
		private SKFont font;

		internal SKPaint (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPaint ()
			: this (SkiaApi.sk_compatpaint_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
			}
		}

		public SKPaint (SKFont font)
			: this (IntPtr.Zero, true)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			Handle = SkiaApi.sk_compatpaint_new_with_font (font.Handle);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_compatpaint_delete (Handle);

		// Reset

		public void Reset () =>
			SkiaApi.sk_compatpaint_reset (Handle);

		// properties

		public bool IsAntialias {
			get => SkiaApi.sk_paint_is_antialias (Handle);
			set => SkiaApi.sk_paint_set_antialias (Handle, value);
		}

		public bool IsDither {
			get => SkiaApi.sk_paint_is_dither (Handle);
			set => SkiaApi.sk_paint_set_dither (Handle, value);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public bool IsVerticalText {
			get => false;
			set { }
		}

		public bool IsLinearText {
			get => GetFont ().LinearMetrics;
			set => GetFont ().LinearMetrics = value;
		}

		public bool SubpixelText {
			get => GetFont ().Subpixel;
			set => GetFont ().Subpixel = value;
		}

		public bool LcdRenderText {
			get => GetFont ().Edging == SKFontEdging.SubpixelAntialias;
			set => GetFont ().Edging = value ? SKFontEdging.SubpixelAntialias : SKFontEdging.Antialias;
		}

		public bool IsEmbeddedBitmapText {
			get => GetFont ().EmbeddedBitmaps;
			set => GetFont ().EmbeddedBitmaps = value;
		}

		public bool IsAutohinted {
			get => GetFont ().ForceAutoHinting;
			set => GetFont ().ForceAutoHinting = value;
		}

		public SKPaintHinting HintingLevel {
			get => (SKPaintHinting)GetFont ().Hinting;
			set => GetFont ().Hinting = (SKFontHinting)value;
		}

		public bool FakeBoldText {
			get => GetFont ().Embolden;
			set => GetFont ().Embolden = value;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public bool DeviceKerningEnabled {
			get => false;
			set { }
		}

		public bool IsStroke {
			get => Style != SKPaintStyle.Fill;
			set => Style = value ? SKPaintStyle.Stroke : SKPaintStyle.Fill;
		}

		public SKPaintStyle Style {
			get => SkiaApi.sk_paint_get_style (Handle);
			set => SkiaApi.sk_paint_set_style (Handle, value);
		}

		public SKColor Color {
			get => SkiaApi.sk_paint_get_color (Handle);
			set => SkiaApi.sk_paint_set_color (Handle, (uint)value);
		}

		public float StrokeWidth {
			get => SkiaApi.sk_paint_get_stroke_width (Handle);
			set => SkiaApi.sk_paint_set_stroke_width (Handle, value);
		}

		public float StrokeMiter {
			get => SkiaApi.sk_paint_get_stroke_miter (Handle);
			set => SkiaApi.sk_paint_set_stroke_miter (Handle, value);
		}

		public SKStrokeCap StrokeCap {
			get => SkiaApi.sk_paint_get_stroke_cap (Handle);
			set => SkiaApi.sk_paint_set_stroke_cap (Handle, value);
		}

		public SKStrokeJoin StrokeJoin {
			get => SkiaApi.sk_paint_get_stroke_join (Handle);
			set => SkiaApi.sk_paint_set_stroke_join (Handle, value);
		}

		public SKShader Shader {
			get => SKShader.GetObject (SkiaApi.sk_paint_get_shader (Handle));
			set => SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKMaskFilter MaskFilter {
			get => SKMaskFilter.GetObject (SkiaApi.sk_paint_get_maskfilter (Handle));
			set => SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKColorFilter ColorFilter {
			get => SKColorFilter.GetObject (SkiaApi.sk_paint_get_colorfilter (Handle));
			set => SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKImageFilter ImageFilter {
			get => SKImageFilter.GetObject (SkiaApi.sk_paint_get_imagefilter (Handle));
			set => SkiaApi.sk_paint_set_imagefilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKBlendMode BlendMode {
			get => SkiaApi.sk_paint_get_blendmode (Handle);
			set => SkiaApi.sk_paint_set_blendmode (Handle, value);
		}

		public SKFilterQuality FilterQuality {
			get => SkiaApi.sk_paint_get_filter_quality (Handle);
			set => SkiaApi.sk_paint_set_filter_quality (Handle, value);
		}

		public SKTypeface Typeface {
			get => GetFont ().Typeface;
			set => GetFont ().Typeface = value;
		}

		public float TextSize {
			get => GetFont ().Size;
			set => GetFont ().Size = value;
		}

		public SKTextAlign TextAlign {
			get => SkiaApi.sk_compatpaint_get_text_align (Handle);
			set => SkiaApi.sk_compatpaint_set_text_align (Handle, value);
		}

		public SKTextEncoding TextEncoding {
			get => SkiaApi.sk_compatpaint_get_text_encoding (Handle);
			set => SkiaApi.sk_compatpaint_set_text_encoding (Handle, value);
		}

		public float TextScaleX {
			get => GetFont ().ScaleX;
			set => GetFont ().ScaleX = value;
		}

		public float TextSkewX {
			get => GetFont ().SkewX;
			set => GetFont ().SkewX = value;
		}

		public SKPathEffect PathEffect {
			get => SKPathEffect.GetObject (SkiaApi.sk_paint_get_path_effect (Handle));
			set => SkiaApi.sk_paint_set_path_effect (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		// FontSpacing

		public float FontSpacing =>
			GetFont ().Spacing;

		// FontMetrics

		public SKFontMetrics FontMetrics {
			get {
				return GetFont ().Metrics;
			}
		}

		public float GetFontMetrics (out SKFontMetrics metrics) =>
			GetFont ().GetFontMetrics (out metrics);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetFontMetrics (out SKFontMetrics) instead.")]
		public float GetFontMetrics (out SKFontMetrics metrics, float scale) =>
			GetFontMetrics (out metrics);

		// Clone

		public SKPaint Clone () =>
			GetObject (SkiaApi.sk_compatpaint_clone (Handle));

		// MeasureText

		public float MeasureText (string text) =>
			GetFont ().MeasureText (text, this);

		public float MeasureText (byte[] text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		public float MeasureText (IntPtr buffer, int length) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, this);

		public float MeasureText (IntPtr buffer, IntPtr length) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, this);

		public float MeasureText (string text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		public float MeasureText (byte[] text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		public float MeasureText (IntPtr buffer, int length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, out bounds, this);

		public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, out bounds, this);

		// BreakText

		public long BreakText (string text, float maxWidth) =>
			BreakText (text, maxWidth, out _, out _);

		public long BreakText (string text, float maxWidth, out float measuredWidth) =>
			BreakText (text, maxWidth, out measuredWidth, out _);

		public long BreakText (string text, float maxWidth, out float measuredWidth, out string measuredText)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			var byteLength = (int)BreakText (bytes, maxWidth, out measuredWidth);
			if (byteLength == 0) {
				measuredText = string.Empty;
				return 0;
			}
			if (byteLength == bytes.Length) {
				measuredText = text;
				return text.Length;
			}
			measuredText = StringUtilities.GetString (bytes, 0, byteLength, TextEncoding);
			return measuredText.Length;
		}

		public long BreakText (byte[] text, float maxWidth) =>
			BreakText (text, maxWidth, out _);

		public long BreakText (byte[] text, float maxWidth, out float measuredWidth)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* t = text) {
				return BreakText ((IntPtr)t, (IntPtr)text.Length, maxWidth, out measuredWidth);
			}
		}

		public long BreakText (IntPtr buffer, int length, float maxWidth) =>
			BreakText (buffer, (IntPtr)length, maxWidth, out _);

		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth) =>
			BreakText (buffer, length, maxWidth, out _);

		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth) =>
			BreakText (buffer, (int)length, maxWidth, out measuredWidth);

		public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth)
		{
			if (buffer == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (buffer));

			fixed (float* mw = &measuredWidth) {
				return (long)SkiaApi.sk_compatpaint_break_text (Handle, (void*)buffer, length, maxWidth, mw);
			}
		}

		// GetTextPath

		public SKPath GetTextPath (string text, float x, float y) =>
			GetFont ().GetPath (text, new SKPoint (x, y));

		public SKPath GetTextPath (byte[] text, float x, float y) =>
			GetFont ().GetPath (text, TextEncoding, new SKPoint (x, y));

		public SKPath GetTextPath (IntPtr buffer, int length, float x, float y) =>
			GetFont ().GetPath (buffer, length, TextEncoding, new SKPoint (x, y));

		public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y) =>
			GetFont ().GetPath (buffer, (int)length, TextEncoding, new SKPoint (x, y));

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public SKPath GetTextPath (string text, SKPoint[] points) =>
			GetFont ().GetPath (text, points);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public SKPath GetTextPath (byte[] text, SKPoint[] points) =>
			GetFont ().GetPath (text, TextEncoding, points);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points) =>
			GetFont ().GetPath (buffer, length, TextEncoding, points);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points) =>
			GetFont ().GetPath (buffer, (int)length, TextEncoding, points);

		// GetFillPath

		public SKPath GetFillPath (SKPath src)
			=> GetFillPath (src, 1f);

		public SKPath GetFillPath (SKPath src, float resScale)
		{
			var dst = new SKPath ();

			if (GetFillPath (src, dst, resScale)) {
				return dst;
			} else {
				dst.Dispose ();
				return null;
			}
		}

		public SKPath GetFillPath (SKPath src, SKRect cullRect)
			=> GetFillPath (src, cullRect, 1f);

		public SKPath GetFillPath (SKPath src, SKRect cullRect, float resScale)
		{
			var dst = new SKPath ();

			if (GetFillPath (src, dst, cullRect, resScale)) {
				return dst;
			} else {
				dst.Dispose ();
				return null;
			}
		}

		public bool GetFillPath (SKPath src, SKPath dst)
			=> GetFillPath (src, dst, 1f);

		public bool GetFillPath (SKPath src, SKPath dst, float resScale)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, null, resScale);
		}

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect)
			=> GetFillPath (src, dst, cullRect, 1f);

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, &cullRect, resScale);
		}

		// CountGlyphs

		public int CountGlyphs (string text) =>
			GetFont ().CountGlyphs (text);

		public int CountGlyphs (byte[] text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		public int CountGlyphs (IntPtr text, int length) =>
			GetFont ().CountGlyphs (text, length, TextEncoding);

		public int CountGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().CountGlyphs (text, (int)length, TextEncoding);

		// GetGlyphs

		public ushort[] GetGlyphs (string text) =>
			GetFont ().GetGlyphs (text);

		public ushort[] GetGlyphs (byte[] text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		public ushort[] GetGlyphs (IntPtr text, int length) =>
			GetFont ().GetGlyphs (text, length, TextEncoding);

		public ushort[] GetGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphs (text, (int)length, TextEncoding);

		// ContainsGlyphs

		public bool ContainsGlyphs (string text) =>
			GetFont ().ContainsGlyphs (text);

		public bool ContainsGlyphs (byte[] text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		public bool ContainsGlyphs (IntPtr text, int length) =>
			GetFont ().ContainsGlyphs (text, length, TextEncoding);

		public bool ContainsGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().ContainsGlyphs (text, (int)length, TextEncoding);

		// GetGlyphWidths

		public float[] GetGlyphWidths (string text) =>
			GetFont ().GetGlyphWidths (text, this);

		public float[] GetGlyphWidths (byte[] text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		public float[] GetGlyphWidths (IntPtr text, int length) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, this);

		public float[] GetGlyphWidths (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, this);

		public float[] GetGlyphWidths (string text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, out bounds, this);

		public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, out bounds, this);

		// GetTextIntercepts

		public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text, (int)length, x, y, upperBounds, lowerBounds);

		public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, length, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetTextIntercepts (SKTextBlob)

		public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			return text.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetPositionedTextIntercepts

		public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text, (IntPtr)length, positions, upperBounds, lowerBounds);


		public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, (int)length, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetHorizontalTextIntercepts

		public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text, (IntPtr)length, xpositions, y, upperBounds, lowerBounds);

		public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, (int)length, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// Font

		public SKFont ToFont () =>
			SKFont.GetObject (SkiaApi.sk_compatpaint_make_font (Handle));

		internal SKFont GetFont () =>
			OwnedBy (font ??= SKFont.GetObject (SkiaApi.sk_compatpaint_get_font (Handle), false), this);

		//

		internal static SKPaint GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKPaint (h, o));
	}
}
