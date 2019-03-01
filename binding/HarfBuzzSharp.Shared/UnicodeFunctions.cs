using System;

namespace HarfBuzzSharp
{
	public class UnicodeFunctions : NativeObject
	{
		public static UnicodeFunctions Default => new UnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_default ());

		public static UnicodeFunctions Empty => new UnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_empty ());

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

		protected override void Dispose (bool disposing)
		{
			HarfBuzzApi.hb_unicode_funcs_destroy (Handle);

			base.Dispose (disposing);
		}
	}
}
