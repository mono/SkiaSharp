using System;
using System.Text;

namespace HarfBuzzSharp
{
	internal delegate bool FontExtentsProxyDelegate (IntPtr font, IntPtr fontData, out FontExtents extents,
		IntPtr context);
	internal delegate bool NominalGlyphProxyDelegate (IntPtr font, IntPtr fontData, uint unicode,
		out uint glyph, IntPtr context);
	internal delegate bool VariationGlyphProxyDelegate (IntPtr font, IntPtr fontData, uint unicode,
		uint variationSelector, out uint glyph, IntPtr context);
	internal delegate uint NominalGlyphsProxyDelegate (IntPtr font, IntPtr fontData, uint count,
		IntPtr firstUnicode, uint unicodeStride, IntPtr firstGlyph, out uint glyphStride, IntPtr context);
	internal delegate int GlyphAdvanceProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, IntPtr context);
	internal delegate void GlyphAdvancesProxyDelegate (IntPtr font, IntPtr fontData, uint count,
		IntPtr firstGlyph, uint glyphStride, IntPtr firstAdvance, out uint advanceStride, IntPtr context);
	internal delegate bool GlyphOriginProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, out int x,
		out int y, uint userData);
	internal delegate int GlyphKerningProxyDelegate (IntPtr font, IntPtr fontData, uint firstGlyph,
		uint secondGlyph, IntPtr context);
	internal delegate bool GlyphExtentsProxyDelegate (IntPtr font, IntPtr fontData, uint glyph,
		out GlyphExtents extents, IntPtr context);
	internal delegate bool GlyphContourPointProxyDelegate (IntPtr font, IntPtr fontData, uint glyph,
		uint pointIndex, out int x, out int y, IntPtr context);
	internal delegate bool GlyphNameProxyDelegate (IntPtr font, IntPtr fontData, uint glyph, out StringBuilder name,
		out uint size, IntPtr context);
	internal delegate bool GlyphFromNameProxyDelegate (IntPtr font, IntPtr fontData, StringBuilder name,
		int len, out uint glyph, IntPtr context);

	public class FontFunctions : NativeObject
	{
		private static readonly Lazy<FontFunctions> emptyFontFunctions =
			new Lazy<FontFunctions> (() => new StaticFontFunctions (HarfBuzzApi.hb_font_funcs_get_empty ()));

		public delegate bool FontExtentsDelegate (Font font, object fontData, out FontExtents extents);

		public delegate bool NominalGlyphDelegate (Font font, object fontData, uint unicode, out uint glyph);

		public delegate bool VariationGlyphDelegate (Font font, object fontData, uint unicode, uint variationSelector, out uint glyph);

		public delegate uint NominalGlyphsDelegate (Font font, object fontData, uint count, ReadOnlySpan<uint> codepoints, out ReadOnlySpan<uint> glyphs);

		public delegate int GlyphAdvanceDelegate (Font font, object fontData, uint glyph);

		public delegate void GlyphAdvancesDelegate (Font font, object fontData, uint count, ReadOnlySpan<uint> glyphs, out ReadOnlySpan<int> advances);

		public delegate bool GlyphOriginDelegate (Font font, object fontData, uint glyph, out int x, out int y);

		public delegate int GlyphKerningDelegate (Font font, object fontData, uint firstGlyph, uint secondGlyph);

		public delegate bool GlyphExtentsDelegate (Font font, object fontData, uint glyph, out GlyphExtents extents);

		public delegate bool GlyphContourPointDelegate (Font font, object fontData, uint glyph, uint pointIndex, out int x, out int y);

		public delegate bool GlyphNameDelegate (Font font, object fontData, uint glyph, out string name);

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

		private static readonly GlyphFromNameProxyDelegate GlyphFromNameProxy = GlyphFromNameProxyDelegateImplementation;

		private static readonly GlyphNameProxyDelegate GlyphNameProxy = GlyphNameProxyDelegateImplementation;

		public void SetHorizontalFontExtentsDelegate (FontExtentsDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new FontExtentsDelegate ((Font _, object __, out FontExtents e) =>
				del.Invoke (Font, FontData, out e));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (FontExtentsDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetVerticalFontExtentsDelegate (FontExtentsDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new FontExtentsDelegate ((Font _, object __, out FontExtents e) =>
				del.Invoke (Font, FontData, out e));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (FontExtentsDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetNominalGlyphDelegate (NominalGlyphDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new NominalGlyphDelegate ((Font _, object __, uint u, out uint g) =>
				del.Invoke (Font, FontData, u, out g));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (NominalGlyphDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetNominalGlyphsDelegate (NominalGlyphsDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new NominalGlyphsDelegate ((Font _, object __, uint c, ReadOnlySpan<uint> u, out ReadOnlySpan<uint> g) =>
				del.Invoke (Font, FontData, c, u, out g));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (NominalGlyphsDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetVariationGlyphDelegate (VariationGlyphDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new VariationGlyphDelegate ((Font _, object __, uint u, uint v, out uint g) =>
				del.Invoke (Font, FontData, u, v, out g));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (VariationGlyphDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetHorizontalGlyphAdvanceDelegate (GlyphAdvanceDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphAdvanceDelegate ((_, __, g) =>
				del.Invoke (Font, FontData, g));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphAdvanceDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetVerticalGlyphAdvanceDelegate (GlyphAdvanceDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphAdvanceDelegate ((_, __, g) =>
				del.Invoke (Font, FontData, g));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphAdvanceDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetHorizontalGlyphAdvancesDelegate (GlyphAdvancesDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphAdvancesDelegate ((Font _, object __, uint c, ReadOnlySpan<uint> u, out ReadOnlySpan<int> a) =>
				del.Invoke (Font, FontData, c, u, out a));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphAdvancesDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetVerticalGlyphAdvancesDelegate (GlyphAdvancesDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphAdvancesDelegate ((Font _, object __, uint c, ReadOnlySpan<uint> u, out ReadOnlySpan<int> a) =>
				del.Invoke (Font, FontData, c, u, out a));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphAdvancesDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetHorizontalGlyphOriginDelegate (GlyphOriginDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphOriginDelegate ((Font _, object __, uint g, out int x, out int y) =>
				del.Invoke (Font, FontData, g, out x, out y));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphOriginDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetVerticalGlyphOriginDelegate (GlyphOriginDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphOriginDelegate ((Font _, object __, uint g, out int x, out int y) =>
				del.Invoke (Font, FontData, g, out x, out y));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphOriginDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetHorizontalGlyphKerningDelegate (GlyphKerningDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphKerningDelegate ((_, __, f, s) =>
				del.Invoke (Font, FontData, f, s));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphKerningDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetGlyphExtentsDelegate (GlyphExtentsDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphExtentsDelegate ((Font _, object __, uint g, out GlyphExtents e) =>
				del.Invoke (Font, FontData, g, out e));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphExtentsDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		public void SetGlyphContourPointDelegate (GlyphContourPointDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphContourPointDelegate (
				(Font _, object __, uint g, uint i, out int x, out int y) =>
					del.Invoke (Font, FontData, g, i, out x, out y));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphContourPointDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);
		}

		[MonoPInvokeCallback (typeof (GlyphNameProxyDelegate))]
		private static bool GlyphNameProxyDelegateImplementation (IntPtr font, IntPtr fontData, uint glyph, out StringBuilder name, out uint size, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphNameDelegate)multi.Invoke (typeof (GlyphNameDelegate));
			var sucess = del.Invoke (null, null, glyph, out var s);

			name = new StringBuilder (s);
			size = (uint)name.Length;

			return sucess;
		}

		public void SetGlyphNameDelegate (GlyphNameDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphNameDelegate ((Font _, object __, uint g, out string n) =>
				del.Invoke (Font, FontData, g, out n));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphNameDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);

			HarfBuzzApi.hb_font_funcs_set_glyph_name_func (Handle, GlyphNameProxy, ctx, DelegateProxies.ReleaseDelegateProxyForGetTable);
		}

		[MonoPInvokeCallback (typeof (GlyphFromNameProxyDelegate))]
		private static bool GlyphFromNameProxyDelegateImplementation (IntPtr font, IntPtr fontData, StringBuilder name,
			int len, out uint glyph, IntPtr context)
		{
			var multi = DelegateProxies.Get<GetMultiDelegateDelegate> (context, out _);
			var del = (GlyphFromNameDelegate)multi.Invoke (typeof (GlyphFromNameDelegate));
			return del.Invoke (null, null, name.ToString (), out glyph);
		}

		public void SetGlyphFromNameDelegate (GlyphFromNameDelegate del, ReleaseDelegate destroy = null, object context = null)
		{
			if (del == null) {
				throw new ArgumentException (nameof (del));
			}

			var wrappedFunction = new GlyphFromNameDelegate ((Font _, object __, string n, out uint g) =>
				del.Invoke (Font, FontData, n, out g));

			var destroyDelegate = destroy != null && context != null
				? (_) => destroy (context)
				: destroy;

			var multi = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (GlyphFromNameDelegate))
					return wrappedFunction;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			DelegateProxies.Create (multi, out _, out var ctx);

			HarfBuzzApi.hb_font_funcs_set_glyph_from_name_func (Handle, GlyphFromNameProxy, ctx,
				DelegateProxies.ReleaseDelegateProxyForGetTable);
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
