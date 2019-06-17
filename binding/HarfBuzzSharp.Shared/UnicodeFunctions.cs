﻿using System;

namespace HarfBuzzSharp
{
	public class UnicodeFunctions : NativeObject
	{
		private static readonly Lazy<UnicodeFunctions> defaultFunctions = new Lazy<UnicodeFunctions> (() => new StaticUnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_default ()));
		private static readonly Lazy<UnicodeFunctions> emptyFunctions = new Lazy<UnicodeFunctions> (() => new StaticUnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_empty ()));

		public static UnicodeFunctions Default => defaultFunctions.Value;

		public static UnicodeFunctions Empty => emptyFunctions.Value;

		internal UnicodeFunctions (IntPtr handle)
			: base (handle)
		{
		}

		public bool IsImmutable => HarfBuzzApi.hb_unicode_funcs_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_unicode_funcs_make_immutable (Handle);

		public UnicodeCombiningClass GetCombiningClass (int unicode)
		{
			if (unicode < 0) {
				throw new ArgumentOutOfRangeException (nameof (unicode), "Unicode must be non negative.");
			}

			return HarfBuzzApi.hb_unicode_combining_class (Handle, (uint)unicode);
		}

		public UnicodeCombiningClass GetCombiningClass (uint unicode) =>
			HarfBuzzApi.hb_unicode_combining_class (Handle, unicode);

		public UnicodeGeneralCategory GetGeneralCategory (int unicode)
		{
			if (unicode < 0) {
				throw new ArgumentOutOfRangeException (nameof (unicode), "Unicode must be non negative.");
			}

			return HarfBuzzApi.hb_unicode_general_category (Handle, (uint)unicode);
		}

		public UnicodeGeneralCategory GetGeneralCategory (uint unicode) =>
			HarfBuzzApi.hb_unicode_general_category (Handle, unicode);

		public Script GetScript (int unicode)
		{
			if (unicode < 0) {
				throw new ArgumentOutOfRangeException (nameof (unicode), "Unicode must be non negative.");
			}

			return HarfBuzzApi.hb_unicode_script (Handle, (uint)unicode);
		}

		public Script GetScript (uint unicode) => HarfBuzzApi.hb_unicode_script (Handle, unicode);

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_unicode_funcs_destroy (Handle);
			}
		}

		private class StaticUnicodeFunctions : UnicodeFunctions
		{
			public StaticUnicodeFunctions (IntPtr handle)
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
