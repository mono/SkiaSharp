#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>The delegate that is invoked when <see cref="M:HarfBuzzSharp.Font.TryGetHorizontalFontExtents(HarfBuzzSharp.FontExtents@)" /> or <see cref="M:HarfBuzzSharp.Font.TryGetVerticalFontExtents(HarfBuzzSharp.FontExtents@)" /> is invoked.</summary>
	/// <param name="font">The font.</param>
	/// <param name="fontData">The additional data passed to <see cref="M:HarfBuzzSharp.Font.SetFontFunctions(HarfBuzzSharp.FontFunctions,System.Object,HarfBuzzSharp.ReleaseDelegate)" /> when the functions were set.</param>
	/// <param name="extents">The font extents.</param>
	/// <returns>Returns <see langword="true" /> if the <see cref="T:HarfBuzzSharp.Font" /> has extents; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool FontExtentsDelegate (Font font, object fontData, out FontExtents extents);

	/// <summary>A callback delegate used to retrieve the nominal glyph identifier for a Unicode code point.</summary>
	/// <param name="font">The <see cref="T:HarfBuzzSharp.Font" /> instance invoking this callback.</param>
	/// <param name="fontData">The user-specified font data associated with the font.</param>
	/// <param name="unicode">The Unicode code point to map.</param>
	/// <param name="glyph">When this method returns, contains the glyph identifier for the code point.</param>
	/// <returns><see langword="true" /> if a glyph was found for the code point; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool NominalGlyphDelegate (Font font, object fontData, uint unicode, out uint glyph);

	/// <summary>A callback delegate used to map multiple Unicode code points to their nominal glyph identifiers in a single batch operation.</summary>
	/// <param name="font">The <see cref="T:HarfBuzzSharp.Font" /> instance invoking this callback.</param>
	/// <param name="fontData">The user-specified font data associated with the font.</param>
	/// <param name="count">The number of code points to map.</param>
	/// <param name="codepoints">A read-only span of Unicode code points to map to glyphs.</param>
	/// <param name="glyphs">A span to receive the resulting glyph identifiers.</param>
	/// <returns>The number of code points successfully mapped to glyphs.</returns>
	/// <remarks />
	public delegate uint NominalGlyphsDelegate (Font font, object fontData, uint count, ReadOnlySpan<uint> codepoints, Span<uint> glyphs);

	/// <summary>A callback delegate used to retrieve the glyph identifier for a Unicode code point with a specific variation selector.</summary>
	/// <param name="font">The <see cref="T:HarfBuzzSharp.Font" /> instance invoking this callback.</param>
	/// <param name="fontData">The user-specified font data associated with the font.</param>
	/// <param name="unicode">The Unicode code point to map.</param>
	/// <param name="variationSelector">The Unicode variation selector code point.</param>
	/// <param name="glyph">When this method returns, contains the glyph identifier for the variation.</param>
	/// <returns><see langword="true" /> if a variation glyph was found; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool VariationGlyphDelegate (Font font, object fontData, uint unicode, uint variationSelector, out uint glyph);

	/// <summary>The delegate that is invoked when <see cref="M:HarfBuzzSharp.Font.GetHorizontalGlyphAdvance(System.UInt32)" /> or <see cref="M:HarfBuzzSharp.Font.GetVerticalGlyphAdvance(System.UInt32)" /> is invoked.</summary>
	/// <param name="font">The font.</param>
	/// <param name="fontData">The additional data passed to <see cref="M:HarfBuzzSharp.Font.SetFontFunctions(HarfBuzzSharp.FontFunctions,System.Object,HarfBuzzSharp.ReleaseDelegate)" /> when the functions were set.</param>
	/// <param name="glyph">The glyph.</param>
	/// <returns>Returns the advance amount.</returns>
	/// <remarks />
	public delegate int GlyphAdvanceDelegate (Font font, object fontData, uint glyph);

	/// <summary>A callback delegate used to retrieve the advance widths or heights for multiple glyphs in a single batch operation.</summary>
	/// <param name="font">The <see cref="T:HarfBuzzSharp.Font" /> instance invoking this callback.</param>
	/// <param name="fontData">The user-specified font data associated with the font.</param>
	/// <param name="count">The number of glyphs to process.</param>
	/// <param name="glyphs">A read-only span of glyph identifiers to query.</param>
	/// <param name="advances">A span to receive the advance widths or heights in font units for each glyph.</param>
	/// <remarks />
	public delegate void GlyphAdvancesDelegate (Font font, object fontData, uint count, ReadOnlySpan<uint> glyphs, Span<int> advances);

	/// <summary>A callback delegate used to retrieve the origin coordinates of a glyph for horizontal or vertical layout.</summary>
	/// <param name="font">The <see cref="T:HarfBuzzSharp.Font" /> instance invoking this callback.</param>
	/// <param name="fontData">The user-specified font data associated with the font.</param>
	/// <param name="glyph">The glyph identifier to query.</param>
	/// <param name="x">When this method returns, contains the X coordinate of the glyph origin in font units.</param>
	/// <param name="y">When this method returns, contains the Y coordinate of the glyph origin in font units.</param>
	/// <returns><see langword="true" /> if the origin was found; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool GlyphOriginDelegate (Font font, object fontData, uint glyph, out int x, out int y);

	/// <summary>Represents a callback method that retrieves the kerning adjustment between two glyphs.</summary>
	/// <param name="font">The font being queried.</param>
	/// <param name="fontData">User-supplied font data.</param>
	/// <param name="firstGlyph">The first glyph in the pair.</param>
	/// <param name="secondGlyph">The second glyph in the pair.</param>
	/// <returns>The horizontal kerning adjustment in font units.</returns>
	/// <remarks />
	public delegate int GlyphKerningDelegate (Font font, object fontData, uint firstGlyph, uint secondGlyph);

	/// <summary>Represents a callback method that retrieves the extents (bounding box) of a glyph.</summary>
	/// <param name="font">The font being queried.</param>
	/// <param name="fontData">User-supplied font data.</param>
	/// <param name="glyph">The glyph identifier.</param>
	/// <param name="extents">When this method returns, contains the glyph extents.</param>
	/// <returns><see langword="true" /> if the extents were found; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool GlyphExtentsDelegate (Font font, object fontData, uint glyph, out GlyphExtents extents);

	/// <summary>A callback delegate used to retrieve the coordinates of a specific contour point within a glyph outline.</summary>
	/// <param name="font">The <see cref="T:HarfBuzzSharp.Font" /> instance invoking this callback.</param>
	/// <param name="fontData">The user-specified font data associated with the font.</param>
	/// <param name="glyph">The glyph identifier to query.</param>
	/// <param name="pointIndex">The zero-based index of the contour point to retrieve.</param>
	/// <param name="x">When this method returns, contains the X coordinate of the contour point in font units.</param>
	/// <param name="y">When this method returns, contains the Y coordinate of the contour point in font units.</param>
	/// <returns><see langword="true" /> if the contour point was found; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool GlyphContourPointDelegate (Font font, object fontData, uint glyph, uint pointIndex, out int x, out int y);

	/// <summary>A callback delegate used to retrieve the name of a glyph.</summary>
	/// <param name="font">The <see cref="T:HarfBuzzSharp.Font" /> instance invoking this callback.</param>
	/// <param name="fontData">The user-specified font data associated with the font.</param>
	/// <param name="glyph">The glyph identifier to query.</param>
	/// <param name="name">When this method returns, contains the name of the glyph.</param>
	/// <returns><see langword="true" /> if the glyph name was found; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool GlyphNameDelegate (Font font, object fontData, uint glyph, out string name);

	/// <summary>Represents a callback method that retrieves a glyph identifier from a glyph name.</summary>
	/// <param name="font">The font being queried.</param>
	/// <param name="fontData">User-supplied font data.</param>
	/// <param name="name">The glyph name to look up.</param>
	/// <param name="glyph">When this method returns, contains the glyph identifier if found.</param>
	/// <returns><see langword="true" /> if the glyph was found; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool GlyphFromNameDelegate (Font font, object fontData, string name, out uint glyph);

	internal static unsafe partial class DelegateProxies
	{
		// internal proxy implementations

		private static partial bool FontGetFontExtentsProxyImplementation (IntPtr font, void* font_data, FontExtents* extents, void* user_data)
		{
			var del = GetMulti<FontExtentsDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			var result = del.Invoke (userData.Font, userData.FontData, out var extentsManaged);
			if (extents != null)
				*extents = extentsManaged;
			return result;
		}

		private static partial bool FontGetNominalGlyphProxyImplementation (IntPtr font, void* font_data, uint unicode, uint* glyph, void* user_data)
		{
			var del = GetMulti<NominalGlyphDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			var result = del.Invoke (userData.Font, userData.FontData, unicode, out var glyphManaged);
			if (glyph != null)
				*glyph = glyphManaged;
			return result;
		}

		private static partial uint FontGetNominalGlyphsProxyImplementation (IntPtr font, void* font_data, uint count, uint* first_unicode, uint unicode_stride, uint* first_glyph, uint glyph_stride, void* user_data)
		{
			var del = GetMulti<NominalGlyphsDelegate> ((IntPtr)user_data, out _);
			var unicodes = new ReadOnlySpan<uint> (first_unicode, (int)count);
			var glyphs = new Span<uint> (first_glyph, (int)count);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			return del.Invoke (userData.Font, userData.FontData, count, unicodes, glyphs);
		}

		private static partial bool FontGetVariationGlyphProxyImplementation (IntPtr font, void* font_data, uint unicode, uint variation_selector, uint* glyph, void* user_data)
		{
			var del = GetMulti<VariationGlyphDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			var result = del.Invoke (userData.Font, userData.FontData, unicode, variation_selector, out var glyphManaged);
			if (glyph != null)
				*glyph = glyphManaged;
			return result;
		}

		private static partial int FontGetGlyphAdvanceProxyImplementation (IntPtr font, void* font_data, uint glyph, void* user_data)
		{
			var del = GetMulti<GlyphAdvanceDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			return del.Invoke (userData.Font, userData.FontData, glyph);
		}

		private static partial void FontGetGlyphAdvancesProxyImplementation (IntPtr font, void* font_data, uint count, uint* first_glyph, uint glyph_stride, int* first_advance, uint advance_stride, void* user_data)
		{
			var del = GetMulti<GlyphAdvancesDelegate> ((IntPtr)user_data, out _);
			var glyphs = new ReadOnlySpan<uint> (first_glyph, (int)count);
			var advances = new Span<int> (first_advance, (int)count);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			del.Invoke (userData.Font, userData.FontData, count, glyphs, advances);
		}

		private static partial bool FontGetGlyphOriginProxyImplementation (IntPtr font, void* font_data, uint glyph, int* x, int* y, void* user_data)
		{
			var del = GetMulti<GlyphOriginDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, out var xManaged, out var yManaged);
			if (x != null)
				*x = xManaged;
			if (y != null)
				*y = yManaged;
			return result;
		}

		private static partial int FontGetGlyphKerningProxyImplementation (IntPtr font, void* font_data, uint first_glyph, uint second_glyph, void* user_data)
		{
			var del = GetMulti<GlyphKerningDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			return del.Invoke (userData.Font, userData.FontData, first_glyph, second_glyph);
		}

		private static partial bool FontGetGlyphExtentsProxyImplementation (IntPtr font, void* font_data, uint glyph, GlyphExtents* extents, void* user_data)
		{
			var del = GetMulti<GlyphExtentsDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, out var extentsManaged);
			if (extents != null)
				*extents = extentsManaged;
			return result;
		}

		private static partial bool FontGetGlyphContourPointProxyImplementation (IntPtr font, void* font_data, uint glyph, uint point_index, int* x, int* y, void* user_data)
		{
			var del = GetMulti<GlyphContourPointDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, point_index, out var xManaged, out var yManaged);
			if (x != null)
				*x = xManaged;
			if (y != null)
				*y = yManaged;
			return result;
		}

		private static partial bool FontGetGlyphNameProxyImplementation (IntPtr font, void* font_data, uint glyph, void* name, uint size, void* user_data)
		{
			var del = GetMulti<GlyphNameDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, out var realName);

			var nameSpan = realName.AsSpan ();
			var bufferSpan = new Span<char> (name, (int)size);
			nameSpan.CopyTo (bufferSpan);

			return result;
		}

		private static partial bool FontGetGlyphFromNameProxyImplementation (IntPtr font, void* font_data, void* name, int len, uint* glyph, void* user_data)
		{
			var del = GetMulti<GlyphFromNameDelegate> ((IntPtr)user_data, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)font_data, out _);

			var actualName = len < 0
				? new string ((char*)name)
				: new string ((char*)name, 0, len);

			var result = del.Invoke (userData.Font, userData.FontData, actualName, out var glyphManaged);
			if (glyph != null)
				*glyph = glyphManaged;
			return result;
		}
	}
}
