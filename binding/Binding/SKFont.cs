using System;

namespace SkiaSharp
{
	public unsafe class SKFont : SKObject
	{
		private const float DefaultSize = 12f;
		private const float DefaultScaleX = 1f;
		private const float DefaultSkewX = 0f;

		[Preserve]
		internal SKFont (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKFont ()
			: this (SkiaApi.sk_font_new (), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKFont instance.");
		}

		public SKFont (SKTypeface typeface)
			: this (typeface, DefaultSize, DefaultScaleX, DefaultSkewX)
		{
		}

		public SKFont (SKTypeface typeface, float size)
			: this (typeface, size, DefaultScaleX, DefaultSkewX)
		{
		}

		public SKFont (SKTypeface typeface, float size, float scaleX, float skewX)
			: this (SkiaApi.sk_font_new_with_values (typeface?.Handle ?? IntPtr.Zero, size, scaleX, skewX), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKFont instance.");
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_font_delete (Handle);

		public bool ForceAutoHinting {
			get => SkiaApi.sk_font_is_force_auto_hinting (Handle);
			set => SkiaApi.sk_font_set_force_auto_hinting (Handle, value);
		}

		public bool EmbeddedBitmaps {
			get => SkiaApi.sk_font_is_embedded_bitmaps (Handle);
			set => SkiaApi.sk_font_set_embedded_bitmaps (Handle, value);
		}

		public bool Subpixel {
			get => SkiaApi.sk_font_is_subpixel (Handle);
			set => SkiaApi.sk_font_set_subpixel (Handle, value);
		}

		public bool LinearMetrics {
			get => SkiaApi.sk_font_is_linear_metrics (Handle);
			set => SkiaApi.sk_font_set_linear_metrics (Handle, value);
		}

		public SKFontHinting HintingLevel {
			get => SkiaApi.sk_font_get_hinting (Handle);
			set => SkiaApi.sk_font_set_hinting (Handle, value);
		}

		public bool Embolden {
			get => SkiaApi.sk_font_is_embolden (Handle);
			set => SkiaApi.sk_font_set_embolden (Handle, value);
		}

		public bool BaselineSnap {
			get => SkiaApi.sk_font_is_baseline_snap (Handle);
			set => SkiaApi.sk_font_set_baseline_snap (Handle, value);
		}

		public SKFontEdging Edging {
			get => SkiaApi.sk_font_get_edging (Handle);
			set => SkiaApi.sk_font_set_edging (Handle, value);
		}

		public SKFontHinting Hinting {
			get => SkiaApi.sk_font_get_hinting (Handle);
			set => SkiaApi.sk_font_set_hinting (Handle, value);
		}

		public SKTypeface Typeface {
			get => GetObject<SKTypeface> (SkiaApi.sk_font_get_typeface (Handle));
			set => SkiaApi.sk_font_set_typeface (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public float Size {
			get => SkiaApi.sk_font_get_size (Handle);
			set => SkiaApi.sk_font_set_size (Handle, value);
		}

		public float ScaleX {
			get => SkiaApi.sk_font_get_scale_x (Handle);
			set => SkiaApi.sk_font_set_scale_x (Handle, value);
		}

		public float SkewX {
			get => SkiaApi.sk_font_get_skew_x (Handle);
			set => SkiaApi.sk_font_set_skew_x (Handle, value);
		}

		// FontMetrics

		public float Spacing =>
			SkiaApi.sk_font_get_metrics (Handle, null);

		public SKFontMetrics Metrics {
			get {
				GetFontMetrics (out var metrics);
				return metrics;
			}
		}

		public float GetFontMetrics (out SKFontMetrics metrics)
		{
			fixed (SKFontMetrics* m = &metrics) {
				return SkiaApi.sk_font_get_metrics (Handle, m);
			}
		}

		// GetGlyph

		public ushort GetGlyph (int utf32) =>
			SkiaApi.sk_font_unichar_to_glyph (Handle, utf32);

		// GetGlyphs

		public ushort[] GetGlyphs (int[] utf32) =>
			GetGlyphs ((ReadOnlySpan<int>)utf32);

		public ushort[] GetGlyphs (ReadOnlySpan<int> utf32)
		{
			var glyphs = new ushort[utf32.Length];
			fixed (int* up = utf32)
			fixed (ushort* gp = glyphs) {
				SkiaApi.sk_font_unichars_to_glyphs (Handle, up, utf32.Length, gp);
			}
			return glyphs;
		}

		// GetGlyphs

		public ushort[] GetGlyphs (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (char* p = text) {
				return GetGlyphs ((IntPtr)p, (IntPtr)text.Length, SKTextEncoding.Utf16);
			}
		}

		public ushort[] GetGlyphs (ReadOnlySpan<char> text)
		{
			fixed (char* p = text) {
				return GetGlyphs ((IntPtr)p, (IntPtr)text.Length, SKTextEncoding.Utf16);
			}
		}

		public ushort[] GetGlyphs (byte[] text, SKTextEncoding encoding) =>
			GetGlyphs ((ReadOnlySpan<byte>)text, encoding);

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			fixed (byte* p = text) {
				return GetGlyphs ((IntPtr)p, (IntPtr)text.Length, encoding);
			}
		}

		public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			GetGlyphs (text, (IntPtr)length, encoding);

		public ushort[] GetGlyphs (IntPtr text, IntPtr length, SKTextEncoding encoding)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			var n = SkiaApi.sk_font_text_to_glyphs (Handle, (void*)text, length, encoding, null, 0);
			if (n <= 0)
				return new ushort[0];

			var glyphs = new ushort[n];
			fixed (ushort* gp = glyphs) {
				SkiaApi.sk_font_text_to_glyphs (Handle, (void*)text, length, encoding, gp, n);
			}
			return glyphs;
		}

		// CountGlyphs

		public int CountGlyphs (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (char* p = text) {
				return CountGlyphs ((IntPtr)p, (IntPtr)text.Length, SKTextEncoding.Utf16);
			}
		}

		public int CountGlyphs (ReadOnlySpan<char> text)
		{
			fixed (char* p = text) {
				return CountGlyphs ((IntPtr)p, (IntPtr)text.Length, SKTextEncoding.Utf16);
			}
		}

		public int CountGlyphs (byte[] text, SKTextEncoding encoding) =>
			CountGlyphs ((ReadOnlySpan<byte>)text, encoding);

		public int CountGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			fixed (byte* p = text) {
				return CountGlyphs ((IntPtr)p, (IntPtr)text.Length, encoding);
			}
		}

		public int CountGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			CountGlyphs (text, (IntPtr)length, encoding);

		public int CountGlyphs (IntPtr text, IntPtr length, SKTextEncoding encoding)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			return SkiaApi.sk_font_text_to_glyphs (Handle, (void*)text, length, encoding, null, 0);
		}

		// MeasureText

		public float MeasureText (string text) =>
			MeasureText (text, null);

		public float MeasureText (ReadOnlySpan<char> text) =>
			MeasureText (text, null);

		public float MeasureText (byte[] text, SKTextEncoding encoding) =>
			MeasureText (text, encoding, null);

		public float MeasureText (ReadOnlySpan<byte> text, SKTextEncoding encoding) =>
			MeasureText (text, encoding, null);

		public float MeasureText (IntPtr text, int length, SKTextEncoding encoding) =>
			MeasureText (text, length, encoding, null);

		public float MeasureText (IntPtr text, IntPtr length, SKTextEncoding encoding) =>
			MeasureText (text, length, encoding, null);

		public float MeasureText (string text, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (char* p = text) {
				return MeasureText ((IntPtr)p, (IntPtr)text.Length, SKTextEncoding.Utf16, paint);
			}
		}

		public float MeasureText (ReadOnlySpan<char> text, SKPaint paint)
		{
			fixed (char* t = text) {
				return MeasureText ((IntPtr)t, (IntPtr)text.Length, SKTextEncoding.Utf16, paint);
			}
		}

		public float MeasureText (byte[] text, SKTextEncoding encoding, SKPaint paint) =>
			MeasureText ((ReadOnlySpan<byte>)text, encoding, paint);

		public float MeasureText (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint)
		{
			fixed (byte* t = text) {
				return MeasureText ((IntPtr)t, (IntPtr)text.Length, encoding, paint);
			}
		}

		public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, SKPaint paint) =>
			MeasureText (text, (IntPtr)length, encoding, paint);

		public float MeasureText (IntPtr text, IntPtr length, SKTextEncoding encoding, SKPaint paint)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			return SkiaApi.sk_font_measure_text (Handle, (void*)text, length, encoding, null, paint?.Handle ?? IntPtr.Zero);
		}

		public float MeasureText (string text, out SKRect bounds) =>
			MeasureText (text, out bounds, null);

		public float MeasureText (ReadOnlySpan<char> text, out SKRect bounds) =>
			MeasureText (text, out bounds, null);

		public float MeasureText (byte[] text, SKTextEncoding encoding, out SKRect bounds) =>
			MeasureText (text, encoding, out bounds, null);

		public float MeasureText (ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect bounds) =>
			MeasureText (text, encoding, out bounds, null);

		public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, out SKRect bounds) =>
			MeasureText (text, length, encoding, out bounds, null);

		public float MeasureText (IntPtr text, IntPtr length, SKTextEncoding encoding, out SKRect bounds) =>
			MeasureText (text, length, encoding, out bounds, null);

		public float MeasureText (string text, out SKRect bounds, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (char* p = text) {
				return MeasureText ((IntPtr)p, (IntPtr)text.Length, SKTextEncoding.Utf16, out bounds, paint);
			}
		}

		public float MeasureText (ReadOnlySpan<char> text, out SKRect bounds, SKPaint paint)
		{
			fixed (char* t = text) {
				return MeasureText ((IntPtr)t, (IntPtr)text.Length, SKTextEncoding.Utf16, out bounds, paint);
			}
		}

		public float MeasureText (byte[] text, SKTextEncoding encoding, out SKRect bounds, SKPaint paint) =>
			MeasureText ((ReadOnlySpan<byte>)text, encoding, out bounds, paint);

		public float MeasureText (ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect bounds, SKPaint paint)
		{
			fixed (byte* t = text) {
				return MeasureText ((IntPtr)t, (IntPtr)text.Length, encoding, out bounds, paint);
			}
		}

		public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, out SKRect bounds, SKPaint paint) =>
			MeasureText (text, (IntPtr)length, encoding, out bounds, paint);

		public float MeasureText (IntPtr text, IntPtr length, SKTextEncoding encoding, out SKRect bounds, SKPaint paint)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			fixed (SKRect* b = &bounds) {
				return SkiaApi.sk_font_measure_text (Handle, (void*)text, length, encoding, b, paint?.Handle ?? IntPtr.Zero);
			}
		}

		// GetGlyphWidths

		public float[] GetGlyphWidths (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return GetGlyphWidths (bytes);
		}

		public float[] GetGlyphWidths (byte[] text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* p = text) {
				return GetGlyphWidths ((IntPtr)p, (IntPtr)text.Length);
			}
		}

		public float[] GetGlyphWidths (IntPtr text, int length) =>
			GetGlyphWidths (text, (IntPtr)length);

		public float[] GetGlyphWidths (IntPtr text, IntPtr length)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			var n = SkiaApi.sk_font_get_text_widths (Handle, (void*)text, length, null, null);

			if (n <= 0) {
				return new float[0];
			}

			var widths = new float[n];
			fixed (float* wp = widths) {
				SkiaApi.sk_font_get_text_widths (Handle, (void*)text, length, wp, null);
			}
			return widths;
		}

		public float[] GetGlyphWidths (string text, out SKRect[] bounds, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var glyphs = GetGlyphs (text);
			return GetGlyphWidths (glyphs, out bounds, paint);
		}

		public float[] GetGlyphWidths (ushort[] glyphs, out SKRect[] bounds, SKPaint paint) =>
			GetGlyphWidths ((ReadOnlySpan<ushort>)glyphs, out bounds, paint);

		public float[] GetGlyphWidths (ReadOnlySpan<ushort> glyphs, out SKRect[] bounds, SKPaint paint)
		{
			var widths = new float[glyphs.Length];
			bounds = new SKRect[glyphs.Length];
			fixed (ushort* gp = glyphs)
			fixed (float* wp = widths)
			fixed (SKRect* bp = bounds) {
				SkiaApi.sk_font_get_widths_bounds (Handle, gp, glyphs.Length, wp, bp, paint?.Handle ?? IntPtr.Zero);
			}
			return widths;
		}

		// GetTextPath

		public SKPath GetTextPath (string text, float x, float y)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return GetTextPath (bytes, x, y);
		}

		public SKPath GetTextPath (byte[] text, float x, float y)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* t = text) {
				return GetTextPath ((IntPtr)t, (IntPtr)text.Length, x, y);
			}
		}

		public SKPath GetTextPath (IntPtr buffer, int length, float x, float y) =>
			GetTextPath (buffer, (IntPtr)length, x, y);

		public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y)
		{
			if (buffer == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return GetObject<SKPath> (SkiaApi.sk_font_get_text_path (Handle, (void*)buffer, length, x, y));
		}

		public SKPath GetTextPath (string text, SKPoint[] points)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return GetTextPath (bytes, points);
		}

		public SKPath GetTextPath (byte[] text, SKPoint[] points)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* t = text) {
				return GetTextPath ((IntPtr)t, (IntPtr)text.Length, points);
			}
		}

		public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points) =>
			GetTextPath (buffer, (IntPtr)length, points);

		public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points)
		{
			if (buffer == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			fixed (SKPoint* p = points) {
				return GetObject<SKPath> (SkiaApi.sk_font_get_pos_text_path (Handle, (void*)buffer, length, p));
			}
		}

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

			return SkiaApi.sk_font_get_fill_path (Handle, src.Handle, dst.Handle, null, resScale);
		}

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect)
			=> GetFillPath (src, dst, cullRect, 1f);

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_font_get_fill_path (Handle, src.Handle, dst.Handle, &cullRect, resScale);
		}

		// ContainsGlyphs

		public bool ContainsGlyphs (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return ContainsGlyphs (bytes);
		}

		public bool ContainsGlyphs (byte[] text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* p = text) {
				return ContainsGlyphs ((IntPtr)p, (IntPtr)text.Length);
			}
		}

		public bool ContainsGlyphs (IntPtr text, int length) =>
			ContainsGlyphs (text, (IntPtr)length);

		public bool ContainsGlyphs (IntPtr text, IntPtr length)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			return SkiaApi.sk_font_contains_text (Handle, (void*)text, length);
		}

		// GetTextIntercepts

		public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return GetTextIntercepts (bytes, x, y, upperBounds, lowerBounds);
		}

		public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* p = text) {
				return GetTextIntercepts ((IntPtr)p, (IntPtr)text.Length, x, y, upperBounds, lowerBounds);
			}
		}

		public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text, (IntPtr)length, x, y, upperBounds, lowerBounds);

		public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			var bounds = new[] { upperBounds, lowerBounds };

			fixed (float* b = bounds) {
				var n = SkiaApi.sk_font_get_text_intercepts (Handle, (void*)text, length, x, y, b, null);

				if (n <= 0) {
					return new float[0];
				}

				var intervals = new float[n];
				fixed (float* ip = intervals) {
					SkiaApi.sk_font_get_text_intercepts (Handle, (void*)text, length, x, y, b, ip);
				}
				return intervals;
			}
		}

		// GetTextIntercepts (SKTextBlob)

		public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bounds = new[] { upperBounds, lowerBounds };

			fixed (float* b = bounds) {
				var n = SkiaApi.sk_font_get_pos_text_blob_intercepts (Handle, text.Handle, b, null);

				if (n <= 0) {
					return new float[0];
				}

				var intervals = new float[n];
				fixed (float* ip = intervals) {
					SkiaApi.sk_font_get_pos_text_blob_intercepts (Handle, text.Handle, b, ip);
				}
				return intervals;
			}
		}

		// GetPositionedTextIntercepts

		public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return GetPositionedTextIntercepts (bytes, positions, upperBounds, lowerBounds);
		}

		public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* p = text) {
				return GetPositionedTextIntercepts ((IntPtr)p, (IntPtr)text.Length, positions, upperBounds, lowerBounds);
			}
		}

		public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text, (IntPtr)length, positions, upperBounds, lowerBounds);

		public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			var bounds = new[] { upperBounds, lowerBounds };

			fixed (float* b = bounds)
			fixed (SKPoint* p = positions) {
				var n = SkiaApi.sk_font_get_pos_text_intercepts (Handle, (void*)text, length, p, b, null);

				if (n <= 0) {
					return new float[0];
				}

				var intervals = new float[n];
				fixed (float* ip = intervals) {
					SkiaApi.sk_font_get_pos_text_intercepts (Handle, (void*)text, length, p, b, ip);
				}
				return intervals;
			}
		}

		// GetHorizontalTextIntercepts

		public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return GetHorizontalTextIntercepts (bytes, xpositions, y, upperBounds, lowerBounds);
		}

		public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* p = text) {
				return GetHorizontalTextIntercepts ((IntPtr)p, (IntPtr)text.Length, xpositions, y, upperBounds, lowerBounds);
			}
		}

		public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text, (IntPtr)length, xpositions, y, upperBounds, lowerBounds);

		public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			var bounds = new[] { upperBounds, lowerBounds };

			fixed (float* x = xpositions)
			fixed (float* b = bounds) {
				var n = SkiaApi.sk_font_get_pos_text_h_intercepts (Handle, (void*)text, length, x, y, b, null);

				if (n <= 0) {
					return new float[0];
				}

				var intervals = new float[n];
				fixed (float* ip = intervals) {
					SkiaApi.sk_font_get_pos_text_h_intercepts (Handle, (void*)text, length, x, y, b, ip);
				}
				return intervals;
			}
		}
	}
}
