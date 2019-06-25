using System;
using System.Buffers;
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

		public Font (Font parent)
			: this (IntPtr.Zero)
		{
			if (parent == null) {
				throw new ArgumentNullException (nameof (parent));
			}

			if (parent.Handle == IntPtr.Zero) {
				throw new ArgumentException (nameof (parent.Handle));
			}

			Handle = HarfBuzzApi.hb_font_create_sub_font (parent.Handle);
		}

		internal Font (IntPtr handle)
			: base (handle)
		{
		}

		public Font Parent {
			get {
				var parent = HarfBuzzApi.hb_font_get_parent (Handle);
				if (parent == IntPtr.Zero)
					return null;
				return new Font (parent);
			}
		}

		public string[] SupportedShapers =>
			PtrToStringArray (HarfBuzzApi.hb_shape_list_shapers ()).ToArray ();

		public void SetFontFunctions (FontFunctions fontFunctions, object fontData = null,
			ReleaseDelegate destroy = null)
		{
			if (fontFunctions == null) {
				throw new ArgumentException (nameof (fontFunctions));
			}

			var del = destroy != null && fontData != null
				? (_) => destroy (fontData)
				: destroy;
			var proxy = DelegateProxies.Create (del, DelegateProxies.ReleaseDelegateProxy, out _, out var ctx);

			fontFunctions.Font = this;

			fontFunctions.FontData = fontData;

			HarfBuzzApi.hb_font_set_funcs (Handle, fontFunctions.Handle, ctx, proxy);
		}

		public void GetScale (out int xScale, out int yScale) =>
			HarfBuzzApi.hb_font_get_scale (Handle, out xScale, out yScale);

		public void SetScale (int xScale, int yScale) =>
			HarfBuzzApi.hb_font_set_scale (Handle, xScale, yScale);

		public int GetHorizontalGlyphAdvance (uint glyph) =>
			HarfBuzzApi.hb_font_get_glyph_h_advance (Handle, glyph);

		public int GetVerticalGlyphAdvance (uint glyph) =>
			HarfBuzzApi.hb_font_get_glyph_v_advance (Handle, glyph);

		public unsafe int[] GetHorizontalGlyphAdvances (uint[] glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetHorizontalGlyphAdvances ((IntPtr)firstGlyph, glyphs.Length);
			}
		}

		public unsafe int[] GetHorizontalGlyphAdvances (ReadOnlySpan<uint> glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetHorizontalGlyphAdvances ((IntPtr)firstGlyph, glyphs.Length);
			}
		}

		public unsafe int[] GetHorizontalGlyphAdvances (IntPtr firstGlyph, int count)
		{
			var advances = new int[count];

			fixed (int* firstAdvance = advances) {
				HarfBuzzApi.hb_font_get_glyph_h_advances (Handle, count, firstGlyph, 4, (IntPtr)firstAdvance, 4);
			}

			return advances;
		}

		public unsafe int[] GetVerticalGlyphAdvances (uint[] glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetVerticalGlyphAdvances ((IntPtr)firstGlyph, glyphs.Length);
			}
		}

		public unsafe int[] GetVerticalGlyphAdvances (ReadOnlySpan<uint> glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetVerticalGlyphAdvances ((IntPtr)firstGlyph, glyphs.Length);
			}
		}

		public unsafe int[] GetVerticalGlyphAdvances (IntPtr firstGlyph, int count)
		{
			var advances = new int[count];

			fixed (int* firstAdvance = advances) {
				HarfBuzzApi.hb_font_get_glyph_v_advances (Handle, count, firstGlyph, 4, (IntPtr)firstAdvance, 4);
			}

			return advances;
		}

		public bool TryGetHorizontalFontExtents (out FontExtents extents) =>
			HarfBuzzApi.hb_font_get_h_extents (Handle, out extents);

		public bool TryGetVerticalFontExtents (out FontExtents extents) =>
			HarfBuzzApi.hb_font_get_v_extents (Handle, out extents);

		public bool TryGetHorizontalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin) =>
			HarfBuzzApi.hb_font_get_glyph_h_origin (Handle, glyph, out xOrigin, out yOrigin);

		public bool TryGetVerticalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin) =>
			HarfBuzzApi.hb_font_get_glyph_v_origin (Handle, glyph, out xOrigin, out yOrigin);

		public bool TryGetGlyph (int unicode, out uint glyph) => TryGetGlyph ((uint)unicode, 0, out glyph);

		public bool TryGetGlyph (uint unicode, out uint glyph) => TryGetGlyph (unicode, 0, out glyph);

		public bool TryGetGlyph (int unicode, uint variationSelector, out uint glyph) =>
			TryGetGlyph ((uint)unicode, variationSelector, out glyph);

		public bool TryGetGlyph (uint unicode, uint variationSelector, out uint glyph)
		{
			return HarfBuzzApi.hb_font_get_glyph (Handle, unicode, variationSelector, out glyph);
		}

		public bool TryGetGlyphExtents (uint glyph, out GlyphExtents extents)
		{
			return HarfBuzzApi.hb_font_get_glyph_extents (Handle, glyph, out extents);
		}

		public bool TryGetGlyphName (uint glyph, out string name)
		{
			var buffer = ArrayPool<char>.Shared.Rent (128);

			unsafe {
				fixed (char* first = buffer) {
					if (!HarfBuzzApi.hb_font_get_glyph_name (Handle, glyph, first, buffer.Length)) {
						name = string.Empty;
						ArrayPool<char>.Shared.Return (buffer);
						return false;
					}
				}
			}

			var length = 0;

			foreach (var c in buffer) {
				if (c == 0)
					break;
				length++;
			}

			ArrayPool<char>.Shared.Return (buffer);

			name = new string (buffer, 0, length);

			return true;
		}

		public bool TryGetGlyphFromName (string name, out uint glyph)
		{
			return HarfBuzzApi.hb_font_get_glyph_from_name (Handle, name, name.Length, out glyph);
		}

		public bool TryGetGlyphContourPoint (uint glyph,
			uint pointIndex, out int x, out int y)
		{
			return HarfBuzzApi.hb_font_get_glyph_contour_point (Handle, glyph, pointIndex, out x, out y);
		}

		public bool TryGetGlyphContourPointForOrigin (uint glyph,
			uint pointIndex, Direction direction, out int x, out int y)
		{
			return HarfBuzzApi.hb_font_get_glyph_contour_point_for_origin (Handle, glyph, pointIndex, direction, out x, out y);
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
				features?.Count ?? 0,
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
