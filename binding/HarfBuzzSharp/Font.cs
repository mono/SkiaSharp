#nullable disable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>
	/// Represents a specific font face.
	/// </summary>
	public unsafe class Font : NativeObject
	{
		internal const int NameBufferLength = 128;

		/// <summary>
		/// Creates a new <see cref="T:HarfBuzzSharp.Font" /> using a specific font face.
		/// </summary>
		/// <param name="face">The face to use.</param>
		public Font (Face face)
			: base (IntPtr.Zero)
		{
			if (face == null)
				throw new ArgumentNullException (nameof (face));

			Handle = HarfBuzzApi.hb_font_create (face.Handle);
			OpenTypeMetrics = new OpenTypeMetrics (this);
		}

		public Font (Font parent)
			: base (IntPtr.Zero)
		{
			if (parent == null)
				throw new ArgumentNullException (nameof (parent));
			if (parent.Handle == IntPtr.Zero)
				throw new ArgumentException (nameof (parent.Handle));

			Parent = parent;
			Handle = HarfBuzzApi.hb_font_create_sub_font (parent.Handle);
			OpenTypeMetrics = new OpenTypeMetrics (this);
		}

		public Font Parent { get; }

		public OpenTypeMetrics OpenTypeMetrics { get; }

		public string[] SupportedShapers =>
			PtrToStringArray ((IntPtr)HarfBuzzApi.hb_shape_list_shapers ()).ToArray ();

		public void SetFontFunctions (FontFunctions fontFunctions) =>
			SetFontFunctions (fontFunctions, null, null);

		public void SetFontFunctions (FontFunctions fontFunctions, object fontData) =>
			SetFontFunctions (fontFunctions, fontData, null);

		public void SetFontFunctions (FontFunctions fontFunctions, object fontData, ReleaseDelegate destroy)
		{
			_ = fontFunctions ?? throw new ArgumentNullException (nameof (fontFunctions));

			var container = new FontUserData (this, fontData);
			var ctx = DelegateProxies.CreateMultiUserData (destroy, container);
			HarfBuzzApi.hb_font_set_funcs (Handle, fontFunctions.Handle, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
		}

		/// <summary>
		/// Retrieves the font scale.
		/// </summary>
		/// <param name="xScale">The scale along the x-axis.</param>
		/// <param name="yScale">The scale along the y-axis.</param>
		public void GetScale (out int xScale, out int yScale)
		{
			fixed (int* x = &xScale)
			fixed (int* y = &yScale) {
				HarfBuzzApi.hb_font_get_scale (Handle, x, y);
			}
		}

		/// <summary>
		/// Sets the font scale.
		/// </summary>
		/// <param name="xScale">The scale along the x-axis.</param>
		/// <param name="yScale">The scale along the y-axis.</param>
		public void SetScale (int xScale, int yScale) =>
			HarfBuzzApi.hb_font_set_scale (Handle, xScale, yScale);

		public bool TryGetHorizontalFontExtents (out FontExtents extents)
		{
			fixed (FontExtents* e = &extents) {
				return HarfBuzzApi.hb_font_get_h_extents (Handle, e);
			}
		}

		public bool TryGetVerticalFontExtents (out FontExtents extents)
		{
			fixed (FontExtents* e = &extents) {
				return HarfBuzzApi.hb_font_get_v_extents (Handle, e);
			}
		}

		public bool TryGetNominalGlyph (int unicode, out uint glyph) =>
			TryGetNominalGlyph ((uint)unicode, out glyph);

		public bool TryGetNominalGlyph (uint unicode, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				return HarfBuzzApi.hb_font_get_nominal_glyph (Handle, unicode, g);
			}
		}

		public bool TryGetVariationGlyph (int unicode, out uint glyph) =>
			TryGetVariationGlyph (unicode, 0, out glyph);

		public bool TryGetVariationGlyph (uint unicode, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				return HarfBuzzApi.hb_font_get_variation_glyph (Handle, unicode, 0, g);
			}
		}

		public bool TryGetVariationGlyph (int unicode, uint variationSelector, out uint glyph) =>
			TryGetVariationGlyph ((uint)unicode, variationSelector, out glyph);

		public bool TryGetVariationGlyph (uint unicode, uint variationSelector, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				return HarfBuzzApi.hb_font_get_variation_glyph (Handle, unicode, variationSelector, g);
			}
		}

		public int GetHorizontalGlyphAdvance (uint glyph) =>
			HarfBuzzApi.hb_font_get_glyph_h_advance (Handle, glyph);

		public int GetVerticalGlyphAdvance (uint glyph) =>
			HarfBuzzApi.hb_font_get_glyph_v_advance (Handle, glyph);

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
				HarfBuzzApi.hb_font_get_glyph_h_advances (Handle, (uint)count, (uint*)firstGlyph, 4, firstAdvance, 4);
			}

			return advances;
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
				HarfBuzzApi.hb_font_get_glyph_v_advances (Handle, (uint)count, (uint*)firstGlyph, 4, firstAdvance, 4);
			}

			return advances;
		}

		public bool TryGetHorizontalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin)
		{
			fixed (int* x = &xOrigin)
			fixed (int* y = &yOrigin) {
				return HarfBuzzApi.hb_font_get_glyph_h_origin (Handle, glyph, x, y);
			}
		}

		public bool TryGetVerticalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin)
		{
			fixed (int* x = &xOrigin)
			fixed (int* y = &yOrigin) {
				return HarfBuzzApi.hb_font_get_glyph_v_origin (Handle, glyph, x, y);
			}
		}

		public int GetHorizontalGlyphKerning (uint leftGlyph, uint rightGlyph) =>
			HarfBuzzApi.hb_font_get_glyph_h_kerning (Handle, leftGlyph, rightGlyph);

		public bool TryGetGlyphExtents (uint glyph, out GlyphExtents extents)
		{
			fixed (GlyphExtents* e = &extents) {
				return HarfBuzzApi.hb_font_get_glyph_extents (Handle, glyph, e);
			}
		}

		public bool TryGetGlyphContourPoint (uint glyph, uint pointIndex, out int x, out int y)
		{
			fixed (int* xPtr = &x)
			fixed (int* yPtr = &y) {
				return HarfBuzzApi.hb_font_get_glyph_contour_point (Handle, glyph, pointIndex, xPtr, yPtr);
			}
		}

		public unsafe bool TryGetGlyphName (uint glyph, out string name)
		{
			var pool = ArrayPool<byte>.Shared;
			var buffer = pool.Rent (NameBufferLength);
			try {
				fixed (byte* first = buffer) {
					if (!HarfBuzzApi.hb_font_get_glyph_name (Handle, glyph, first, (uint)buffer.Length)) {
						name = string.Empty;
						return false;
					}
					name = Marshal.PtrToStringAnsi ((IntPtr)first);
					return true;
				}
			} finally {
				pool.Return (buffer);
			}
		}

		public bool TryGetGlyphFromName (string name, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				return HarfBuzzApi.hb_font_get_glyph_from_name (Handle, name, name.Length, g);
			}
		}

		public bool TryGetGlyph (int unicode, out uint glyph) =>
			TryGetGlyph ((uint)unicode, 0, out glyph);

		public bool TryGetGlyph (uint unicode, out uint glyph) =>
			TryGetGlyph (unicode, 0, out glyph);

		public bool TryGetGlyph (int unicode, uint variationSelector, out uint glyph) =>
			TryGetGlyph ((uint)unicode, variationSelector, out glyph);

		public bool TryGetGlyph (uint unicode, uint variationSelector, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				return HarfBuzzApi.hb_font_get_glyph (Handle, unicode, variationSelector, g);
			}
		}

		public FontExtents GetFontExtentsForDirection (Direction direction)
		{
			FontExtents extents;
			HarfBuzzApi.hb_font_get_extents_for_direction (Handle, direction, &extents);
			return extents;
		}

		public void GetGlyphAdvanceForDirection (uint glyph, Direction direction, out int x, out int y)
		{
			fixed (int* xPtr = &x)
			fixed (int* yPtr = &y) {
				HarfBuzzApi.hb_font_get_glyph_advance_for_direction (Handle, glyph, direction, xPtr, yPtr);
			}
		}

		public unsafe int[] GetGlyphAdvancesForDirection (ReadOnlySpan<uint> glyphs, Direction direction)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetGlyphAdvancesForDirection ((IntPtr)firstGlyph, glyphs.Length, direction);
			}
		}

		public unsafe int[] GetGlyphAdvancesForDirection (IntPtr firstGlyph, int count, Direction direction)
		{
			var advances = new int[count];

			fixed (int* firstAdvance = advances) {
				HarfBuzzApi.hb_font_get_glyph_advances_for_direction (Handle, direction, (uint)count, (uint*)firstGlyph, 4, firstAdvance, 4);
			}

			return advances;
		}

		public bool TryGetGlyphContourPointForOrigin (uint glyph, uint pointIndex, Direction direction, out int x, out int y)
		{
			fixed (int* xPtr = &x)
			fixed (int* yPtr = &y) {
				return HarfBuzzApi.hb_font_get_glyph_contour_point_for_origin (Handle, glyph, pointIndex, direction, xPtr, yPtr);
			}
		}

		public unsafe string GlyphToString (uint glyph)
		{
			var pool = ArrayPool<byte>.Shared;
			var buffer = pool.Rent (NameBufferLength);
			try {
				fixed (byte* first = buffer) {
					HarfBuzzApi.hb_font_glyph_to_string (Handle, glyph, first, (uint)buffer.Length);
					return Marshal.PtrToStringAnsi ((IntPtr)first);
				}
			} finally {
				pool.Return (buffer);
			}
		}

		public bool TryGetGlyphFromString (string s, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				return HarfBuzzApi.hb_font_glyph_from_string (Handle, s, -1, g);
			}
		}

		/// <summary>
		/// Sets the font functions to that of OpenType.
		/// </summary>
		public void SetFunctionsOpenType () =>
			HarfBuzzApi.hb_ot_font_set_funcs (Handle);

		/// <summary>
		/// Shapes the specified buffer using the current font.
		/// </summary>
		/// <param name="buffer">The buffer to shape.</param>
		/// <param name="features">The features to control the shaping process.</param>
		public void Shape (Buffer buffer, params Feature[] features) =>
			Shape (buffer, features, null);

		public void Shape (Buffer buffer, IReadOnlyList<Feature> features, IReadOnlyList<string> shapers)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			if (buffer.Direction == Direction.Invalid)
				throw new InvalidOperationException ("Buffer's Direction must be valid.");

			if (buffer.ContentType != ContentType.Unicode) {
				throw new InvalidOperationException ("Buffer's ContentType must of type Unicode.");
			}

			void*[] shapersPtrs = null;
			if (shapers?.Count > 0) {
				shapersPtrs = new void*[shapers.Count + 1];
				int i;
				for (i = 0; i < shapers.Count; i++) {
					shapersPtrs[i] = (void*)Marshal.StringToHGlobalAnsi (shapers[i]);
				}
				shapersPtrs[i] = null;
			}

			var featuresArray = features?.ToArray ();

			fixed (Feature* fPtr = featuresArray)
			fixed (void** sPtr = shapersPtrs) {
				HarfBuzzApi.hb_shape_full (
					Handle,
					buffer.Handle,
					fPtr,
					(uint)(features?.Count ?? 0),
					sPtr);
			}

			if (shapersPtrs != null) {
				for (var i = 0; i < shapersPtrs.Length; i++) {
					if (shapersPtrs[i] != null)
						Marshal.FreeHGlobal ((IntPtr)shapersPtrs[i]);
				}
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_font_destroy (Handle);
			}
		}
	}

	internal class FontUserData
	{
		public FontUserData (Font font, object fontData)
		{
			Font = font;
			FontData = fontData;
		}

		public Font Font { get; }

		public object FontData { get; }
	}
}
