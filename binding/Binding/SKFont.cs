using System;
using System.Buffers;

namespace SkiaSharp
{
	public unsafe class SKFont : SKObject
	{
		internal const float DefaultSize = 12f;
		internal const float DefaultScaleX = 1f;
		internal const float DefaultSkewX = 0f;

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

		public SKFont (SKTypeface typeface, float size = DefaultSize, float scaleX = DefaultScaleX, float skewX = DefaultSkewX)
			: this (SkiaApi.sk_font_new_with_values (typeface?.Handle ?? IntPtr.Zero, size, scaleX, skewX), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKFont instance.");
		}

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

		// FontSpacing

		public float Spacing =>
			SkiaApi.sk_font_get_metrics (Handle, null);

		// FontMetrics

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
			if (codepoints.IsEmpty)
				return;

			if (glyphs.Length != codepoints.Length)
				throw new ArgumentException ("The length of glyphs must be the same as the length of codepoints.", nameof (glyphs));

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
			fixed (void* t = text) {
				return GetGlyphs (t, text.Length * 2, SKTextEncoding.Utf16);
			}
		}

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			fixed (void* t = text) {
				return GetGlyphs (t, text.Length, encoding);
			}
		}

		public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			GetGlyphs ((void*)text, length, encoding);

		public void GetGlyphs (string text, Span<ushort> glyphs) =>
			GetGlyphs (text.AsSpan (), glyphs);

		public void GetGlyphs (ReadOnlySpan<char> text, Span<ushort> glyphs)
		{
			fixed (void* t = text) {
				GetGlyphs (t, text.Length * 2, SKTextEncoding.Utf16, glyphs);
			}
		}

		public void GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<ushort> glyphs)
		{
			fixed (void* t = text) {
				GetGlyphs (t, text.Length, encoding, glyphs);
			}
		}

		public void GetGlyphs (IntPtr text, int length, SKTextEncoding encoding, Span<ushort> glyphs) =>
			GetGlyphs ((void*)text, length, encoding, glyphs);

		internal ushort[] GetGlyphs (void* text, int length, SKTextEncoding encoding)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return new ushort[0];

			var n = CountGlyphs (text, length, encoding);
			if (n <= 0)
				return new ushort[0];

			var glyphs = new ushort[n];
			GetGlyphs (text, length, encoding, glyphs);
			return glyphs;
		}

		internal void GetGlyphs (void* text, int length, SKTextEncoding encoding, Span<ushort> glyphs)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return;

			fixed (ushort* gp = glyphs) {
				SkiaApi.sk_font_text_to_glyphs (Handle, text, (IntPtr)length, encoding, gp, glyphs.Length);
			}
		}

		// ContainsGlyph

		public bool ContainsGlyph (int codepoint) =>
			GetGlyph (codepoint) != 0;

		// ContainsGlyphs

		public bool ContainsGlyphs (ReadOnlySpan<int> codepoints) =>
			ContainsGlyphs (GetGlyphs (codepoints));

		public bool ContainsGlyphs (string text) =>
			ContainsGlyphs (GetGlyphs (text));

		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			ContainsGlyphs (GetGlyphs (text));

		public bool ContainsGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding) =>
			ContainsGlyphs (GetGlyphs (text, encoding));

		public bool ContainsGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			ContainsGlyphs (GetGlyphs (text, length, encoding));

		private bool ContainsGlyphs (ushort[] glyphs) =>
			Array.IndexOf (glyphs, 0) == -1;

		// CountGlyphs

		public int CountGlyphs (string text) =>
			CountGlyphs (text.AsSpan ());

		public int CountGlyphs (ReadOnlySpan<char> text)
		{
			fixed (void* t = text) {
				return CountGlyphs (t, text.Length * 2, SKTextEncoding.Utf16);
			}
		}

		public int CountGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			fixed (void* t = text) {
				return CountGlyphs (t, text.Length, encoding);
			}
		}

		public int CountGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			CountGlyphs ((void*)text, length, encoding);

		internal int CountGlyphs (void* text, int length, SKTextEncoding encoding)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return 0;

			return SkiaApi.sk_font_text_to_glyphs (Handle, text, (IntPtr)length, encoding, null, 0);
		}

		// MeasureText

		public float MeasureText (string text, SKPaint paint = null) =>
			MeasureText (text.AsSpan (), paint);

		public float MeasureText (ReadOnlySpan<char> text, SKPaint paint = null)
		{
			fixed (void* t = text) {
				return MeasureText (t, text.Length * 2, SKTextEncoding.Utf16, null, paint);
			}
		}

		public float MeasureText (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint = null)
		{
			fixed (void* t = text) {
				return MeasureText (t, text.Length, encoding, null, paint);
			}
		}

		public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, SKPaint paint = null) =>
			MeasureText ((void*)text, length, encoding, null, paint);

		public float MeasureText (string text, out SKRect bounds, SKPaint paint = null) =>
			MeasureText (text.AsSpan (), out bounds, paint);

		public float MeasureText (ReadOnlySpan<char> text, out SKRect bounds, SKPaint paint = null)
		{
			fixed (void* t = text)
			fixed (SKRect* b = &bounds) {
				return MeasureText (t, text.Length * 2, SKTextEncoding.Utf16, b, paint);
			}
		}

		public float MeasureText (ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect bounds, SKPaint paint = null)
		{
			fixed (void* t = text)
			fixed (SKRect* b = &bounds) {
				return MeasureText (t, text.Length, encoding, b, paint);
			}
		}

		public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, out SKRect bounds, SKPaint paint = null)
		{
			fixed (SKRect* b = &bounds) {
				return MeasureText ((void*)text, length, encoding, b, paint);
			}
		}

		internal float MeasureText (void* text, int length, SKTextEncoding encoding, SKRect* bounds, SKPaint paint)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return 0;

			return SkiaApi.sk_font_measure_text (Handle, text, (IntPtr)length, encoding, bounds, paint?.Handle ?? IntPtr.Zero);
		}

		// GetGlyphPositions (text)

		public SKPoint[] GetGlyphPositions (string text, SKPoint origin = default) =>
			GetGlyphPositions (text.AsSpan (), origin);

		public SKPoint[] GetGlyphPositions (ReadOnlySpan<char> text, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return GetGlyphPositions (t, text.Length * 2, SKTextEncoding.Utf16, origin);
			}
		}

		public SKPoint[] GetGlyphPositions (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return GetGlyphPositions (t, text.Length, encoding, origin);
			}
		}

		public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKTextEncoding encoding, SKPoint origin = default) =>
			GetGlyphPositions ((void*)text, length, encoding, origin);

		public void GetGlyphPositions (string text, Span<SKPoint> offsets, SKPoint origin = default) =>
			GetGlyphPositions (text.AsSpan (), offsets, origin);

		public void GetGlyphPositions (ReadOnlySpan<char> text, Span<SKPoint> offsets, SKPoint origin = default)
		{
			fixed (void* t = text) {
				GetGlyphPositions (t, text.Length * 2, SKTextEncoding.Utf16, offsets, origin);
			}
		}

		public void GetGlyphPositions (ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<SKPoint> offsets, SKPoint origin = default)
		{
			fixed (void* t = text) {
				GetGlyphPositions (t, text.Length, encoding, offsets, origin);
			}
		}

		public void GetGlyphPositions (IntPtr text, int length, SKTextEncoding encoding, Span<SKPoint> offsets, SKPoint origin = default) =>
			GetGlyphPositions ((void*)text, length, encoding, offsets, origin);

		internal SKPoint[] GetGlyphPositions (void* text, int length, SKTextEncoding encoding, SKPoint origin)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return new SKPoint[0];

			var n = CountGlyphs (text, length, encoding);
			if (n <= 0)
				return new SKPoint[0];

			var positions = new SKPoint[n];
			GetGlyphPositions (text, length, encoding, positions, origin);
			return positions;
		}

		internal void GetGlyphPositions (void* text, int length, SKTextEncoding encoding, Span<SKPoint> offsets, SKPoint origin)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return;

			var n = offsets.Length;
			if (n <= 0)
				return;

			var pool = ArrayPool<ushort>.Shared;
			var glyphs = pool.Rent (n);
			try {
				var span = glyphs.AsSpan (0, n);
				GetGlyphs (text, length, encoding, span);
				GetGlyphPositions (span, offsets, origin);
			} finally {
				pool.Return (glyphs);
			}
		}

		// GetGlyphPositions (glyphs)

		public SKPoint[] GetGlyphPositions (ReadOnlySpan<ushort> glyphs, SKPoint origin = default)
		{
			var positions = new SKPoint[glyphs.Length];
			GetGlyphPositions (glyphs, positions, origin);
			return positions;
		}

		public void GetGlyphPositions (ReadOnlySpan<ushort> glyphs, Span<SKPoint> positions, SKPoint origin = default)
		{
			if (glyphs.Length != positions.Length)
				throw new ArgumentException ("The length of glyphs must be the same as the length of positions.", nameof (positions));

			fixed (ushort* gp = glyphs)
			fixed (SKPoint* pp = positions) {
				SkiaApi.sk_font_get_pos (Handle, gp, glyphs.Length, pp, &origin);
			}
		}

		// GetGlyphOffsets (text)

		public float[] GetGlyphOffsets (string text, float origin = 0f) =>
			GetGlyphOffsets (text.AsSpan (), origin);

		public float[] GetGlyphOffsets (ReadOnlySpan<char> text, float origin = 0f)
		{
			fixed (void* t = text) {
				return GetGlyphOffsets (t, text.Length * 2, SKTextEncoding.Utf16, origin);
			}
		}

		public float[] GetGlyphOffsets (ReadOnlySpan<byte> text, SKTextEncoding encoding, float origin = 0f)
		{
			fixed (void* t = text) {
				return GetGlyphOffsets (t, text.Length, encoding, origin);
			}
		}

		public float[] GetGlyphOffsets (IntPtr text, int length, SKTextEncoding encoding, float origin = 0f) =>
			GetGlyphOffsets ((void*)text, length, encoding, origin);

		public void GetGlyphOffsets (string text, Span<float> offsets, float origin = 0f) =>
			GetGlyphOffsets (text.AsSpan (), offsets, origin);

		public void GetGlyphOffsets (ReadOnlySpan<char> text, Span<float> offsets, float origin = 0f)
		{
			fixed (void* t = text) {
				GetGlyphOffsets (t, text.Length * 2, SKTextEncoding.Utf16, offsets, origin);
			}
		}

		public void GetGlyphOffsets (ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<float> offsets, float origin = 0f)
		{
			fixed (void* t = text) {
				GetGlyphOffsets (t, text.Length, encoding, offsets, origin);
			}
		}

		public void GetGlyphOffsets (IntPtr text, int length, SKTextEncoding encoding, Span<float> offsets, float origin = 0f) =>
			GetGlyphOffsets ((void*)text, length, encoding, offsets, origin);

		internal float[] GetGlyphOffsets (void* text, int length, SKTextEncoding encoding, float origin)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return new float[0];

			var n = CountGlyphs (text, length, encoding);
			if (n <= 0)
				return new float[0];

			var offsets = new float[n];
			GetGlyphOffsets (text, length, encoding, offsets, origin);
			return offsets;
		}

		internal void GetGlyphOffsets (void* text, int length, SKTextEncoding encoding, Span<float> offsets, float origin)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return;

			var n = offsets.Length;
			if (n <= 0)
				return;

			var pool = ArrayPool<ushort>.Shared;
			var glyphs = pool.Rent (n);
			try {
				var span = glyphs.AsSpan (0, n);
				GetGlyphs (text, length, encoding, span);
				GetGlyphOffsets (span, offsets, origin);
			} finally {
				pool.Return (glyphs);
			}
		}

		// GetGlyphOffsets (glyphs)

		public float[] GetGlyphOffsets (ReadOnlySpan<ushort> glyphs, float origin = 0f)
		{
			var offsets = new float[glyphs.Length];
			GetGlyphOffsets (glyphs, offsets, origin);
			return offsets;
		}

		public void GetGlyphOffsets (ReadOnlySpan<ushort> glyphs, Span<float> offsets, float origin = 0f)
		{
			if (glyphs.Length != offsets.Length)
				throw new ArgumentException ("The length of glyphs must be the same as the length of offsets.", nameof (offsets));

			fixed (ushort* gp = glyphs)
			fixed (float* pp = offsets) {
				SkiaApi.sk_font_get_xpos (Handle, gp, glyphs.Length, pp, origin);
			}
		}

		// GetGlyphWidths (text)

		public float[] GetGlyphWidths (string text, SKPaint paint = null) =>
			GetGlyphWidths (text.AsSpan (), paint);

		public float[] GetGlyphWidths (ReadOnlySpan<char> text, SKPaint paint = null)
		{
			fixed (void* t = text) {
				return GetGlyphWidths (t, text.Length * 2, SKTextEncoding.Utf16, paint);
			}
		}

		public float[] GetGlyphWidths (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint = null)
		{
			fixed (void* t = text) {
				return GetGlyphWidths (t, text.Length, encoding, paint);
			}
		}

		public float[] GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, SKPaint paint = null) =>
			GetGlyphWidths ((void*)text, length, encoding, paint);

		public float[] GetGlyphWidths (string text, out SKRect[] bounds, SKPaint paint = null) =>
			GetGlyphWidths (text.AsSpan (), out bounds, paint);

		public float[] GetGlyphWidths (ReadOnlySpan<char> text, out SKRect[] bounds, SKPaint paint = null)
		{
			fixed (void* t = text) {
				return GetGlyphWidths (t, text.Length * 2, SKTextEncoding.Utf16, out bounds, paint);
			}
		}

		public float[] GetGlyphWidths (ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint = null)
		{
			fixed (void* t = text) {
				return GetGlyphWidths (t, text.Length, encoding, out bounds, paint);
			}
		}

		public float[] GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint = null) =>
			GetGlyphWidths ((void*)text, length, encoding, out bounds, paint);

		public void GetGlyphWidths (string text, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null) =>
			GetGlyphWidths (text.AsSpan (), widths, bounds, paint);

		public void GetGlyphWidths (ReadOnlySpan<char> text, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null)
		{
			fixed (void* t = text) {
				GetGlyphWidths (t, text.Length * 2, SKTextEncoding.Utf16, widths, bounds, paint);
			}
		}

		public void GetGlyphWidths (ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null)
		{
			fixed (void* t = text) {
				GetGlyphWidths (t, text.Length, encoding, widths, bounds, paint);
			}
		}

		public void GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null) =>
			GetGlyphWidths ((void*)text, length, encoding, widths, bounds, paint);

		internal float[] GetGlyphWidths (void* text, int length, SKTextEncoding encoding, SKPaint paint)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return new float[0];

			var n = CountGlyphs (text, length, encoding);
			if (n <= 0)
				return new float[0];

			var widths = new float[n];
			GetGlyphWidths (text, length, encoding, widths, Span<SKRect>.Empty, paint);
			return widths;
		}

		internal float[] GetGlyphWidths (void* text, int length, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint)
		{
			if (!ValidateTextArgs (text, length, encoding)) {
				bounds = new SKRect[0];
				return new float[0];
			}

			var n = CountGlyphs (text, length, encoding);
			if (n <= 0) {
				bounds = new SKRect[0];
				return new float[0];
			}

			bounds = new SKRect[n];
			var widths = new float[n];
			GetGlyphWidths (text, length, encoding, widths, bounds, paint);
			return widths;
		}

		internal void GetGlyphWidths (void* text, int length, SKTextEncoding encoding, Span<float> widths, Span<SKRect> bounds, SKPaint paint)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return;

			var n = Math.Max (widths.Length, bounds.Length);
			if (n <= 0)
				return;

			// make sure that the destination spans are either empty/null or the same length
			if (widths.Length != 0 && widths.Length != n)
				throw new ArgumentException ("The length of widths must be equal to the length of bounds or empty.", nameof (widths));
			if (bounds.Length != 0 && bounds.Length != n)
				throw new ArgumentException ("The length of bounds must be equal to the length of widths or empty.", nameof (bounds));

			var pool = ArrayPool<ushort>.Shared;
			var glyphs = pool.Rent (n);
			try {
				var span = glyphs.AsSpan (0, n);
				GetGlyphs (text, length, encoding, span);
				GetGlyphWidths (span, widths, bounds, paint);
			} finally {
				pool.Return (glyphs);
			}
		}

		// GetGlyphWidths (glyphs)

		public float[] GetGlyphWidths (ReadOnlySpan<ushort> glyphs, SKPaint paint = null)
		{
			var widths = new float[glyphs.Length];
			GetGlyphWidths (glyphs, widths, Span<SKRect>.Empty, paint);
			return widths;
		}

		public float[] GetGlyphWidths (ReadOnlySpan<ushort> glyphs, out SKRect[] bounds, SKPaint paint = null)
		{
			var widths = new float[glyphs.Length];
			bounds = new SKRect[glyphs.Length];
			GetGlyphWidths (glyphs, widths, bounds, paint);
			return widths;
		}

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

		// GetPath (gyphs)

		public SKPath GetPath (ushort glyph)
		{
			var path = new SKPath ();
			if (!SkiaApi.sk_font_get_path (Handle, glyph, path.Handle)) {
				path.Dispose ();
				path = null;
			}
			return path;
		}

		// GetPath (text)

		public SKPath GetPath (string text, SKPoint origin = default) =>
			GetPath (text.AsSpan (), origin);

		public SKPath GetPath (ReadOnlySpan<char> text, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return GetPath (t, text.Length * 2, SKTextEncoding.Utf16, origin);
			}
		}

		public SKPath GetPath (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return GetPath (t, text.Length, encoding, origin);
			}
		}

		public SKPath GetPath (IntPtr text, int length, SKTextEncoding encoding, SKPoint origin = default) =>
			GetPath ((void*)text, length, encoding, origin);

		internal SKPath GetPath (void* text, int length, SKTextEncoding encoding, SKPoint origin)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return new SKPath ();

			var path = new SKPath ();
			SkiaApi.sk_text_utils_get_path (text, (IntPtr)length, encoding, origin.X, origin.Y, Handle, path.Handle);
			return path;
		}

		// GetPath (positioned)

		internal SKPath GetPath (string text, SKPoint[] positions) =>
			GetPath (text.AsSpan (), positions);

		internal SKPath GetPath (ReadOnlySpan<char> text, SKPoint[] positions)
		{
			fixed (void* t = text) {
				return GetPath (t, text.Length * 2, SKTextEncoding.Utf16, positions);
			}
		}

		internal SKPath GetPath (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint[] positions)
		{
			fixed (void* t = text) {
				return GetPath (t, text.Length, encoding, positions);
			}
		}

		internal SKPath GetPath (IntPtr text, int length, SKTextEncoding encoding, SKPoint[] positions) =>
			GetPath ((void*)text, length, encoding, positions);

		internal SKPath GetPath (void* text, int length, SKTextEncoding encoding, SKPoint[] positions)
		{
			if (!ValidateTextArgs (text, length, encoding))
				return new SKPath ();

			var path = new SKPath ();
			fixed (SKPoint* p = positions) {
				SkiaApi.sk_text_utils_get_pos_path (text, (IntPtr)length, encoding, p, Handle, path.Handle);
			}
			return path;
		}

		// GetPaths

		public void GetPaths (ReadOnlySpan<ushort> glyphs, SKGlyphPathDelegate glyphPathDelegate) =>
			GetPaths (glyphs, glyphPathDelegate, null);

		public void GetPaths (ReadOnlySpan<ushort> glyphs, SKGlyphPathDelegate glyphPathDelegate, object context)
		{
			var del = glyphPathDelegate != null && context != null
				? new SKGlyphPathDelegate ((p, m, _) => glyphPathDelegate (p, m, context))
				: glyphPathDelegate;
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKGlyphPathDelegateProxy, out var gch, out var ctx);
			try {
				fixed (ushort* g = glyphs) {
					SkiaApi.sk_font_get_paths (Handle, g, glyphs.Length, proxy, (void*)ctx);
				}
			} finally {
				gch.Free ();
			}
		}

		// Utils

		private bool ValidateTextArgs (void* text, int length, SKTextEncoding encoding)
		{
			if (length == 0)
				return false;

			if (text == null)
				throw new ArgumentNullException (nameof (text));

			return true;
		}
	}
}
