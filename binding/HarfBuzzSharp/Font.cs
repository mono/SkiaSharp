#nullable disable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>Represents a specific font face.</summary>
	/// <remarks />
	public unsafe class Font : NativeObject
	{
		internal const int NameBufferLength = 128;

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Font" /> class using a specific font face.</summary>
		/// <param name="face">The face to use.</param>
		/// <remarks />
		public Font (Face face)
			: base (IntPtr.Zero)
		{
			if (face == null)
				throw new ArgumentNullException (nameof (face));

			Handle = HarfBuzzApi.hb_font_create (face.Handle);
			GC.KeepAlive (face);
			OpenTypeMetrics = new OpenTypeMetrics (this);
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Font" /> class as a sub-font of the specified parent font.</summary>
		/// <param name="parent">The parent font to use as a basis for this font.</param>
		/// <remarks>The new font inherits settings from the parent that can be overridden.</remarks>
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

		/// <summary>Gets the parent font, if this font was created as a sub-font.</summary>
		/// <value>The parent font, or <see langword="null" /> if this font has no parent.</value>
		/// <remarks />
		public Font Parent { get; }

		/// <summary>Gets access to OpenType metrics for this font.</summary>
		/// <value>The OpenType metrics accessor.</value>
		/// <remarks>Provides access to metrics defined in the OpenType MVAR table.</remarks>
		public OpenTypeMetrics OpenTypeMetrics { get; }

		/// <summary>Gets the list of supported shaper names.</summary>
		/// <value>An array of shaper names that can be used with the Shape method.</value>
		/// <remarks />
		public string[] SupportedShapers =>
			PtrToStringArray ((IntPtr)HarfBuzzApi.hb_shape_list_shapers ()).ToArray ();

		/// <summary>Sets custom font functions for this font.</summary>
		/// <param name="fontFunctions">The font functions to use.</param>
		/// <remarks>Font functions provide callbacks for glyph metrics and other font information.</remarks>
		public void SetFontFunctions (FontFunctions fontFunctions) =>
			SetFontFunctions (fontFunctions, null, null);

		/// <summary>Sets custom font functions for this font with associated user data.</summary>
		/// <param name="fontFunctions">The font functions to use.</param>
		/// <param name="fontData">User data to pass to the font functions.</param>
		/// <remarks>Font functions provide callbacks for glyph metrics and other font information.</remarks>
		public void SetFontFunctions (FontFunctions fontFunctions, object fontData) =>
			SetFontFunctions (fontFunctions, fontData, null);

		/// <summary>Sets custom font functions for this font with associated user data and a destroy callback.</summary>
		/// <param name="fontFunctions">The font functions to use.</param>
		/// <param name="fontData">User data to pass to the font functions.</param>
		/// <param name="destroy">A delegate to call when the font data is no longer needed.</param>
		/// <remarks />
		public void SetFontFunctions (FontFunctions fontFunctions, object fontData, ReleaseDelegate destroy)
		{
			_ = fontFunctions ?? throw new ArgumentNullException (nameof (fontFunctions));

			var container = new FontUserData (this, fontData);
			var ctx = DelegateProxies.CreateMultiUserData (destroy, container);
			HarfBuzzApi.hb_font_set_funcs (Handle, fontFunctions.Handle, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
			GC.KeepAlive (fontFunctions);
			GC.KeepAlive (this);
		}

		/// <summary>Retrieves the font scale.</summary>
		/// <param name="xScale">The scale along the x-axis.</param>
		/// <param name="yScale">The scale along the y-axis.</param>
		/// <remarks />
		public void GetScale (out int xScale, out int yScale)
		{
			fixed (int* x = &xScale)
			fixed (int* y = &yScale) {
				HarfBuzzApi.hb_font_get_scale (Handle, x, y);
			}
			GC.KeepAlive (this);
		}

		/// <summary>Sets the font scale.</summary>
		/// <param name="xScale">The scale along the x-axis.</param>
		/// <param name="yScale">The scale along the y-axis.</param>
		/// <remarks />
		public void SetScale (int xScale, int yScale)
		{
			HarfBuzzApi.hb_font_set_scale (Handle, xScale, yScale);
			GC.KeepAlive (this);
		}

		/// <summary>Tries to get the horizontal extents of the font.</summary>
		/// <param name="extents">Returns the horizontal font extents.</param>
		/// <returns><see langword="true" /> if the extents were found; otherwise, <see langword="false" />.</returns>
		/// <remarks>Extents include ascender, descender, and line gap values.</remarks>
		public bool TryGetHorizontalFontExtents (out FontExtents extents)
		{
			fixed (FontExtents* e = &extents) {
				var r = HarfBuzzApi.hb_font_get_h_extents (Handle, e);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Tries to get the vertical extents of the font.</summary>
		/// <param name="extents">Returns the vertical font extents.</param>
		/// <returns><see langword="true" /> if the extents were found; otherwise, <see langword="false" />.</returns>
		/// <remarks>Extents include ascender, descender, and line gap values for vertical text.</remarks>
		public bool TryGetVerticalFontExtents (out FontExtents extents)
		{
			fixed (FontExtents* e = &extents) {
				var r = HarfBuzzApi.hb_font_get_v_extents (Handle, e);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Tries to get the nominal glyph ID for a Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="glyph">Returns the nominal glyph ID if found.</param>
		/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks>The nominal glyph is the default glyph for a character, without variation selectors.</remarks>
		public bool TryGetNominalGlyph (int unicode, out uint glyph) =>
			TryGetNominalGlyph ((uint)unicode, out glyph);

		/// <summary>Tries to get the nominal glyph ID for a Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="glyph">Returns the nominal glyph ID if found.</param>
		/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks>The nominal glyph is the default glyph for a character, without variation selectors.</remarks>
		public bool TryGetNominalGlyph (uint unicode, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				var r = HarfBuzzApi.hb_font_get_nominal_glyph (Handle, unicode, g);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Tries to get a variation glyph for a Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="glyph">Returns the variation glyph ID if found.</param>
		/// <returns><see langword="true" /> if a variation glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetVariationGlyph (int unicode, out uint glyph) =>
			TryGetVariationGlyph (unicode, 0, out glyph);

		/// <summary>Tries to get a variation glyph for a Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="glyph">Returns the variation glyph ID if found.</param>
		/// <returns><see langword="true" /> if a variation glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetVariationGlyph (uint unicode, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				var r = HarfBuzzApi.hb_font_get_variation_glyph (Handle, unicode, 0, g);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Tries to get a variation glyph for a Unicode code point with a specific variation selector.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="variationSelector">The variation selector code point.</param>
		/// <param name="glyph">Returns the variation glyph ID if found.</param>
		/// <returns><see langword="true" /> if a variation glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetVariationGlyph (int unicode, uint variationSelector, out uint glyph) =>
			TryGetVariationGlyph ((uint)unicode, variationSelector, out glyph);

		/// <summary>Tries to get a variation glyph for a Unicode code point with a specific variation selector.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="variationSelector">The variation selector code point.</param>
		/// <param name="glyph">Returns the variation glyph ID if found.</param>
		/// <returns><see langword="true" /> if a variation glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetVariationGlyph (uint unicode, uint variationSelector, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				var r = HarfBuzzApi.hb_font_get_variation_glyph (Handle, unicode, variationSelector, g);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the horizontal advance width of a glyph.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <returns>The horizontal advance width in font units.</returns>
		/// <remarks />
		public int GetHorizontalGlyphAdvance (uint glyph)
		{
			var r = HarfBuzzApi.hb_font_get_glyph_h_advance (Handle, glyph);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Gets the vertical advance height of a glyph.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <returns>The vertical advance height in font units.</returns>
		/// <remarks />
		public int GetVerticalGlyphAdvance (uint glyph)
		{
			var r = HarfBuzzApi.hb_font_get_glyph_v_advance (Handle, glyph);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Gets the horizontal advance widths of multiple glyphs.</summary>
		/// <param name="glyphs">The span of glyph IDs.</param>
		/// <returns>An array of horizontal advances in font units.</returns>
		/// <remarks />
		public unsafe int[] GetHorizontalGlyphAdvances (ReadOnlySpan<uint> glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetHorizontalGlyphAdvances ((IntPtr)firstGlyph, glyphs.Length);
			}
		}

		/// <summary>Gets the horizontal advance widths of multiple glyphs.</summary>
		/// <param name="firstGlyph">A pointer to the first glyph ID.</param>
		/// <param name="count">The number of glyphs.</param>
		/// <returns>An array of horizontal advances in font units.</returns>
		/// <remarks />
		public unsafe int[] GetHorizontalGlyphAdvances (IntPtr firstGlyph, int count)
		{
			var advances = new int[count];

			fixed (int* firstAdvance = advances) {
				HarfBuzzApi.hb_font_get_glyph_h_advances (Handle, (uint)count, (uint*)firstGlyph, 4, firstAdvance, 4);
			}

			GC.KeepAlive (this);
			return advances;
		}

		/// <summary>Gets the vertical advance heights of multiple glyphs.</summary>
		/// <param name="glyphs">The span of glyph IDs.</param>
		/// <returns>An array of vertical advances in font units.</returns>
		/// <remarks />
		public unsafe int[] GetVerticalGlyphAdvances (ReadOnlySpan<uint> glyphs)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetVerticalGlyphAdvances ((IntPtr)firstGlyph, glyphs.Length);
			}
		}

		/// <summary>Gets the vertical advance heights of multiple glyphs.</summary>
		/// <param name="firstGlyph">A pointer to the first glyph ID.</param>
		/// <param name="count">The number of glyphs.</param>
		/// <returns>An array of vertical advances in font units.</returns>
		/// <remarks />
		public unsafe int[] GetVerticalGlyphAdvances (IntPtr firstGlyph, int count)
		{
			var advances = new int[count];

			fixed (int* firstAdvance = advances) {
				HarfBuzzApi.hb_font_get_glyph_v_advances (Handle, (uint)count, (uint*)firstGlyph, 4, firstAdvance, 4);
			}

			GC.KeepAlive (this);
			return advances;
		}

		/// <summary>Tries to get the horizontal origin of a glyph.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <param name="xOrigin">Returns the X coordinate of the origin.</param>
		/// <param name="yOrigin">Returns the Y coordinate of the origin.</param>
		/// <returns><see langword="true" /> if the origin was found; otherwise, <see langword="false" />.</returns>
		/// <remarks>The origin is the point from which the glyph is drawn for horizontal text.</remarks>
		public bool TryGetHorizontalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin)
		{
			fixed (int* x = &xOrigin)
			fixed (int* y = &yOrigin) {
				var r = HarfBuzzApi.hb_font_get_glyph_h_origin (Handle, glyph, x, y);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Tries to get the vertical origin of a glyph.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <param name="xOrigin">Returns the X coordinate of the origin.</param>
		/// <param name="yOrigin">Returns the Y coordinate of the origin.</param>
		/// <returns><see langword="true" /> if the origin was found; otherwise, <see langword="false" />.</returns>
		/// <remarks>The origin is the point from which the glyph is drawn for vertical text.</remarks>
		public bool TryGetVerticalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin)
		{
			fixed (int* x = &xOrigin)
			fixed (int* y = &yOrigin) {
				var r = HarfBuzzApi.hb_font_get_glyph_v_origin (Handle, glyph, x, y);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the horizontal kerning adjustment between two glyphs.</summary>
		/// <param name="leftGlyph">The glyph ID of the left glyph in the pair.</param>
		/// <param name="rightGlyph">The glyph ID of the right glyph in the pair.</param>
		/// <returns>The kerning adjustment in font units.</returns>
		/// <remarks>Kerning adjusts the spacing between specific pairs of glyphs for better visual appearance.</remarks>
		public int GetHorizontalGlyphKerning (uint leftGlyph, uint rightGlyph)
		{
			var r = HarfBuzzApi.hb_font_get_glyph_h_kerning (Handle, leftGlyph, rightGlyph);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Tries to get the extents (bounding box) of a glyph.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <param name="extents">Returns the extents of the glyph.</param>
		/// <returns><see langword="true" /> if the extents were found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetGlyphExtents (uint glyph, out GlyphExtents extents)
		{
			fixed (GlyphExtents* e = &extents) {
				var r = HarfBuzzApi.hb_font_get_glyph_extents (Handle, glyph, e);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Tries to get the coordinates of a contour point in a glyph.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <param name="pointIndex">The index of the contour point.</param>
		/// <param name="x">Returns the X coordinate of the point.</param>
		/// <param name="y">Returns the Y coordinate of the point.</param>
		/// <returns><see langword="true" /> if the point was found; otherwise, <see langword="false" />.</returns>
		/// <remarks>Contour points are used for TrueType hinting.</remarks>
		public bool TryGetGlyphContourPoint (uint glyph, uint pointIndex, out int x, out int y)
		{
			fixed (int* xPtr = &x)
			fixed (int* yPtr = &y) {
				var r = HarfBuzzApi.hb_font_get_glyph_contour_point (Handle, glyph, pointIndex, xPtr, yPtr);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Tries to get the name of a glyph.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <param name="name">Returns the glyph name if available.</param>
		/// <returns><see langword="true" /> if the name was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public unsafe bool TryGetGlyphName (uint glyph, out string name)
		{
			var pool = ArrayPool<byte>.Shared;
			var buffer = pool.Rent (NameBufferLength);
			try {
				fixed (byte* first = buffer) {
					if (!HarfBuzzApi.hb_font_get_glyph_name (Handle, glyph, first, (uint)buffer.Length)) {
						GC.KeepAlive (this);
						name = string.Empty;
						return false;
					}
					GC.KeepAlive (this);
					name = Marshal.PtrToStringAnsi ((IntPtr)first);
					return true;
				}
			} finally {
				pool.Return (buffer);
			}
		}

		/// <summary>Tries to get the glyph ID for a glyph name.</summary>
		/// <param name="name">The glyph name.</param>
		/// <param name="glyph">Returns the glyph ID if found.</param>
		/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetGlyphFromName (string name, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				var r = HarfBuzzApi.hb_font_get_glyph_from_name (Handle, name, name.Length, g);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Tries to get the glyph ID for a Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="glyph">Returns the glyph ID if found.</param>
		/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetGlyph (int unicode, out uint glyph) =>
			TryGetGlyph ((uint)unicode, 0, out glyph);

		/// <summary>Tries to get the glyph ID for a Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="glyph">Returns the glyph ID if found.</param>
		/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetGlyph (uint unicode, out uint glyph) =>
			TryGetGlyph (unicode, 0, out glyph);

		/// <summary>Tries to get the glyph ID for a Unicode code point with a variation selector.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="variationSelector">The variation selector code point.</param>
		/// <param name="glyph">Returns the glyph ID if found.</param>
		/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetGlyph (int unicode, uint variationSelector, out uint glyph) =>
			TryGetGlyph ((uint)unicode, variationSelector, out glyph);

		/// <summary>Tries to get the glyph ID for a Unicode code point with a variation selector.</summary>
		/// <param name="unicode">The Unicode code point.</param>
		/// <param name="variationSelector">The variation selector code point.</param>
		/// <param name="glyph">Returns the glyph ID if found.</param>
		/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetGlyph (uint unicode, uint variationSelector, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				var r = HarfBuzzApi.hb_font_get_glyph (Handle, unicode, variationSelector, g);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the font extents for the specified text direction.</summary>
		/// <param name="direction">The text direction.</param>
		/// <returns>The font extents for the given direction.</returns>
		/// <remarks>For horizontal text, returns horizontal extents; for vertical text, returns vertical extents.</remarks>
		public FontExtents GetFontExtentsForDirection (Direction direction)
		{
			FontExtents extents;
			HarfBuzzApi.hb_font_get_extents_for_direction (Handle, direction, &extents);
			GC.KeepAlive (this);
			return extents;
		}

		/// <summary>Gets the advance of a glyph for the specified direction.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <param name="direction">The text direction.</param>
		/// <param name="x">Returns the horizontal advance.</param>
		/// <param name="y">Returns the vertical advance.</param>
		/// <remarks>For horizontal text, x contains the advance; for vertical text, y contains the advance.</remarks>
		public void GetGlyphAdvanceForDirection (uint glyph, Direction direction, out int x, out int y)
		{
			fixed (int* xPtr = &x)
			fixed (int* yPtr = &y) {
				HarfBuzzApi.hb_font_get_glyph_advance_for_direction (Handle, glyph, direction, xPtr, yPtr);
			}
			GC.KeepAlive (this);
		}

		/// <summary>Gets the advances of multiple glyphs for the specified direction.</summary>
		/// <param name="glyphs">The span of glyph IDs.</param>
		/// <param name="direction">The text direction.</param>
		/// <returns>An array of advances for the glyphs.</returns>
		/// <remarks />
		public unsafe int[] GetGlyphAdvancesForDirection (ReadOnlySpan<uint> glyphs, Direction direction)
		{
			fixed (uint* firstGlyph = glyphs) {
				return GetGlyphAdvancesForDirection ((IntPtr)firstGlyph, glyphs.Length, direction);
			}
		}

		/// <summary>Gets the advances of multiple glyphs for the specified direction.</summary>
		/// <param name="firstGlyph">A pointer to the first glyph ID.</param>
		/// <param name="count">The number of glyphs.</param>
		/// <param name="direction">The text direction.</param>
		/// <returns>An array of advances for the glyphs.</returns>
		/// <remarks />
		public unsafe int[] GetGlyphAdvancesForDirection (IntPtr firstGlyph, int count, Direction direction)
		{
			var advances = new int[count];

			fixed (int* firstAdvance = advances) {
				HarfBuzzApi.hb_font_get_glyph_advances_for_direction (Handle, direction, (uint)count, (uint*)firstGlyph, 4, firstAdvance, 4);
			}

			GC.KeepAlive (this);
			return advances;
		}

		/// <summary>Tries to get the coordinates of a contour point in a glyph, adjusted for text direction.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <param name="pointIndex">The index of the contour point.</param>
		/// <param name="direction">The text direction for origin adjustment.</param>
		/// <param name="x">Returns the X coordinate of the point.</param>
		/// <param name="y">Returns the Y coordinate of the point.</param>
		/// <returns><see langword="true" /> if the point was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetGlyphContourPointForOrigin (uint glyph, uint pointIndex, Direction direction, out int x, out int y)
		{
			fixed (int* xPtr = &x)
			fixed (int* yPtr = &y) {
				var r = HarfBuzzApi.hb_font_get_glyph_contour_point_for_origin (Handle, glyph, pointIndex, direction, xPtr, yPtr);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Converts a glyph ID to its string representation.</summary>
		/// <param name="glyph">The glyph ID.</param>
		/// <returns>The string representation of the glyph, typically its name if available, otherwise its ID.</returns>
		/// <remarks />
		public unsafe string GlyphToString (uint glyph)
		{
			var pool = ArrayPool<byte>.Shared;
			var buffer = pool.Rent (NameBufferLength);
			try {
				fixed (byte* first = buffer) {
					HarfBuzzApi.hb_font_glyph_to_string (Handle, glyph, first, (uint)buffer.Length);
					GC.KeepAlive (this);
					return Marshal.PtrToStringAnsi ((IntPtr)first);
				}
			} finally {
				pool.Return (buffer);
			}
		}

		/// <summary>Tries to parse a string to get a glyph ID.</summary>
		/// <param name="s">The string representation of the glyph (name or ID).</param>
		/// <param name="glyph">Returns the glyph ID if found.</param>
		/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
		/// <remarks>The string can be a glyph name or a numeric glyph ID.</remarks>
		public bool TryGetGlyphFromString (string s, out uint glyph)
		{
			fixed (uint* g = &glyph) {
				var r = HarfBuzzApi.hb_font_glyph_from_string (Handle, s, -1, g);
				GC.KeepAlive (this);
				return r;
			}
		}

		// Variable font support

		/// <summary>Sets the variation coordinates for this font using an array of tag-value pairs.</summary>
		/// <param name="variations">A read-only span of <see cref="T:HarfBuzzSharp.Variation" /> values specifying the axis tag and design-space value for each axis to set.</param>
		/// <remarks />
		public void SetVariations (ReadOnlySpan<Variation> variations)
		{
			fixed (Variation* ptr = variations) {
				HarfBuzzApi.hb_font_set_variations (Handle, ptr, (uint)variations.Length);
			}
			GC.KeepAlive (this);
		}

		/// <summary>Sets the variation coordinates for this font using design-space values.</summary>
		/// <param name="coords">A read-only span of design-space coordinate values, one per variation axis.</param>
		/// <remarks />
		public void SetVariationCoordsDesign (ReadOnlySpan<float> coords)
		{
			fixed (float* ptr = coords) {
				HarfBuzzApi.hb_font_set_var_coords_design (Handle, ptr, (uint)coords.Length);
			}
			GC.KeepAlive (this);
		}

		/// <summary>Sets the variation coordinates for this font using normalized values.</summary>
		/// <param name="coords">A read-only span of normalized coordinate values in the range [-16384, 16384], one per variation axis.</param>
		/// <remarks />
		public void SetVariationCoordsNormalized (ReadOnlySpan<int> coords)
		{
			fixed (int* ptr = coords) {
				HarfBuzzApi.hb_font_set_var_coords_normalized (Handle, ptr, (uint)coords.Length);
			}
			GC.KeepAlive (this);
		}

		/// <summary>Gets the current normalized variation coordinates for this font.</summary>
		/// <value>An array of normalized variation coordinate values in the range [-16384, 16384], one per variation axis.</value>
		/// <remarks />
		public int[] VariationCoordsNormalized
		{
			get {
				uint length;
				var ptr = HarfBuzzApi.hb_font_get_var_coords_normalized (Handle, &length);
				if (length == 0 || ptr == null) {
					GC.KeepAlive (this);
					return Array.Empty<int> ();
				}

				var count = (int)length;
				var coords = new int[count];
				for (int i = 0; i < count; i++)
					coords[i] = ptr[i];
				GC.KeepAlive (this);
				return coords;
			}
		}

		/// <summary>Fills a span with the current normalized variation coordinates for this font.</summary>
		/// <param name="coords">A span to receive the normalized coordinate values in the range [-16384, 16384].</param>
		/// <returns>The number of normalized coordinates written to <paramref name="coords" />.</returns>
		/// <remarks />
		public int GetVariationCoordsNormalized (Span<int> coords)
		{
			uint length;
			var ptr = HarfBuzzApi.hb_font_get_var_coords_normalized (Handle, &length);
			if (length == 0 || ptr == null) {
				GC.KeepAlive (this);
				return 0;
			}

			var count = Math.Min ((int)length, coords.Length);
			for (int i = 0; i < count; i++)
				coords[i] = ptr[i];
			GC.KeepAlive (this);
			return count;
		}

		/// <summary>Sets the variation coordinates for this font to those of the specified named instance.</summary>
		/// <param name="instanceIndex">The zero-based index of the named instance to apply.</param>
		/// <remarks />
		public void SetVariationNamedInstance (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));
			HarfBuzzApi.hb_font_set_var_named_instance (Handle, (uint)instanceIndex);
			GC.KeepAlive (this);
		}

		/// <summary>Sets the font functions to that of OpenType.</summary>
		/// <remarks />
		public void SetFunctionsOpenType ()
		{
			HarfBuzzApi.hb_ot_font_set_funcs (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Shapes the specified buffer using the current font.</summary>
		/// <param name="buffer">The buffer to shape.</param>
		/// <param name="features">The features to control the shaping process.</param>
		/// <remarks />
		public void Shape (Buffer buffer, params Feature[] features) =>
			Shape (buffer, features, null);

		/// <summary>Shapes the specified buffer using the current font with explicit shapers.</summary>
		/// <param name="buffer">The buffer to shape.</param>
		/// <param name="features">The features to control the shaping process, or <see langword="null" />.</param>
		/// <param name="shapers">A list of shaper names to try, or <see langword="null" /> for the default.</param>
		/// <remarks>Shapers determine which text shaping engine is used (e.g., "ot" for OpenType, "fallback" for the fallback shaper).</remarks>
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

			// Avoid a defensive copy when the caller already handed us an array (the common
			// `params Feature[]` path). hb_shape_full only reads the features, so pinning the
			// caller's array directly is safe and identical.
			var featuresArray = features as Feature[] ?? features?.ToArray ();

			fixed (Feature* fPtr = featuresArray)
			fixed (void** sPtr = shapersPtrs) {
				HarfBuzzApi.hb_shape_full (
					Handle,
					buffer.Handle,
					fPtr,
					(uint)(features?.Count ?? 0),
					sPtr);
			}

			GC.KeepAlive (buffer);
			GC.KeepAlive (this);

			if (shapersPtrs != null) {
				for (var i = 0; i < shapersPtrs.Length; i++) {
					if (shapersPtrs[i] != null)
						Marshal.FreeHGlobal ((IntPtr)shapersPtrs[i]);
				}
			}
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:HarfBuzzSharp.Font" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:HarfBuzzSharp.Font" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Releases the unmanaged resources used.</summary>
		/// <remarks />
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
