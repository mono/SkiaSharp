using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public class Font : NativeObject
	{
		public Font (Face face)
			: this (IntPtr.Zero)
		{
			if (face == null) {
				throw new ArgumentNullException (nameof (face));
			}

			Handle = HarfBuzzApi.hb_font_create (face.Handle);
		}

		internal Font (IntPtr handle)
			: base (handle)
		{
		}

		public FontExtents HorizontalFontExtents {
			get {
				if (HarfBuzzApi.hb_font_get_h_extents (Handle, out var fontExtents)) {
					return fontExtents;
				}
				return new FontExtents ();
			}
		}

		public FontExtents VerticalFontExtents {
			get {
				if (HarfBuzzApi.hb_font_get_v_extents (Handle, out var fontExtents)) {
					return fontExtents;
				}
				return new FontExtents ();
			}
		}

		public IReadOnlyList<string> SupportedShapers =>
			PtrToStringArray (HarfBuzzApi.hb_shape_list_shapers ()).ToArray ();

		public void GetScale (out int xScale, out int yScale) =>
			HarfBuzzApi.hb_font_get_scale (Handle, out xScale, out yScale);

		public void SetScale (int xScale, int yScale) =>
			HarfBuzzApi.hb_font_set_scale (Handle, xScale, yScale);

		public void GetHorizontalGlyphOrigin (int glyph, out int xOrigin, out int yOrigin) =>
			GetHorizontalGlyphOrigin ((uint)glyph, out xOrigin, out yOrigin);

		public void GetHorizontalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin) =>
			HarfBuzzApi.hb_font_get_glyph_h_origin (Handle, glyph, out xOrigin, out yOrigin);

		public void GetVerticalGlyphOrigin (int glyph, out int xOrigin, out int yOrigin) =>
			GetVerticalGlyphOrigin ((uint)glyph, out xOrigin, out yOrigin);

		public void GetVerticalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin) =>
			HarfBuzzApi.hb_font_get_glyph_v_origin (Handle, glyph, out xOrigin, out yOrigin);

		public int GetHorizontalGlyphAdvance (int glyph)
		{
			if (glyph < 0) {
				throw new ArgumentOutOfRangeException (nameof (glyph), "Glyph must be non negative.");
			}

			return HarfBuzzApi.hb_font_get_glyph_h_advance (Handle, (uint)glyph);
		}

		public int GetHorizontalGlyphAdvance (uint glyph) =>
			HarfBuzzApi.hb_font_get_glyph_h_advance (Handle, glyph);

		public int GetVerticalGlyphAdvance (int glyph)
		{
			if (glyph < 0) {
				throw new ArgumentOutOfRangeException (nameof (glyph), "Glyph must be non negative.");
			}

			return HarfBuzzApi.hb_font_get_glyph_v_advance (Handle, (uint)glyph);
		}

		public int GetVerticalGlyphAdvance (uint glyph) =>
			HarfBuzzApi.hb_font_get_glyph_v_advance (Handle, glyph);

		public unsafe int[] GetHorizontalGlyphAdvances (int[] glyphs)
		{
			fixed (int* firstGlyph = glyphs) {
				return GetHorizontalGlyphAdvances (glyphs.Length, (IntPtr)firstGlyph);
			}
		}

		public unsafe int[] GetHorizontalGlyphAdvances (ReadOnlySpan<int> glyphs)
		{
			fixed (int* firstGlyph = glyphs) {
				return GetHorizontalGlyphAdvances (glyphs.Length, (IntPtr)firstGlyph);
			}
		}

		public unsafe int[] GetHorizontalGlyphAdvances (uint[] glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetHorizontalGlyphAdvances (glyphs.Length, (IntPtr)firstGlyph);
			}
		}

		public unsafe int[] GetHorizontalGlyphAdvances (ReadOnlySpan<uint> glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetHorizontalGlyphAdvances (glyphs.Length, (IntPtr)firstGlyph);
			}
		}

		public unsafe int[] GetHorizontalGlyphAdvances (int count, IntPtr firstGlyph)
		{
			var advances = new int[count];

			fixed (int* firstAdvance = advances) {
				HarfBuzzApi.hb_font_get_glyph_h_advances (Handle, count, firstGlyph, 4, (IntPtr)firstAdvance, 4);
			}

			return advances;
		}

		public unsafe int[] GetVerticalGlyphAdvances (int[] glyphs)
		{
			fixed (int* firstGlyph = glyphs) {
				return GetVerticalGlyphAdvances (glyphs.Length, (IntPtr)firstGlyph);
			}
		}

		public unsafe int[] GetVerticalGlyphAdvances (ReadOnlySpan<int> glyphs)
		{
			fixed (int* firstGlyph = glyphs) {
				return GetVerticalGlyphAdvances (glyphs.Length, (IntPtr)firstGlyph);
			}
		}

		public unsafe int[] GetVerticalGlyphAdvances (uint[] glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetVerticalGlyphAdvances (glyphs.Length, (IntPtr)firstGlyph);
			}
		}

		public unsafe int[] GetVerticalGlyphAdvances (ReadOnlySpan<uint> glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetVerticalGlyphAdvances (glyphs.Length, (IntPtr)firstGlyph);
			}
		}

		public unsafe int[] GetVerticalGlyphAdvances (int count, IntPtr firstGlyph)
		{
			var advances = new int[count];

			fixed (int* firstAdvance = advances) {
				HarfBuzzApi.hb_font_get_glyph_v_advances (Handle, count, firstGlyph, 4, (IntPtr)firstAdvance, 4);
			}

			return advances;
		}

		public uint GetGlyph (int unicode) => GetGlyph (unicode, 0);

		public uint GetGlyph (int unicode, int variationSelector = 0)
		{
			if (unicode < 0) {
				throw new ArgumentOutOfRangeException (nameof (unicode), "Unicode must be non negative.");
			}

			if (variationSelector < 0) {
				throw new ArgumentOutOfRangeException (nameof (variationSelector), "VariationSelector must be non negative.");
			}

			return GetGlyph ((uint)unicode, (uint)variationSelector);
		}

		public uint GetGlyph (uint unicode) => GetGlyph (unicode, 0);

		public uint GetGlyph (uint unicode, uint variationSelector)
		{
			if (HarfBuzzApi.hb_font_get_glyph (Handle, unicode, variationSelector, out var glyph)) {
				return glyph;
			}

			return 0;
		}

		public GlyphExtents GetGlyphExtents (int glyph)
		{
			if (glyph < 0) {
				throw new ArgumentOutOfRangeException (nameof (glyph), "Glyph must be non negative.");
			}

			if (HarfBuzzApi.hb_font_get_glyph_extents (Handle, (uint)glyph, out var extents)) {
				return extents;
			}

			return new GlyphExtents ();
		}

		public GlyphExtents GetGlyphExtents (uint glyph)
		{
			if (HarfBuzzApi.hb_font_get_glyph_extents (Handle, glyph, out var extents)) {
				return extents;
			}

			return new GlyphExtents ();
		}

		public void SetFunctionsOpenType () =>
			HarfBuzzApi.hb_ot_font_set_funcs (Handle);

		public void Shape (Buffer buffer, params Feature[] features) =>
			Shape (buffer, features, null);

		public void Shape (Buffer buffer, IReadOnlyList<Feature> features, IReadOnlyList<string> shapers)
		{
			if (buffer == null) {
				throw new ArgumentNullException (nameof (buffer));
			}

			var featuresPtr = features == null || features.Count == 0 ? IntPtr.Zero : StructureArrayToPtr (features);
			var shapersPtr = shapers == null || shapers.Count == 0 ? IntPtr.Zero : StructureArrayToPtr (shapers);

			HarfBuzzApi.hb_shape_full (
				Handle,
				buffer.Handle,
				featuresPtr,
				featuresPtr != IntPtr.Zero ? features.Count : 0,
				shapersPtr);

			if (featuresPtr != IntPtr.Zero) {
				Marshal.FreeCoTaskMem (featuresPtr);
			}

			if (shapersPtr != IntPtr.Zero) {
				Marshal.FreeCoTaskMem (shapersPtr);
			}
		}

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_font_destroy (Handle);
			}
		}
	}
}
