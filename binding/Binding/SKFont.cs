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

		public ushort GetGlyph (int codepoint) =>
			SkiaApi.sk_font_unichar_to_glyph (Handle, codepoint);

		// GetGlyphs

		public ushort[] GetGlyphs (ReadOnlySpan<int> codepoints)
		{
			var glyphs = new ushort[codepoints.Length];
			GetGlyphs (codepoints, glyphs);
			return glyphs;
		}

		public void GetGlyphs (ReadOnlySpan<int> codepoints, Span<ushort> glyphs)
		{
			fixed (int* up = codepoints)
			fixed (ushort* gp = glyphs) {
				SkiaApi.sk_font_unichars_to_glyphs (Handle, up, codepoints.Length, gp);
			}
		}

		// GetGlyphs

		public ushort[] GetGlyphs (string text) =>
			GetGlyphs (text.AsSpan ());

		public ushort[] GetGlyphs (ReadOnlySpan<char> text)
		{
			var n = CountGlyphs (text);
			if (n <= 0)
				return new ushort[0];

			var glyphs = new ushort[n];
			GetGlyphs (text, glyphs);
			return glyphs;
		}

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			var n = CountGlyphs (text, encoding);
			if (n <= 0)
				return new ushort[0];

			var glyphs = new ushort[n];
			GetGlyphs (text, encoding, glyphs);
			return glyphs;
		}

		public void GetGlyphs (string text, Span<ushort> glyphs) =>
			GetGlyphs (text.AsSpan (), glyphs);

		public void GetGlyphs (ReadOnlySpan<char> text, Span<ushort> glyphs)
		{
			fixed (void* p = text)
			fixed (ushort* gp = glyphs) {
				SkiaApi.sk_font_text_to_glyphs (Handle, p, (IntPtr)text.Length, SKTextEncoding.Utf16, gp, glyphs.Length);
			}
		}

		public void GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<ushort> glyphs)
		{
			fixed (void* p = text)
			fixed (ushort* gp = glyphs) {
				SkiaApi.sk_font_text_to_glyphs (Handle, p, (IntPtr)text.Length, encoding, gp, glyphs.Length);
			}
		}

		// CountGlyphs

		public int CountGlyphs (string text) =>
			CountGlyphs (text.AsSpan ());

		public int CountGlyphs (ReadOnlySpan<char> text)
		{
			fixed (void* p = text) {
				return SkiaApi.sk_font_text_to_glyphs (Handle, p, (IntPtr)text.Length, SKTextEncoding.Utf16, null, 0);
			}
		}

		public int CountGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			fixed (void* p = text) {
				return SkiaApi.sk_font_text_to_glyphs (Handle, p, (IntPtr)text.Length, encoding, null, 0);
			}
		}

		// MeasureText

		public float MeasureText (string text, SKPaint paint = null) =>
			MeasureText (text.AsSpan (), paint);

		public float MeasureText (ReadOnlySpan<char> text, SKPaint paint = null)
		{
			fixed (void* t = text) {
				return SkiaApi.sk_font_measure_text (Handle, t, (IntPtr)text.Length, SKTextEncoding.Utf16, null, paint?.Handle ?? IntPtr.Zero);
			}
		}

		public float MeasureText (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint = null)
		{
			fixed (void* t = text) {
				return SkiaApi.sk_font_measure_text (Handle, t, (IntPtr)text.Length, encoding, null, paint?.Handle ?? IntPtr.Zero);
			}
		}

		public float MeasureText (string text, out SKRect bounds, SKPaint paint = null) =>
			MeasureText (text.AsSpan (), out bounds, paint);

		public float MeasureText (ReadOnlySpan<char> text, out SKRect bounds, SKPaint paint = null)
		{
			fixed (SKRect* b = &bounds)
			fixed (void* t = text) {
				return SkiaApi.sk_font_measure_text (Handle, t, (IntPtr)text.Length, SKTextEncoding.Utf16, b, paint?.Handle ?? IntPtr.Zero);
			}
		}

		public float MeasureText (ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect bounds, SKPaint paint = null)
		{
			fixed (SKRect* b = &bounds)
			fixed (void* t = text) {
				return SkiaApi.sk_font_measure_text (Handle, t, (IntPtr)text.Length, encoding, b, paint?.Handle ?? IntPtr.Zero);
			}
		}

		// GetGlyphPositions

		public SKPoint[] GetGlyphPositions (ReadOnlySpan<ushort> glyphs)
		{
			var positions = new SKPoint[glyphs.Length];
			GetGlyphPositions (glyphs, positions);
			return positions;
		}

		public SKPoint[] GetGlyphPositions (ReadOnlySpan<ushort> glyphs, SKPoint origin)
		{
			var positions = new SKPoint[glyphs.Length];
			GetGlyphPositions (glyphs, positions, origin);
			return positions;
		}

		public void GetGlyphPositions (ReadOnlySpan<ushort> glyphs, Span<SKPoint> positions) =>
			GetGlyphPositions (glyphs, positions, SKPoint.Empty);

		public void GetGlyphPositions (ReadOnlySpan<ushort> glyphs, Span<SKPoint> positions, SKPoint origin)
		{
			if (glyphs.Length != positions.Length)
				throw new ArgumentException ("The length of glyphs must be the same as the length of positions.", nameof (positions));

			fixed (ushort* gp = glyphs)
			fixed (SKPoint* pp = positions) {
				SkiaApi.sk_font_get_pos (Handle, gp, glyphs.Length, pp, &origin);
			}
		}

		// GetGlyphOffsets

		public float[] GetGlyphOffsets (ReadOnlySpan<ushort> glyphs)
		{
			var offsets = new float[glyphs.Length];
			GetGlyphOffsets (glyphs, offsets);
			return offsets;
		}

		public float[] GetGlyphOffsets (ReadOnlySpan<ushort> glyphs, float origin)
		{
			var offsets = new float[glyphs.Length];
			GetGlyphOffsets (glyphs, offsets, origin);
			return offsets;
		}

		public void GetGlyphOffsets (ReadOnlySpan<ushort> glyphs, Span<float> offsets) =>
			GetGlyphOffsets (glyphs, offsets, 0);

		public void GetGlyphOffsets (ReadOnlySpan<ushort> glyphs, Span<float> offsets, float origin)
		{
			if (glyphs.Length != offsets.Length)
				throw new ArgumentException ("The length of glyphs must be the same as the length of offsets.", nameof (offsets));

			fixed (ushort* gp = glyphs)
			fixed (float* pp = offsets) {
				SkiaApi.sk_font_get_xpos (Handle, gp, glyphs.Length, pp, origin);
			}
		}

		// GetGlyphWidths

		public float[] GetGlyphWidths (ReadOnlySpan<ushort> glyphs, SKPaint paint = null)
		{
			var widths = new float[glyphs.Length];
			GetGlyphWidths (glyphs, widths, paint);
			return widths;
		}

		public float[] GetGlyphWidths (ReadOnlySpan<ushort> glyphs, out SKRect[] bounds, SKPaint paint = null)
		{
			var widths = new float[glyphs.Length];
			bounds = new SKRect[glyphs.Length];
			GetGlyphWidths (glyphs, widths, bounds, paint);
			return widths;
		}

		public void GetGlyphWidths (ReadOnlySpan<ushort> glyphs, Span<SKRect> bounds, SKPaint paint = null) =>
			GetGlyphWidths (glyphs, Span<float>.Empty, bounds, paint);

		public void GetGlyphWidths (ReadOnlySpan<ushort> glyphs, Span<float> widths, SKPaint paint = null) =>
			GetGlyphWidths (glyphs, widths, Span<SKRect>.Empty, paint);

		public void GetGlyphWidths (ReadOnlySpan<ushort> glyphs, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null)
		{
			fixed (ushort* gp = glyphs)
			fixed (float* wp = widths)
			fixed (SKRect* bp = bounds) {
				var w = widths.Length > 0 ? wp : null;
				var b = bounds.Length > 0 ? bp : null;
				SkiaApi.sk_font_get_widths_bounds (Handle, gp, glyphs.Length, w, b, paint?.Handle ?? IntPtr.Zero);
			}
		}

		// GetPath

		public SKPath GetPath (ushort glyph)
		{
			var path = new SKPath ();
			if (!SkiaApi.sk_font_get_path (Handle, glyph, path.Handle)) {
				path.Dispose ();
				path = null;
			}
			return path;
		}

		// GetPaths

		public void GetPaths (ReadOnlySpan<ushort> glyphs, SKGlyphPathDelegate glyphPathDelegate)
		{
			var proxy = DelegateProxies.Create (glyphPathDelegate, DelegateProxies.SKGlyphPathDelegateProxy, out var gch, out var ctx);
			try {
				fixed (ushort* g = glyphs) {
					SkiaApi.sk_font_get_paths (Handle, g, glyphs.Length, proxy, (void*)ctx);
				}
			} finally {
				gch.Free ();
			}
		}
	}
}
