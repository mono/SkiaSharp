#nullable disable
// ReSharper disable PartialMethodParameterNameMismatch

using System;

namespace HarfBuzzSharp
{
	/// <summary>
	/// The delegate that is invoked when <see cref="M:HarfBuzzSharp.Font.TryGetHorizontalFontExtents(HarfBuzzSharp.FontExtents@)" /> or <see cref="M:HarfBuzzSharp.Font.TryGetVerticalFontExtents(HarfBuzzSharp.FontExtents@)" /> is invoked.
	/// </summary>
	/// <param name="font">The font.</param>
	/// <param name="fontData">The additional data passed to <see cref="M:HarfBuzzSharp.Font.SetFontFunctions(HarfBuzzSharp.FontFunctions,System.Object,HarfBuzzSharp.ReleaseDelegate)" /> when the functions were set.</param>
	/// <param name="extents">The font extents.</param>
	/// <returns>Return true if the <see cref="T:HarfBuzzSharp.Font" /> has extents, otherwise false.</returns>
	/// <remarks></remarks>
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

		private static partial bool FontGetFontExtentsProxyImplementation (IntPtr font, void* fontData, FontExtents* extents, void* context)
		{
			var del = GetMulti<FontExtentsDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			var result = del.Invoke (userData.Font, userData.FontData, out var extentsManaged);
			if (extents != null)
				*extents = extentsManaged;
			return result;
		}

		private static partial bool FontGetNominalGlyphProxyImplementation (IntPtr font, void* fontData, uint unicode, uint* glyph, void* context)
		{
			var del = GetMulti<NominalGlyphDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			var result = del.Invoke (userData.Font, userData.FontData, unicode, out var glyphManaged);
			if (glyph != null)
				*glyph = glyphManaged;
			return result;
		}

		private static partial uint FontGetNominalGlyphsProxyImplementation (IntPtr font, void* fontData, uint count, uint* firstUnicode, uint unicodeStride, uint* firstGlyph, uint glyphStride, void* context)
		{
			var del = GetMulti<NominalGlyphsDelegate> ((IntPtr)context, out _);
			var unicodes = new ReadOnlySpan<uint> (firstUnicode, (int)count);
			var glyphs = new Span<uint> (firstGlyph, (int)count);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			return del.Invoke (userData.Font, userData.FontData, count, unicodes, glyphs);
		}

		private static partial bool FontGetVariationGlyphProxyImplementation (IntPtr font, void* fontData, uint unicode, uint variationSelector, uint* glyph, void* context)
		{
			var del = GetMulti<VariationGlyphDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			var result = del.Invoke (userData.Font, userData.FontData, unicode, variationSelector, out var glyphManaged);
			if (glyph != null)
				*glyph = glyphManaged;
			return result;
		}

		private static partial int FontGetGlyphAdvanceProxyImplementation (IntPtr font, void* fontData, uint glyph, void* context)
		{
			var del = GetMulti<GlyphAdvanceDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			return del.Invoke (userData.Font, userData.FontData, glyph);
		}

		private static partial void FontGetGlyphAdvancesProxyImplementation (IntPtr font, void* fontData, uint count, uint* firstGlyph, uint glyphStride, int* firstAdvance, uint advanceStride, void* context)
		{
			var del = GetMulti<GlyphAdvancesDelegate> ((IntPtr)context, out _);
			var glyphs = new ReadOnlySpan<uint> (firstGlyph, (int)count);
			var advances = new Span<int> (firstAdvance, (int)count);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			del.Invoke (userData.Font, userData.FontData, count, glyphs, advances);
		}

		private static partial bool FontGetGlyphOriginProxyImplementation (IntPtr font, void* fontData, uint glyph, int* x, int* y, void* context)
		{
			var del = GetMulti<GlyphOriginDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, out var xManaged, out var yManaged);
			if (x != null)
				*x = xManaged;
			if (y != null)
				*y = yManaged;
			return result;
		}

		private static partial int FontGetGlyphKerningProxyImplementation (IntPtr font, void* fontData, uint firstGlyph, uint secondGlyph, void* context)
		{
			var del = GetMulti<GlyphKerningDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			return del.Invoke (userData.Font, userData.FontData, firstGlyph, secondGlyph);
		}

		private static partial bool FontGetGlyphExtentsProxyImplementation (IntPtr font, void* fontData, uint glyph, GlyphExtents* extents, void* context)
		{
			var del = GetMulti<GlyphExtentsDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, out var extentsManaged);
			if (extents != null)
				*extents = extentsManaged;
			return result;
		}

		private static partial bool FontGetGlyphContourPointProxyImplementation (IntPtr font, void* fontData, uint glyph, uint pointIndex, int* x, int* y, void* context)
		{
			var del = GetMulti<GlyphContourPointDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, pointIndex, out var xManaged, out var yManaged);
			if (x != null)
				*x = xManaged;
			if (y != null)
				*y = yManaged;
			return result;
		}

		private static partial bool FontGetGlyphNameProxyImplementation (IntPtr font, void* fontData, uint glyph, void* nameBuffer, uint size, void* context)
		{
			var del = GetMulti<GlyphNameDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, out var realName);

			var nameSpan = realName.AsSpan ();
			var bufferSpan = new Span<char> (nameBuffer, (int)size);
			nameSpan.CopyTo (bufferSpan);

			return result;
		}

		private static partial bool FontGetGlyphFromNameProxyImplementation (IntPtr font, void* fontData, void* name, int len, uint* glyph, void* context)
		{
			var del = GetMulti<GlyphFromNameDelegate> ((IntPtr)context, out _);
			var userData = GetMultiUserData<FontUserData> ((IntPtr)fontData, out _);

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
