#nullable disable

using System;

namespace HarfBuzzSharp
{
	public delegate bool FontExtentsDelegate (Font font, object fontData, out FontExtents extents);

	public delegate bool NominalGlyphDelegate (Font font, object fontData, uint unicode, out uint glyph);

	public delegate uint NominalGlyphsDelegate (Font font, object fontData, uint count, ReadOnlySpan<uint> codepoints, Span<uint> glyphs);

	public delegate bool VariationGlyphDelegate (Font font, object fontData, uint unicode, uint variationSelector, out uint glyph);

	public delegate int GlyphAdvanceDelegate (Font font, object fontData, uint glyph);

	public delegate void GlyphAdvancesDelegate (Font font, object fontData, uint count, ReadOnlySpan<uint> glyphs, Span<int> advances);

	public delegate bool GlyphOriginDelegate (Font font, object fontData, uint glyph, out int x, out int y);

	public delegate int GlyphKerningDelegate (Font font, object fontData, uint firstGlyph, uint secondGlyph);

	public delegate bool GlyphExtentsDelegate (Font font, object fontData, uint glyph, out GlyphExtents extents);

	public delegate bool GlyphContourPointDelegate (Font font, object fontData, uint glyph, uint pointIndex, out int x, out int y);

	public delegate bool GlyphNameDelegate (Font font, object fontData, uint glyph, out string name);

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
