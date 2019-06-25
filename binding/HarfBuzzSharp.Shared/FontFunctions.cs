using System;

namespace HarfBuzzSharp
{
	internal delegate bool FontExtentsProxyDelegate (IntPtr font, IntPtr fontData, out FontExtents extents,
		IntPtr context);
	internal delegate bool NominalGlyphProxyDelegate (IntPtr font, IntPtr fontData, uint unicode,
		out uint glyph, IntPtr context);
	internal delegate bool VariationGlyphProxyDelegate (IntPtr font, IntPtr fontData, uint unicode,
		uint variationSelector, out uint glyph, IntPtr context);
	internal delegate uint NominalGlyphsProxyDelegate (IntPtr font, IntPtr fontData, uint count,
		IntPtr firstUnicode, uint unicodeStride, out IntPtr firstGlyph, out uint glyphStride, IntPtr context);
	internal delegate int GlyphAdvanceProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, IntPtr context);
	internal delegate void GlyphAdvancesProxyDelegate (IntPtr font, IntPtr fontData, uint count,
		IntPtr firstGlyph, uint glyphStride, out IntPtr firstAdvance, out uint advanceStride, IntPtr context);
	internal delegate bool GlyphOriginProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, out int x,
		out int y, IntPtr context);
	internal delegate int GlyphKerningProxyDelegate (IntPtr font, IntPtr fontData, uint firstGlyph,
		uint secondGlyph, IntPtr context);
	internal delegate bool GlyphExtentsProxyDelegate (IntPtr font, IntPtr fontData, uint glyph,
		out GlyphExtents extents, IntPtr context);
	internal delegate bool GlyphContourPointProxyDelegate (IntPtr font, IntPtr fontData, uint glyph,
		uint pointIndex, out int x, out int y, IntPtr context);
	internal unsafe delegate bool GlyphNameProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, char* name,
		int size, IntPtr context);
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
			ReadOnlySpan<uint> codepoints, out ReadOnlySpan<uint> glyphs);
		public delegate int GlyphAdvanceDelegate (Font font, object fontData, uint glyph);
		public delegate void GlyphAdvancesDelegate (Font font, object fontData, uint count, ReadOnlySpan<uint> glyphs,
			out ReadOnlySpan<int> advances);
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

		internal Font Font { get; set; }

		internal object FontData { get; set; }

		public bool IsImmutable => HarfBuzzApi.hb_font_funcs_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_font_funcs_make_immutable (Handle);

		[MonoPInvokeCallback (typeof (FontExtentsProxyDelegate))]
		private static bool FontExtentsProxyImplementation (IntPtr font, IntPtr fontData, out FontExtents extents, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (FontExtentsDelegate)multi.Invoke (typeof (FontExtentsDelegate));
			return del.Invoke (null, null, out extents);
		}

		public void SetHorizontalFontExtentsDelegate (FontExtentsDelegate del, object context = null, ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new FontExtentsDelegate ((Font _, object __, out FontExtents e) =>
				del.Invoke (Font, FontData, out e));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_font_h_extents_func (Handle, FontExtentsProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalFontExtentsDelegate (FontExtentsDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new FontExtentsDelegate ((Font _, object __, out FontExtents e) =>
				del.Invoke (Font, FontData, out e));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_font_v_extents_func (Handle, FontExtentsProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (NominalGlyphProxyDelegate))]
		private static bool NominalGlyphProxyImplementation (IntPtr font, IntPtr fontData, uint unicode, out uint glyph, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (NominalGlyphDelegate)multi.Invoke (typeof (NominalGlyphDelegate));
			return del.Invoke (null, null, unicode, out glyph);
		}

		public void SetNominalGlyphDelegate (NominalGlyphDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new NominalGlyphDelegate ((Font _, object __, uint u, out uint g) =>
				del.Invoke (Font, FontData, u, out g));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_nominal_glyph_func (Handle, NominalGlyphProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (NominalGlyphsProxyDelegate))]
		private static uint NominalGlyphsProxyImplementation (IntPtr font, IntPtr fontData, uint count,
			IntPtr firstUnicode, uint unicodeStride, out IntPtr firstGlyph, out uint glyphStride, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (NominalGlyphsDelegate)multi.Invoke (typeof (NominalGlyphsDelegate));

			unsafe {
				var unicodes = new ReadOnlySpan<uint> ((void*)firstUnicode, (int)unicodeStride);
				var glyphCount = del.Invoke (null, null, count, unicodes, out var glyphs);

				fixed (uint* glyphsPtr = glyphs) {
					firstGlyph = (IntPtr)glyphsPtr;
					glyphStride = (uint)glyphs.Length;
				}

				return glyphCount;
			}
		}

		public void SetNominalGlyphsDelegate (NominalGlyphsDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new NominalGlyphsDelegate (
				(Font _, object __, uint c, ReadOnlySpan<uint> u, out ReadOnlySpan<uint> g) =>
					del.Invoke (Font, FontData, c, u, out g));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_nominal_glyphs_func (Handle, NominalGlyphsProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (VariationGlyphProxyDelegate))]
		private static bool VariationGlyphProxyImplementation (IntPtr font, IntPtr fontData, uint unicode,
			uint variationSelector, out uint glyph, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (VariationGlyphDelegate)multi.Invoke (typeof (VariationGlyphDelegate));
			return del.Invoke (null, null, unicode, variationSelector, out glyph);
		}

		public void SetVariationGlyphDelegate (VariationGlyphDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new VariationGlyphDelegate ((Font _, object __, uint u, uint v, out uint g) =>
				del.Invoke (Font, FontData, u, v, out g));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_variation_glyph_func (Handle, VariationGlyphProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphAdvanceProxyDelegate))]
		private static int GlyphAdvanceProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphAdvanceDelegate)multi.Invoke (typeof (GlyphAdvanceDelegate));
			return del.Invoke (null, null, glyph);
		}

		public void SetHorizontalGlyphAdvanceDelegate (GlyphAdvanceDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphAdvanceDelegate ((_, __, g) =>
				del.Invoke (Font, FontData, g));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_advance_func (Handle, GlyphAdvanceProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphAdvanceDelegate (GlyphAdvanceDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphAdvanceDelegate ((_, __, g) =>
				del.Invoke (Font, FontData, g));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_advance_func (Handle, GlyphAdvanceProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphAdvancesProxyDelegate))]
		private static void GlyphAdvancesProxyImplementation (IntPtr font, IntPtr fontData, uint count,
			IntPtr firstGlyph, uint glyphStride, out IntPtr firstAdvance, out uint advanceStride, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphAdvancesDelegate)multi.Invoke (typeof (GlyphAdvancesDelegate));

			unsafe {
				var glyphs = new ReadOnlySpan<uint> ((void*)firstGlyph, (int)glyphStride);

				del.Invoke (null, null, count, glyphs, out var advances);

				fixed (int* advancesPtr = advances) {
					firstAdvance = (IntPtr)advancesPtr;
					advanceStride = (uint)advances.Length;
				}
			}
		}

		public void SetHorizontalGlyphAdvancesDelegate (GlyphAdvancesDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphAdvancesDelegate (
				(Font _, object __, uint c, ReadOnlySpan<uint> u, out ReadOnlySpan<int> a) =>
					del.Invoke (Font, FontData, c, u, out a));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_advances_func (Handle, GlyphAdvancesProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphAdvancesDelegate (GlyphAdvancesDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphAdvancesDelegate (
				(Font _, object __, uint c, ReadOnlySpan<uint> u, out ReadOnlySpan<int> a) =>
					del.Invoke (Font, FontData, c, u, out a));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_v_advances_func (Handle, GlyphAdvancesProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphOriginProxyDelegate))]
		private static bool GlyphOriginProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, out int x, out int y, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphOriginDelegate)multi.Invoke (typeof (GlyphOriginDelegate));
			return del.Invoke (null, null, glyph, out x, out y);
		}

		public void SetHorizontalGlyphOriginDelegate (GlyphOriginDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphOriginDelegate ((Font _, object __, uint g, out int x, out int y) =>
				del.Invoke (Font, FontData, g, out x, out y));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_origin_func (Handle, GlyphOriginProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphOriginDelegate (GlyphOriginDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphOriginDelegate ((Font _, object __, uint g, out int x, out int y) =>
				del.Invoke (Font, FontData, g, out x, out y));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_v_origin_func (Handle, GlyphOriginProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphKerningProxyDelegate))]
		private static int GlyphKerningProxyImplementation (IntPtr font, IntPtr fontData, uint firstGlyph, uint secondGlyph, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphKerningDelegate)multi.Invoke (typeof (GlyphKerningDelegate));
			return del.Invoke (null, null, firstGlyph, secondGlyph);
		}

		public void SetHorizontalGlyphKerningDelegate (GlyphKerningDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphKerningDelegate ((_, __, f, s) =>
				del.Invoke (Font, FontData, f, s));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_h_kerning_func (Handle, GlyphKerningProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphExtentsProxyDelegate))]
		private static bool GlyphExtentsProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, out GlyphExtents extents, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphExtentsDelegate)multi.Invoke (typeof (GlyphExtentsDelegate));
			return del.Invoke (null, null, glyph, out extents);
		}

		public void SetGlyphExtentsDelegate (GlyphExtentsDelegate del, object context = null, ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphExtentsDelegate ((Font _, object __, uint g, out GlyphExtents e) =>
				del.Invoke (Font, FontData, g, out e));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_extents_func (Handle, GlyphExtentsProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphContourPointProxyDelegate))]
		private static bool GlyphContourPointProxyImplementation (IntPtr font, IntPtr fontData, uint glyph, uint pointIndex, out int x, out int y, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphContourPointDelegate)multi.Invoke (typeof (GlyphContourPointDelegate));
			return del.Invoke (null, null, glyph, pointIndex, out x, out y);
		}

		public void SetGlyphContourPointDelegate (GlyphContourPointDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphContourPointDelegate (
				(Font _, object __, uint g, uint i, out int x, out int y) =>
					del.Invoke (Font, FontData, g, i, out x, out y));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

			HarfBuzzApi.hb_font_funcs_set_glyph_contour_point_func (Handle, GlyphContourPointProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		[MonoPInvokeCallback (typeof (GlyphNameProxyDelegate))]
		private static unsafe bool GlyphNameProxyImplementation (IntPtr font, IntPtr fontData, uint glyph,
			char* nameBuffer, int size, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphNameDelegate)multi.Invoke (typeof (GlyphNameDelegate));
			var success = del.Invoke (null, null, glyph, nameBuffer, size);

			return success;
		}

		public void SetGlyphNameDelegate (GlyphNameDelegate del, object context = null, ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			unsafe {
				var wrappedDelegate = new GlyphNameDelegate ((_, __, g, nb, s) =>
					del.Invoke (Font, FontData, g, nb, s));

				var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

				HarfBuzzApi.hb_font_funcs_set_glyph_name_func (Handle, GlyphNameProxy, ctx,
					DelegateProxies.ReleaseDelegateProxyForMulti);
			}
		}

		[MonoPInvokeCallback (typeof (GlyphFromNameProxyDelegate))]
		private static bool GlyphFromNameProxyImplementation (IntPtr font, IntPtr fontData, string name,
			int len, out uint glyph, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphFromNameDelegate)multi.Invoke (typeof (GlyphFromNameDelegate));
			return del.Invoke (null, null, name, out glyph);
		}

		public void SetGlyphFromNameDelegate (GlyphFromNameDelegate del, object context = null,
			ReleaseDelegate destroy = null)
		{
			if (IsImmutable) {
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			}

			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedDelegate = new GlyphFromNameDelegate ((Font _, object __, string n, out uint g) =>
				del.Invoke (Font, FontData, n, out g));

			var ctx = DelegateProxies.CreateMulti (wrappedDelegate, context, destroy);

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
}
