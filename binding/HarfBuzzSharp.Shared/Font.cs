using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public class Font : NativeObject
	{
		internal Font (IntPtr handle)
			: base (handle)
		{
		}

		public Font (Face face)
			: this (IntPtr.Zero)
		{
			if (face == null) {
				throw new ArgumentNullException (nameof (face));
			}

			Handle = HarfBuzzApi.hb_font_create (face.Handle);
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

		public Scale Scale {
			get {
				HarfBuzzApi.hb_font_get_scale (Handle, out var x, out var y);
				return new Scale (x, y);
			}
			set {
				HarfBuzzApi.hb_font_set_scale (Handle, value.X, value.Y);
			}
		}

		public IReadOnlyList<string> SupportedShapers {
			get {
				return PtrToStringArray (HarfBuzzApi.hb_shape_list_shapers ()).ToArray ();
			}
		}

		public int GetHorizontalGlyphAdvance (int glyph)
		{
			if (glyph < 0) {
				throw new ArgumentOutOfRangeException (nameof (glyph), "Glyph must be non negative.");
			}

			return HarfBuzzApi.hb_font_get_glyph_h_advance (Handle, (uint)glyph);
		}

		public int GetHorizontalGlyphAdvance (uint glyph) => HarfBuzzApi.hb_font_get_glyph_h_advance (Handle, glyph);

		public int GetVerticalGlyphAdvance (int glyph)
		{
			if (glyph < 0) {
				throw new ArgumentOutOfRangeException (nameof (glyph), "Glyph must be non negative.");
			}

			return HarfBuzzApi.hb_font_get_glyph_v_advance (Handle, (uint)glyph);
		}

		public int GetVerticalGlyphAdvance (uint glyph) => HarfBuzzApi.hb_font_get_glyph_v_advance (Handle, glyph);

		public uint GetGlyph (int unicode, int variationSelector = 0)
		{
			if (unicode < 0) {
				throw new ArgumentOutOfRangeException (nameof (unicode), "Unicode must be non negative.");
			}

			if (variationSelector < 0) {
				throw new ArgumentOutOfRangeException (nameof (variationSelector), "VariationSelector must be non negative.");
			}

			if (HarfBuzzApi.hb_font_get_glyph (Handle, (uint)unicode, (uint)variationSelector, out var glyph)) {
				return glyph;
			}

			return 0;
		}

		public uint GetGlyph (uint unicode, uint variationSelector = 0)
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

		public void SetFunctionsOpenType () => HarfBuzzApi.hb_ot_font_set_funcs (Handle);

		public void Shape (Buffer buffer, params Feature[] features)
		{
			if (buffer == null) {
				throw new ArgumentNullException (nameof (buffer));
			}

			if (features == null || features.Length == 0) {
				HarfBuzzApi.hb_shape (Handle, buffer.Handle, IntPtr.Zero, 0);
			} else {
				var ptr = StructureArrayToPtr (features);
				HarfBuzzApi.hb_shape (Handle, buffer.Handle, ptr, features.Length);
				Marshal.FreeCoTaskMem (ptr);
			}
		}

		public void ShapeFull (Buffer buffer, IReadOnlyList<Feature> features = null, IReadOnlyList<string> shapers = null)
		{
			if (buffer == null) {
				throw new ArgumentNullException (nameof (buffer));
			}

			var featuresPtr = features == null || features.Count == 0 ? IntPtr.Zero : StructureArrayToPtr (features);
			var shapersPtr = shapers == null || shapers.Count == 0 ? IntPtr.Zero : StructureArrayToPtr (shapers);

			HarfBuzzApi.hb_shape_full (
				Handle,
				buffer.Handle,
				IntPtr.Zero,
				featuresPtr != IntPtr.Zero ? features.Count : 0,
				shapersPtr);

			if (featuresPtr != IntPtr.Zero) {
				Marshal.FreeCoTaskMem (featuresPtr);
			}

			if (shapersPtr != IntPtr.Zero) {
				Marshal.FreeCoTaskMem (shapersPtr);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_font_destroy (Handle);
			}

			base.Dispose (disposing);
		}
	}
}
