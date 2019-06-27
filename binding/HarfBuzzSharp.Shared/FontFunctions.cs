using System;

namespace HarfBuzzSharp
{
	public class FontFunctions : NativeObject
	{
		private static readonly Lazy<FontFunctions> emptyFontFunctions =
			new Lazy<FontFunctions> (() => new StaticFontFunctions (HarfBuzzApi.hb_font_funcs_get_empty ()));

		public FontFunctions ()
			: this (HarfBuzzApi.hb_font_funcs_create ())
		{
		}

		internal FontFunctions (IntPtr handle)
			: base (handle)
		{
		}

		public static FontFunctions Empty => emptyFontFunctions.Value;

		public bool IsImmutable => HarfBuzzApi.hb_font_funcs_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_font_funcs_make_immutable (Handle);

		public void SetHorizontalFontExtentsDelegate (FontExtentsDelegate del) =>
			SetHorizontalFontExtentsDelegate (del, null);

		public void SetHorizontalFontExtentsDelegate (FontExtentsDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_font_h_extents_func (Handle, DelegateProxies.FontExtentsProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalFontExtentsDelegate (FontExtentsDelegate del) =>
			SetVerticalFontExtentsDelegate (del, null);

		public void SetVerticalFontExtentsDelegate (FontExtentsDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_font_v_extents_func (Handle, DelegateProxies.FontExtentsProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetNominalGlyphDelegate (NominalGlyphDelegate del) =>
			SetNominalGlyphDelegate (del, null);

		public void SetNominalGlyphDelegate (NominalGlyphDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_nominal_glyph_func (Handle, DelegateProxies.NominalGlyphProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetNominalGlyphsDelegate (NominalGlyphsDelegate del) =>
			SetNominalGlyphsDelegate (del, null);

		public void SetNominalGlyphsDelegate (NominalGlyphsDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_nominal_glyphs_func (Handle, DelegateProxies.NominalGlyphsProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVariationGlyphDelegate (VariationGlyphDelegate del) =>
			SetVariationGlyphDelegate (del, null);

		public void SetVariationGlyphDelegate (VariationGlyphDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_variation_glyph_func (Handle, DelegateProxies.VariationGlyphProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetHorizontalGlyphAdvanceDelegate (GlyphAdvanceDelegate del) =>
			SetHorizontalGlyphAdvanceDelegate (del, null);

		public void SetHorizontalGlyphAdvanceDelegate (GlyphAdvanceDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_h_advance_func (Handle, DelegateProxies.GlyphAdvanceProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphAdvanceDelegate (GlyphAdvanceDelegate del) =>
			SetVerticalGlyphAdvanceDelegate (del, null);

		public void SetVerticalGlyphAdvanceDelegate (GlyphAdvanceDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_v_advance_func (Handle, DelegateProxies.GlyphAdvanceProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetHorizontalGlyphAdvancesDelegate (GlyphAdvancesDelegate del) =>
			SetHorizontalGlyphAdvancesDelegate (del, null);

		public void SetHorizontalGlyphAdvancesDelegate (GlyphAdvancesDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_h_advances_func (Handle, DelegateProxies.GlyphAdvancesProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphAdvancesDelegate (GlyphAdvancesDelegate del) =>
			SetVerticalGlyphAdvancesDelegate (del, null);

		public void SetVerticalGlyphAdvancesDelegate (GlyphAdvancesDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_v_advances_func (Handle, DelegateProxies.GlyphAdvancesProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetHorizontalGlyphOriginDelegate (GlyphOriginDelegate del) =>
			SetHorizontalGlyphOriginDelegate (del, null);

		public void SetHorizontalGlyphOriginDelegate (GlyphOriginDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_h_origin_func (Handle, DelegateProxies.GlyphOriginProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetVerticalGlyphOriginDelegate (GlyphOriginDelegate del) =>
			SetVerticalGlyphOriginDelegate (del, null);

		public void SetVerticalGlyphOriginDelegate (GlyphOriginDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_v_origin_func (Handle, DelegateProxies.GlyphOriginProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetHorizontalGlyphKerningDelegate (GlyphKerningDelegate del) =>
			SetHorizontalGlyphKerningDelegate (del, null);

		public void SetHorizontalGlyphKerningDelegate (GlyphKerningDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_h_kerning_func (Handle, DelegateProxies.GlyphKerningProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetGlyphExtentsDelegate (GlyphExtentsDelegate del) =>
			SetGlyphExtentsDelegate (del, null);

		public void SetGlyphExtentsDelegate (GlyphExtentsDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_extents_func (Handle, DelegateProxies.GlyphExtentsProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetGlyphContourPointDelegate (GlyphContourPointDelegate del) =>
			SetGlyphContourPointDelegate (del, null);

		public void SetGlyphContourPointDelegate (GlyphContourPointDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_contour_point_func (Handle, DelegateProxies.GlyphContourPointProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetGlyphNameDelegate (GlyphNameDelegate del) =>
			SetGlyphNameDelegate (del, null);

		public void SetGlyphNameDelegate (GlyphNameDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_name_func (Handle, DelegateProxies.GlyphNameProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetGlyphFromNameDelegate (GlyphFromNameDelegate del) =>
			SetGlyphFromNameDelegate (del, null);

		public void SetGlyphFromNameDelegate (GlyphFromNameDelegate del, ReleaseDelegate destroy)
		{
			if (IsImmutable)
				throw new NotSupportedException ($"{nameof (FontFunctions)} is immutable and can't be changed.");
			if (del == null)
				throw new ArgumentException (nameof (del));

			var ctx = DelegateProxies.CreateMulti (del, destroy);
			HarfBuzzApi.hb_font_funcs_set_glyph_from_name_func (Handle, DelegateProxies.GlyphFromNameProxy, ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
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
