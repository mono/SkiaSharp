#nullable disable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public unsafe class Font : NativeObject
	{
		internal const int NameBufferLength = 128;

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

		public void GetScale (out int xScale, out int yScale)
		{
			fixed (int* x = &xScale)
			fixed (int* y = &yScale) {
				HarfBuzzApi.hb_font_get_scale (Handle, x, y);
			}
		}

		public void SetScale (int xScale, int yScale) =>
			HarfBuzzApi.hb_font_set_scale (Handle, xScale, yScale);

		/// <summary>
		/// Gets or sets the synthetic slant of the font.
		/// </summary>
		/// <remarks>
		/// The synthetic slant is in "run" units to achieve a "rise" of 1.
		/// The typical slant value for an idealized oblique font is 0.2 (about 12°).
		/// HarfBuzz needs to know this value to adjust shaping results, such as offsets.
		/// </remarks>
		public float SyntheticSlant
		{
			get => HarfBuzzApi.hb_font_get_synthetic_slant (Handle);
			set => HarfBuzzApi.hb_font_set_synthetic_slant (Handle, value);
		}

		/// <summary>
		/// Sets synthetic bold parameters for the font.
		/// </summary>
		/// <remarks>
		/// Positive values for emboldening in X and Y directions. Typical value
		/// is 2% of UPEM for X, and Y embolden of 0. Setting both to 0 removes
		/// the synthetic bold. If inPlace is true, glyphs are widened in-place
		/// rather than adding side-bearing.
		/// </remarks>
		/// <param name="xEmbolden">Horizontal emboldening amount</param>
		/// <param name="yEmbolden">Vertical emboldening amount</param>
		/// <param name="inPlace">Whether to embolden in-place</param>
		public void SetSyntheticBold (float xEmbolden, float yEmbolden, bool inPlace = false) =>
			HarfBuzzApi.hb_font_set_synthetic_bold (Handle, xEmbolden, yEmbolden, inPlace);

		/// <summary>
		/// Gets the synthetic bold parameters of the font.
		/// </summary>
		/// <param name="xEmbolden">Horizontal emboldening amount</param>
		/// <param name="yEmbolden">Vertical emboldening amount</param>
		/// <param name="inPlace">Whether emboldening is in-place</param>
		public void GetSyntheticBold (out float xEmbolden, out float yEmbolden, out bool inPlace)
		{
			fixed (float* x = &xEmbolden)
			fixed (float* y = &yEmbolden)
			fixed (bool* ip = &inPlace) {
				HarfBuzzApi.hb_font_get_synthetic_bold (Handle, x, y, ip);
			}
		}

		/// <summary>
		/// Sets a single variation axis value.
		/// </summary>
		/// <remarks>
		/// This is useful for setting a single axis on a variable font.
		/// For multiple axes, use SetVariations instead.
		/// </remarks>
		/// <param name="tag">The axis tag (e.g., "wght" for weight)</param>
		/// <param name="value">The axis value in design space</param>
		public void SetVariation (uint tag, float value) =>
			HarfBuzzApi.hb_font_set_variation (Handle, tag, value);

		/// <summary>
		/// Sets a single variation axis value using a string tag.
		/// </summary>
		/// <remarks>
		/// Common tags are "wght" for weight, "wdth" for width, 
		/// "slnt" for slant, "ital" for italic.
		/// </remarks>
		/// <param name="tag">The axis tag string (e.g., "wght")</param>
		/// <param name="value">The axis value in design space</param>
		public void SetVariation (string tag, float value)
		{
			if (tag == null)
				throw new ArgumentNullException (nameof (tag));
			if (tag.Length != 4)
				throw new ArgumentException ("Tag must be exactly 4 characters", nameof (tag));

			var tagValue = Tag.Parse (tag);
			HarfBuzzApi.hb_font_set_variation (Handle, tagValue, value);
		}

		/// <summary>
		/// Gets or sets the named instance index for the font.
		/// </summary>
		/// <remarks>
		/// A value of HB_FONT_NO_VAR_NAMED_INSTANCE (0xFFFFFFFF) means 
		/// no named instance is set.
		/// </remarks>
		public uint NamedInstance
		{
			get => HarfBuzzApi.hb_font_get_var_named_instance (Handle);
			set => HarfBuzzApi.hb_font_set_var_named_instance (Handle, value);
		}

		/// <summary>
		/// Sets design-space coordinates for variable font axes.
		/// </summary>
		/// <remarks>
		/// Note that this is in design-space coordinates, not normalized.
		/// This is the primary API for working with variable fonts.
		/// </remarks>
		/// <param name="variations">An array of variation settings</param>
		public void SetVariations (Variation[] variations)
		{
			if (variations == null)
				throw new ArgumentNullException (nameof (variations));

			fixed (Variation* v = variations) {
				HarfBuzzApi.hb_font_set_variations (Handle, v, (uint)variations.Length);
			}
		}

		/// <summary>
		/// Sets design-space coordinates for variable font axes.
		/// </summary>
		/// <remarks>
		/// Note that this is in design-space coordinates, not normalized.
		/// This is the primary API for working with variable fonts.
		/// </remarks>
		/// <param name="variations">A span of variation settings</param>
		public void SetVariations (ReadOnlySpan<Variation> variations)
		{
			fixed (Variation* v = variations) {
				HarfBuzzApi.hb_font_set_variations (Handle, v, (uint)variations.Length);
			}
		}

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

		public void SetFunctionsOpenType () =>
			HarfBuzzApi.hb_ot_font_set_funcs (Handle);

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
