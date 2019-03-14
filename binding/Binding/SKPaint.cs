using System;

namespace SkiaSharp
{
	public class SKPaint : SKObject
	{
		[Preserve]
		internal SKPaint (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPaint ()
			: this (SkiaApi.sk_paint_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_paint_delete (Handle);
			}

			base.Dispose (disposing);
		}

		public void Reset ()
		{
			SkiaApi.sk_paint_reset (Handle);
		}

		public bool IsAntialias {
			get => SkiaApi.sk_paint_is_antialias (Handle);
			set => SkiaApi.sk_paint_set_antialias (Handle, value);
		}

		public bool IsDither {
			get => SkiaApi.sk_paint_is_dither (Handle);
			set => SkiaApi.sk_paint_set_dither (Handle, value);
		}

		public bool IsVerticalText {
			get => SkiaApi.sk_paint_is_verticaltext (Handle);
			set => SkiaApi.sk_paint_set_verticaltext (Handle, value);
		}

		public bool IsLinearText {
			get => SkiaApi.sk_paint_is_linear_text (Handle);
			set => SkiaApi.sk_paint_set_linear_text (Handle, value);
		}

		public bool SubpixelText {
			get => SkiaApi.sk_paint_is_subpixel_text (Handle);
			set => SkiaApi.sk_paint_set_subpixel_text (Handle, value);
		}

		public bool LcdRenderText {
			get => SkiaApi.sk_paint_is_lcd_render_text (Handle);
			set => SkiaApi.sk_paint_set_lcd_render_text (Handle, value);
		}

		public bool IsEmbeddedBitmapText {
			get => SkiaApi.sk_paint_is_embedded_bitmap_text (Handle);
			set => SkiaApi.sk_paint_set_embedded_bitmap_text (Handle, value);
		}

		public bool IsAutohinted {
			get => SkiaApi.sk_paint_is_autohinted (Handle);
			set => SkiaApi.sk_paint_set_autohinted (Handle, value);
		}

		public SKPaintHinting HintingLevel {
			get => SkiaApi.sk_paint_get_hinting (Handle);
			set => SkiaApi.sk_paint_set_hinting (Handle, value);
		}

		public bool FakeBoldText {
			get => SkiaApi.sk_paint_is_fake_bold_text (Handle);
			set => SkiaApi.sk_paint_set_fake_bold_text (Handle, value);
		}

		public bool DeviceKerningEnabled {
			get => SkiaApi.sk_paint_is_dev_kern_text (Handle);
			set => SkiaApi.sk_paint_set_dev_kern_text (Handle, value);
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
			set => SkiaApi.sk_paint_set_color (Handle, value);
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
			get => GetObject<SKShader> (SkiaApi.sk_paint_get_shader (Handle), false);
			set => SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKMaskFilter MaskFilter {
			get => GetObject<SKMaskFilter> (SkiaApi.sk_paint_get_maskfilter (Handle), false);
			set => SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKColorFilter ColorFilter {
			get => GetObject<SKColorFilter> (SkiaApi.sk_paint_get_colorfilter (Handle), false);
			set => SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKImageFilter ImageFilter {
			get => GetObject<SKImageFilter> (SkiaApi.sk_paint_get_imagefilter (Handle), false);
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
			get => GetObject<SKTypeface> (SkiaApi.sk_paint_get_typeface (Handle), false);
			set => SkiaApi.sk_paint_set_typeface (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public float TextSize {
			get => SkiaApi.sk_paint_get_textsize (Handle);
			set => SkiaApi.sk_paint_set_textsize (Handle, value);
		}

		public SKTextAlign TextAlign {
			get => SkiaApi.sk_paint_get_text_align (Handle);
			set => SkiaApi.sk_paint_set_text_align (Handle, value);
		}

		public SKTextEncoding TextEncoding {
			get => SkiaApi.sk_paint_get_text_encoding (Handle);
			set => SkiaApi.sk_paint_set_text_encoding (Handle, value);
		}

		public float TextScaleX {
			get => SkiaApi.sk_paint_get_text_scale_x (Handle);
			set => SkiaApi.sk_paint_set_text_scale_x (Handle, value);
		}

		public float TextSkewX {
			get => SkiaApi.sk_paint_get_text_skew_x (Handle);
			set => SkiaApi.sk_paint_set_text_skew_x (Handle, value);
		}

		public SKPathEffect PathEffect {
			get => GetObject<SKPathEffect> (SkiaApi.sk_paint_get_path_effect (Handle), false);
			set => SkiaApi.sk_paint_set_path_effect (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public float FontSpacing =>
			SkiaApi.sk_paint_get_fontmetrics (Handle, IntPtr.Zero, 0);

		public SKFontMetrics FontMetrics {
			get {
				SkiaApi.sk_paint_get_fontmetrics (Handle, out var metrics, 0);
				return metrics;
			}
		}

		public float GetFontMetrics (out SKFontMetrics metrics, float scale = 0f) =>
			SkiaApi.sk_paint_get_fontmetrics (Handle, out metrics, scale);

		public SKPaint Clone () =>
			GetObject<SKPaint> (SkiaApi.sk_paint_clone (Handle));

		// MeasureText

		public float MeasureText (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return MeasureText (bytes);
		}

		public float MeasureText (byte[] text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				fixed (byte* t = text) {
					return MeasureText ((IntPtr)t, (IntPtr)text.Length);
				}
			}
		}

		public float MeasureText (IntPtr buffer, int length) =>
			MeasureText (buffer, (IntPtr)length);

		public float MeasureText (IntPtr buffer, IntPtr length)
		{
			if (buffer == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return SkiaApi.sk_paint_measure_text (Handle, buffer, length, IntPtr.Zero);
		}

		public float MeasureText (string text, ref SKRect bounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return MeasureText (bytes, ref bounds);
		}

		public float MeasureText (byte[] text, ref SKRect bounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				fixed (byte* t = text) {
					return MeasureText ((IntPtr)t, (IntPtr)text.Length, ref bounds);
				}
			}
		}

		public float MeasureText (IntPtr buffer, int length, ref SKRect bounds) =>
			MeasureText (buffer, (IntPtr)length, ref bounds);

		public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds)
		{
			if (buffer == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return SkiaApi.sk_paint_measure_text (Handle, buffer, length, ref bounds);
		}

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

			unsafe {
				fixed (byte* t = text) {
					return BreakText ((IntPtr)t, (IntPtr)text.Length, maxWidth, out measuredWidth);
				}
			}
		}

		public long BreakText (IntPtr buffer, int length, float maxWidth) =>
			BreakText (buffer, (IntPtr)length, maxWidth, out _);

		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth) =>
			BreakText (buffer, length, maxWidth, out _);

		public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth) =>
			BreakText (buffer, (IntPtr)length, maxWidth, out measuredWidth);

		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth)
		{
			if (buffer == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return (long)SkiaApi.sk_paint_break_text (Handle, buffer, length, maxWidth, out measuredWidth);
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

			unsafe {
				fixed (byte* t = text) {
					return GetTextPath ((IntPtr)t, (IntPtr)text.Length, x, y);
				}
			}
		}

		public SKPath GetTextPath (IntPtr buffer, int length, float x, float y) =>
			GetTextPath (buffer, (IntPtr)length, x, y);

		public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y)
		{
			if (buffer == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return GetObject<SKPath> (SkiaApi.sk_paint_get_text_path (Handle, buffer, length, x, y));
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

			unsafe {
				fixed (byte* t = text) {
					return GetTextPath ((IntPtr)t, (IntPtr)text.Length, points);
				}
			}
		}

		public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points) =>
			GetTextPath (buffer, (IntPtr)length, points);

		public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points)
		{
			if (buffer == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return GetObject<SKPath> (SkiaApi.sk_paint_get_pos_text_path (Handle, buffer, length, points));
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

			return SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, IntPtr.Zero, resScale);
		}

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect)
			=> GetFillPath (src, dst, cullRect, 1f);

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, ref cullRect, resScale);
		}

		// CountGlyphs

		public int CountGlyphs (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return CountGlyphs (bytes);
		}

		public int CountGlyphs (byte[] text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				fixed (byte* p = text) {
					return CountGlyphs ((IntPtr)p, (IntPtr)text.Length);
				}
			}
		}

		public int CountGlyphs (IntPtr text, int length) =>
			CountGlyphs (text, (IntPtr)length);

		public int CountGlyphs (IntPtr text, IntPtr length)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			return SkiaApi.sk_paint_count_text (Handle, text, length);
		}

		// GetGlyphs

		public ushort[] GetGlyphs (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return GetGlyphs (bytes);
		}

		public ushort[] GetGlyphs (byte[] text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				fixed (byte* p = text) {
					return GetGlyphs ((IntPtr)p, (IntPtr)text.Length);
				}
			}
		}

		public ushort[] GetGlyphs (IntPtr text, int length) =>
			GetGlyphs (text, (IntPtr)length);

		public ushort[] GetGlyphs (IntPtr text, IntPtr length)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				var n = SkiaApi.sk_paint_text_to_glyphs (Handle, text, length, (ushort*)IntPtr.Zero);

				if (n <= 0) {
					return new ushort[0];
				}

				var glyphs = new ushort[n];
				fixed (ushort* gp = glyphs) {
					SkiaApi.sk_paint_text_to_glyphs (Handle, text, length, gp);
				}
				return glyphs;
			}
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

			unsafe {
				fixed (byte* p = text) {
					return ContainsGlyphs ((IntPtr)p, (IntPtr)text.Length);
				}
			}
		}

		public bool ContainsGlyphs (IntPtr text, int length) =>
			ContainsGlyphs (text, (IntPtr)length);

		public bool ContainsGlyphs (IntPtr text, IntPtr length)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			return SkiaApi.sk_paint_contains_text (Handle, text, length);
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

			unsafe {
				fixed (byte* p = text) {
					return GetGlyphWidths ((IntPtr)p, (IntPtr)text.Length);
				}
			}
		}

		public float[] GetGlyphWidths (IntPtr text, int length) =>
			GetGlyphWidths (text, (IntPtr)length);

		public float[] GetGlyphWidths (IntPtr text, IntPtr length)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				var n = SkiaApi.sk_paint_get_text_widths (Handle, text, length, (float*)IntPtr.Zero, (SKRect*)IntPtr.Zero);

				if (n <= 0) {
					return new float[0];
				}

				var widths = new float[n];
				fixed (float* wp = widths) {
					SkiaApi.sk_paint_get_text_widths (Handle, text, length, wp, (SKRect*)IntPtr.Zero);
				}
				return widths;
			}
		}

		public float[] GetGlyphWidths (string text, out SKRect[] bounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, TextEncoding);
			return GetGlyphWidths (bytes, out bounds);
		}

		public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				fixed (byte* p = text) {
					return GetGlyphWidths ((IntPtr)p, (IntPtr)text.Length, out bounds);
				}
			}
		}

		public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds) =>
			GetGlyphWidths (text, (IntPtr)length, out bounds);

		public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				var n = SkiaApi.sk_paint_get_text_widths (Handle, text, length, (float*)IntPtr.Zero, (SKRect*)IntPtr.Zero);

				if (n <= 0) {
					bounds = new SKRect[0];
					return new float[0];
				}

				var widths = new float[n];
				bounds = new SKRect[n];
				fixed (float* wp = widths)
				fixed (SKRect* bp = bounds) {
					SkiaApi.sk_paint_get_text_widths (Handle, text, length, wp, bp);
				}
				return widths;
			}
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

			unsafe {
				fixed (byte* p = text) {
					return GetTextIntercepts ((IntPtr)p, (IntPtr)text.Length, x, y, upperBounds, lowerBounds);
				}
			}
		}

		public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text, (IntPtr)length, x, y, upperBounds, lowerBounds);


		public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				var bounds = new[] { upperBounds, lowerBounds };

				var n = SkiaApi.sk_paint_get_text_intercepts (Handle, text, length, x, y, bounds, (float*)IntPtr.Zero);

				if (n <= 0) {
					return new float[0];
				}

				var intervals = new float[n];
				fixed (float* ip = intervals) {
					SkiaApi.sk_paint_get_text_intercepts (Handle, text, length, x, y, bounds, ip);
				}
				return intervals;
			}
		}

		// GetTextIntercepts (SKTextBlob)

		public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				var bounds = new[] { upperBounds, lowerBounds };

				var n = SkiaApi.sk_paint_get_pos_text_blob_intercepts (Handle, text.Handle, bounds, (float*)IntPtr.Zero);

				if (n <= 0) {
					return new float[0];
				}

				var intervals = new float[n];
				fixed (float* ip = intervals) {
					SkiaApi.sk_paint_get_pos_text_blob_intercepts (Handle, text.Handle, bounds, ip);
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

			unsafe {
				fixed (byte* p = text) {
					return GetPositionedTextIntercepts ((IntPtr)p, (IntPtr)text.Length, positions, upperBounds, lowerBounds);
				}
			}
		}

		public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text, (IntPtr)length, positions, upperBounds, lowerBounds);


		public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				var bounds = new[] { upperBounds, lowerBounds };

				var n = SkiaApi.sk_paint_get_pos_text_intercepts (Handle, text, length, positions, bounds, (float*)IntPtr.Zero);

				if (n <= 0) {
					return new float[0];
				}

				var intervals = new float[n];
				fixed (float* ip = intervals) {
					SkiaApi.sk_paint_get_pos_text_intercepts (Handle, text, length, positions, bounds, ip);
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

			unsafe {
				fixed (byte* p = text) {
					return GetHorizontalTextIntercepts ((IntPtr)p, (IntPtr)text.Length, xpositions, y, upperBounds, lowerBounds);
				}
			}
		}

		public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text, (IntPtr)length, xpositions, y, upperBounds, lowerBounds);

		public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				var bounds = new[] { upperBounds, lowerBounds };

				var n = SkiaApi.sk_paint_get_pos_text_h_intercepts (Handle, text, length, xpositions, y, bounds, (float*)IntPtr.Zero);

				if (n <= 0) {
					return new float[0];
				}

				var intervals = new float[n];
				fixed (float* ip = intervals) {
					SkiaApi.sk_paint_get_pos_text_h_intercepts (Handle, text, length, xpositions, y, bounds, ip);
				}
				return intervals;
			}
		}
	}
}
