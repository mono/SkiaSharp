using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate bool FontExtentsProxyDelegate (IntPtr font, IntPtr fontData, out FontExtents extents,
		IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate bool NominalGlyphProxyDelegate (IntPtr font, IntPtr fontData, uint unicode,
		out uint glyph, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate bool VariationGlyphProxyDelegate (IntPtr font, IntPtr fontData, uint unicode,
		uint variationSelector, out uint glyph, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate uint NominalGlyphsProxyDelegate (IntPtr font, IntPtr fontData, uint count,
		IntPtr firstUnicode, uint unicodeStride, IntPtr firstGlyph, uint glyphStride, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate int GlyphAdvanceProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void GlyphAdvancesProxyDelegate (IntPtr font, IntPtr fontData, uint count,
		IntPtr firstGlyph, uint glyphStride, IntPtr firstAdvance, uint advanceStride, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate bool GlyphOriginProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, out int x,
		out int y, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate int GlyphKerningProxyDelegate (IntPtr font, IntPtr fontData, uint firstGlyph,
		uint secondGlyph, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate bool GlyphExtentsProxyDelegate (IntPtr font, IntPtr fontData, uint glyph,
		out GlyphExtents extents, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate bool GlyphContourPointProxyDelegate (IntPtr font, IntPtr fontData, uint glyph,
		uint pointIndex, out int x, out int y, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate bool GlyphNameProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, char* name,
		int size, IntPtr context);
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate bool GlyphFromNameProxyDelegate (IntPtr font, IntPtr fontData, string name,
		int len, out uint glyph, IntPtr context);

	public class FontFunctions : NativeObject
	{
		private static readonly Lazy<FontFunctions> emptyFontFunctions =
			new Lazy<FontFunctions> (() => new StaticFontFunctions (HarfBuzzApi.hb_font_funcs_get_empty ()));

		private static readonly FontExtentsProxyDelegate FontExtentsProxy = FontExtentsProxyImplementation;
		private static readonly NominalGlyphProxyDelegate NominalGlyphProxy = NominalGlyphProxyImplementation;
		private static readonly VariationGlyphProxyDelegate VariationGlyphProxy = VariationGlyphProxyImplementation;
		private static readonly NominalGlyphsProxyDelegate NominalGlyphsProxy = NominalGlyphsProxyImplementation;
		private static readonly GlyphAdvanceProxyDelegate GlyphAdvanceProxy = GlyphAdvanceProxyImplementation;
		private static readonly GlyphAdvancesProxyDelegate GlyphAdvancesProxy = GlyphAdvancesProxyImplementation;
		private static readonly GlyphOriginProxyDelegate GlyphOriginProxy = GlyphOriginProxyImplementation;
		private static readonly GlyphKerningProxyDelegate GlyphKerningProxy = GlyphKerningProxyImplementation;
		private static readonly GlyphExtentsProxyDelegate GlyphExtentsProxy = GlyphExtentsProxyImplementation;
		private static readonly GlyphContourPointProxyDelegate GlyphContourPointProxy =
			GlyphContourPointProxyImplementation;
		private static readonly unsafe GlyphNameProxyDelegate GlyphNameProxy = GlyphNameProxyImplementation;
		private static readonly GlyphFromNameProxyDelegate GlyphFromNameProxy = GlyphFromNameProxyImplementation;

		public delegate bool FontExtentsDelegate (Font font, object fontData, out FontExtents extents);
		public delegate bool NominalGlyphDelegate (Font font, object fontData, uint unicode, out uint glyph);
		public delegate bool VariationGlyphDelegate (Font font, object fontData, uint unicode, uint variationSelector,
			out uint glyph);
		public delegate uint NominalGlyphsDelegate (Font font, object fontData, uint count,
			ReadOnlySpan<uint> codepoints, Span<uint> glyphs);
		public delegate int GlyphAdvanceDelegate (Font font, object fontData, uint glyph);
		public delegate void GlyphAdvancesDelegate (Font font, object fontData, uint count, ReadOnlySpan<uint> glyphs,
			Span<int> advances);
		public delegate bool GlyphOriginDelegate (Font font, object fontData, uint glyph, out int x, out int y);
		public delegate int GlyphKerningDelegate (Font font, object fontData, uint firstGlyph, uint secondGlyph);
		public delegate bool GlyphExtentsDelegate (Font font, object fontData, uint glyph, out GlyphExtents extents);
		public delegate bool GlyphContourPointDelegate (Font font, object fontData, uint glyph, uint pointIndex,
			out int x, out int y);
		public unsafe delegate bool GlyphNameDelegate (Font font, object fontData, uint glyph, char* nameBuffer, int size);
		public delegate bool GlyphFromNameDelegate (Font font, object fontData, string name, out uint glyph);

		public FontFunctions () : this (IntPtr.Zero)
		{
			Handle = HarfBuzzApi.hb_font_funcs_create ();
		}

		internal FontFunctions (IntPtr handle) : base (handle) { }

		internal FontFunctions (IntPtr handle, bool zero) : base (handle, zero) { }

		public static FontFunctions Empty => emptyFontFunctions.Value;

		public bool IsImmutable => HarfBuzzApi.hb_font_funcs_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_font_funcs_make_immutable (Handle);

		private static T GetUserData<T> (IntPtr context)
		{
			var del = DelegateProxies.GetMulti<UserDataDelegate> (context, out _);

			var userData = del.Invoke ();

			return (T)userData;
		}

		[MonoPInvokeCallback (typeof (FontExtentsProxyDelegate))]
		private static bool FontExtentsProxyImplementation (IntPtr font, IntPtr fontData, out FontExtents extents, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<FontExtentsDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, out extents);
		}

		public void SetHorizontalFontExtentsDelegate (FontExtentsDelegate del, ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_font_h_extents_func (Handle, FontExtentsProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalFontExtentsDelegate (FontExtentsDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_font_v_extents_func (Handle, FontExtentsProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (NominalGlyphProxyDelegate))]
		private static bool NominalGlyphProxyImplementation (IntPtr font, IntPtr fontData, uint unicode, out uint glyph, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<NominalGlyphDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, unicode, out glyph);
		}

		public void SetNominalGlyphDelegate (NominalGlyphDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_nominal_glyph_func (Handle, NominalGlyphProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (NominalGlyphsProxyDelegate))]
		private static uint NominalGlyphsProxyImplementation (IntPtr font, IntPtr fontData, uint count,
			IntPtr firstUnicode, uint unicodeStride, IntPtr firstGlyph, uint glyphStride, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<NominalGlyphsDelegate> (context, out _);

			unsafe {
				var glyphs = new Span<uint> ((void*)firstGlyph, (int)count);

				var unicodes = new ReadOnlySpan<uint> ((void*)firstUnicode, (int)count);

				var userData = GetUserData<FontUserData> (fontData);

				var glyphCount = del.Invoke (userData.Font, userData.FontData, count, unicodes, glyphs);

				return glyphCount;
			}
		}

		public void SetNominalGlyphsDelegate (NominalGlyphsDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_nominal_glyphs_func (Handle, NominalGlyphsProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (VariationGlyphProxyDelegate))]
		private static bool VariationGlyphProxyImplementation (IntPtr font, IntPtr fontData, uint unicode,
			uint variationSelector, out uint glyph, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<VariationGlyphDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, unicode, variationSelector, out glyph);
		}

		public void SetVariationGlyphDelegate (VariationGlyphDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_variation_glyph_func (Handle, VariationGlyphProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphAdvanceProxyDelegate))]
		private static int GlyphAdvanceProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<GlyphAdvanceDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, glyph);
		}

		public void SetHorizontalGlyphAdvanceDelegate (GlyphAdvanceDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_advance_func (Handle, GlyphAdvanceProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphAdvanceDelegate (GlyphAdvanceDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_v_advance_func (Handle, GlyphAdvanceProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphAdvancesProxyDelegate))]
		private static void GlyphAdvancesProxyImplementation (IntPtr font, IntPtr fontData, uint count,
			IntPtr firstGlyph, uint glyphStride, IntPtr firstAdvance, uint advanceStride, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<GlyphAdvancesDelegate> (context, out _);

			unsafe {
				var advances = new Span<int> ((void*)firstAdvance, (int)count);

				var glyphs = new ReadOnlySpan<uint> ((void*)firstGlyph, (int)count);

				var userData = GetUserData<FontUserData> (fontData);

				del.Invoke (userData.Font, userData.FontData, count, glyphs, advances);
			}
		}

		public void SetHorizontalGlyphAdvancesDelegate (GlyphAdvancesDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_advances_func (Handle, GlyphAdvancesProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphAdvancesDelegate (GlyphAdvancesDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_v_advances_func (Handle, GlyphAdvancesProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphOriginProxyDelegate))]
		private static bool GlyphOriginProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, out int x, out int y, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<GlyphOriginDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, glyph, out x, out y);
		}

		public void SetHorizontalGlyphOriginDelegate (GlyphOriginDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_origin_func (Handle, GlyphOriginProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphOriginDelegate (GlyphOriginDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_v_origin_func (Handle, GlyphOriginProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphKerningProxyDelegate))]
		private static int GlyphKerningProxyImplementation (IntPtr font, IntPtr fontData, uint firstGlyph, uint secondGlyph, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<GlyphKerningDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, firstGlyph, secondGlyph);
		}

		public void SetHorizontalGlyphKerningDelegate (GlyphKerningDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_kerning_func (Handle, GlyphKerningProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphExtentsProxyDelegate))]
		private static bool GlyphExtentsProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, out GlyphExtents extents, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<GlyphExtentsDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, glyph, out extents);
		}

		public void SetGlyphExtentsDelegate (GlyphExtentsDelegate del, ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_extents_func (Handle, GlyphExtentsProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphContourPointProxyDelegate))]
		private static bool GlyphContourPointProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, uint pointIndex, out int x, out int y, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<GlyphContourPointDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, glyph, pointIndex, out x, out y);
		}

		public void SetGlyphContourPointDelegate (GlyphContourPointDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_contour_point_func (Handle, GlyphContourPointProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphNameProxyDelegate))]
		private static unsafe bool GlyphNameProxyImplementation (IntPtr font, IntPtr fontData, uint glyph,
			char* nameBuffer, int size, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<GlyphNameDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, glyph, nameBuffer, size);
		}

		public void SetGlyphNameDelegate (GlyphNameDelegate del, ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_name_func (Handle, GlyphNameProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphFromNameProxyDelegate))]
		private static bool GlyphFromNameProxyImplementation (IntPtr font, IntPtr fontData, string name,
			int len, out uint glyph, IntPtr context)
		{
			var del = DelegateProxies.GetMulti<GlyphFromNameDelegate> (context, out _);

			var userData = GetUserData<FontUserData> (fontData);

			return del.Invoke (userData.Font, userData.FontData, name, out glyph);
		}

		public void SetGlyphFromNameDelegate (GlyphFromNameDelegate del,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var ctx = DelegateProxies.CreateMulti (del, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_from_name_func (Handle, GlyphFromNameProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_font_funcs_destroy (Handle);
			}
		}

		private class StaticFontFunctions : FontFunctions
		{
			public StaticFontFunctions (IntPtr handle)
				: base (handle)
			{
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
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
