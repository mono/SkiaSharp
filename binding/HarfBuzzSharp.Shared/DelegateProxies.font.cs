using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	// public delegates

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

	// internal proxy delegates

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontExtentsProxyDelegate (IntPtr font, IntPtr fontData, out FontExtents extents, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool NominalGlyphProxyDelegate (IntPtr font, IntPtr fontData, uint unicode, out uint glyph, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate uint NominalGlyphsProxyDelegate (IntPtr font, IntPtr fontData, uint count, uint* firstUnicode, uint unicodeStride, uint* firstGlyph, uint glyphStride, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool VariationGlyphProxyDelegate (IntPtr font, IntPtr fontData, uint unicode, uint variationSelector, out uint glyph, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate int GlyphAdvanceProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void GlyphAdvancesProxyDelegate (IntPtr font, IntPtr fontData, uint count, uint* firstGlyph, uint glyphStride, int* firstAdvance, uint advanceStride, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool GlyphOriginProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, out int x, out int y, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate int GlyphKerningProxyDelegate (IntPtr font, IntPtr fontData, uint firstGlyph, uint secondGlyph, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool GlyphExtentsProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, out GlyphExtents extents, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool GlyphContourPointProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, uint pointIndex, out int x, out int y, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool GlyphNameProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, char* name, int size, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool GlyphFromNameProxyDelegate (IntPtr font, IntPtr fontData, char* name, int len, out uint glyph, IntPtr context);

	internal static unsafe partial class DelegateProxies
	{
		// references to the proxy implementations
		public static readonly FontExtentsProxyDelegate FontExtentsProxy = FontExtentsProxyImplementation;
		public static readonly NominalGlyphProxyDelegate NominalGlyphProxy = NominalGlyphProxyImplementation;
		public static readonly VariationGlyphProxyDelegate VariationGlyphProxy = VariationGlyphProxyImplementation;
		public static readonly NominalGlyphsProxyDelegate NominalGlyphsProxy = NominalGlyphsProxyImplementation;
		public static readonly GlyphAdvanceProxyDelegate GlyphAdvanceProxy = GlyphAdvanceProxyImplementation;
		public static readonly GlyphAdvancesProxyDelegate GlyphAdvancesProxy = GlyphAdvancesProxyImplementation;
		public static readonly GlyphOriginProxyDelegate GlyphOriginProxy = GlyphOriginProxyImplementation;
		public static readonly GlyphKerningProxyDelegate GlyphKerningProxy = GlyphKerningProxyImplementation;
		public static readonly GlyphExtentsProxyDelegate GlyphExtentsProxy = GlyphExtentsProxyImplementation;
		public static readonly GlyphContourPointProxyDelegate GlyphContourPointProxy = GlyphContourPointProxyImplementation;
		public static readonly unsafe GlyphNameProxyDelegate GlyphNameProxy = GlyphNameProxyImplementation;
		public static readonly GlyphFromNameProxyDelegate GlyphFromNameProxy = GlyphFromNameProxyImplementation;

		// helper methods

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static T GetFontData<T> (IntPtr context)
		{
			var del = GetMulti<UserDataDelegate> (context, out _);
			var userData = del.Invoke ();
			return (T)userData;
		}

		// internal proxy implementations

		[MonoPInvokeCallback (typeof (FontExtentsProxyDelegate))]
		private static bool FontExtentsProxyImplementation (IntPtr font, IntPtr fontData, out FontExtents extents, IntPtr context)
		{
			var del = GetMulti<FontExtentsDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, out extents);
		}

		[MonoPInvokeCallback (typeof (NominalGlyphProxyDelegate))]
		private static bool NominalGlyphProxyImplementation (IntPtr font, IntPtr fontData, uint unicode, out uint glyph, IntPtr context)
		{
			var del = GetMulti<NominalGlyphDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, unicode, out glyph);
		}

		[MonoPInvokeCallback (typeof (NominalGlyphsProxyDelegate))]
		private static uint NominalGlyphsProxyImplementation (IntPtr font, IntPtr fontData, uint count, uint* firstUnicode, uint unicodeStride, uint* firstGlyph, uint glyphStride, IntPtr context)
		{
			var del = GetMulti<NominalGlyphsDelegate> (context, out _);
			var unicodes = new ReadOnlySpan<uint> (firstUnicode, (int)count);
			var glyphs = new Span<uint> (firstGlyph, (int)count);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, count, unicodes, glyphs);
		}

		[MonoPInvokeCallback (typeof (VariationGlyphProxyDelegate))]
		private static bool VariationGlyphProxyImplementation (IntPtr font, IntPtr fontData, uint unicode, uint variationSelector, out uint glyph, IntPtr context)
		{
			var del = GetMulti<VariationGlyphDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, unicode, variationSelector, out glyph);
		}

		[MonoPInvokeCallback (typeof (GlyphAdvanceProxyDelegate))]
		private static int GlyphAdvanceProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, IntPtr context)
		{
			var del = GetMulti<GlyphAdvanceDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, glyph);
		}

		[MonoPInvokeCallback (typeof (GlyphAdvancesProxyDelegate))]
		private static void GlyphAdvancesProxyImplementation (IntPtr font, IntPtr fontData, uint count, uint* firstGlyph, uint glyphStride, int* firstAdvance, uint advanceStride, IntPtr context)
		{
			var del = GetMulti<GlyphAdvancesDelegate> (context, out _);
			var glyphs = new ReadOnlySpan<uint> (firstGlyph, (int)count);
			var advances = new Span<int> (firstAdvance, (int)count);
			var userData = GetFontData<FontUserData> (fontData);
			del.Invoke (userData.Font, userData.FontData, count, glyphs, advances);
		}

		[MonoPInvokeCallback (typeof (GlyphOriginProxyDelegate))]
		private static bool GlyphOriginProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, out int x, out int y, IntPtr context)
		{
			var del = GetMulti<GlyphOriginDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, glyph, out x, out y);
		}

		[MonoPInvokeCallback (typeof (GlyphKerningProxyDelegate))]
		private static int GlyphKerningProxyImplementation (IntPtr font, IntPtr fontData, uint firstGlyph, uint secondGlyph, IntPtr context)
		{
			var del = GetMulti<GlyphKerningDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, firstGlyph, secondGlyph);
		}

		[MonoPInvokeCallback (typeof (GlyphExtentsProxyDelegate))]
		private static bool GlyphExtentsProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, out GlyphExtents extents, IntPtr context)
		{
			var del = GetMulti<GlyphExtentsDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, glyph, out extents);
		}

		[MonoPInvokeCallback (typeof (GlyphContourPointProxyDelegate))]
		private static bool GlyphContourPointProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, uint pointIndex, out int x, out int y, IntPtr context)
		{
			var del = GetMulti<GlyphContourPointDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			return del.Invoke (userData.Font, userData.FontData, glyph, pointIndex, out x, out y);
		}

		[MonoPInvokeCallback (typeof (GlyphNameProxyDelegate))]
		private static bool GlyphNameProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, char* nameBuffer, int size, IntPtr context)
		{
			var del = GetMulti<GlyphNameDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);
			var name = new Span<char> (nameBuffer, size);
			var result = del.Invoke (userData.Font, userData.FontData, glyph, out var realName);

			var nameSpan = realName.AsSpan ();
			var bufferSpan = new Span<char> (nameBuffer, size);
			nameSpan.CopyTo (bufferSpan);

			return result;
		}

		[MonoPInvokeCallback (typeof (GlyphFromNameProxyDelegate))]
		private static bool GlyphFromNameProxyImplementation (IntPtr font, IntPtr fontData, char* name, int len, out uint glyph, IntPtr context)
		{
			var del = GetMulti<GlyphFromNameDelegate> (context, out _);
			var userData = GetFontData<FontUserData> (fontData);

			var actualName = len < 0
				? new string (name)
				: new string (name, 0, len);

			return del.Invoke (userData.Font, userData.FontData, actualName, out glyph);
		}
	}
}
